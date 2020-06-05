// Original Java Code by Ramotion
// Ported to C# by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using Android.Animation;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.CardView.Widget;
using Demo.Implementors;
using Demo.Utils;
using R = Demo.Resource;

namespace Demo
{
    [Register("id.karamunting.cardsliderdemo.DetailsActivity")]
    [Activity(Theme = "@style/AppTheme")]
    public sealed class DetailsActivity : AppCompatActivity, DecodeBitmapTask.IListener
    {
        public const string BundleImageId = "BUNDLE_IMAGE_ID";

        private ImageView _imageView;
        private DecodeBitmapTask _decodeBitmapTask;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(R.Layout.activity_details);

            var smallResId = Intent.GetIntExtra(BundleImageId, -1);
            if (smallResId == -1)
            {
                Finish();
                return;
            }

            _imageView = FindViewById<ImageView>(R.Id.image);
            _imageView.SetImageResource(smallResId);
            _imageView.Click += (s, e) => OnBackPressed();

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                LoadFullSizeBitmap(smallResId);
            }
            else
            {
                var isClosing = false;

                Window.SharedElementEnterTransition.AddListener(
                    new MyTransitionListener(
                        transition =>
                        {
                            if (isClosing)
                                AddCardCorners();
                        },
                        null,
                        transition =>
                        {
                            if (isClosing) return;

                            isClosing = true;

                            RemoveCardCorners();
                            LoadFullSizeBitmap(smallResId);
                        },
                        null,
                        null
                    )
                );
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (IsFinishing)
                _decodeBitmapTask?.Cancel(true);
        }

        private void AddCardCorners()
        {
            FindViewById<CardView>(R.Id.card).Radius = 25f;
        }

        private void RemoveCardCorners()
        {
            var cardView = FindViewById<CardView>(R.Id.card);
            ObjectAnimator.OfFloat(cardView, "radius", 0f)
                .SetDuration(50)
                .Start();
        }

        private void LoadFullSizeBitmap(int smallResId)
        {
            var bigResId = smallResId switch
            {
                R.Drawable.p1 => R.Drawable.p1_big,
                R.Drawable.p2 => R.Drawable.p2_big,
                R.Drawable.p3 => R.Drawable.p3_big,
                R.Drawable.p4 => R.Drawable.p4_big,
                R.Drawable.p5 => R.Drawable.p5_big,
                _ => R.Drawable.p1_big
            };

            var metrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetRealMetrics(metrics);

            var w = metrics.WidthPixels;
            var h = metrics.HeightPixels;

            _decodeBitmapTask = new DecodeBitmapTask(Resources, bigResId, w, h, this);
            _decodeBitmapTask.Execute();
        }

        public void OnPostExecuted(Bitmap bitmap)
            => _imageView.SetImageBitmap(bitmap);
    }
}