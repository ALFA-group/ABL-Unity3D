using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ABLUnitySimulation
{
    public static class SimExtensions
    {
        public static string DebugName(this ISimObject simObject)
        {
            return null == simObject ? "nullSimObject" : $"{simObject.Name}/{simObject.SimId}";
        }


        public static T GetRandom<T>(this List<T> candidates, Random random)
        {
            if (candidates.Count < 1) return default;

            return candidates[random.Next(candidates.Count)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Team GetEnemyTeam(this Team myTeam)
        {
            return myTeam switch
            {
                Team.Red => Team.Blue,
                Team.Blue => Team.Red,
                _ => Team.Undefined
            };
        }
    }
}