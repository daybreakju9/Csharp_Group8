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

        public async Task<AuthResponse> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var response = await _httpClient.PostAsync<AuthResponse>("auth/login", loginDto);
                if (response != null)
                {
                    _httpClient.SetToken(response.Token);
                    _currentUser = new User
                    {
                        Username = response.Username,
                        Role = response.Role
                    };
                    return response;
                }
                throw new Exception("登录失败");
            }
            catch (Exception ex)
            {
                throw new Exception($"登录失败: {ex.Message}", ex);
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
