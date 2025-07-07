namespace CarInsuranceBot.Infrastructure.Services;


public sealed class GeminiService : IGeminiService
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _opts;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GeminiService> _log;

    public GeminiService(
        IOptions<GeminiOptions> opts,
        IHttpClientFactory httpFactory,
        IUnitOfWork uow,
        ILogger<GeminiService> log)
    {
        _opts = opts.Value;
        _http = httpFactory.CreateClient("gemini");
        _uow = uow;
        _log = log;

        _http.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
        _http.DefaultRequestHeaders.Add("x-goog-api-key", _opts.ApiKey);
        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // ------------------------------------------------------------------
    public async Task<string> AskAsync(long chatId,
                                       string prompt,
                                       CancellationToken ct)
    {
        const int maxRetries = 3;
        const int baseDelayMs = 2_000;
        int attempt = 0;
        var startUtc = DateTime.UtcNow;

        while (true)
        {
            var reqBody = new
            {
                contents = new[]
                {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
            };

            string route = $"v1beta/models/{_opts.Model ?? "gemini-2.0-flash"}:generateContent";

            var resp = await _http.PostAsJsonAsync(route, reqBody, ct);

            if (resp.IsSuccessStatusCode)
            {
                var data = await resp.Content.ReadFromJsonAsync<GeminiResponse>(ct);

                string answer = data?.Candidates
                                     ?.FirstOrDefault()?
                                     .Content?
                                     .Parts?
                                     .FirstOrDefault()?
                                     .Text?
                                     .Trim()
                               ?? "Sorry, I couldn’t generate a reply.";

                var userId = await _uow.Users.GetAsync(chatId, ct);
                _uow.Conversations.Add(new Conversation
                {
                    UserId = userId!.Id,
                    Prompt = prompt,
                    Response = answer,
                    CreatedUtc = startUtc
                });
                await _uow.SaveChangesAsync(ct);

                return answer;
            }
            resp.EnsureSuccessStatusCode();
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Response DTOs that match generateContent’s JSON payload

    private sealed record GeminiResponse(
        [property: JsonPropertyName("candidates")] Candidate[] Candidates);

    private sealed record Candidate(
        [property: JsonPropertyName("content")] Content Content);

    private sealed record Content(
        [property: JsonPropertyName("parts")] Part[] Parts);

    private sealed record Part(
        [property: JsonPropertyName("text")] string Text);
}
