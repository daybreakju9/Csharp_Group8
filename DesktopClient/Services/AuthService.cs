using ImageAnnotationApp.Models;

namespace ImageAnnotationApp.Services
{
    public class AuthService
    {
        private static AuthService? _instance;
        private static readonly object _lock = new object();

        public static AuthService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new AuthService();
                    }
                }
                return _instance;
            }
        }

        private readonly HttpClientService _httpClient;
        private User? _currentUser;

        public User? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;
        public bool IsAdmin => _currentUser?.Role == "Admin";

        public AuthService()
        {
            _httpClient = HttpClientService.Instance;
        }

        // 内部类：用于解析错误响应
        private class ErrorResponse
        {
            public string Message { get; set; } = "";
        }

        public async Task<AuthResponse> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var httpResponse = await _httpClient.RawPostAsync("auth/login", loginDto);
                var content = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    // 尝试解析后端返回的JSON格式错误信息
                    string errorMessage = "登录失败";
                    
                    try
                    {
                        // 使用专门的错误响应类进行解析
                        var errorResponse = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(content, new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                        {
                            errorMessage = errorResponse.Message;
                        }
                        else
                        {
                            // 如果解析失败，尝试使用字符串查找提取message
                            int messageStartIndex = content.IndexOf("\"message\":\"") + 11;
                            if (messageStartIndex > 10)
                            {
                                int messageEndIndex = content.IndexOf('"', messageStartIndex);
                                if (messageEndIndex > messageStartIndex)
                                {
                                    errorMessage = content.Substring(messageStartIndex, messageEndIndex - messageStartIndex);
                                }
                            }
                        }
                    }
                    catch { }
                    
                    throw new Exception(errorMessage);
                }

                // 成功：正常反序列化，启用大小写不敏感选项
                var response = System.Text.Json.JsonSerializer.Deserialize<AuthResponse>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (response == null)
                    throw new Exception("登录成功但解析响应失败");

                _httpClient.SetToken(response.Token);
                _currentUser = new User
                {
                    Username = response.Username,
                    Role = response.Role
                };
                return response;
            }
            catch (Exception ex)
            {
                // 直接抛出原始错误，不再包装
                throw;
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var httpResponse = await _httpClient.RawPostAsync("auth/register", registerDto);

                var content = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    // 尝试解析后端返回的错误文本
                    var message = string.IsNullOrWhiteSpace(content)
                        ? $"注册失败（状态码: {httpResponse.StatusCode}）"
                        : content;

                    throw new Exception(message);
                }

                // 成功：正常反序列化
                var response = System.Text.Json.JsonSerializer.Deserialize<AuthResponse>(content);
                if (response == null)
                    throw new Exception("注册成功但解析响应失败");

                _httpClient.SetToken(response.Token);
                _currentUser = new User
                {
                    Username = response.Username,
                    Role = response.Role
                };

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"注册失败：{ex.Message}", ex);
            }
        }


        public void Logout()
        {
            _httpClient.SetToken(null);
            _currentUser = null;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await _httpClient.GetAsync<object>("auth/test");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}