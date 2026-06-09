using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StationeryStoreManagementSystem.BL
{
    /// <summary>
    /// Engine for interfacing with Large Language Models (LLMs).
    /// Supports both local Ollama and external OpenAI-compatible APIs.
    /// </summary>
    public static class LocalAIEngine
    {
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };

        public static async Task<string> GetDailyExecutiveSummaryAsync(string jsonAggregatedData)
        {
            string provider = GlobalSettings.AIProvider;
            string url = GlobalSettings.AIUrl;
            string apiKey = GlobalSettings.AIApiKey;
            string model = GlobalSettings.AIModel;

            try
            {
                if (provider == "Ollama")
                {
                    return await CallOllamaAsync(url, model, "", jsonAggregatedData);
                }
                else
                {
                    return await CallOpenAIAsync(url, apiKey, model, "", jsonAggregatedData);
                }
            }
            catch (Exception ex)
            {
                return $"AI General Error: {ex.Message}";
            }
        }

        private static async Task<string> CallOllamaAsync(string url, string model, string system, string data)
        {
            var requestBody = new
            {
                model = model,
                prompt = $"Data for analysis: {data}",
                system = GlobalSettings.AISystemPrompt,
                stream = false
            };

            string jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
                return $"Ollama Error: {response.StatusCode}. Check URL: {url}";

            string responseBody = await response.Content.ReadAsStringAsync();
            using (JsonDocument doc = JsonDocument.Parse(responseBody))
            {
                return doc.RootElement.GetProperty("response").GetString() ?? "Empty response";
            }
        }

        private static async Task<string> CallOpenAIAsync(string url, string apiKey, string model, string system, string data)
        {
            // Standard OpenAI Chat Completion format
            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = GlobalSettings.AISystemPrompt },
                    new { role = "user", content = $"Data for analysis: {data}" }
                },
                temperature = 0.5,
                top_p = 1,
                max_tokens = 1024
            };

            string jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = content;
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
            }

            // OpenRouter specific headers (highly recommended)
            request.Headers.Add("HTTP-Referer", "https://github.com/stationery-store-pos");
            request.Headers.Add("X-Title", "Stationery Store POS");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                return $"AI API Error: {response.StatusCode}. {error}";
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            using (JsonDocument doc = JsonDocument.Parse(responseBody))
            {
                // OpenAI response structure: choices[0].message.content
                return doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "Empty response";
            }
        }
    }
}
