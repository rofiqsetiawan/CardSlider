// Original Java Code by Ramotion
// Ported to C# by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Annotations;
using AndroidX.AppCompat.App;
using AndroidX.CardView.Widget;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using Demo.Cards;
using Demo.Implementors;
using Demo.Utils;
using Ramotion.CardSliderLib;
using System;
using Object = Java.Lang.Object;
using R = Demo.Resource;

namespace Demo
{
    [Register("id.karamunting.cardsliderdemo.MainActivity")]
    [Activity(MainLauncher = true, Label = "@string/app_name", Theme = "@style/AppTheme")]
    public sealed class MainActivity : AppCompatActivity
    {
        private const string MyTag = "RamotionCardSlider";

        private readonly int[,] _dotCoords = new int[5, 2];

        private readonly int[] _pics =
        {
            R.Drawable.p1, R.Drawable.p2, R.Drawable.p3, R.Drawable.p4, R.Drawable.p5
        };

        private readonly int[] _maps =
        {
            R.Drawable.map_paris, R.Drawable.map_seoul, R.Drawable.map_london, R.Drawable.map_beijing,
            R.Drawable.map_greece
        };

        private readonly int[] _descriptions =
        {
            R.String.text1, R.String.text2, R.String.text3, R.String.text4, R.String.text5
        };

        private readonly string[] _countries =
        {
            "PARIS", "SEOUL", "LONDON", "BEIJING", "THIRA"
        };

        private readonly string[] _places =
        {
            "The Louvre", "Gwanghwamun", "Tower Bridge", "Temple of Heaven", "Aegeana Sea"
        };

        private readonly string[] _temperatures =
        {
            "21°C", "19°C", "17°C", "23°C", "20°C"
        };

        private readonly string[] _times =
        {
            "Aug 1 - Dec 15    7:00-18:00", "Sep 5 - Nov 10    8:00-16:00", "Mar 8 - May 21    7:00-18:00"
        };

        private SliderAdapter MySliderAdapter => new SliderAdapter(_pics, 20, OnCardClickListener);

        private CardSliderLayoutManager _layoutManger;
        private RecyclerView _recyclerView;
        private ImageSwitcher _mapSwitcher;
        private TextSwitcher _temperatureSwitcher, _placeSwitcher, _clockSwitcher, _descriptionsSwitcher;
        private View _greenDot;

        private TextView _country1TextView, _country2TextView;
        private int _countryOffset1, _countryOffset2;
        private long _countryAnimDuration;
        private int _currentPosition;

        private DecodeBitmapTask _decodeMapBitmapTask;
        private DecodeBitmapTask.IListener _mapLoadListener;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(R.Layout.activity_main);

            InitRecyclerView();
            InitCountryText();
            InitSwitchers();
            InitGreenDot();
        }

        private void InitRecyclerView()
        {
            _recyclerView = FindViewById<RecyclerView>(R.Id.recycler_view);
            _recyclerView.SetAdapter(MySliderAdapter);
            _recyclerView.HasFixedSize = true;
            _recyclerView.AddOnScrollListener(
                new MyRvOnScrollListener(
                    null,
                    (rv, newState) =>
                    {
                        if (newState == RecyclerView.ScrollStateIdle)
                            OnActiveCardChange();
                    }
                )
            );

            _layoutManger = (CardSliderLayoutManager)_recyclerView.GetLayoutManager();

            new CardSnapHelper().AttachToRecyclerView(_recyclerView);
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (IsFinishing)
                _decodeMapBitmapTask?.Cancel(true);
        }

        private void InitSwitchers()
        {
            _temperatureSwitcher = FindViewById<TextSwitcher>(R.Id.ts_temperature);
            _temperatureSwitcher.SetFactory(new TextViewFactory(this, R.Style.TemperatureTextView, true));
            _temperatureSwitcher.SetCurrentText(_temperatures[0]);

            _placeSwitcher = FindViewById<TextSwitcher>(R.Id.ts_place);
            _placeSwitcher.SetFactory(new TextViewFactory(this, R.Style.PlaceTextView, false));
            _placeSwitcher.SetCurrentText(_places[0]);

            _clockSwitcher = FindViewById<TextSwitcher>(R.Id.ts_clock);
            _clockSwitcher.SetFactory(new TextViewFactory(this, R.Style.ClockTextView, false));
            _clockSwitcher.SetCurrentText(_times[0]);

            _descriptionsSwitcher = FindViewById<TextSwitcher>(R.Id.ts_description);
            _descriptionsSwitcher.SetInAnimation(this, Android.Resource.Animation.FadeIn);
            _descriptionsSwitcher.SetOutAnimation(this, Android.Resource.Animation.FadeOut);
            _descriptionsSwitcher.SetFactory(new TextViewFactory(this, R.Style.DescriptionTextView, false));
            _descriptionsSwitcher.SetCurrentText(GetString(_descriptions[0]));

            _mapSwitcher = FindViewById<ImageSwitcher>(R.Id.ts_map);
            _mapSwitcher.SetInAnimation(this, R.Animation.fade_in);
            _mapSwitcher.SetOutAnimation(this, R.Animation.fade_out);
            _mapSwitcher.SetFactory(new ImageViewFactory(this));
            _mapSwitcher.SetImageResource(_maps[0]);

            _mapLoadListener = new DecodeBitmapTask.Listener(
                bmp =>
                {
                    (_mapSwitcher.NextView as ImageView)?.SetImageBitmap(bmp);
                    _mapSwitcher.ShowNext();
                }
            );
        }

