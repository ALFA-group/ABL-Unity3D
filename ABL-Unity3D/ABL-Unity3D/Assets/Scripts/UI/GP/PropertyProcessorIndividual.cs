#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using Cysharp.Threading.Tasks;
using GP;
using GP.Experiments;
using GP.FitnessFunctions;
using Planner;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UI.ABLUnitySimulation;
using UI.ABLUnitySimulation.Runners;
using UI.OdinHelpers;
using UI.Planner;
using UI.Trees;
using UI.Trees.Editor;
using UnityEngine;
using Utilities.GeneralCSharp;
using Object = UnityEngine.Object;

namespace UI.GP.Editor
{
    public class PropertyProcessorIndividual : OdinPropertyProcessor<Individual>
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            if (this.Property.ValueEntry.WeakSmartValue is Individual i)
            {
                propertyInfos.AddDelegate("Display Genome", () => DisplayGenomeInTreeViewVisualizer(i),
                    new ButtonGroupAttribute());

                // Look up hierarchy to see what we're being held by.  We want a parent with a GpExperimentParameters field/property.
                // Right now, that's a GpExperimentResults object.
                if (this.Property.TryGetValueInParents(out GpExperimentResults results))
                    propertyInfos.AddDelegate("Run Genome In Sim",
                        () => SetupParametersAndWorldStateThenRunInSim(results, i),
                        new DisableInEditorModeAttribute(),
                        new ButtonGroupAttribute());
                
            }
        }

        private static void DisplayGenomeInTreeViewVisualizer(Individual i)
        {
            var tree = VisualTree.GpTreeToVisualTree(i.genome);

            TreeGraph.OpenNewTreeGraphWindow(i.FitnessSummary, tree);
        }

        private static async UniTask RunInSim(Individual i, GpRunner gpRunner,
            SimRunner simRunner, GpExperimentResults results) 
        {
            IEnumerable<GpFieldsWrapper> fieldsWrappers;
            if (gpRunner.fitnessFunction is ICreatesSimStatesToEvaluate
                fitnessFunctionWhichCreatesSimStatesToEvaluate)
            {
                fieldsWrappers = fitnessFunctionWhichCreatesSimStatesToEvaluate
                    .CreateEvaluationWorldStates()
                    .Select(state => new GpFieldsWrapper(gpRunner, state));
            }
            else if (gpRunner.fitnessFunction is IUsesASuppliedSimWorldState)
            {
                fieldsWrappers = new GpFieldsWrapper(gpRunner, gpRunner.worldState).ToEnumerable();
            }
            else
            {
                throw new NotSupportedException(
                    "The given fitness function does not implement an appropriate interface " +
                    "(either ICreatesSimStatesToEvaluate, or IUsesASuppliedSimWorldState)");
            }
            
            foreach (var gpFieldsWrapper in fieldsWrappers)
            {
                await simRunner.StartShowIndividualExecution(i, gpFieldsWrapper);
                
                var anyBusy = true;
                while (anyBusy)
                {
                    await UniTask.WaitForEndOfFrame();
                    var worldState = RefSimWorldState.Fetch(simRunner.stateRef);
                    if (null == worldState)
                    {
                        RefSimWorldState.Set(simRunner.stateRef,
                            gpRunner.worldState); // reset world state after showing the execution
                        throw new Exception("Null world state");
                    }

                    anyBusy = worldState.AreAnyBusy(worldState.GetTeamHandles(worldState.teamFriendly));
                }

                RefSimWorldState.Set(simRunner.stateRef,
                    gpRunner.worldState); // reset world state after showing the execution
            }
        }

        private static async void SetupParametersAndWorldStateThenRunInSim(GpExperimentResults results, Individual i)
        {
            var simRunnerAll = Object.FindObjectOfType<SimRunner>();
            if (!simRunnerAll) throw new Exception("Unable to run Individual in sim; no SimRunner found.");

            var simEvalParams = Object.FindObjectOfType<SimEvaluationParametersHolder>().simEvaluationParameters ??
                                throw new InvalidOperationException("Sim evaluation parameters are null.");
            var manyWorldsPlannerRunner = Object.FindObjectOfType<ManyWorldsPlannerRunner>();
            manyWorldsPlannerRunner.SetWaypointsAsCircles();
            
            var worldState = Object.FindObjectOfType<RefSimWorldState>()?.data;
            var gpRunner = results.gpParameters.gpParameters.GetGp(
                worldState,
                simEvalParams, 
                null,
                manyWorldsPlannerRunner);
            
            
            // It's not possible for any of these values to end up null, so we use the null-forgiving operator
            SimCreator simCreator = null!;
            SimInitParameters originalSimInitParameters = null!;
            // ManyWorldsPlannerRunner manyWorldsPlannerRunner = null!;
            PlannerParameters originalPlannerParameters = null!;


            if (null != results.simInitParameters)
            {
                simCreator = Object.FindObjectOfType<SimCreator>();
                originalSimInitParameters = simCreator.simInitParameters;
                simCreator.simInitParameters.simInitParametersPureData = results.simInitParameters;
                simCreator.simInitParameters.simInitParametersJsonUnityData.listOfSimAgentsJsonAsset = null;
                simCreator.CreateNow();
            }

            if (null != results.plannerParameters)
            {
                originalPlannerParameters = manyWorldsPlannerRunner.plannerParameters;
                manyWorldsPlannerRunner.plannerParameters.pureData = results.plannerParameters;
                manyWorldsPlannerRunner.plannerParameters.waypointsAsCapsuleColliders.waypointOptionsAsCapsuleColliders = new List<CapsuleCollider>();
                manyWorldsPlannerRunner.plannerParameters.waypointsAsCapsuleColliders.goalWaypointAsCapsuleCollider = null;
            }

            await RunInSim(i, gpRunner, simRunnerAll, results);

            // Reset parameters
            if (null != results.simInitParameters)
            {
                simCreator.simInitParameters = originalSimInitParameters;
                simCreator.CreateNow();
            }

            if (null != results.plannerParameters) manyWorldsPlannerRunner.plannerParameters = originalPlannerParameters;
        }
    }
}