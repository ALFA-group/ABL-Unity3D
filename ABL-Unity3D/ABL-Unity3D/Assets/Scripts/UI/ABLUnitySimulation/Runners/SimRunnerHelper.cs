using System;
using System.Linq;
using ABLUnitySimulation;
using Planner;
using UnityEngine;
using UnityEngine.Serialization;

#nullable enable

namespace UI.ABLUnitySimulation.Runners
{
    [Serializable]
    public class SimRunnerHelper
    {
        /// <summary>
        ///     The number of seconds to simulate per real second.
        /// </summary>
        public float unityTimeMultiplier = 1;

        /// <summary>
        ///     The number of seconds to simulate for each simulator step.
        ///     This is used when visualizing a simulation, as opposed to when running the planner.
        /// </summary>
        public int simTimeStep = 1;

        /// <summary>
        ///     Maximum number of seconds allowed for executing a plan. Only effects when executing a plan visually, not during
        ///     planning.
        /// </summary>
        [FormerlySerializedAs("msecMaxRealExecutionTime")] 
        public float maxRealExecutionTimeInSeconds = 200;

        /// <summary>
        ///     Pause the simulator when a plan has finished executing.
        /// </summary>
        public bool pauseOnPlanFinished;

        // It's possible that the realtime timeLimitInSeconds will execute fewer steps than desire.
        // If so, we pretend those unsimulated seconds never existed.   
        private float _unusedRealSeconds;

        /// <summary>
        ///     Update the given simulator state based on the fields in this class.
        /// </summary>
        /// <param name="state">The simulator state to update</param>
        /// <param name="realTimeElapsed">The amount of time passed in the simulator</param>
        /// <param name="caller">The SimRunner object that contains the instance of this class</param>
        public void Update(SimWorldState state, float realTimeElapsed, SimRunner caller)
        {
            if (this.simTimeStep <= 0) return;

            float simTimeElapsed = realTimeElapsed * this.unityTimeMultiplier;
            this._unusedRealSeconds += simTimeElapsed;
            int secondsToSimulate = Mathf.FloorToInt(this._unusedRealSeconds);

            if (secondsToSimulate < 1) return;

            this.UpdateSimInPlace(state, secondsToSimulate, caller);
        }

        /// <summary>
        ///     Update the given simulator world state.
        /// </summary>
        /// <param name="state">The simulator world state to update</param>
        /// <param name="secondsToSimulate">The number of seconds to simulate in the world state</param>
        /// <param name="caller">The SimRunner object that contains this instance of this class</param>
        private void UpdateSimInPlace(SimWorldState state, int secondsToSimulate, SimRunner caller)
        {
            var realStartTime = DateTime.UtcNow;
            float msecTimeout = this.maxRealExecutionTimeInSeconds;

            bool TimedOut(SimWorldState _)
            {
                return (DateTime.UtcNow - realStartTime).TotalMilliseconds > msecTimeout;
            }

            if (secondsToSimulate <= 0) return;

            var plannerSimActions = state.actions.actions
                .Where(ae => ae.subAction is PlannerSimAction)
                .Select(ae => ae.subAction)
                .ToList();
            int numPlansIncompleteBefore = plannerSimActions
                .Count(a =>
                    a.GetStatus(state, false).status != ActionStatus.CompletedSuccessfully);

            state.ExecuteUntil(TimedOut, this.simTimeStep, secondsToSimulate, default);

            this._unusedRealSeconds -=
                secondsToSimulate; // it's possible that the realtime timeLimitInSeconds will execute fewer steps than desired.
            // If so, we pretend those unsimulated seconds never existed.


            if (this.pauseOnPlanFinished)
            {
                int numPlansIncompleteAfter = plannerSimActions
                    .Count(a =>
                        a.GetStatus(state, false).status != ActionStatus.CompletedSuccessfully);

                if (numPlansIncompleteAfter < numPlansIncompleteBefore) caller.status = SimRunner.RunStatus.Paused;
            }
        }
    }
}