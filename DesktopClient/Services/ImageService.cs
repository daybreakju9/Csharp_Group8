using ImageAnnotationApp.Models;

namespace ImageAnnotationApp.Services
{
    public class ImageService
    {
        private readonly HttpClientService _httpClient;

        public ImageService()
        {
            _httpClient = HttpClientService.Instance;
        }

        /// <summary>
        /// 获取队列的图片（分页）
        /// </summary>
        public async Task<PagedResult<Models.Image>> GetQueueImagesPagedAsync(
            int queueId,
            int pageNumber = 1,
            int pageSize = 50,
            string? searchTerm = null,
            int? groupId = null)
        {
            try
            {
                var url = $"images/queue/{queueId}?pageNumber={pageNumber}&pageSize={pageSize}";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }

                if (groupId.HasValue)
                {
                    url += $"&groupId={groupId}";
                }

                var result = await _httpClient.GetAsync<PagedResult<Models.Image>>(url);
                return result ?? new PagedResult<Models.Image>();
            }
            catch (Exception ex)
            {
                throw new Exception($"获取队列图片失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取队列的所有图片（兼容旧代码）
        /// </summary>
        public async Task<List<Models.Image>> GetQueueImagesAsync(int queueId)
        {
            try
            {
                // 使用大的 pageSize 获取所有图片
                var result = await GetQueueImagesPagedAsync(queueId, 1, 10000);
                return result.Items;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取队列图片失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取图片组（分页）
        /// </summary>
        public async Task<PagedResult<ImageGroup>> GetImageGroupsPagedAsync(
            int queueId,
            int pageNumber = 1,
            int pageSize = 50,
            string? searchTerm = null)
        {
            try
            {
                var url = $"images/groups/{queueId}?pageNumber={pageNumber}&pageSize={pageSize}";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }

                var result = await _httpClient.GetAsync<PagedResult<ImageGroup>>(url);
                return result ?? new PagedResult<ImageGroup>();
            }
            catch (Exception ex)
            {
                throw new Exception($"获取图片组失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取下一个未标注的图片组
        /// </summary>
        public async Task<ImageGroup?> GetNextGroupAsync(int queueId)
        {
            try
            {
                var response = await _httpClient.GetRawResponseAsync($"images/next-group/{queueId}");

                if (response == null)
                {
                    return null;
                }

                // 检查是否已完成
                if (response.Contains("\"completed\"") || response.Contains("已完成"))
                {
                    return null;
                }

                // 反序列化为 ImageGroup
                var result = System.Text.Json.JsonSerializer.Deserialize<ImageGroup>(response, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null || result.Images == null || result.Images.Count == 0)
                {
                    return null;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取下一组图片失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除单个图片
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                return await _httpClient.DeleteAsync($"images/{id}");
            }
            catch (Exception ex)
            {
                throw new Exception($"删除图片失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 批量删除图片
        /// </summary>
        public async Task<int> DeleteBatchAsync(List<int> ids)
        {
            try
            {
                var response = await _httpClient.PostAsync<Dictionary<string, object>>(
                    "images/delete-batch",
                    new Dictionary<string, object> { ["ids"] = ids }
                );

                if (response != null && response.ContainsKey("deletedCount"))
                {
                    return Convert.ToInt32(response["deletedCount"]);
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"批量删除图片失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取图片数据
        /// </summary>
        public async Task<byte[]> GetImageDataAsync(string imagePath)
        {
            try
            {
                return await _httpClient.DownloadAsync($"images/file?path={Uri.EscapeDataString(imagePath)}");
            }
            catch (Exception ex)
            {
                throw new Exception($"获取图片数据失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 上传单个图片
        /// </summary>
        public async Task UploadImageAsync(
            int queueId,
            string folderName,
            string fileName,
            byte[] fileData,
            IProgress<double>? progress = null)
        {
            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(queueId.ToString()), "queueId");
                content.Add(new StringContent(folderName), "folderName");
                content.Add(new ByteArrayContent(fileData), "file", fileName);

                await _httpClient.PostMultipartAsync<object>("images/upload", content);
                progress?.Report(100);
            }
            catch (Exception ex)
            {
                throw new Exception($"上传图片失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 并行上传多个图片
        /// </summary>
        public async Task<UploadResult> UploadImagesParallelAsync(
            int queueId,
            Dictionary<string, List<(string fileName, byte[] data)>> folderFiles,
            int maxConcurrency = 5,
            IProgress<ParallelUploadProgress>? progress = null)
        {
            var result = new UploadResult();
            var totalFiles = folderFiles.Sum(f => f.Value.Count);
            var completedFiles = 0;
            int successCount = 0;
            int failureCount = 0;
            var errors = new List<string>();
            var semaphore = new SemaphoreSlim(maxConcurrency);

            var tasks = folderFiles.SelectMany(folder =>
                folder.Value.Select(async file =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        await UploadImageAsync(queueId, folder.Key, file.fileName, file.data);
                        Interlocked.Increment(ref successCount);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref failureCount);
                        lock (errors)
                        {
                            errors.Add($"{folder.Key}/{file.fileName}: {ex.Message}");
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                        Interlocked.Increment(ref completedFiles);

                        progress?.Report(new ParallelUploadProgress
                        {
                            TotalFiles = totalFiles,
                            CompletedFiles = completedFiles,
                            SuccessCount = successCount,
                            FailureCount = failureCount,
                            CurrentFile = $"{folder.Key}/{file.fileName}",
                            Percentage = (double)completedFiles / totalFiles * 100
                        });
                    }
                })
            );

            await Task.WhenAll(tasks);

            result.SuccessCount = successCount;
            result.FailureCount = failureCount;
            result.Errors = errors;
            return result;
        }
    }

    public class UploadResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ParallelUploadProgress
    {
        public int TotalFiles { get; set; }
        public int CompletedFiles { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public string CurrentFile { get; set; } = string.Empty;
        public double Percentage { get; set; }
    }
}
