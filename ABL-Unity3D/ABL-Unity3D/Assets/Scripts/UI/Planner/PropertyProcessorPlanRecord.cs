using System;
using System.Collections.Generic;
using ABLUnitySimulation;
using Planner;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UI.ABLUnitySimulation.Runners;
using UI.Trees;
using UI.Trees.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.Planner
{
    public class PropertyProcessorPlanRecord : OdinPropertyProcessor<PlanStorage.PlanRecord>
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            if (this.Property.ValueEntry.WeakSmartValue is PlanStorage.PlanRecord planRecord)
            {
                
                var runner = Object.FindObjectOfType<ManyWorldsPlannerRunner>();
                
                propertyInfos.AddDelegate("View Plan", () => this.ShowGraph(planRecord),
                    new ButtonGroupAttribute());
                
                propertyInfos.AddDelegate("Run Plan Against Original Sim",
                    () => this.RunPlanAgainst(planRecord, planRecord.plan.startState),
                    new DisableInEditorModeAttribute(),
                    new ButtonGroupAttribute());
                // // Look up hierarchy to see what we're being held by. We want the start state stored in the plan storage.
                // if (this.Property.TryGetValueInParents(out PlannerResults plannerResults))
                // {
                //     propertyInfos.AddDelegate("Run Plan Against Original Sim",
                //         () => this.RunPlanAgainst(planRecord, plannerResults.simInitParameters.CreateNewSim()),
                //         new DisableInEditorModeAttribute(),
                //         new ButtonGroupAttribute());
                // }
                // else
                // {
                //     propertyInfos.AddDelegate("Run Plan Against Original Sim",
                //         () => this.RunPlanAgainst(planRecord, plannerResults.simInitParameters.CreateNewSim()),
                //         new DisableInEditorModeAttribute(),
                //         new ButtonGroupAttribute());
                // }

                if (null == runner.stateRef) throw new Exception($"{nameof(runner.stateRef)} is null");
                
                propertyInfos.AddDelegate("Run Plan Against Current Sim",
                    () => this.RunPlanAgainst(planRecord, runner.stateRef.data),
                    new DisableInEditorModeAttribute(),
                    new ButtonGroupAttribute());
            }
        }

        
        /// <summary>
        ///     Function to show a graph of the given plan in our custom Unity Tree Graph Visualizer.
        /// </summary>
        public void ShowGraph(PlanStorage.PlanRecord planRecord)
        {
            var tree = VisualTree.MethodDecompositionDictionaryToVisualTree(planRecord.plan ?? throw new NullReferenceException());
            TreeGraph.OpenNewTreeGraphWindow(planRecord.ScoreString, tree);
        }
        
        /// <summary>
        ///     Run a plan against a given world state.
        /// </summary>
        /// <param name="state">The world state to run this plan against.</param>
        /// <param name="planRecord"></param>
        public void RunPlanAgainst(PlanStorage.PlanRecord planRecord, SimWorldState state)
        {
            var runner = Object.FindObjectOfType<SimRunner>();
            runner.StartShowPlanNoReplan(state, planRecord.plan ?? throw new NullReferenceException());
        }
        
    }
}