// Original Java Code by Ramotion
// Ported to C# by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using Android.Graphics;
using Android.Util;
using System;
using Number = Java.Lang.Number;
using Object = Java.Lang.Object;

namespace Demo.Utils
{
    /// <summary>
    /// <see cref="LruCache"/> for caching background bitmaps for <see cref="DecodeBitmapTask"/>.
    /// </summary>
    public class BackgroundBitmapCache
    {
        private LruCache _backgroundsCache; // LruCache<int, Bitmap>

        private static BackgroundBitmapCache _instance;

        public static BackgroundBitmapCache GetInstance()
        {
            if (_instance != null) return _instance;

            _instance = new BackgroundBitmapCache();
            _instance.Init();
            return _instance;
        }

        private void Init()
        {
            var maxMemory = (int)(Java.Lang.Runtime.GetRuntime().MaxMemory() / 1024);
            var cacheSize = maxMemory / 5;

            _backgroundsCache = new MyLruCache<Bitmap>(
                cacheSize,
                // The cache size will be measured in kilobytes rather than number of items.
                (key, bitmap) => bitmap.ByteCount / 1024
            );
        }

        public void AddBitmapToBgMemoryCache(int key, Bitmap bitmap)
        {
            if (GetBitmapFromBgMemCache(key) == null)
                _backgroundsCache.Put(key, bitmap);
        }

        public Bitmap GetBitmapFromBgMemCache(int key)
            => (Bitmap)_backgroundsCache.Get(key);

        private sealed class MyLruCache<T> : LruCache where T : Object
        {
            private readonly Func<int, T, int> _sizeOf;

            public MyLruCache(int maxSize, Func<int, T, int> sizeOf) : base(maxSize)
            {
                _sizeOf = sizeOf;
            }

            // You can cast `key` to int directly
            protected override int SizeOf(Object key, Object value)
                => _sizeOf((key as Number)?.IntValue() ?? 0, (T)value);
        }
    }
}