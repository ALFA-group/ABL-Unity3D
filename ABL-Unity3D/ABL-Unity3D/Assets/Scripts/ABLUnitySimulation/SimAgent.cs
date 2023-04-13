using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ABLUnitySimulation.Actions.Helpers;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using Utilities.GeneralCSharp;

#nullable enable

namespace ABLUnitySimulation
{
    /// <summary>
    ///     A <see cref="SimAgent" /> is an agent that has a position within the simulator world space.
    ///     This is a higher level representation of agents than <see cref="SimEntity" />, used to represent
    ///     something like a platoon. A <see cref="SimAgent" /> is effectively a set of <see cref="SimEntity" />s.
    /// </summary>
    [Serializable]
    public class SimAgent : SimPositional
    {
        /// <summary>
        ///     List of default weapons that are used by agents.
        /// </summary>
        [SerializeField]
        public static List<SimWeapon> DefaultWeapons;

        /// <summary>
        ///     The team the agent is on.
        /// </summary>
        [SerializeField]
        public Team team = Team.Undefined;

        /// <summary>
        ///     The furthest distance in kilometers which this agent can see.
        /// </summary>
        [SerializeField]
        public float kmVisualRange = 2;

        /// <summary>
        ///     The fastest speed in kilometers this agent can move.
        /// </summary>
        [SerializeField]
        public float kphMaxSpeed = 30;

        /// <summary>
        ///     The angle at which a agent is moving.
        /// </summary>
        [SerializeField]
        public float mapHeadingInDegrees;

        /// <summary>
        ///     Sub entities that are contained within this <see cref="SimAgent" />.
        ///     We do not fully simulate these sub entities. Instead, this agent
        ///     has all the weapons and health the sub agents have. Essentially,
        ///     a <see cref="SimAgent" /> is an abstraction for this list of sub entities.
        /// </summary>
        [ShowInInspector] 
        [SerializeField]
        public List<SimEntity> entities = new List<SimEntity>();

        [SerializeField]
        protected bool forceCantMove;

        static SimAgent()
        {
            DefaultWeapons = new List<SimWeapon> { SimWeapon.defaultWeapon1 };
        }

        [JsonConstructor]
        public SimAgent(
            Team team,
            float kmVisualRange,
            float kphMaxSpeed,
            float mapHeadingInDegrees,
            List<SimEntity> entities,
            SimId id,
            string name) : base(id, name)
        {
            this.team = team;
            this.kmVisualRange = kmVisualRange;
            this.kphMaxSpeed = kphMaxSpeed;
            this.mapHeadingInDegrees = mapHeadingInDegrees;
            this.entities = entities;
        }

        protected SimAgent(bool addDefaultEntity)
        {
            if (addDefaultEntity) this.AddDefaultEntity();
        }

        public SimAgent() : base(new SimId(100), "test")
        {
            
        }

        public SimAgent(SimWorldState state, string name, bool addDefaultEntity = true) : base(state, name)
        {
            if (addDefaultEntity) this.AddDefaultEntity();
        }

        public SimAgent(SimId id, string name, bool addDefaultEntity = true) : base(id, name)
        {
            if (addDefaultEntity) this.AddDefaultEntity();
        }

        [ShowInInspector]
        public bool IsDestroyed => !this.entities.Any(entity => entity.IsActive);
        [ShowInInspector] public bool IsDamaged => this.entities.Any(e => e.damage > 0);

        [ShowInInspector] public bool CanFire => this.entities.Any(entity => entity.CanFire);

        [ShowInInspector] public bool IsActive => this.entities.Any(e => e.IsActive);

        [ShowInInspector]
        public bool CanMove
        {
            get => this.IsActive && !this.forceCantMove;
            set => this.forceCantMove = !value;
        }

        public Handle<SimAgent> Handle => new Handle<SimAgent>(this);
        

        public string GetHumanReadableEntityCounts()
        {
            var counts = this.GetEntityCounts();
            return string.Join(", ", counts.Select(kvp => $"{kvp.Value} {kvp.Key}"));
        }

        public bool IsWorthShooting()
        {
            return this.CalculateIsWorthShooting();
        }

