using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

#nullable enable

namespace ABLUnitySimulation
{
    [HideReferenceObjectPicker]
    public class SimInitParameters
    {
        [HideReferenceObjectPicker]
        public class SimInitParametersJsonUnityData
        {
            public TextAsset? listOfSimAgentsJsonAsset;

            
            public SimInitParametersJsonUnityData DeepCopy()
            {
                return new SimInitParametersJsonUnityData()
                {
                    listOfSimAgentsJsonAsset = this.listOfSimAgentsJsonAsset
                };
            }
        }

        [HideReferenceObjectPicker]
        public class SimInitParametersPureData
        {
            [HideInInspector]
            public bool showJsonTextCache = false;
            
            public bool addBlueOpportunityFire;
            public bool addRedOpportunityFire;

            [NonSerialized, OdinSerialize]
            public int? randomSeed = 1;
        
            [ShowIf("@this.initializationMethod == SimInitializationType.FromListOfSimAgentsJsonAsset")]
            public Team teamFriendly = Team.Red;

            
            // public float extraHeightAboveTerrainForSimAgents = 5f;

            public bool initOnStart = true;

            public SimInitParameters.SimInitializationType initializationMethod = SimInitParameters.SimInitializationType.FromListOfSimAgentsJsonAsset;

            [ShowIf("$showJsonTextCache")]
            [Indent]
            public string? listOfSimAgentsJsonTextCache;
            
            public SimInitParametersPureData DeepCopy()
            {
                return new SimInitParametersPureData
                {
                    addBlueOpportunityFire = this.addBlueOpportunityFire,
                    addRedOpportunityFire = this.addRedOpportunityFire,
                    initializationMethod = this.initializationMethod,
                    teamFriendly = this.teamFriendly,
                    randomSeed = this.randomSeed,
                    listOfSimAgentsJsonTextCache = this.listOfSimAgentsJsonTextCache
                };
            }

        }
        
        public enum SimInitializationType
        {
            FromListOfSimAgentsJsonAsset = 1
        }
        
        [OnInspectorInit("InitPureData")]
        [HideLabel]
        public SimInitParametersPureData simInitParametersPureData = new SimInitParametersPureData();
        
        [OnInspectorInit("InitJsonUnityData")]
        [ShowIf("@this.simInitParametersPureData.initializationMethod == SimInitializationType.FromListOfSimAgentsJsonAsset &&" +
                "!this.simInitParametersPureData.showJsonTextCache")]
        [HideLabel]
        [Indent]
        public SimInitParametersJsonUnityData simInitParametersJsonUnityData = new SimInitParametersJsonUnityData();

        
        public SimInitParameters DeepCopy()
        {
            return new SimInitParameters
            {
                simInitParametersPureData = this.simInitParametersPureData.DeepCopy(),
                simInitParametersJsonUnityData = this.simInitParametersJsonUnityData.DeepCopy()
            };
        }
        
        public SimWorldState CreateNewSim()
        {
            switch (this.simInitParametersPureData.initializationMethod)
            {
                case SimInitParameters.SimInitializationType.FromListOfSimAgentsJsonAsset:
                    if (null == this.simInitParametersPureData.listOfSimAgentsJsonTextCache)
                    {
                        if (null == this.simInitParametersJsonUnityData.listOfSimAgentsJsonAsset)
                        {
                            throw new Exception("No list of sim agents json text asset found");
                        }
                        this.simInitParametersPureData.listOfSimAgentsJsonTextCache =
                            this.simInitParametersJsonUnityData.listOfSimAgentsJsonAsset.text;
                    }
                    var simAgents =
                        JsonConvert.DeserializeObject<List<SimAgent>>(this.simInitParametersPureData.listOfSimAgentsJsonTextCache);
                    return new SimWorldState(simAgents, this.simInitParametersPureData.teamFriendly,
                        this.simInitParametersPureData.randomSeed);
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Don't know how to create {this.simInitParametersPureData.initializationMethod} ");
            }
        }

        public void InitJsonUnityData()
        {
            this.simInitParametersJsonUnityData ??= new SimInitParametersJsonUnityData();
        }
        
        public void InitPureData()
        {
            this.simInitParametersPureData ??= new SimInitParametersPureData();
        }
    }
}