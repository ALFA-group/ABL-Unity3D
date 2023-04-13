using System;
using System.Threading;
using UnityEngine;

#nullable enable

namespace GP.Experiments
{
    public class GpExperimentParent : MonoBehaviour
    {
        private CancellationTokenSource? _cancelToken;

        [ContextMenu("Run Child Experiments")]
        public void RunChildExperiments()
        {
            this._cancelToken = new CancellationTokenSource();
            var runner = this.GetComponentInParent<GpExperimentRunner>() ??
                         throw new Exception("No Gp Experiment Runner found in parents");
            runner.RunAllExperiments(this.transform, this._cancelToken).Forget();
        }

        [ContextMenu("Cancel Child Experiments")]
        public void CancelChildExperiments()
        {
            this._cancelToken?.Cancel();
        }
    }
}