        public bool HasTypeCAgent()
        {
            return this.entities.Any(entity => entity.data.simAgentType == SimAgentType.TypeC);
        }

        public bool HasArtillery()
        {
            return this.entities.Any(entity => entity.data.weapons.Any(w => w.IsArtillery));
        }

        /// <summary>
        ///     Returns whether this agent is worth shooting. We define a agent to be worth shooting
        ///     if it has any entities which can fire.
        /// </summary>
        /// <returns>Whether this agent is worth shooting.</returns>
        private bool CalculateIsWorthShooting()
        {
            foreach (var entity in this.entities)
            {
                if (!entity.CanFire) continue;
                return true;
            }

            return false;
        }

        private void AddDefaultEntity()
        {
            this.entities.Add(new SimEntity(new SimEntityData()));
        }

        public float TotalMaxHealth()
        {
            return this.entities
                .Select(entity => entity.MaxHealth)
                .DefaultIfEmpty(0)
                .Sum();
        }

        public float TotalCurrentHealth()
        {
            return this.entities
                .Select(entity => entity.CurrentHealth)
                .DefaultIfEmpty(0)
                .Sum();
        }

        public SimAgentType HighestPriorityType()
        {
            return this.GetHighestPriorityType();
        }

        private SimAgentType GetHighestPriorityType()
        {
            var highestPriorityTypeSoFar = SimAgentType.TypeA;

            foreach (var entity in this.entities)
                switch (entity.data.simAgentType)
                {
                    case SimAgentType.TypeC:
                        return SimAgentType.TypeC;
                    case SimAgentType.TypeB:
                        highestPriorityTypeSoFar = SimAgentType.TypeB;
                        break;
                    case SimAgentType.TypeD:
                        return SimAgentType.TypeD;
                        break;
                }

            return highestPriorityTypeSoFar;
        }

        public float KmMaxRange()
        {
            return this.CalculateMaxRanges().max;
        }

        public float KmSmallestMaxRange()
        {
            return this.CalculateMaxRanges().min;
        }

        /// <summary>
        ///     Calculate the smallest max range and the largest max range of all active weapons in the <see cref="SimAgent" />
        /// </summary>
        /// <returns>The smallest max range and the largest max range of all active weapons in the <see cref="SimAgent" />.</returns>
        private (float min, float max) CalculateMaxRanges()
        {
            var min = float.MaxValue;
            float max = 0;

            foreach (var entity in this.entities)
                if (entity.CanFire)
                {
                    float entityMax = entity.data.weapons.KmMaxRange();
                    if (entityMax > max) max = entityMax;
                    if (entityMax < min) min = entityMax;
                }

            return (min, max);
        }

        public bool IsNearActual(SimAgent otherAgent, float kmDistance)
        {
            return this.IsNearActual(otherAgent.positionActual, kmDistance);
        }

        public bool IsNearAccordingToMe(SimAgent otherAgent, float kmDistance)
        {
            var otherPosition = otherAgent.GetObservedPosition(this.team);
            return this.IsNearActual(otherPosition, kmDistance);
        }

        public bool IsNearActual(Vector2 targetPosition, float kmDistance)
        {
            return this.DistanceSqr2dActual(targetPosition) <= kmDistance * kmDistance;
        }

        public bool IsInsideActual(Circle circle)
        {
            return this.IsNearActual(circle.center, circle.kmRadius);
        }

        public float DistanceSqr2dActual(Vector2 vector2)
        {
            return (this.positionActual - vector2).sqrMagnitude;
        }

        public float DistanceSqr2dAccordingToMe(SimAgent target)
        {
            var position = target.GetObservedPosition(this.team);
            return this.DistanceSqr2dActual(position);
        }

        public float Distance2dAccordingToMe(SimAgent target)
        {
            return Mathf.Sqrt(this.DistanceSqr2dAccordingToMe(target));
        }

        public float Distance2dActual(Vector2 vector2)
        {
            return (this.positionActual - vector2).magnitude;
        }

        public float Distance2dActual(SimAgent knownAgent)
        {
            return this.Distance2dActual(knownAgent.positionActual);
        }

        public object GetAttributeValue(string attributeName)
        {
            return this.GetType()
                       .GetField(attributeName, BindingFlags.Instance | BindingFlags.NonPublic)?
                       .GetValue(this) ??
                   throw new MissingFieldException(nameof(SimAgent), attributeName);
        }

