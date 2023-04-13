using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using Unity.Collections;
using UnityEngine;

namespace Utilities.Unity
{
    public class CoroutineManager : MonoBehaviour
    {
        private static CoroutineManager _singleton;
        public List<CoroutineController> controllers = new List<CoroutineController>();

        public static CoroutineManager Singleton
        {
            get
            {
                if (!Application.isPlaying) throw new ApplicationException("Cannot run coroutine in edit mode");

                if (!_singleton)
                {
                    var go = new GameObject("CoroutineManager");
                    _singleton = go.AddComponent<CoroutineManager>();
                }

                return _singleton;
            }
        }
    }

    public class CoroutineController : IObservable<CoroutineController.CoroutineStatus>
    {
        public enum CoroutineStatus
        {
            Running,
            Paused,
            Cancelled,
            Completed,
            Exception
        }

        private readonly Coroutine _coroutine;
        private readonly IEnumerator _enumerator;
        private readonly Subject<CoroutineStatus> _subject = new Subject<CoroutineStatus>();
        public string name;

        [ReadOnly] public CoroutineStatus status;

        public CoroutineController(IEnumerator enumerator)
        {
            this._enumerator = enumerator;
            CoroutineManager.Singleton.controllers.Add(this);
            this._coroutine = CoroutineManager.Singleton.StartCoroutine(this.CoroutineWrapper(this._enumerator));
            this.status = CoroutineStatus.Running;
        }

        public bool IsActive => this.IsRunning || this.IsPaused;
        public bool IsRunning => this.status == CoroutineStatus.Running;
        public bool IsPaused => this.status == CoroutineStatus.Paused;
        public bool WasCancelled => this.status == CoroutineStatus.Cancelled;
        public bool IsCompleted => this.status == CoroutineStatus.Completed;

        public IDisposable Subscribe(IObserver<CoroutineStatus> observer)
        {
            return this._subject.Subscribe(observer);
        }

        public void Pause()
        {
            if (this.status == CoroutineStatus.Running)
            {
                this.status = CoroutineStatus.Paused;
                this._subject.OnNext(this.status);
            }
        }

        public void Resume()
        {
            if (this.status == CoroutineStatus.Paused)
            {
                this.status = CoroutineStatus.Running;
                this._subject.OnNext(this.status);
            }
            else
            {
                Debug.LogWarning($"Trying to resume a stopped coroutine {this.name} {this.status}");
            }
        }

        public void Cancel()
        {
            switch (this.status)
            {
                case CoroutineStatus.Paused:
                case CoroutineStatus.Running:
                    this.StopInternalCoroutine(false);
                    break;
            }
        }

        protected void StopInternalCoroutine(bool completedSuccessfully)
        {
            if (this.status == CoroutineStatus.Cancelled || this.status == CoroutineStatus.Cancelled) return;

            CoroutineManager.Singleton.StopCoroutine(this._coroutine);
            CoroutineManager.Singleton.controllers.Remove(this);

            this.status = completedSuccessfully ? CoroutineStatus.Completed : CoroutineStatus.Cancelled;
            this._subject.OnNext(this.status);
            this._subject.OnCompleted();
        }

        protected IEnumerator CoroutineWrapper(IEnumerator iterateCoroutine)
        {
            while (true)
                switch (this.status)
                {
                    case CoroutineStatus.Exception:
                    case CoroutineStatus.Cancelled:
                    case CoroutineStatus.Completed:
                        Debug.LogWarning($"Still executing coroutine {this.name} {this.status}");
                        yield break;
                    case CoroutineStatus.Paused:
                        yield return null;
                        break;
                    case CoroutineStatus.Running:
                    {
                        object iterator = null;

                        try
                        {
                            if (!this._enumerator.MoveNext())
                            {
                                this.StopInternalCoroutine(true);
                                yield break;
                            }

                            iterator = this._enumerator.Current;
                        }
                        catch (Exception e)
                        {
                            this.status = CoroutineStatus.Exception;
                            Debug.LogException(e);
                            yield break;
                        }

                        yield return iterator;
                        break;
                    }
                }
        }
    }
}