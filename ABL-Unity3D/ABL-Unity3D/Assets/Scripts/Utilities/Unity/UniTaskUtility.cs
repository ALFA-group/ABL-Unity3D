using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Profiling;

#nullable enable

namespace Utilities.Unity
{
    public static class UniTaskUtility
    {
        public static SwitchToThreadAwaitable SwitchToThread(string name = "UniTaskUtility Thread")
        {
            return new SwitchToThreadAwaitable
            {
                name = name
            };
        }

        public static SwitchToThreadAwaitable SwitchToThread(bool profileThread, string name = "UniTaskUtility Thread")
        {
            return new SwitchToThreadAwaitable
            {
                name = name,
                profileThread = profileThread
            };
        }


        public static SwitchToLongRunningTaskPoolAwaitable SwitchToLongRunningTaskPool()
        {
            return new SwitchToLongRunningTaskPoolAwaitable();
        }
    }

    public struct SwitchToThreadAwaitable
    {
        public string name;
        public bool profileThread;

        public Awaiter GetAwaiter()
        {
            return new Awaiter { name = this.name, profileThread = this.profileThread };
        }

        public struct Awaiter : ICriticalNotifyCompletion
        {
            public string name;
            public bool profileThread;

            public bool IsCompleted => false;

            public void GetResult()
            {
            }

            public void OnCompleted(Action continuation)
            {
                var self = this;

                void Start()
                {
                    Continue(self, continuation);
                }

                var t = new Thread((ThreadStart)Start)
                {
                    Priority = ThreadPriority.Lowest,
                    Name = this.name,
                    IsBackground = false //true,
                };
                // Thread t = new Thread((ThreadStart) Start)
                // {
                //     Priority = ThreadPriority.Lowest, 
                //     Name = this.name,
                //     IsBackground = true,
                // };
                t.Start();
            }

            private static void Continue(Awaiter self, Action continuation)
            {
                try
                {
                    if (self.profileThread) Profiler.BeginThreadProfiling("UniTaskUtility Threads", self.name);
                    continuation();
                }
                finally
                {
                    if (self.profileThread) Profiler.EndThreadProfiling();
                }
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                this.OnCompleted(continuation);
            }
        }
    }

    public struct SwitchToLongRunningTaskPoolAwaitable
    {
        public Awaiter GetAwaiter()
        {
            return new Awaiter();
        }

        public struct Awaiter : ICriticalNotifyCompletion
        {
            private static readonly Action<object> SwitchToCallback = Callback;

            public bool IsCompleted => false;

            public void GetResult()
            {
            }

            public void OnCompleted(Action continuation)
            {
                Task.Factory.StartNew(
                    SwitchToCallback,
                    continuation,
                    CancellationToken.None,
                    TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                this.OnCompleted(continuation);
            }

            private static void Callback(object state)
            {
                var continuation = (Action)state;
                continuation();
            }
        }
    }
}