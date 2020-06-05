// Created by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using System;
using AndroidX.RecyclerView.Widget;

namespace Demo.Implementors
{
    internal sealed class MyRvOnScrollListener : RecyclerView.OnScrollListener
    {
        private readonly Action<RecyclerView, int, int> _onScrolled;
        private readonly Action<RecyclerView, int> _onScrollStateChanged;

        public MyRvOnScrollListener(Action<RecyclerView, int, int> onScrolled,
            Action<RecyclerView, int> onScrollStateChanged)
        {
            _onScrolled = onScrolled;
            _onScrollStateChanged = onScrollStateChanged;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);

            _onScrolled?.Invoke(recyclerView, dx, dy);
        }

        public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
        {
            base.OnScrollStateChanged(recyclerView, newState);

            _onScrollStateChanged?.Invoke(recyclerView, newState);
        }
    }
}