// Original Java Code by Ramotion
// Ported to C# by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.Annotations;
using AndroidX.RecyclerView.Widget;
using Demo.Implementors;
using Demo.Utils;
using R = Demo.Resource;

namespace Demo.Cards
{
    public sealed class SliderCard : RecyclerView.ViewHolder, DecodeBitmapTask.IListener
    {
        private static int _viewWidth = 0;
        private static int _viewHeight = 0;

        private readonly ImageView _imageView;

        private DecodeBitmapTask _task;

        public SliderCard(View itemView) : base(itemView)
        {
            _imageView = itemView.FindViewById<ImageView>(R.Id.image);
        }

        public void SetContent([DrawableRes] int resId)
        {
            if (_viewWidth == 0)
            {
                var l = new MyVtoOnGlobalLayoutListener();
                l.GlobalLayoutEvent += (s, e) =>
                {
                    ItemView.ViewTreeObserver.RemoveOnGlobalLayoutListener(l);
                    _viewWidth = ItemView.Width;
                    _viewHeight = ItemView.Height;
                    LoadBitmap(resId);
                };
                ItemView.ViewTreeObserver.AddOnGlobalLayoutListener(l);
            }
            else
            {
                LoadBitmap(resId);
            }
        }

        public void ClearContent()
            => _task?.Cancel(true);

        private void LoadBitmap([DrawableRes] int resId)
        {
            _task = new DecodeBitmapTask(ItemView.Resources, resId, _viewWidth, _viewHeight, this);
            _task.Execute();
        }

        public void OnPostExecuted(Bitmap bitmap)
            => _imageView?.SetImageBitmap(bitmap);
    }
}