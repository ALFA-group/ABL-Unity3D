using System;
using ABLUnitySimulation.Exceptions;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable

namespace ABLUnitySimulation
{
    /// <summary>
    ///     Essentially a pointer to an instance <see cref="ISimObject" />. This is necessary for two reasons.
    ///     <list type="number">
    ///         <item>
    ///             If we pass around an instance of <see cref="ISimObject" />, we will be passing around an outdated version
    ///             whose field values are from a previous simulation time of the world state. Thus, this is used to make sure
    ///             we are getting current values related to the.
    ///         </item>
    ///         <item>
    ///             It saves memory space.
    ///         </item>
    ///     </list>
    /// </summary>
    /// <typeparam name="T">A subclass of <see cref="ISimObject" />.</typeparam>
    [Serializable]
    public struct Handle<T> where T : class, ISimObject
    {
        // [SerializeReference]
        public SimId simId;

        /// <summary>
        ///     Helper property to get a <see cref="Handle{T}" /> with an invalid <see cref="SimId" />.
        /// </summary>
        public static Handle<T> Invalid
        {
            get
            {
                var bogus = new Handle<T>();
                return bogus;
            }
        }

        [JsonConstructor]
        public Handle(SimId simId)
        {
            this.simId = simId;
        }

        public Handle(T t)
        {
            this.simId = t.SimId;
        }

        /// <summary>
        ///     Get the concrete value of this handle from a given simulation world <paramref name="state" />.
        /// </summary>
        /// <param name="state">The simulation world state to reference.</param>
        /// <returns>The concrete value of this handle from a given simulation world <paramref name="state" />.</returns>
        /// <exception cref="HandleException">Throws if the given state does not contain the concrete value for this handle.</exception>
        public readonly T Get(SimWorldState state)
        {
            if (this.simId == 0) Debug.LogError("Found handle with simId 0");

            var t = state.Get(this) ?? throw HandleException.CreateNotFound(this, state);
            Debug.Assert(t.SimId == this.simId);
            return t;
        }

        /// <summary>
        ///     A wrapper for <see cref="SimWorldState.GetCanFail" />.
        ///     If the handle is not found in the given simulation world <paramref name="state" />, return null.
        /// </summary>
        /// <param name="state">The simulation world state to reference.</param>
        /// <returns>
        ///     The concrete value of this handle from a given simulation world <paramref name="state" />, or null if the
        ///     handle is not found.
        /// </returns>
        public readonly T? GetCanFail(SimWorldState state)
        {
            if (this.simId == 0) return null;
            var t = state.GetCanFail(this);
            Debug.Assert(null == t || t.SimId == this.simId);
            return t;
        }

        public static implicit operator Handle<T>(T? t)
        {
            return null == t ? new Handle<T>() : new Handle<T>(t);
        }

        public static bool operator ==(Handle<T> a, Handle<T> b)
        {
            return a.simId == b.simId;
        }

        public static bool operator !=(Handle<T> a, Handle<T> b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return $"H{this.simId.id}:{typeof(T).Name}";
        }

        /// <summary>
        ///     A more compact version of <see cref="ToString" /> which does not include the name of type <typeparamref name="T" />.
        /// </summary>
        /// <returns></returns>
        public string ToStringSmall()
        {
            return $"H{this.simId.id}";
        }

        /// <summary>
        ///     Is the <see cref="simId" /> for this <see cref="Handle{T}" /> valid?
        /// </summary>
        public bool IsValid => this.simId.IsValid;

        public override int GetHashCode()
        {
            return this.simId.id;
        }

        public bool Equals(Handle<T> handle)
        {
            return handle.simId == this.simId;
        }

        public override bool Equals(object obj)
        {
            if (obj is Handle<T> otherHandle) return this.simId == otherHandle.simId;

            return false;
        }
    }
}