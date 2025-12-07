using ImageAnnotationApp.Models;

namespace ImageAnnotationApp.Services
{
    public class UserService
    {
        private readonly HttpClientService _httpClient;

        public UserService()
        {
            _httpClient = HttpClientService.Instance;
        }

        public async Task<List<UserDto>> GetGuestUsersAsync()
        {
            try
            {
                var users = await _httpClient.GetAsync<List<UserDto>>("users/guests");
                return users ?? new List<UserDto>();
            }
            catch (Exception ex)
            {
                throw new Exception($"获取待审核游客失败: {ex.Message}", ex);
            }
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _httpClient.GetAsync<List<UserDto>>("users");
                return users ?? new List<UserDto>();
            }
            catch (Exception ex)
            {
                throw new Exception($"获取用户列表失败: {ex.Message}", ex);
            }
        }

        public async Task<bool> ApproveUserAsync(int userId)
        {
            try
            {
                var dto = new ApproveUserDto { UserId = userId };
                await _httpClient.PostAsync<object>("users/approve", dto);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"批准用户失败: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                return await _httpClient.DeleteAsync($"users/{userId}");
            }
            catch (Exception ex)
            {
                throw new Exception($"删除用户失败: {ex.Message}", ex);
            }
        }

        public async Task<UserDto> UpdateUserRoleAsync(int userId, string role)
        {
            try
            {
                var dto = new UpdateUserRoleDto { Role = role };
                var result = await _httpClient.PutAsync<UserDto>($"users/{userId}/role", dto);
                if (result == null)
                {
                    throw new Exception("更新用户角色失败：服务器返回空结果");
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"更新用户角色失败: {ex.Message}", ex);
            }
        }
    }
}
