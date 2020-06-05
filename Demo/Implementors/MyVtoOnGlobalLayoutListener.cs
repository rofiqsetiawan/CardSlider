// Created by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using System;
using Android.Views;

namespace Demo.Implementors
{
    internal sealed class MyVtoOnGlobalLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        private EventHandler<EventArgs> _onGlobalLayout;

        public event EventHandler<EventArgs> GlobalLayoutEvent
        {
            add
            {
                lock (this)
                {
                    _onGlobalLayout += value;
                }
            }
            remove
            {
                lock (this)
                {
                    _onGlobalLayout -= value;
                }
            }
        }

        public void OnGlobalLayout() => _onGlobalLayout?.Invoke(this, EventArgs.Empty);
    }
}