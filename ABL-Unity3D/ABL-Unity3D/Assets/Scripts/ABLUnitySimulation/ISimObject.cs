using Newtonsoft.Json;
using UnityEngine;

namespace ABLUnitySimulation
{
    public interface ISimObject
    {
        [JsonProperty]
        [SerializeField]
        SimId SimId { get; }
        [JsonProperty]
        [SerializeField]
        string Name { get; }

        void SetSimId(SimWorldState state);
    }
}