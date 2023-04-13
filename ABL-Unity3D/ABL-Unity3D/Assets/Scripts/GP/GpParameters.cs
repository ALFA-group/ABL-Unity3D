using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ABLUnitySimulation;
using GP.ExecutableNodeTypes.ABLUnityActionTypes;
using GP.FitnessFunctions;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UI.Planner;
using UnityEngine;
using Utilities.GeneralCSharp;
using Utilities.Unity;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

#nullable enable

namespace GP
{

    [HideReferenceObjectPicker]
    
    public class GpParameters
    {
        [FoldoutGroup("Genetic Operator Parameters"), MinValue(0), MaxValue(1)]
        public double crossoverProbability = GpPopulationParameters.DEFAULT_CROSSOVER_PROBABILITY;
        
        [FoldoutGroup("Genetic Operator Parameters"), MinValue(0), MaxValue(1)]
        public double mutationProbability = GpPopulationParameters.DEFAULT_MUTATION_PROBABILITY;
        
        [FoldoutGroup("Evolutionary Search Loop"), MinValue(0)]
        public int eliteSize = GpPopulationParameters.DEFAULT_ELITE_SIZE;
        

        [FoldoutGroup("Fitness Function Parameters")]
        [OnInspectorInit("InitFitnessFunctionType")]
        [ValueDropdown("EnumerateFitnessFunctionTypes")]
        public Type fitnessFunctionType = typeof(TestFitnessFunction);

        [FoldoutGroup("Population Initialization Parameters")]
        [ValueDropdown("$PopulationInitializationMethodInstances")]
        [OnInspectorInit("InitPopulationInitializationMethod")]
        public PopulationInitializationMethod? initializationMethod;

        [FoldoutGroup("Evolutionary Search Loop"), MinValue(0)]
        public int maxDepth = GpPopulationParameters.DEFAULT_MAX_DEPTH;
        
        [FoldoutGroup("Evolutionary Search Loop"), MinValue(0)]
        public int numberGenerations = GpPopulationParameters.DEFAULT_NUMBER_GENERATIONS;

        [FoldoutGroup("Evolutionary Search Loop"), MinValue("$tournamentSize")]
        public int populationSize = GpPopulationParameters.DEFAULT_POPULATION_SIZE;

        [FoldoutGroup("Strong Typing and Probabilities")]
        [NonSerialized]
        [OdinSerialize]
        [ListDrawerSettings(AlwaysAddDefaultValue = true, CustomAddFunction = "DefaultTypeProbability")]
        [OnInspectorInit("InitProbabilityDistributionEditor")]
        [Indent]
        public List<TypeProbability>? probabilityDistributionEditor;

        [FoldoutGroup("Evolutionary Search Loop")]
        public bool ramp = GpPopulationParameters.DEFAULT_RAMP;


        
        [FoldoutGroup("Strong Typing and Probabilities")]
        [ValueDropdown("EnumerateSolutionReturnTypes")]
        [OnInspectorInit("InitSolutionReturnType")]
        public Type solutionReturnType = typeof(MoveToSimAgent);
        
        [FoldoutGroup("Evolutionary Search Loop"), MinValue(2)]
        public int tournamentSize = GpPopulationParameters.DEFAULT_TOURNAMENT_SIZE;

        [FoldoutGroup("Misc GP Parameters")] public bool useRandomSeed = false;
        
        [FoldoutGroup("Misc GP Parameters"), ShowIf("@this.useRandomSeed == true"), Indent]
        public int randomSeed = 0;

        [FoldoutGroup("Misc GP Parameters")] public bool verbose = false;

        public ProbabilityDistribution ProbabilityDistribution =>
            new ProbabilityDistribution(this.probabilityDistributionEditor ?? new List<TypeProbability>());
            

        private IEnumerable<PopulationInitializationMethod>? PopulationInitializationMethodInstances =>
            PopulationInitializationMethod.GetPopulationInitializationMethodTypes()?
                .Select(t =>
                    (PopulationInitializationMethod)Activator.CreateInstance(t));
        public GpRunner GetGp(SimWorldState? simWorldState, 
            SimEvaluationParameters simEvaluationParameters,
            TimeoutInfo? timeoutInfo = null,
            ManyWorldsPlannerRunner? manyWorldsPlannerRunner = null) 
        {
            var popParams = new GpPopulationParameters(
                this.eliteSize,
                this.tournamentSize,
                this.populationSize,
                this.maxDepth,
                this.numberGenerations,
                this.crossoverProbability,
                this.mutationProbability,
                this.ramp,
                populationInitializationMethod: this.initializationMethod,
                probabilityDistribution: this.ProbabilityDistribution
            );
            
            // Set fitness function
            var fitnessFunction = FitnessFunctionHelper.ConstructAndVerify(this.fitnessFunctionType,
                simEvaluationParameters.primaryScoringFunction ?? throw new Exception("TODO make scoring function optional"),
                simEvaluationParameters.gpScoringFunctionWeight);

            var state =
                FitnessFunctionHelper.GetCorrectSimWorldStateGivenAFitnessFunction(fitnessFunction, simWorldState).DeepCopy();
            
            var plannerWrapper = null == manyWorldsPlannerRunner ? null : 
                PlannerWrapper.GetPlannerWrapperBasedOnFitnessFunctionAndManyWorldsPlannerRunner(fitnessFunction,
                    manyWorldsPlannerRunner);

            var timeoutInfoNotNull = timeoutInfo ?? new TimeoutInfo()
                { ignoreGenerationsUseTimeout = false, cancelTokenSource = new CancellationTokenSource() };
            
            return new GpRunner(
                fitnessFunction,
                popParams,
                this.solutionReturnType,
                randomSeed: this.useRandomSeed ? this.randomSeed : (int?)null,
                verbose: this.verbose,
                plannerWrapper: plannerWrapper,
                simEvaluationParameters: simEvaluationParameters,
                simWorldState: state,
                timeoutInfo: timeoutInfoNotNull
            );
        }

        private static IEnumerable<Type> EnumerateSolutionReturnTypes()
        {
            return GpRunner.GetAllReturnTypes();
        }

        private static IEnumerable<Type> EnumerateFitnessFunctionTypes()
        {
            return GpRunner.GetFitnessFunctionTypes();
        }

        [UsedImplicitly]
        public void InitPopulationInitializationMethod()
        {
            this.initializationMethod ??= this.PopulationInitializationMethodInstances?.First();
        }

        [UsedImplicitly]
        public void InitSolutionReturnType()
        {
            // ReSharper disable once ConstantNullCoalescingCondition
            this.solutionReturnType ??= EnumerateSolutionReturnTypes().First();
        }

        [UsedImplicitly]
        public void InitFitnessFunctionType()
        {
            // ReSharper disable once ConstantNullCoalescingCondition
            this.fitnessFunctionType ??= EnumerateFitnessFunctionTypes().First();
        }

        [UsedImplicitly]
        public void InitProbabilityDistributionEditor()
        {
            this.probabilityDistributionEditor ??= new List<TypeProbability>();
        }
        
        [UsedImplicitly]
        public TypeProbability DefaultTypeProbability()
        {
            return new TypeProbability();
        }
    }
}