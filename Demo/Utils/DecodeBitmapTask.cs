// Original Java Code by Ramotion
// Ported to C# by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using AndroidX.Annotations;
using System;
using R = Demo.Resource;

namespace Demo.Utils
{
    public sealed class DecodeBitmapTask : AsyncTask<object, object, Bitmap>
    {
        private readonly BackgroundBitmapCache _cache;
        private readonly Resources _resources;
        private readonly int _bitmapResId, _reqWidth, _reqHeight;

        private readonly WeakReference<IListener> _refListener;

        public interface IListener
        {
            void OnPostExecuted(Bitmap bitmap);
        }

        public sealed class Listener : IListener
        {
            private readonly Action<Bitmap> _onPostExecuted;

            public Listener(Action<Bitmap> onPostExecuted)
            {
                _onPostExecuted = onPostExecuted;
            }

            public void OnPostExecuted(Bitmap bitmap)
            {
                _onPostExecuted?.Invoke(bitmap);
            }
        }

        public DecodeBitmapTask(Resources resources, [DrawableRes] int bitmapResId,
            int reqWidth, int reqHeight,
            [NonNull] IListener listener)
        {
            _cache = BackgroundBitmapCache.GetInstance();
            _resources = resources;
            _bitmapResId = bitmapResId;
            _reqWidth = reqWidth;
            _reqHeight = reqHeight;
            _refListener = new WeakReference<IListener>(listener);
        }

        protected override Bitmap RunInBackground(params object[] @params)
        {
            var cachedBitmap = _cache.GetBitmapFromBgMemCache(_bitmapResId);
            if (cachedBitmap != null)
                return cachedBitmap;

            var options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeResource(_resources, _bitmapResId, options);

            var width = options.OutWidth;
            var height = options.OutHeight;

            var inSampleSize = 1;
            if (height > _reqHeight || width > _reqWidth)
            {
                var halfWidth = width / 2;
                var halfHeight = height / 2;

                while (halfHeight / inSampleSize >= _reqHeight && halfWidth / inSampleSize >= _reqWidth
                                                               && !IsCancelled)
                {
                    inSampleSize *= 2;
                }
            }

            if (IsCancelled)
                return null;

            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            options.InPreferredConfig = Bitmap.Config.Argb8888;

            var decodedBitmap = BitmapFactory.DecodeResource(_resources, _bitmapResId, options);

            Bitmap result;
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                result = GetRoundedCornerBitmap(
                    decodedBitmap, _resources.GetDimension(R.Dimension.card_corner_radius), _reqWidth, _reqHeight
                );
                decodedBitmap.Recycle();
            }
            else
            {
                result = decodedBitmap;
            }

            _cache.AddBitmapToBgMemoryCache(_bitmapResId, result);
            return result;
        }

        protected override void OnPostExecute(Bitmap result)
        {
            _refListener.TryGetTarget(out var listener);

            listener?.OnPostExecuted(result);
        }

        public static Bitmap GetRoundedCornerBitmap(Bitmap bitmap, float pixels, int width, int height)
        {
            var output = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
            var canvas = new Canvas(output);
            var sourceWidth = bitmap.Width;
            var sourceHeight = bitmap.Height;

            var xScale = (float)width / bitmap.Width;
            var yScale = (float)height / bitmap.Height;
            var scale = Math.Max(xScale, yScale);

            var scaledWidth = scale * sourceWidth;
            var scaledHeight = scale * sourceHeight;

            var left = (width - scaledWidth) / 2;
            var top = (height - scaledHeight) / 2;

            const int color = unchecked((int)0xff424242);

            var rect = new Rect(0, 0, width, height);
            var rectF = new RectF(rect);

            var targetRect = new RectF(left, top, left + scaledWidth, top + scaledHeight);
            var paint = new Paint
            {
                AntiAlias = true,
                Color = new Color(color)
            };

            canvas.DrawARGB(0, 0, 0, 0);
            canvas.DrawRoundRect(rectF, pixels, pixels, paint);

            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
            canvas.DrawBitmap(bitmap, null, targetRect, paint);

            return output;
        }
    }
}