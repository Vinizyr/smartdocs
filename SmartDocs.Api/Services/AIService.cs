using System.Text;
using System.Text.Json;

namespace SmartDocs.Api.Services
{
    public partial class AIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public AIService(IConfiguration config)
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(10)
            };
            _baseUrl = config["AI:BaseUrl"]!;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<string> GenerateSummaryAsync(string text)
        {
            var request = new
            {
                model = "llama3.2:1b",
                prompt = $"Resuma o seguinte texto de forma objetiva e clara: {text}",
                stream = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<OllamaResponse>(json, _jsonOptions);
            return result?.Response ?? "";
        }

        public async Task<string> GenerateInsightsAsync(string text)
        {
            var request = new
            {
                model = "llama3.2:1b",
                prompt = $"Liste insights importantes sobre o texto abaixo: {text}",
                stream = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<OllamaResponse>(json, _jsonOptions);
            return result?.Response ?? "";
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var request = new
            {
                model = "nomic-embed-text",
                input = text
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/embeddings", content);
            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(json, _jsonOptions);
            return result?.Embedding ?? Array.Empty<float>();
        }

        public async Task<string> GenerateFromPromptAsync(string model, string prompt)
        {
            var request = new
            {
                model = model,
                prompt = prompt,
                stream = false
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OllamaResponse>(json, _jsonOptions);
            return result?.Response ?? string.Empty;
        }
    }
}
