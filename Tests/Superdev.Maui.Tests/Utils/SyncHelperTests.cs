using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Superdev.Maui.Utils.Threading;
using Xunit.Abstractions;

namespace Superdev.Maui.Tests.Utils
{
    public class SyncHelperTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public SyncHelperTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ShouldRunOnce_IsRunningFalse()
        {
            // Arrange
            var syncHelper = new SyncHelper();

            // Act
            var isRunning = syncHelper.IsRunning;

            // Assert
            isRunning.Should().BeFalse();
        }

        [Fact]
        public void ShouldRunOnce_IsRunningTrue()
        {
            // Arrange
            var isRunning = false;
            var syncHelper = new SyncHelper();

            // Act
            syncHelper.RunOnce(() =>
            {
                isRunning = syncHelper.IsRunning;
            });

            // Assert
            isRunning.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldRunOnce_WithoutReturnValue()
        {
            // Arrange
            var counter = 0;
            var parallelTasks = 64;
            var syncHelper = new SyncHelper();
            using var ready = new CountdownEvent(parallelTasks);
            using var start = new ManualResetEventSlim(false);

            // Act
            var tasks = Enumerable.Range(1, parallelTasks)
                .Select(id => Task.Run(() =>
                {
                    ready.Signal();
                    start.Wait();

                    syncHelper.RunOnce(() =>
                    {
                        Thread.Sleep(100);
                        var value = Interlocked.Increment(ref counter);
                        this.testOutputHelper.WriteLine($"Run #{id}: \t\tcounter={value}");
                    });
                }))
                .ToList();
            ready.Wait();
            start.Set();
            await Task.WhenAll(tasks);

            // Assert
            counter.Should().Be(1);
        }

        [Fact]
        public async Task ShouldRunOnce_WithReturnValue()
        {
            // Arrange
            var counter = 0;
            var parallelTasks = 64;
            var syncHelper = new SyncHelper();
            using var ready = new CountdownEvent(parallelTasks);
            using var start = new ManualResetEventSlim(false);

            // Act
            var tasks = Enumerable.Range(1, parallelTasks)
                .Select(id => Task.Run(() =>
                {
                    ready.Signal();
                    start.Wait();

                    return syncHelper.RunOnce(() =>
                    {
                        Thread.Sleep(100);
                        var value = Interlocked.Increment(ref counter);
                        this.testOutputHelper.WriteLine($"Run #{id}: \t\tcounter={value}");
                        return value;
                    });
                }))
                .ToList();
            ready.Wait();
            start.Set();
            var results = await Task.WhenAll(tasks);

            // Assert
            counter.Should().Be(1);
            results.Should().HaveCount(parallelTasks);
            results.Should().AllSatisfy(i => i.Should().Be(1));
        }

        [Fact]
        public async Task ShouldRunOnce_WithAnotherResultType_RunsAfterTheCurrentExecution()
        {
            // Arrange
            var syncHelper = new SyncHelper();
            var firstStarted = new ManualResetEventSlim();
            var releaseFirst = new ManualResetEventSlim();

            var first = Task.Run(() => syncHelper.RunOnce(() =>
            {
                firstStarted.Set();
                releaseFirst.Wait();
                return 42;
            }));
            firstStarted.Wait();

            // Act - the second execution has another result type, so it cannot take over the result of the first one.
            var second = Task.Run(() => syncHelper.RunOnce(() => "text"));
            await Task.Delay(100);
            releaseFirst.Set();

            // Assert
            (await first).Should().Be(42);
            (await second).Should().Be("text", "it ran on its own once the first execution finished, instead of failing with an invalid cast");
        }

        [Fact]
        public async Task ShouldRunOnceAsync_WithoutReturnValue()
        {
            // Arrange
            var counter = 0;
            var parallelTasks = 64;
            var syncHelper = new SyncHelper();
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var ready = 0;

            // Act
            var tasks = Enumerable.Range(1, parallelTasks)
                .Select(id => Task.Run(async () =>
                {
                    Interlocked.Increment(ref ready);
                    await start.Task;

                    await syncHelper.RunOnceAsync(async () =>
                    {
                        await Task.Delay(100);
                        var value = Interlocked.Increment(ref counter);
                        this.testOutputHelper.WriteLine($"Run #{id}: \t\tcounter={value}");
                    });
                }))
                .ToList();
            SpinWait.SpinUntil(() => Volatile.Read(ref ready) == parallelTasks, TimeSpan.FromSeconds(5)).Should().BeTrue();
            start.SetResult();
            await Task.WhenAll(tasks);

            // Assert
            counter.Should().Be(1);
        }

        [Fact]
        public async Task ShouldRunOnceAsync_WithReturnValue()
        {
            // Arrange
            var counter = 0;
            var parallelTasks = 64;
            var syncHelper = new SyncHelper();
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var ready = 0;

            // Act
            var tasks = Enumerable.Range(1, parallelTasks)
                .Select(id => Task.Run(async () =>
                {
                    Interlocked.Increment(ref ready);
                    await start.Task;

                    return await syncHelper.RunOnceAsync(async () =>
                    {
                        await Task.Delay(100);
                        var value = Interlocked.Increment(ref counter);
                        this.testOutputHelper.WriteLine($"Run #{id}: \t\tcounter={value}");
                        return value;
                    });
                }))
                .ToList();
            SpinWait.SpinUntil(() => Volatile.Read(ref ready) == parallelTasks, TimeSpan.FromSeconds(5)).Should().BeTrue();
            start.SetResult();
            var results = await Task.WhenAll(tasks);

            // Assert
            counter.Should().Be(1);
            results.Should().HaveCount(parallelTasks);
            results.Should().AllSatisfy(i => i.Should().Be(1));
        }

        [Fact]
        public async Task RunOnce_CalledFromItsOwnFunction_Throws()
        {
            // Arrange
            var syncHelper = new SyncHelper();

            // Act
            Func<Task> act = () => Task.Run(() => syncHelper.RunOnce(() => syncHelper.RunOnce(() => 1)))
                .WaitAsync(TimeSpan.FromSeconds(5));

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>("the inner call would wait for the execution it runs in");
        }

        [Fact]
        public async Task RunOnce_CalledFromRunningAction_Throws()
        {
            // Arrange
            var syncHelper = new SyncHelper();
            Exception? caught = null;

            // Act
            await Task.Run(() => syncHelper.RunOnce(() =>
                {
                    caught = Record.Exception(() => syncHelper.RunOnce(() => 1));
                }))
                .WaitAsync(TimeSpan.FromSeconds(5));

            // Assert
            caught.Should().BeOfType<InvalidOperationException>();
        }

        [Fact]
        public async Task RunOnceAsync_CalledFromItsOwnTask_Throws()
        {
            // Arrange
            var syncHelper = new SyncHelper();

            // Act
            Func<Task> act = () => syncHelper.RunOnceAsync(async () =>
                {
                    await Task.Yield();
                    return await syncHelper.RunOnceAsync(() => Task.FromResult(1));
                })
                .WaitAsync(TimeSpan.FromSeconds(5));

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>("the inner call would wait for the execution it runs in");
        }

        [Fact]
        public async Task RunOnce_JoiningAsyncExecutionOnThreadWithSynchronizationContext_Throws()
        {
            // Arrange
            var syncHelper = new SyncHelper();
            var release = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var running = syncHelper.RunOnceAsync(() => release.Task);
            Exception? caught = null;
            var uiThread = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                caught = Record.Exception(() => syncHelper.RunOnce(() => 2));
            });

