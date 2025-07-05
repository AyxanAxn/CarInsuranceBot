namespace CarInsuranceBot.Infrastructure.Services;

public class OpenAIService : IOpenAIService
{
    private readonly HttpClient _http;
    private readonly OpenAIOptions _opts;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<OpenAIService> _log;

    public OpenAIService(
        IOptions<OpenAIOptions> opts,
        IHttpClientFactory httpFactory,
        IUnitOfWork uow,
        ILogger<OpenAIService> log)
    {
        _opts = opts.Value;
        _http = httpFactory.CreateClient("openai");
        _uow = uow;
        _log = log;

        // configure once
        _http.BaseAddress = new Uri("https://api.openai.com/v1/");
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _opts.ApiKey);
    }

    // ------------------------------------------------------------------
    public async Task<string> AskAsync(long chatId, string prompt, CancellationToken ct)
    {
        const int maxRetries = 3;
        const int baseDelayMs = 2_000;     // first retry after 2 s
        int attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                var reqBody = new
                {
                    model = _opts.Model ?? "gpt-3.5-turbo",
                    messages = new[]
                    {
                    new { role = "system", content = "You are a helpful car-insurance assistant." },
                    new { role = "user",   content = prompt }
                }
                };

                var resp = await _http.PostAsJsonAsync("chat/completions", reqBody, ct);

                if (resp.IsSuccessStatusCode)
                {
                    var data = await resp.Content.ReadFromJsonAsync<OpenAIResponse>(ct);
                    var answer = data?.Choices[0].Message.Content?.Trim()
                                 ?? "Sorry, I couldn’t generate a reply.";
                    return answer;
                }

                // 429: handle retry-after
                if ((int)resp.StatusCode == 429 && attempt <= maxRetries)
                {
                    int delay = GetRetryDelay(resp, attempt, baseDelayMs);
                    _log.LogWarning("OpenAI 429, retrying in {Delay} ms (attempt {A})", delay, attempt);
                    await Task.Delay(delay, ct);
                    continue;
                }

                // any other error
                resp.EnsureSuccessStatusCode(); // will throw
            }
            catch (Exception ex) when (attempt <= maxRetries && !ct.IsCancellationRequested)
            {
                // network or timeout – exponential back-off
                int delay = baseDelayMs * attempt;
                _log.LogWarning(ex, "OpenAI call failed, retrying in {Delay} ms (attempt {A})", delay, attempt);
                await Task.Delay(delay, ct);
            }
        }
    }

    // helper ------------------------------------------------------------
    private static int GetRetryDelay(HttpResponseMessage resp, int attempt, int baseDelay)
    {
        if (resp.Headers.TryGetValues("Retry-After", out var hdr) &&
            int.TryParse(hdr.First(), out int seconds))
            return seconds * 1_000;

        // fallback exponential back-off
        return baseDelay * attempt;
    }


    // DTOs for quick JSON binding
    private sealed record OpenAIResponse(
        [property: JsonPropertyName("choices")] Choice[] Choices);

    private sealed record Choice(
        [property: JsonPropertyName("message")] Message Message);

    private sealed record Message(
        [property: JsonPropertyName("content")] string Content);
}