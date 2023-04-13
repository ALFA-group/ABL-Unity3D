using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Assertions;
using Utilities.GeneralCSharp;

#nullable enable

namespace UI.ABLUnitySimulation
{
    public class SimCreator : SerializedMonoBehaviour
    {
        public RefSimWorldState? stateRef;

        /// <summary>
        ///     Holds the most recent <see cref="SimInitParameters.SimInitializationType" /> used to
        ///     initialize the simulation.
        /// </summary>
        /// <remarks>
        ///     This is necessary for the saving off <see cref="simInitParameters" /> in the results of an experiment.
        /// </remarks>
        [HideInInspector] public SimInitParameters.SimInitializationType? mostRecentInitializationType;

        [OdinSerialize] public SimInitParameters simInitParameters = new SimInitParameters();

        private void Awake()
        {
            if (this.simInitParameters.simInitParametersPureData.initOnStart) this.CreateNow();
        }

        [Button("Create Now")]
        public void CreateNow()
        {
            this.mostRecentInitializationType = this.simInitParameters.simInitParametersPureData.initializationMethod;

            var newSim = this.simInitParameters.CreateNewSim();

            if (this.simInitParameters.simInitParametersPureData.addRedOpportunityFire) this.AddOpportunityFire(newSim, Team.Red);
            if (this.simInitParameters.simInitParametersPureData.addBlueOpportunityFire) this.AddOpportunityFire(newSim);
            
            RefSimWorldState.Set(this.stateRef, newSim);
        }



        private void AddOpportunityFire(SimWorldState? state = null, Team attackingTeam = Team.Blue)
        {
            state ??= RefSimWorldState.Fetch(this.stateRef);
            if (null == state) return;

            var existingOpportunityFire = state.actions.actions
                .Select(ae => ae.subAction)
                .ConditionalCast<SimAction, ActionOpportunityFire>();

            if (existingOpportunityFire.Any(opp => opp.attackingTeam == attackingTeam))
                // Already have opportunity fire like parameters.
                return;

            state.actions.Add(new ActionOpportunityFire(attackingTeam, state.GetEnemyTeamOf(attackingTeam)));
        }
    }
}