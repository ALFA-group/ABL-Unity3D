using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

#nullable enable

namespace ABLUnitySimulation
{
    public class SimWeaponSet : Collection<SimWeapon>
    {
        private static readonly List<SimWeapon> Empty = new List<SimWeapon>(1);

        public SimWeaponSet() : base(Empty)
        {
        }

        public SimWeaponSet(List<SimWeapon> weapons) : base(weapons.ToList())
        {
        }

        public SimWeaponSet(IEnumerable<SimWeapon> weapons) : base(weapons.ToList())
        {
        }

        public SimWeaponSet(SimWeapon onlyWeapon) : base(new List<SimWeapon> { onlyWeapon })
        {
        }

        public float CalculateBestDpsDirect(float kmDistanceToTarget, SimAgentType simAgentType)
        {
            float bestDps = 0;

            // Using for loop and temp variable for Count because this is a very tight loop called very often.
            //  Avoids extra enumerators and other overhead.
            int count = this.Count;
            for (var index = 0; index < count; index++)
            {
                var weapon = this[index];

                if (weapon.InRange(kmDistanceToTarget))
                {
                    float dps = weapon.damage.DpsVs(simAgentType);
                    bestDps = Mathf.Max(bestDps, dps);
                }
            }

            return bestDps;
        }

        public float KmMaxRange()
        {
            return this.Count <= 0 ? 0 : this.Max(w => w.kmMaxRange);
        }
    }
}