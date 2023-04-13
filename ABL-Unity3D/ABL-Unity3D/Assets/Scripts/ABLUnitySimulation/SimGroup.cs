using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;

#nullable enable

namespace ABLUnitySimulation
{
    /// <summary>
    ///     A wrapper for a group of <see cref="SimAgent" />s.
    ///     Objects of this type should not be modified after creation.
    /// </summary>
    /// <remarks>
    ///     Unity does not support serialization for the Collection class, so we implement a custom serialization
    ///     and deserialization method.
    /// </remarks>
    [Serializable]
    public class SimGroup : Collection<Handle<SimAgent>>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<Handle<SimAgent>> _handlesForSerialization = new List<Handle<SimAgent>>();

        // Only use upon deserialization
        public SimGroup()
        {
            foreach (var handle in this._handlesForSerialization)
            {
                this.Add(handle);
            }
        }
        
        public SimGroup(string name)
        {
            this.Name = name;
        }

        public SimGroup(SimGroup copyMe) : base(copyMe)
        {
            this.Name = copyMe.Name;
        }

        public SimGroup(SimAgent onlyMember)
        {
            this.Add(onlyMember);
        }

        public SimGroup(Handle<SimAgent> onlyMember)
        {
            this.Add(onlyMember);
        }

        public SimGroup(IEnumerable<Handle<SimAgent>> members)
        {
            foreach (var newMember in members) this.Add(newMember);
        }

        public SimGroup(string name, IEnumerable<Handle<SimAgent>> members) : this(name)
        {
            foreach (var newMember in members) this.Add(newMember);
        }

        public SimGroup(params Handle<SimAgent>[] agentHandles)
        {
            foreach (var newMember in agentHandles) this.Add(newMember);
        }


        public SimGroup(IEnumerable<SimAgent> members)
        {
            foreach (var newMember in members) this.Add(newMember);
        }

        public SimGroup(string name, IEnumerable<SimAgent> members) : this(name)
        {
            foreach (var newMember in members) this.Add(newMember);
        }

        /// <summary>
        ///     The name of this <see cref="SimGroup" />. Used for display purposes.
        /// </summary>
        public string Name { get; protected set; } = "";


        /// <summary>
        ///     Add a <see cref="SimAgent" /> to this <see cref="SimGroup" />
        /// </summary>
        /// <param name="t"></param>
        public void Add(SimAgent t)
        {
            if (!this.Contains(t)) this.Add(t.Handle);
        }

        public static implicit operator SimGroup(SimAgent simAgent)
        {
            return new SimGroup(simAgent);
        }

        public override string ToString()
        {
            var sb = new StringBuilder("[");
            if (!string.IsNullOrWhiteSpace(this.Name))
            {
                sb.Append(this.Name);
                sb.Append(':');
            }

            sb.Append(string.Join(",", this.Select(thing => thing.ToStringSmall())));
            sb.Append("]");

            return sb.ToString();
        }


        /// <summary>
        ///     Get all the <see cref="SimAgent" />s in this <see cref="SimGroup" /> from a given simulation world
        ///     <paramref name="state" />.
        /// </summary>
        /// <param name="state">The simulation world state to reference.</param>
        /// <returns>
        ///     All <see cref="SimAgent" />s in this <see cref="SimGroup" /> from a given simulation world
        ///     <paramref name="state" />.
        /// </returns>
        public IReadOnlyList<SimAgent> Get(SimWorldState state)
        {
            return state.GetGroupMembers(this);
        }

        public void OnBeforeSerialize()
        {
            this._handlesForSerialization.Clear();

            foreach (var handle in this.Items)
            {
                this._handlesForSerialization.Add(handle);
            }
        }

        public void OnAfterDeserialize()
        {
        }
    }
}