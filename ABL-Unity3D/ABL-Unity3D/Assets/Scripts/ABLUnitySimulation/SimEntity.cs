using System;
using System.Linq;
using UnityEngine;

#nullable enable

namespace ABLUnitySimulation
{
    /// <summary>
    ///     Data about a single group of sim entities.
    /// </summary>
    public class SimEntityData
    {
        public readonly bool isSimAgentTypeCertain;
        public readonly float kmVisualRange;
        public readonly float kphMaxSpeed;
        public readonly float maxHealth;
        public readonly string name;
        public readonly SimAgentType simAgentType;
        public readonly SimWeaponSet weapons;

        public SimEntityData(string name = "DEFAULT", SimAgentType simAgentType = SimAgentType.TypeA,
            bool isSimAgentTypeCertain = false, float maxHealth = 1, float kphMaxSpeed = 35, float kmVisualRange = 3,
            SimWeaponSet? weapons = null)
        {
            this.name = name;
            this.simAgentType = simAgentType;
            this.maxHealth = maxHealth;
            this.kphMaxSpeed = kphMaxSpeed;
            this.kmVisualRange = kmVisualRange;
            this.isSimAgentTypeCertain = isSimAgentTypeCertain;

            this.weapons = null == weapons
                ? new SimWeaponSet(SimWeapon.defaultWeapon1)
                : new SimWeaponSet(weapons);
        }

        public bool IsVerySimilarTo(SimEntityData other)
        {
            bool sameExceptWeapons = this.simAgentType == other.simAgentType &&
                                     Mathf.Approximately(this.maxHealth, other.maxHealth) &&
                                     Mathf.Approximately(this.kphMaxSpeed, other.kphMaxSpeed) &&
                                     Mathf.Approximately(this.kmVisualRange, other.kmVisualRange);

            if (!sameExceptWeapons) return false;

            if (this.weapons.Count != other.weapons.Count) return false;

            for (var index = 0; index < this.weapons.Count; index++)
                if (!this.weapons[index].IsVerySimilarTo(other.weapons[index]))
                    return false;

            return true;
        }

        public override string ToString()
        {
            return this.name;
        }
    }

    /// <summary>
    ///     A lower level representation of a simulated agent than <see cref="SimAgent" />.
    ///     This can be as low as a single soldier or vehicle, or multiple soldiers or multiple vehicles.
    /// </summary>
    public class SimEntity
    {
        
        public int count;
        
        public float damage;
        public SimEntityData data; 

        public SimEntity(SimEntityData data, int count = 1, float damage = 0)
        {
            this.count = count;
            this.damage = damage;
            this.data = data;
        }

        public float ActiveCount => this.count - this.damage / this.data.maxHealth;

        public bool IsActive => this.MaxHealth > this.damage;
        public float MaxHealth => this.count * this.data.maxHealth;
        public float CurrentHealth => Math.Max(this.data.maxHealth * this.count - this.damage, 0);
        public bool CanFire => this.IsActive && this.data.weapons.Count > 0;
        public bool HasWeapons => this.data.weapons.Count > 0;

        /// <summary>
        ///     Returns true if in range of target and firing.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool AttackDirect(SimWorldState state, SimAgent attacker, SimAgent target)
        {
            if (state.SecondsSinceLastUpdate < 1) return false;
            if (this.data.weapons.Count < 1) return false;
            if (target.GetObservedPosition(attacker).lastObservationTimestamp != state.SecondsElapsed) return false;
            if (!this.CanFire) return false;

            int dt = state.SecondsSinceLastUpdate;

            float kmDistanceToTarget = attacker.Distance2dActual(target);

            var damagePossibility = new DamagePossibility
            {
                target = null,
                dps = 0
            };

            var shotSomeone = false;

            while (dt > 0)
            {
                foreach (var entityTarget in target.entities)
                {
                    if (!entityTarget.IsActive) continue;

                    float bestDps =
                        this.data.weapons.CalculateBestDpsDirect(kmDistanceToTarget, entityTarget.data.simAgentType);

                    if (bestDps > damagePossibility.dps)
                    {
                        damagePossibility.dps = bestDps;
                        damagePossibility.target = entityTarget;
                    }
                }

                if (null == damagePossibility.target || damagePossibility.dps <= 0) return shotSomeone;
                float damageNeeded = damagePossibility.target.MaxHealth - damagePossibility.target.damage;

                // Even though we know the target to be IsActive, damageNeeded can end up zero.
                //  Possibly floating point error.
                if (damageNeeded <= 0) damageNeeded = 0.1f; // Shoot it again just to be sure.

                // We have a best attack choice.  Execute.
                float actualDps = this.ActiveCount * damagePossibility.dps;
                // float actualDps  = this.count * damagePossibility.dps;
                float maxDamage = actualDps * dt;
                Debug.Assert(damageNeeded > 0);

                if (maxDamage < damageNeeded)
                {
                    // Can't kill it in the time allowed.
                    damagePossibility.target.damage += maxDamage;
                    return true;
                }

                // We can kill it with time remaining.
                shotSomeone = true;
                damagePossibility.target.damage = damagePossibility.target.MaxHealth;

                float timeTaken = damageNeeded / actualDps;
                var newDt = (int)(dt - timeTaken);
                if (newDt >= dt) throw new Exception("dt not reduced in AttackDirect!");
                dt = newDt;
            }

            return shotSomeone;
        }

        public bool CanAttack(SimAgent attacker, SimAgent target)
        {
            if (this.data.weapons.Count < 1) return false;
            if (!this.IsActive) return false;

            float kmDistanceToTarget = attacker.Distance2dActual(target);
            foreach (var entityTarget in target.entities)
            {
                if (!entityTarget.IsActive) continue;

                foreach (var weapon in this.data.weapons)
                    if (weapon.CanDamage(entityTarget.data.simAgentType) && weapon.InRange(kmDistanceToTarget))
                        return true;
            }

            return false;
        }

        public bool CanDamage(SimAgent target)
        {
            if (!this.CanFire) return false;

            foreach (var entityTarget in target.entities)
            {
                if (!entityTarget.IsActive) continue;

                foreach (var weapon in this.data.weapons)
                    if (weapon.CanDamage(entityTarget.data.simAgentType))
                            return true;
            }

            return false;
        }

        public SimEntity DeepCopy()
        {
            return (SimEntity)this.MemberwiseClone();
        }

        public override string ToString()
        {
            return this.data.name;
        }

        private struct DamagePossibility
        {
            internal SimEntity? target;
            internal float dps;
        }
    }
}