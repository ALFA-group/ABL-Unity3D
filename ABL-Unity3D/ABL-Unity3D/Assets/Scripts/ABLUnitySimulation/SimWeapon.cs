using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace ABLUnitySimulation
{
    [Serializable]
    public struct SimWeapon
    {
        public static SimWeapon defaultWeapon3 = new SimWeapon
        {
            name = "Default Weapon 3",
            kmMaxRange = 2,
            kmStrikeRadius = 1,
            numSimultaneouslyFiring = 1,
            secondsOfAmmo = 30 * 60,
            damage = SimDamageProfile.DefaultDamageProfile3
        };

        public static SimWeapon defaultWeapon1 = new SimWeapon
        {
            name = "Default Weapon 2",
            kmMaxRange = 2,
            kmStrikeRadius = 0,
            numSimultaneouslyFiring = 1,
            secondsOfAmmo = 30 * 60,
            damage = SimDamageProfile.DefaultDamageProfile1
        };

        public string name;
        public string munitionName;

        /// <summary>
        ///     What is the maximum effective range of the weapon?
        /// </summary>
        public float kmMaxRange;

        /// <summary>
        ///     What is the minimum effective range of the weapon?
        /// </summary>
        public float kmMinRange;

        /// <summary>
        ///     How long can the weapon maintain continuous, full rate of fire before running out of ammo?
        /// </summary>
        public float secondsOfAmmo;

        /// <summary>
        ///     Can the weapon be used for indirect fire?
        /// </summary>
        public bool isIndirect;

        public float kmStrikeRadius;
        public int numSimultaneouslyFiring;

        /// <summary>
        ///     Just to double check, use secondsOfAmmo.
        /// </summary>
        
        public int numRounds;

        /// <summary>
        ///     How much damage (in health points) does the weapon do in a second?
        /// </summary>
        public SimDamageProfile damage;

        public bool IsArtillery => this.isIndirect && this.kmMaxRange > 5;

        public float CalculateDamage(float kmDistance, SimAgentType simAgentType, float dt)
        {
            if (kmDistance <= this.kmMaxRange) return this.damage.DpsVs(simAgentType) * dt;

            return 0;
        }

        public static float CalculateDamage(IEnumerable<SimWeapon> weapons, float kmDistance, SimAgentType simAgentType,
            float dt)
        {
            return weapons.Max(w => w.CalculateDamage(kmDistance, simAgentType, dt));
        }

        public bool IsVerySimilarTo(SimWeapon otherWeapon)
        {
            if (this.HasSimilarRangeTo(otherWeapon) &&
                this.damage.Approximately(otherWeapon.damage) &&
                this.isIndirect == otherWeapon.isIndirect)
                return true;

            return false;
        }

        public bool HasSimilarRangeTo(SimWeapon otherWeapon)
        {
            return Mathf.Approximately(this.kmMinRange, otherWeapon.kmMinRange) &&
                   Mathf.Approximately(this.kmMaxRange, otherWeapon.kmMaxRange);
        }

        public bool InRange(float kmDistanceToTarget)
        {
            return this.kmMinRange <= kmDistanceToTarget && this.kmMaxRange >= kmDistanceToTarget;
        }

        public bool CanDamage(SimAgentType simAgentType)
        {
            return this.damage.DpsVs(simAgentType) > 0;
        }
    }

    public struct SimDamageProfile
    {
        public string name;
        public float damagePerSecondVsTypeA;
        public float damagePerSecondVsTypeB;
        public float damagePerSecondVsTypeC;
        public float damagePerSecondVsTypeD;

        public readonly float DpsVs(SimAgentType simAgentType)
        {
            return simAgentType switch
            {
                SimAgentType.TypeC => this.damagePerSecondVsTypeC,
                SimAgentType.TypeA => this.damagePerSecondVsTypeA,
                SimAgentType.TypeB => this.damagePerSecondVsTypeB,
                SimAgentType.TypeD => this.damagePerSecondVsTypeD,
                _ => throw new ArgumentOutOfRangeException(nameof(simAgentType), simAgentType, null)
            };
        }

        public const float DPS_DEFAULT = 1f / (10f * 60f);

        public bool Approximately(SimDamageProfile otherWeaponDamage)
        {
            return Mathf.Approximately(this.damagePerSecondVsTypeA, otherWeaponDamage.damagePerSecondVsTypeA) &&
                   Mathf.Approximately(this.damagePerSecondVsTypeB, otherWeaponDamage.damagePerSecondVsTypeB) &&
                   Mathf.Approximately(this.damagePerSecondVsTypeC, otherWeaponDamage.damagePerSecondVsTypeC);
        }

        public static readonly SimDamageProfile DefaultDamageProfile1 = new SimDamageProfile
        {
            name = "Default 1",
            damagePerSecondVsTypeA = DPS_DEFAULT,
            damagePerSecondVsTypeB = DPS_DEFAULT / 15,
            damagePerSecondVsTypeC = 0
        };

        public static readonly SimDamageProfile DefaultDamageProfile2 = new SimDamageProfile
        {
            name = "Default 2",
            damagePerSecondVsTypeA = DPS_DEFAULT / 2,
            damagePerSecondVsTypeB = DPS_DEFAULT / 10,
            damagePerSecondVsTypeC = DPS_DEFAULT / 100
        };

        public static readonly SimDamageProfile DefaultDamageProfile3 = new SimDamageProfile
        {
            name = "Default 3",
            damagePerSecondVsTypeA = DPS_DEFAULT,
            damagePerSecondVsTypeB = DPS_DEFAULT,
            damagePerSecondVsTypeC = DPS_DEFAULT
        };
    }
}