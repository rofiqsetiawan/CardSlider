// Created by Rofiq Setiawan (rofiqsetiawan@gmail.com)

using System;
using Android.Transitions;

namespace Demo.Implementors
{
    internal sealed class MyTransitionListener : Java.Lang.Object, Transition.ITransitionListener
    {
        private readonly Action<Transition> _onTransitionStart;
        private readonly Action<Transition> _onTransitionCancel;
        private readonly Action<Transition> _onTransitionEnd;
        private readonly Action<Transition> _onTransitionPause;
        private readonly Action<Transition> _onTransitionResume;

        public MyTransitionListener(
            Action<Transition> onTransitionStart,
            Action<Transition> onTransitionCancel,
            Action<Transition> onTransitionEnd,
            Action<Transition> onTransitionPause,
            Action<Transition> onTransitionResume
        )
        {
            _onTransitionStart = onTransitionStart;
            _onTransitionCancel = onTransitionCancel;
            _onTransitionEnd = onTransitionEnd;
            _onTransitionPause = onTransitionPause;
            _onTransitionResume = onTransitionResume;
        }

        public void OnTransitionStart(Transition transition)
        {
            _onTransitionStart?.Invoke(transition);
        }

        public void OnTransitionCancel(Transition transition)
        {
            _onTransitionCancel?.Invoke(transition);
        }

        public void OnTransitionEnd(Transition transition)
        {
            _onTransitionEnd?.Invoke(transition);
        }

        public void OnTransitionPause(Transition transition)
        {
            _onTransitionPause?.Invoke(transition);
        }

        public void OnTransitionResume(Transition transition)
        {
            _onTransitionResume?.Invoke(transition);
        }
    }
}