        private void InitCountryText()
        {
            _countryAnimDuration = Resources.GetInteger(R.Integer.labels_animation_duration);
            _countryOffset1 = Resources.GetDimensionPixelSize(R.Dimension.left_offset);
            _countryOffset2 = Resources.GetDimensionPixelSize(R.Dimension.card_width);
            _country1TextView = FindViewById<TextView>(R.Id.tv_country_1);
            _country2TextView = FindViewById<TextView>(R.Id.tv_country_2);

            _country1TextView.SetX(_countryOffset1);
            _country2TextView.SetX(_countryOffset2);
            _country1TextView.Text = _countries[0];
            _country2TextView.Alpha = 0f;

            _country1TextView.Typeface = Typeface.CreateFromAsset(Assets, "open-sans-extrabold.ttf");
            _country2TextView.Typeface = Typeface.CreateFromAsset(Assets, "open-sans-extrabold.ttf");
        }

        private void InitGreenDot()
        {
            try
            {
                var l = new MyVtoOnGlobalLayoutListener();
                l.GlobalLayoutEvent += (s, e) =>
                {
                    _mapSwitcher.ViewTreeObserver.RemoveOnGlobalLayoutListener(l);

                    var viewLeft = _mapSwitcher.Left;
                    var viewTop = _mapSwitcher.Top + _mapSwitcher.Height / 3;

                    const int border = 100;
                    var xRange = Math.Max(1, _mapSwitcher.Width - border * 2);
                    var yRange = Math.Max(1, _mapSwitcher.Height / 3 * 2 - border * 2);

                    var rnd = new System.Random();

                    for (int i = 0, cnt = _dotCoords.GetLength(0); i < cnt; i++)
                    {
                        _dotCoords[i, 0] = viewLeft + border + rnd.Next(xRange);
                        _dotCoords[i, 1] = viewTop + border + rnd.Next(yRange);
                    }

                    _greenDot = FindViewById<View>(R.Id.green_dot);
                    _greenDot.SetX(_dotCoords[0, 0]);
                    _greenDot.SetY(_dotCoords[0, 1]);
                };
                _mapSwitcher.ViewTreeObserver.AddOnGlobalLayoutListener(l);
            }
            catch (Exception exc) when (exc is IndexOutOfRangeException)
            {
                LogW(exc.Message);
            }
        }

        private void SetCountryText(string text, bool left2Right)
        {
            TextView invisibleText;
            TextView visibleText;

            if (_country1TextView.Alpha > _country2TextView.Alpha)
            {
                visibleText = _country1TextView;
                invisibleText = _country2TextView;
            }
            else
            {
                visibleText = _country2TextView;
                invisibleText = _country1TextView;
            }

            int vOffset;
            if (left2Right)
            {
                invisibleText.SetX(0);
                vOffset = _countryOffset2;
            }
            else
            {
                invisibleText.SetX(_countryOffset2);
                vOffset = 0;
            }

            invisibleText.Text = text;

            var iAlpha = ObjectAnimator.OfFloat(invisibleText, "alpha", 1f);
            var vAlpha = ObjectAnimator.OfFloat(visibleText, "alpha", 0f);
            var iX = ObjectAnimator.OfFloat(invisibleText, "x", _countryOffset1);
            var vX = ObjectAnimator.OfFloat(visibleText, "x", vOffset);

            var animSet = new AnimatorSet();
            animSet.PlayTogether(iAlpha, vAlpha, iX, vX);
            animSet.SetDuration(_countryAnimDuration);
            animSet.Start();
        }

        private void OnActiveCardChange()
        {
            var pos = _layoutManger.ActiveCardPosition;
            if (pos == RecyclerView.NoPosition || pos == _currentPosition)
                return;

            OnActiveCardChange(pos);
        }

