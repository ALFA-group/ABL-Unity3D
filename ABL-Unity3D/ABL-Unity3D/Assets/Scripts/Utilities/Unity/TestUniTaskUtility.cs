using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Debug = UnityEngine.Debug;

#nullable enable

namespace Utilities.Unity
{
    public class TestUniTaskUtility : MonoBehaviour
    {
        [ContextMenu("Test Thread Switching")]
        public void TestThreadSwitching()
        {
            this.CountStuff().ToObservable().ObserveOnMainThread().Subscribe(this.PrintResult).AddTo(this);
        }

        protected void PrintResult(long l)
        {
            LogThreadInfo($"Finished test with total {l}");
        }

        public async UniTask<long> CountStuff()
        {
            LogThreadInfo("STARTING THREAD INFO:");

            await UniTask.SwitchToTaskPool();
            SleepAndSum("TASK POOL");

            await UniTask.SwitchToThreadPool();
            SleepAndSum("THREAD POOL");

            await UniTaskUtility.SwitchToThread(true, "SleepAndSum");
            long sum = SleepAndSum("THREAD");

            await UniTaskUtility.SwitchToLongRunningTaskPool();
            SleepAndSum("LONG RUNNING TASK");

            await UniTask.SwitchToMainThread();
            SleepAndSum("MAIN THREAD");

            return sum;
        }

        private static long SleepAndSum(string title)
        {
            LogThreadInfo($"{title} begin sleep");

            Thread.Sleep(1000);

            LogThreadInfo($"{title} begin sum");

            long sum = 0;
            for (var i = 0; i < 1000 * 1000; ++i) sum += i;

            LogThreadInfo($"{title} done");
            return sum;
        }

        public static void LogThreadInfo(string header)
        {
            var t = Thread.CurrentThread;

            Debug.Log($@"
{header}
Name: {t.Name}
Id: {t.ManagedThreadId}
State: {t.ThreadState} 
Alive:{t.IsAlive}
Priority: {t.Priority} 
IsThreadPoolThread: {t.IsThreadPoolThread}
{GetProcessInfo()}

");
        }

        private static string GetProcessInfo()
        {
            try
            {
                Process[] newProcessList = Process.GetProcesses();
                int ts = newProcessList.Sum(p => p.Threads.Count);
                double s = newProcessList.Sum(p => p.TotalProcessorTime.TotalMilliseconds);

                Thread.BeginThreadAffinity();
                int id = Thread.CurrentThread.ManagedThreadId;

                var currentProcess = Process.GetCurrentProcess();
                currentProcess.Refresh();

                var i = 0;
                foreach (ProcessThread processThread in currentProcess.Threads)
                {
                    ++i;
                    if (processThread.Id == id)
                    {
                        Thread.EndThreadAffinity();
                        return $"{processThread.TotalProcessorTime}";
                    }
                }

                return
                    $"ProcessThread not found in {i} threads.  Found {newProcessList.Count(process => process.Id != 0)} processes and {ts} Threads taking {s} ms";
            }
            finally
            {
                Thread.EndThreadAffinity();
            }
        }
    }
}