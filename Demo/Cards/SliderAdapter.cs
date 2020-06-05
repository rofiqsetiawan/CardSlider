// Original Java Code by Ramotion
// Ported to C# by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using Android.Views;
using AndroidX.RecyclerView.Widget;
using Java.Lang;
using R = Demo.Resource;

namespace Demo.Cards
{
    public sealed class SliderAdapter : RecyclerView.Adapter
    {
        private readonly int _count;
        private readonly int[] _content;
        private readonly View.IOnClickListener _listener;

        public SliderAdapter(int[] content, int count, View.IOnClickListener listener)
        {
            _content = content;
            _count = count;
            _listener = listener;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var v = LayoutInflater.From(parent.Context)
                .Inflate(R.Layout.layout_slider_card, parent, false);

            if (_listener != null)
                v.Click += (s, e) => _listener?.OnClick((View) s);

            return new SliderCard(v);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            => (holder as SliderCard)?.SetContent(_content[position % _content.Length]);

        public override void OnViewRecycled(Object holder)
            => (holder as SliderCard)?.ClearContent();

        public override int ItemCount
            => _count;
    }
}