using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CarInsuranceBot.Application.Common;
using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarInsuranceBot.Infrastructure.Services;

/// <summary>
/// Wraps Google Gemini’s native “generateContent” REST endpoint.
/// </summary>
public sealed class OpenAIService : IOpenAIService
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
        _http = httpFactory.CreateClient("gemini");
        _uow = uow;
        _log = log;

        _http.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
        _http.DefaultRequestHeaders.Add("x-goog-api-key", _opts.ApiKey);
        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        // “generateContent” uses an API-key header, *not* Bearer.
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
            attempt++;

            try
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
                    // You can add temperature, top_p, safetySettings, etc. here.
                };

                // e.g.  v1beta/models/gemini-2.0-flash:generateContent
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

                    _uow.Conversations.Add(new Conversation
                    {
                        Prompt = prompt,
                        Response = answer,
                        CreatedUtc = startUtc
                    });
                    //await _uow.SaveChangesAsync(ct);

                    return answer;
                }

                // 429 or transient error → retry
                if ((int)resp.StatusCode == 429 && attempt <= maxRetries)
                {
                    int delay = GetRetryDelay(resp, attempt, baseDelayMs);
                    _log.LogWarning("Gemini 429, retrying in {Delay} ms (attempt {Attempt})",
                                    delay, attempt);
                    await Task.Delay(delay, ct);
                    continue;
                }

                resp.EnsureSuccessStatusCode();   // throw for non-success
            }
            catch (Exception ex) when (attempt <= maxRetries &&
                                       !ct.IsCancellationRequested)
            {
                int delay = baseDelayMs * attempt;
                _log.LogWarning(ex,
                    "Gemini call failed, retrying in {Delay} ms (attempt {Attempt})",
                    delay, attempt);
                await Task.Delay(delay, ct);
            }
        }
    }

    // ------------------------------------------------------------------
    private static int GetRetryDelay(HttpResponseMessage resp,
                                     int attempt,
                                     int baseDelay)
    {
        if (resp.Headers.TryGetValues("Retry-After", out var hdr) &&
            int.TryParse(hdr.First(), out int seconds))
            return seconds * 1_000;

        return baseDelay * attempt;
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
