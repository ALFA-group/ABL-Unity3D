using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace ABLUnitySimulation
{
    [Serializable]
    public class SimPositional : SimObjectBase
    {
        // Kilometers from center of Origin.
        // positive x is east.
        // positive z is north.
        // y is height, not used in this simulation.
        [SerializeField]
        public Vector2 positionActual;

        [JsonIgnore] public ObservedPosition positionObservedByBlue;

        // We do not want to deserialize observed positions because this
        // will cause issues with vision checks. They should be regenerated
        // upon deserialization.
        [JsonIgnore] public ObservedPosition positionObservedByRed;

        public SimPositional(SimWorldState state, string name) : base(state, name)
        {
            this.positionObservedByBlue.SetPosition(Vector2.negativeInfinity, int.MinValue);
            this.positionObservedByRed.SetPosition(Vector2.negativeInfinity, int.MinValue);
        }

        protected SimPositional()
        {
            this.positionObservedByBlue.SetPosition(Vector2.negativeInfinity, int.MinValue);
            this.positionObservedByRed.SetPosition(Vector2.negativeInfinity, int.MinValue);
        }

        protected SimPositional(SimId id, string name) : base(id, name)
        {
            this.positionObservedByBlue.SetPosition(Vector2.negativeInfinity, int.MinValue);
            this.positionObservedByRed.SetPosition(Vector2.negativeInfinity, int.MinValue);
        }


        public struct ObservedPosition
        {
            public Vector2 Position { get; private set; }

            public void SetPosition(Vector2 newPosition, int timestamp)
            {
                this.Position = newPosition;
                this.lastObservationTimestamp = timestamp;
            }

            public int lastObservationTimestamp; // = HAVE_NOT_BEEN_SEEN_YET_TIME;

            public static implicit operator Vector2(ObservedPosition observed)
            {
                return observed.Position;
            }

            public bool IsRecent(SimWorldState state, int withinSeconds)
            {
                Assert.IsTrue(withinSeconds >= 0);
                return this.lastObservationTimestamp > state.SecondsElapsed - withinSeconds;
            }
        }
    }
}