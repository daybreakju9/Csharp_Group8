using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace ImageAnnotationApp.Services
{
    /// <summary>
    /// 图片缓存服务，避免重复下载和加载图片
    /// </summary>
    public class ImageCacheService
    {
        private static ImageCacheService? _instance;
        private static readonly object _lock = new object();

        // 使用ConcurrentDictionary存储缓存的图片
        private readonly ConcurrentDictionary<string, Image> _imageCache;
        private readonly ConcurrentDictionary<string, byte[]> _imageDataCache;

        // 缓存大小限制（内存限制）
        private const int MaxCacheSize = 100; // 最多缓存100张图片
        private const long MaxMemorySize = 200 * 1024 * 1024; // 最多200MB

        public static ImageCacheService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ImageCacheService();
                        }
                    }
                }
                return _instance;
            }
        }

        private ImageCacheService()
        {
            _imageCache = new ConcurrentDictionary<string, Image>();
            _imageDataCache = new ConcurrentDictionary<string, byte[]>();
        }

        /// <summary>
        /// 从缓存获取图片，如果不存在则返回null
        /// </summary>
        public Image? GetImage(string key)
        {
            if (_imageCache.TryGetValue(key, out var image))
            {
                // 创建副本以避免GDI+错误
                return CloneImage(image);
            }
            return null;
        }

        /// <summary>
        /// 从缓存获取图片数据，如果不存在则返回null
        /// </summary>
        public byte[]? GetImageData(string key)
        {
            if (_imageDataCache.TryGetValue(key, out var data))
            {
                return data;
            }
            return null;
        }

        /// <summary>
        /// 添加图片到缓存
        /// </summary>
        public void AddImage(string key, Image image)
        {
            // 检查缓存大小
            if (_imageCache.Count >= MaxCacheSize)
            {
                ClearOldestEntries();
            }

            // 克隆图片以避免外部修改
            var clonedImage = CloneImage(image);
            _imageCache.AddOrUpdate(key, clonedImage, (k, old) =>
            {
                old?.Dispose();
                return clonedImage;
            });
        }

        /// <summary>
        /// 添加图片数据到缓存
        /// </summary>
        public void AddImageData(string key, byte[] data)
        {
            // 检查内存使用
            long currentMemory = 0;
            foreach (var item in _imageDataCache.Values)
            {
                currentMemory += item.Length;
            }

            if (currentMemory + data.Length > MaxMemorySize)
            {
                ClearOldestDataEntries();
            }

            _imageDataCache.AddOrUpdate(key, data, (k, old) => data);
        }

        /// <summary>
        /// 克隆图片以避免GDI+���误
        /// </summary>
        private Image CloneImage(Image source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var bitmap = new Bitmap(source.Width, source.Height, source.PixelFormat);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, source.Width, source.Height);
            }
            return bitmap;
        }

        /// <summary>
        /// 清除最旧的缓存条目
        /// </summary>
        private void ClearOldestEntries()
        {
            // 简单策略：清除25%的缓存
            int removeCount = MaxCacheSize / 4;
            int removed = 0;

            foreach (var key in _imageCache.Keys)
            {
                if (removed >= removeCount) break;

                if (_imageCache.TryRemove(key, out var image))
                {
                    image?.Dispose();
                    removed++;
                }
            }
        }

        /// <summary>
        /// 清除最旧的数据缓存条目
        /// </summary>
        private void ClearOldestDataEntries()
        {
            // 清除50%的数据缓存
            int removeCount = _imageDataCache.Count / 2;
            int removed = 0;

            foreach (var key in _imageDataCache.Keys)
            {
                if (removed >= removeCount) break;

                _imageDataCache.TryRemove(key, out _);
                removed++;
            }
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public void ClearAll()
        {
            foreach (var image in _imageCache.Values)
            {
                image?.Dispose();
            }
            _imageCache.Clear();
            _imageDataCache.Clear();
        }

        /// <summary>
        /// 预加载图片到缓存
        /// </summary>
        public async Task PreloadImagesAsync(string[] imagePaths, ImageService imageService)
        {
            var tasks = new List<Task>();

            foreach (var path in imagePaths)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        if (!_imageDataCache.ContainsKey(path))
                        {
                            var data = await imageService.GetImageDataAsync(path);
                            if (data != null && data.Length > 0)
                            {
                                AddImageData(path, data);
                            }
                        }
                    }
                    catch
                    {
                        // 忽略预加载错误
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }
    }
}
