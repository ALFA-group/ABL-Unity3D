using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using GP;

#nullable enable

namespace Utilities.GP
{
    public static class GpUtility
    {
        private static readonly Dictionary<Type, string> BetterNamesDictionary = new Dictionary<Type, string>
        {
            { typeof(float), "Float" },
            { typeof(Handle<SimAgent>), "H:SimAgent" },
            { typeof(int), "Integer" }
        };

        
        // ReSharper disable once ReturnTypeCanBeEnumerable.Global
        public static List<Individual> SortedByFitness(this IEnumerable<Individual> population)
        {
            return population.ToList().SortedByFitness();
        }

        public static List<Individual> SortedByFitness(this List<Individual> population)
        {
            return population.OrderByDescending(i => i.fitness).ToList(); 
        }

        public static string GetBetterClassName(Type t)
        {
            return BetterNamesDictionary.TryGetValue(t, out string? betterName) ? betterName : t.Name;
        }
    }
}