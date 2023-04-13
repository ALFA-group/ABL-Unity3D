using System;
using Newtonsoft.Json;
using UnityEngine;
using Utilities.Unity;

namespace ABLUnitySimulation
{
    // [Serializable]
    public struct SimId : IEquatable<SimId>
    {
        // [SerializeField]
        public int id;

        [JsonConstructor]
        public SimId(int id)
        {
            this.id = id;
        }

        public bool IsValid => this.id > 0;

        public static SimId Invalid => new SimId(0);

        public bool Equals(SimId other)
        {
            return this.id == other.id;
        }

        public override bool Equals(object obj)
        {
            return obj is SimId other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.id;
        }

        public static bool operator ==(SimId a, SimId b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SimId a, SimId b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            if (!this.IsValid)
                CustomPrinter.PrintLine(
                    "The SimId of the current sim object is invalid. " +
                    "You must add the sim object to the world state " +
                    "prior to accessing its SimId.");
            return $"SID[{this.id}]";
        }

        public static implicit operator SimId(int id)
        {
            return new SimId(id);
        }
    }
}