        private void OnActiveCardChange(int pos)
        {
            var animH = new[] { R.Animation.slide_in_right, R.Animation.slide_out_left };
            var animV = new[] { R.Animation.slide_in_top, R.Animation.slide_out_bottom };

            var left2Right = pos < _currentPosition;
            if (left2Right)
            {
                animH[0] = R.Animation.slide_in_left;
                animH[1] = R.Animation.slide_out_right;

                animV[0] = R.Animation.slide_in_bottom;
                animV[1] = R.Animation.slide_out_top;
            }

            SetCountryText(_countries[pos % _countries.Length], left2Right);

            _temperatureSwitcher.SetInAnimation(this, animH[0]);
            _temperatureSwitcher.SetOutAnimation(this, animH[1]);
            _temperatureSwitcher.SetText(_temperatures[pos % _temperatures.Length]);

            _placeSwitcher.SetInAnimation(this, animV[0]);
            _placeSwitcher.SetOutAnimation(this, animV[1]);
            _placeSwitcher.SetText(_places[pos % _places.Length]);

            _clockSwitcher.SetInAnimation(this, animV[0]);
            _clockSwitcher.SetOutAnimation(this, animV[1]);
            _clockSwitcher.SetText(_times[pos % _times.Length]);

            _descriptionsSwitcher.SetText(GetString(_descriptions[pos % _descriptions.Length]));

            ShowMap(_maps[pos % _maps.Length]);

            ViewCompat.Animate(_greenDot)
                .TranslationX(_dotCoords[pos % _dotCoords.GetLength(0), 0])
                .TranslationY(_dotCoords[pos % _dotCoords.GetLength(0), 1])
                .Start();

            _currentPosition = pos;
        }

        private void ShowMap([DrawableRes] int resId)
        {
            _decodeMapBitmapTask?.Cancel(true);

            var w = _mapSwitcher.Width;
            var h = _mapSwitcher.Height;

            _decodeMapBitmapTask = new DecodeBitmapTask(Resources, resId, w, h, _mapLoadListener);
            _decodeMapBitmapTask.Execute();
        }

        private sealed class TextViewFactory : Object, ViewSwitcher.IViewFactory
        {
            private readonly Context _ctx;
            [StyleRes] private readonly int _styleId;
            private readonly bool _center;

            public TextViewFactory(Context ctx, [StyleRes] int styleId, bool center)
            {
                _ctx = ctx;
                _styleId = styleId;
                _center = center;
            }

            public View MakeView()
            {
                var textView = new TextView(_ctx);

                if (_center)
                    textView.Gravity = GravityFlags.Center;

                if (Build.VERSION.SdkInt < BuildVersionCodes.M)
#pragma warning disable 618
                    textView.SetTextAppearance(_ctx, _styleId);
#pragma warning restore 618
                else
                    textView.SetTextAppearance(_styleId);

                return textView;
            }
        }

        private sealed class ImageViewFactory : Object, ViewSwitcher.IViewFactory
        {
            private readonly Context _ctx;

            public ImageViewFactory(Context ctx)
            {
                _ctx = ctx;
            }

            public View MakeView()
            {
                var imageView = new ImageView(_ctx);
                imageView.SetScaleType(ImageView.ScaleType.CenterCrop);
                imageView.LayoutParameters = new FrameLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.MatchParent
                );
                return imageView;
            }
        }

        private View.IOnClickListener OnCardClickListener => new MyViewOnClickListener(
            v =>
            {
                var lm = (CardSliderLayoutManager)_recyclerView.GetLayoutManager();

                if (lm.IsSmoothScrolling)
                    return;

                var activeCardPosition = lm.ActiveCardPosition;
                if (activeCardPosition == RecyclerView.NoPosition)
                    return;

                var clickedPosition = _recyclerView.GetChildAdapterPosition(v);
                if (clickedPosition == activeCardPosition)
                {
                    var intent = new Intent(this, typeof(DetailsActivity));
                    intent.PutExtra(DetailsActivity.BundleImageId, _pics[activeCardPosition % _pics.Length]);

                    if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                    {
                        StartActivity(intent);
                    }
                    else
                    {
                        var cardView = (CardView)v;
                        var sharedView = cardView.GetChildAt(cardView.ChildCount - 1);
                        var options = ActivityOptions.MakeSceneTransitionAnimation(this, sharedView, "shared");
                        StartActivity(intent, options.ToBundle());
                    }
                }
                else if (clickedPosition > activeCardPosition)
                {
                    _recyclerView.SmoothScrollToPosition(clickedPosition);
                    OnActiveCardChange(clickedPosition);
                }
            }
        );

        private static void LogD(string msg) => Log.Debug(MyTag, msg);

        private static void LogW(string msg) => Log.Warn(MyTag, msg);
    }
}