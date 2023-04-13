using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

#nullable enable

namespace ABLUnitySimulation
{
    /// <summary>
    ///     Standard implementation for ISimObject
    /// </summary>
    [Serializable]
    public class SimObjectBase : ISimObject
    {
        [JsonConstructor]
        public SimObjectBase(SimId simId, string name)
        {
            this.SimId = simId;
            this.Name = name;
        }

        public SimObjectBase(SimWorldState state, string name) : this(state.GetUnusedSimId(), name)
        {
        }

        // Exists for deserialization and little else.
        protected SimObjectBase()
        {
        }

        [SerializeField]
        private SimId _simId;
        
        /// <summary>
        ///     simId *must* be set for this object to be valid.
        ///     Grab a new, unique simId from a SimWorldState.
        /// </summary>
        [ShowInInspector]
        [PropertyOrder(-2)]
        [JsonProperty]
        [SerializeField]
        public SimId SimId {
            get => this._simId;
            private set => this._simId = value;
        }
        

        [ShowInInspector, PropertyOrder(-1)] public string Name { get; set; } = "";

        public void SetSimId(SimWorldState state)
        {
            if (this.SimId.IsValid) throw new Exception($"Cannot change valid id {this.SimId} for {this.Name}");

            this.SimId = state.GetUnusedSimId();
        }
    }
}