            // Act
            uiThread.Start();
            var finished = uiThread.Join(TimeSpan.FromSeconds(5));
            release.SetResult(1);
            await running;

            // Assert
            finished.Should().BeTrue("the call must not block the thread");
            caught.Should().BeOfType<InvalidOperationException>();
        }

        [Fact]
        public async Task RunOnce_JoiningAsyncExecutionOnThreadPool_ReturnsItsResult()
        {
            // Arrange
            var syncHelper = new SyncHelper();
            var release = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var running = syncHelper.RunOnceAsync(() => release.Task);
            using var joining = new ManualResetEventSlim();

            // Act
            var joined = Task.Run(() =>
            {
                joining.Set();
                return syncHelper.RunOnce(() => 2);
            });
            joining.Wait(TimeSpan.FromSeconds(5));
            await Task.Delay(100);
            release.SetResult(1);

            // Assert
            (await joined.WaitAsync(TimeSpan.FromSeconds(5))).Should().Be(1, "a thread without a synchronization context may wait for the result");
            (await running).Should().Be(1);
        }

        [Fact]
        public async Task RunOnce_FromWorkStartedInsideAnEndedExecution_JoinsTheRunningOne()
        {
            // Arrange
            var syncHelper = new SyncHelper();
            var callLater = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            Task<int>? laterCall = null;
            syncHelper.RunOnce(() =>
            {
                laterCall = Task.Run(async () =>
                {
                    await callLater.Task;
                    return syncHelper.RunOnce(() => 7);
                });
                return 0;
            });

            using var secondStarted = new ManualResetEventSlim();
            using var releaseSecond = new ManualResetEventSlim();
            var second = Task.Run(() => syncHelper.RunOnce(() =>
            {
                secondStarted.Set();
                releaseSecond.Wait();
                return 5;
            }));
            secondStarted.Wait(TimeSpan.FromSeconds(5));

            // Act
            callLater.SetResult();
            await Task.Delay(100);
            releaseSecond.Set();

            // Assert
            (await laterCall!.WaitAsync(TimeSpan.FromSeconds(5))).Should().Be(5, "the execution it was started from has ended, so it joins the running one");
            (await second).Should().Be(5);
        }
    }
}