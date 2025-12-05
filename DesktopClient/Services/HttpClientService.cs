using System.Net.Http.Headers;
using System.Text.Json;

namespace ImageAnnotationApp.Services
{
    public class HttpClientService
    {
        private static HttpClientService? _instance;
        private static readonly object _lock = new();
        private readonly HttpClient _httpClient;
        private string? _token;

        public string BaseUrl { get; set; } = "http://localhost:5097/api";

        private HttpClientService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        public static HttpClientService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new HttpClientService();
                    }
                }
                return _instance;
            }
        }

        public void SetToken(string? token)
        {
            _token = token;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public string? GetToken()
        {
            return _token;
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{endpoint}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    SetToken(null);
                    throw new UnauthorizedAccessException("未授权，请重新登录");
                }

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"GET 请求失败: {ex.Message}", ex);
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object? data = null)
        {
            try
            {
                var json = data != null ? JsonSerializer.Serialize(data) : "{}";
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/{endpoint}", content);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    SetToken(null);
                    throw new UnauthorizedAccessException("未授权，请重新登录");
                }

                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"POST 请求失败: {ex.Message}", ex);
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{BaseUrl}/{endpoint}", content);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    SetToken(null);
                    throw new UnauthorizedAccessException("未授权，请重新登录");
                }

                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"PUT 请求失败: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{endpoint}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    SetToken(null);
                    throw new UnauthorizedAccessException("未授权，请重新登录");
                }

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"DELETE 请求失败: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> DownloadAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{endpoint}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    SetToken(null);
                    throw new UnauthorizedAccessException("未授权，请重新登录");
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"下载失败: {ex.Message}", ex);
            }
        }

        public async Task<T?> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{BaseUrl}/{endpoint}", content);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    SetToken(null);
                    throw new UnauthorizedAccessException("未授权，请重新登录");
                }

                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"上传失败: {ex.Message}", ex);
            }
        }

        public async Task<string?> GetRawResponseAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{endpoint}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    SetToken(null);
                    throw new UnauthorizedAccessException("未授权，请重新登录");
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"GET 请求失败: {ex.Message}", ex);
            }
        }

        public async Task<HttpResponseMessage> RawPostAsync(string endpoint, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var url = $"{BaseUrl}/{endpoint}";

            var response = await _httpClient.PostAsync(url, content);

            return response;
        }


    }
}
