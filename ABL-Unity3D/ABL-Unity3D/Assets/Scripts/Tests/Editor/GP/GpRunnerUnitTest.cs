using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ABLUnitySimulation;
using ABLUnitySimulation.SimScoringFunction;
using GP;
using GP.ExecutableNodeTypes;
using GP.ExecutableNodeTypes.ABLUnityActionTypes;
using GP.ExecutableNodeTypes.ABLUnityTypes;
using GP.FitnessFunctions;
using UI.ABLUnitySimulation;
using UnityEngine;
using Utilities.GeneralCSharp;

#nullable enable

namespace Tests.Editor.GP
{
    public abstract class GpRunnerUnitTest
    {
        private readonly Type _fitnessFunctionType;
        private readonly GpPopulationParameters _gpPopulationParameters;
        private readonly int _randomSeed;
        private readonly Type _solutionReturnType;

        protected GpRunnerUnitTest(
            GpPopulationParameters gpPopulationParameters,
            Type solutionReturnType,
            Type? fitnessFunctionType = null,
            int randomSeed = 0)
        {
            this._gpPopulationParameters = gpPopulationParameters;
            this._fitnessFunctionType = fitnessFunctionType ?? typeof(TestFitnessFunction);
            this._solutionReturnType = solutionReturnType;
            this._randomSeed = randomSeed;
        }

        protected GpRunner GetGpRunner(bool verbose = false)
        {
            var fitnessFunction = FitnessFunctionHelper.ConstructAndVerify(this._fitnessFunctionType, 
                new SimScoringFunction(new List<SimScoringCriterionAndWeight>()), 
                0);
            var timeoutInfo = new TimeoutInfo()
                { cancelTokenSource = new CancellationTokenSource(), ignoreGenerationsUseTimeout = false };
            return new GpRunner(
                fitnessFunction, this._gpPopulationParameters, this._solutionReturnType,
                new SimWorldState(new List<SimAgent?>(), Team.Blue), timeoutInfo, new SimEvaluationParameters(),
                randomSeed: this._randomSeed, verbose: verbose
            );
        }
    }
}