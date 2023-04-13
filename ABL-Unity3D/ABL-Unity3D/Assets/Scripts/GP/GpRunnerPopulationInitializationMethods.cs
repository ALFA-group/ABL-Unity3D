using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GP.ExecutableNodeTypes.GpPlanner;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Utilities.GeneralCSharp;
using Utilities.Unity;

#nullable enable

namespace GP
{
    public abstract class PopulationInitializationMethod
    {
        public virtual Task<List<Individual>> GetPopulation<T>(GpRunner gp, TimeoutInfo timeoutInfo)
        {
            return Task.FromResult(new List<Individual>());
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Global
        public static List<Type>? GetPopulationInitializationMethodTypes()
        {
            var types = GpReflectionCache.GetAllSubTypes(typeof(PopulationInitializationMethod)).ToList();
            return !types.Any() ? null : types;
        }
        
    }

    public class RampedPopulationInitialization : PopulationInitializationMethod
    {
   
        public override async Task<List<Individual>>
            GetPopulation<T>(GpRunner gp, TimeoutInfo timeoutInfo) // Get around T by using MakeClosedGenericType
        {
            
            // Don't know if we should do this because can't always do that if building block min tree height is lower than the ramped depth

            var population = new List<Individual>();
            var uniqueNodes = new List<Node>();
            int n = gp.populationParameters.populationSize * 10; 
            for (
                var individualNumber = 0;
                individualNumber < n && population.Count < gp.populationParameters.populationSize &&
                !timeoutInfo.ShouldTimeout;
                individualNumber++)
            {
                // Ramp the depth
                var t = typeof(T);
                bool isSubclassOfExecutableTree = GpRunner.IsSubclassOfExecutableTree(t);
                var minTreeData = isSubclassOfExecutableTree
                    ? gp.nodeTypeToMinTreeDictionary[t]
                    : gp.nodeReturnTypeToMinTreeDictionary[new ReturnTypeSpecification(t, null)];

                int possibleRampedDepth = individualNumber % gp.populationParameters.maxDepth + 1;
                int currentMaxDepth =
                    gp.populationParameters.ramp
                        ? Math.Max(minTreeData.heightOfMinTree + 1, possibleRampedDepth)
                        : gp.populationParameters.maxDepth;

                var randomTree = gp.GenerateRandomTreeFromTypeOrReturnType<T>(currentMaxDepth, gp.rand.NextBool());
                if (uniqueNodes.Contains(randomTree, new NodeComparer()))
                {
                    Debug.Log("Duplicate node found in initialization");
                    continue;
                }
                uniqueNodes.Add(randomTree);

                foreach (var child in randomTree.children)
                {
                    foreach (var node in child.IterateNodes())
                    {
                        var tp = gp.populationParameters.probabilityDistribution.distribution
                            .First(typeProbability => typeProbability.type == node.GetType());
                        Debug.Assert(tp.probability != 0);
                    }
                }


                Debug.Assert(randomTree.GetHeight() <= gp.populationParameters.maxDepth);

                var ind = new Individual(randomTree);
                if (!GpRunner.IsInDenyList(ind))
                {
                    await gp.EvaluateFitnessOfIndividual(ind);
                    population.Add(ind);
                }
            }

            return population;
        }
    }
    
    
    public class RandomPopulationInitialization : PopulationInitializationMethod
    {
        public override async Task<List<Individual>>
            GetPopulation<T>(GpRunner gp, TimeoutInfo timeoutInfo) // Get around T by using MakeClosedGenericType
        {
            var population = new List<Individual>();
            for (int i = 0; i < gp.populationParameters.populationSize && !timeoutInfo.ShouldTimeout; i++)
            {
                const bool forceFullyGrow = false;
                Node randomTree = gp.GenerateRandomTreeFromTypeOrReturnType<T>(gp.populationParameters.maxDepth, forceFullyGrow);
                var ind = new Individual(randomTree);
                await gp.EvaluateFitnessOfIndividual(ind);
                population.Add(ind);
            }
            

            return population;
        }
    }
}