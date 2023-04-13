using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Utilities.GeneralCSharp
{
    public static class GenericUtilities
    {
        /// <summary>
        ///     A handy function to put a breakpoint on.
        /// </summary>
        public static void NoOp()
        {
        }

        public static float PythagoreanTheorem(float a, float b)
        {
            return (float)Math.Sqrt(a * a + b * b);
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            // Switched to new syntax which allows for simpler swapping
            (lhs, rhs) = (rhs, lhs);
        }

        public static string GetRelativePath(string file)
        {
            string currentDirectory = Directory.GetCurrentDirectory() ??
                                      throw new Exception("Somehow the current directory is null.");

            return Path.Combine(currentDirectory, file);
        }

        public static float ShiftNumberIntoRange(
            float x,
            float newMin, float newMax,
            float oldMin = 0.0f, float oldMax = 1.0f)
        {
            if (Math.Abs(oldMin - oldMax) < 0.0001)
                throw new Exception($"Old min ({oldMin}) must not equal old max ({oldMax})");

            // https://math.stackexchange.com/questions/914823/shift-numbers-into-a-different-range
            return newMin + (newMax - newMin) / (oldMax - oldMin) * (x - oldMin);
        }

        public static T GetRandomElementFromDistribution<T>(Dictionary<T, double> probabilities, Random rand)
        {
            double pSum = probabilities.Sum(t => t.Value);
            if (pSum == 0) throw new Exception("Sum of the the probability distribution must be greater than zero.");
            double r = rand.NextDouble() * pSum;
            double sum = 0;

            foreach (var kvp in probabilities)
                if (r < (sum += kvp.Value))
                    return kvp.Key;

            throw new Exception($"Somehow no random type was chosen with sum = {sum}.");
        }

        public static string Indent(int count)
        {
            return "".PadLeft(count);
        }

        public static int CombineHashCodes(IEnumerable<int> hashCodes)
        {
            // System.Web.Util.HashCodeCombiner.CombineHashCodes(System.Int32, System.Int32): http://referencesource.microsoft.com/#System.Web/Util/HashCodeCombiner.cs,21fb74ad8bb43f6b
            // System.Array.CombineHashCodes(System.Int32, System.Int32): http://referencesource.microsoft.com/#mscorlib/system/array.cs,87d117c8cc772cca
            var hash = 5381;

            foreach (int hashCode in hashCodes)
                hash = ((hash << 5) + hash) ^ hashCode;

            return hash;
        }

        // https://stackoverflow.com/questions/22818531/how-to-rotate-2d-vector
        public static Vector2 RotateRadians(this Vector2 v, float radians)
        {
            double ca = Math.Cos(radians);
            double sa = Math.Sin(radians);
            double newX = ca * v.x - sa * v.y;
            double newY = sa * v.x + ca * v.y;
            return new Vector2((float)newX, (float)newY);
        }

        public static Vector2 RotateDegrees(this Vector2 v, float degrees)
        {
            return v.RotateRadians(degrees * Mathf.Deg2Rad);
        }


        public static float Quantize(this float x, int steps, float floatMin, float floatMax)
        {
            // Idea: Do math operations, and undo almost all of those math operations
            float range = floatMax - floatMin;
            float normalizedX = (x - floatMin) / range;
            float bigger = normalizedX * steps;
            var big = (int)(bigger + 0.5); // since int always truncates, add 0.5 so that it rounds instead
            // This is basically Math.Round
            return floatMin + big * range / steps;
        }

        public static float SecondsElapsedSince(DateTime start)
        {
            return (float)(DateTime.Now - start).TotalSeconds;
        }
        
        public static float SecondsElapsed(DateTime start, DateTime end)
        {
            return (float)(end - start).TotalSeconds;
        }

    }
}