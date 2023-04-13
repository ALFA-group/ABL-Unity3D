using System;
using System.IO;
using System.Linq;
using ABLUnitySimulation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sirenix.OdinInspector;
using UnityEngine;

#nullable enable

namespace UI.ABLUnitySimulation
{
    public class SerializeAgentsToJson : MonoBehaviour
    {
        public RefSimWorldState? refState;

        public string jsonFile = "";
        
        [Button]
        protected void SimWorldAgentsToJson()
        {
            if (null == this.refState) throw new InvalidOperationException($"{nameof(this.refState)} is null");
            var agents = RefSimWorldState.Fetch(this.refState)?.Agents;
            var json = JsonConvert.SerializeObject(agents, Formatting.Indented);
            File.WriteAllText(this.jsonFile, json);
        }
    }
}