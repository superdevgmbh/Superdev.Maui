using System.Collections.Specialized;
using System.ComponentModel;

namespace Superdev.Maui.Internals
{
    /// <summary>
    /// Event subscriptions that do not keep their subscriber alive. The event source holds a small forwarder with a
    /// weak reference to the subscriber, so that a long-lived source — a view model's form — never keeps a page's
    /// controls in memory. The handler must not capture the subscriber; it receives it as its first argument. A
    /// forwarder whose subscriber was collected removes itself on the next event.
    /// </summary>
    internal static class WeakSubscription
    {
        public static IDisposable ToPropertyChanged<TTarget>(
            INotifyPropertyChanged source,
            TTarget target,
            Action<TTarget, PropertyChangedEventArgs> handler)
            where TTarget : class
        {
            ArgumentNullException.ThrowIfNull(source);

            var forwarder = new Forwarder<TTarget, PropertyChangedEventArgs>(target, handler);
            PropertyChangedEventHandler forward = forwarder.Forward;
            source.PropertyChanged += forward;
            forwarder.Unsubscribe = () => source.PropertyChanged -= forward;
            return forwarder;
        }

        public static IDisposable ToCollectionChanged<TTarget>(
            INotifyCollectionChanged source,
            TTarget target,
            Action<TTarget, NotifyCollectionChangedEventArgs> handler)
            where TTarget : class
        {
            ArgumentNullException.ThrowIfNull(source);

            var forwarder = new Forwarder<TTarget, NotifyCollectionChangedEventArgs>(target, handler);
            NotifyCollectionChangedEventHandler forward = forwarder.Forward;
            source.CollectionChanged += forward;
            forwarder.Unsubscribe = () => source.CollectionChanged -= forward;
            return forwarder;
        }

        public static IDisposable ToEvent<TTarget, TEventArgs>(
            Action<EventHandler<TEventArgs>> subscribe,
            Action<EventHandler<TEventArgs>> unsubscribe,
            TTarget target,
            Action<TTarget, TEventArgs> handler)
            where TTarget : class
        {
            ArgumentNullException.ThrowIfNull(subscribe);
            ArgumentNullException.ThrowIfNull(unsubscribe);

            var forwarder = new Forwarder<TTarget, TEventArgs>(target, handler);
            EventHandler<TEventArgs> forward = forwarder.Forward;
            subscribe(forward);
            forwarder.Unsubscribe = () => unsubscribe(forward);
            return forwarder;
        }

        private sealed class Forwarder<TTarget, TEventArgs> : IDisposable
            where TTarget : class
        {
            private readonly WeakReference<TTarget> target;
            private readonly Action<TTarget, TEventArgs> handler;

            public Forwarder(TTarget target, Action<TTarget, TEventArgs> handler)
            {
                ArgumentNullException.ThrowIfNull(target);
                ArgumentNullException.ThrowIfNull(handler);

                this.target = new WeakReference<TTarget>(target);
                this.handler = handler;
            }

            public Action? Unsubscribe { get; set; }

            public void Forward(object? sender, TEventArgs e)
            {
                if (this.target.TryGetTarget(out var subscriber))
                {
                    this.handler(subscriber, e);
                }
                else
                {
                    this.Dispose();
                }
            }

            public void Dispose()
            {
                var unsubscribe = this.Unsubscribe;
                this.Unsubscribe = null;
                unsubscribe?.Invoke();
            }
        }
    }
}