        public IEnumerable<string> GetAttributesOfType<T>()
        {
            var fields =
                this.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(f => f.FieldType == typeof(T))
                    .Select(f => f.Name);
            return fields;
        }

        public override string ToString()
        {
            return $"U[{this.team}#{this.SimId.id}-{this.Name}";
        }

        public bool IsHostile(SimAgent otherAgent)
        {
            return this.team != otherAgent.team && this.team != Team.Civilian;
        }

        public SimAgent DeepCopy()
        {
            var copy = (SimAgent)this.MemberwiseClone();
            copy.entities = this.entities.Select(e => e.DeepCopy()).ToList();
            return copy;
        }

        public void TakeAreaDamage(SimDamageProfile damageProfile, float numShooters, int dt)
        {
            
            // OR assume that artillery isn't so precise and be less clever with low dt
            foreach (var entity in this.entities)
                entity.damage += damageProfile.DpsVs(entity.data.simAgentType) * numShooters * entity.count;
        }

        /// <summary>
        ///     Creates a basic but fully usable <see cref="SimAgent" /> for testing.
        /// </summary>
        /// <param name="weaponRangeMultiplier">Multiply default weapon range by this number</param>
        /// <param name="addDefaultEntity"></param>
        /// <returns></returns>
        public static SimAgent Create(string name, Team team, Vector2 position, float weaponRangeMultiplier,
            bool addDefaultEntity = true)
        {
            var agent = new SimAgent(addDefaultEntity)
            {
                Name = name,
                team = team,
                positionActual = position
            };

            if (!addDefaultEntity) return agent;

            Debug.Assert(typeof(SimWeapon).IsValueType);
            var weapon = SimWeapon.defaultWeapon3; //SimWeapon.defaultWeapon1;
            weapon.kmMaxRange *= weaponRangeMultiplier;

            var defaultEntityData = agent.entities[0].data;
            var increasedRangeEntityData = new SimEntityData(
                defaultEntityData.name,
                defaultEntityData.simAgentType,
                defaultEntityData.isSimAgentTypeCertain,
                defaultEntityData.maxHealth,
                defaultEntityData.kphMaxSpeed,
                defaultEntityData.kmVisualRange,
                new SimWeaponSet(defaultEntityData.weapons.Skip(1).Append(weapon))
            );
            var increasedRangeEntity = new SimEntity(increasedRangeEntityData)
            {
                count = 10
            };
            agent.entities[0] = increasedRangeEntity;

            return agent;
        }

        public int GetEntityCount()
        {
            return this.entities.Sum(e => e.count);
        }

        public CounterDictionary<SimAgentType> GetEntityCounts()
        {
            var counts = new CounterDictionary<SimAgentType>();
            foreach (var entity in this.entities) counts.Add(entity.data.simAgentType, entity.count);

            return counts;
        }

        public string GetTotalHealthString()
        {
            float maxHealth = 0;
            float currentHealth = 0;
            foreach (var entity in this.entities)
            {
                maxHealth += entity.MaxHealth;
                currentHealth += entity.CurrentHealth;
            }

            return $"{currentHealth}/{maxHealth}";
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObservedPosition GetObservedPosition(SimAgent whoIsAsking)
        {
            return this.GetObservedPosition(whoIsAsking.team);
        }

        public ObservedPosition GetObservedPosition(Team whoIsAsking)
        {
            switch (whoIsAsking)
            {
                case Team.Red:
                    return this.positionObservedByRed;
                case Team.Blue:
                    return this.positionObservedByBlue;
                default:
                    Assert.IsTrue(false, $"Bad team asking for observed position: {whoIsAsking}");
                    return this.positionObservedByRed;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObservedPosition GetPositionObservedByEnemy()
        {
            return this.GetObservedPosition(this.team.GetEnemyTeam());
        }

        public void ForceKill()
        {
            foreach (var entity in this.entities) entity.damage = entity.MaxHealth;
        }

        public bool CanDamage(SimAgent target)
        {
            return this.entities.Any(entity => entity.CanDamage(target));
        }
    }
}