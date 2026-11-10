using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Superdev.Maui.Internals;

namespace Superdev.Maui.Tests.Internals
{
    public class WeakSubscriptionTests
    {
        [Fact]
        public void ShouldForwardACollectionChange_ToTheTarget()
        {
            // Arrange
            var collection = new ObservableCollection<string>();
            var target = new Recorder();
            using var subscription = WeakSubscription.ToCollectionChanged(collection, target, static (recorder, e) => recorder.Actions.Add(e.Action));

            // Act
            collection.Add("maui");

            // Assert
            target.Actions.Should().Equal(NotifyCollectionChangedAction.Add);
        }

        [Fact]
        public void ShouldStopForwarding_WhenDisposed()
        {
            // Arrange
            var collection = new ObservableCollection<string>();
            var target = new Recorder();
            var subscription = WeakSubscription.ToCollectionChanged(collection, target, static (recorder, e) => recorder.Actions.Add(e.Action));

            // Act
            subscription.Dispose();
            collection.Add("maui");

            // Assert
            target.Actions.Should().BeEmpty();
        }

        [Fact]
        public void ShouldNotKeepTheTarget_Alive()
        {
            // Arrange - a collection that outlives its subscriber
            var collection = new ObservableCollection<string>();

            // Act
            var target = Subscribe(collection);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Assert
            target.IsAlive.Should().BeFalse("the collection holds the subscriber weakly");
            var act = () => collection.Add("after");
            act.Should().NotThrow("a forwarder whose subscriber is gone removes itself");
        }

        [Fact]
        public void ShouldUnsubscribe_OnceTheTargetIsGone()
        {
            // Arrange
            var source = new CountingSource();
            Subscribe(source);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Act
            source.Raise();

            // Assert
            source.SubscriberCount.Should().Be(0, "the forwarder removes itself on the first change after its target is gone");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference Subscribe(INotifyCollectionChanged collection)
        {
            var target = new Recorder();
            WeakSubscription.ToCollectionChanged(collection, target, static (recorder, e) => recorder.Actions.Add(e.Action));
            return new WeakReference(target);
        }

        /// <summary>A change source which counts its subscribers.</summary>
        private sealed class CountingSource : INotifyCollectionChanged
        {
            private NotifyCollectionChangedEventHandler? handlers;

            public event NotifyCollectionChangedEventHandler? CollectionChanged
            {
                add => this.handlers += value;
                remove => this.handlers -= value;
            }

            public int SubscriberCount => this.handlers?.GetInvocationList().Length ?? 0;

            public void Raise()
            {
                this.handlers?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private sealed class Recorder
        {
            public List<NotifyCollectionChangedAction> Actions { get; } = new List<NotifyCollectionChangedAction>();
        }
    }
}
