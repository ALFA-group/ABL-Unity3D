#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions.Helpers;
using Planner.Methods;
using UnityEngine;

namespace Planner.ManyWorlds
{
    public static class PlannerGoalBuilder
    {
        
        public static Method MakeGoal(SimWorldState simWorldState, PlannerParameters plannerParameters, 
            Circle? goalCircle = null, IEnumerable<Circle>? waypointOptions = null)
        {
            // First process goals which don't require a goal circle and waypoint options
            
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            if (plannerParameters.pureData.goal == PlannerParameters.PlannerGoal.ClearAllEnemies)
            { 
                return MakeClearAllEnemiesGoal(simWorldState, plannerParameters.pureData.activeTeam);
            }

            // Now make sure `goalWaypointAsCircleCache` and `waypointOptions` are not null,
            // because they are required for the following goals.
            
            if (null == goalCircle)
            {
                throw new Exception($" {nameof(goalCircle)} can not be null if planner goal is NProngs ");
            }
            if (null == waypointOptions)
            {
                throw new Exception($" {nameof(waypointOptions)} can not be null if planner goal is NProngs");
            }

            var waypointOptionsAsList = waypointOptions.ToList();

            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            return plannerParameters.pureData.goal switch
            {
                PlannerParameters.PlannerGoal.AttackCircle => 
                    MakeAttackMainCircleGoal(plannerParameters.pureData.activeTeam, goalCircle, waypointOptionsAsList),
                PlannerParameters.PlannerGoal.NProngs =>
                    MakeNProngGoal(simWorldState, plannerParameters.pureData.numProngs, goalCircle, 
                        waypointOptionsAsList, plannerParameters.pureData.activeTeam),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public static Method MakeNProngGoal(SimWorldState state, int nProngs, Circle goalCircle, List<Circle> waypointOptions, Team activeTeam)
        {
            GetSimAgents(state, out var friendlies, out var enemies, activeTeam);
            if (null == friendlies || null == enemies) throw new Exception("No valid sim agents found");

            friendlies = friendlies.ToList();
            
            if (waypointOptions.Count < nProngs) throw new Exception($"Not enough circles (need {nProngs})");

            var groups = AssignToGroups(friendlies, nProngs, state);

            var doAllProngs = new MethodParallel();

            foreach (var group in groups)
            {
                var methodChooseOne = new MethodChooseOne();

                foreach (var waypoint in waypointOptions)
                {
                    var specClearWaypoint = new SpecClearCircleWith
                    {
                        circle = waypoint,
                        friendlies = group,
                        occupyCircle = true
                    };

                    var methodClearWaypoint = new MethodClearCircleOfKnownEnemiesWithTogether(specClearWaypoint);
                    methodChooseOne.optionalMethods.Add(methodClearWaypoint);
                }

                var specClearMainCircle = new SpecClearCircleWith
                {
                    friendlies = group,
                    circle = goalCircle,
                    occupyCircle = true
                };
                var methodClearMainCircle = new MethodClearCircleOfKnownEnemiesWith(specClearMainCircle);

                var specGetInClose = specClearMainCircle;
                specGetInClose.circle = new Circle(specGetInClose.circle.center, 2);
                var methodGetInClose = new MethodClearCircleOfKnownEnemiesWith(specGetInClose);

                var methodSequential = new MethodDoSequentially
                {
                    notes = string.IsNullOrWhiteSpace(group.Name) ? "Prong" : group.Name,
                    sequentialMethods = new List<Method> { methodChooseOne, methodClearMainCircle, methodGetInClose }
                };

                doAllProngs.methods.Add(methodSequential);
            }

            return doAllProngs;
        }
        

        private static Method MakeAttackMainCircleGoal(Team activeTeam, Circle goalCircle, List<Circle> waypointOptions)
        {
            var spec = new SpecClearCircle
            {
                circle = goalCircle,
                friendlyTeam = activeTeam
            };

            return spec.Any();
        }

        private static Method MakeClearAllEnemiesGoal(SimWorldState state, Team activeTeam)
        {
            var spec = new SpecClearCircle
            {
                circle = Circle.GetCircleWithRectangleInscribedInIt(state.areaOfOperations),
                friendlyTeam = activeTeam
            };

            return spec.Any();
        }
        
        
        private static MethodAny<SpecAttack> MakeAnyAttackGoal(SimWorldState simWorldState, Team activeTeam)
        {
            GetSimAgents(simWorldState, out var friendlies, out var enemies, activeTeam);

            if (friendlies == null) throw new Exception("Cannot create attack parameters.goal without friendly agent");

            if (enemies == null) throw new Exception("Cannot create attack parameters.goal without enemy agent");

            var spec = new SpecAttack
            {
                attackers = friendlies.ToSimGroup(),
                target = enemies.First()
            };

            return spec.Any();
        }

        private static MethodAny<SpecMoveToPoint> MakeMoveAndDoNotAttackMainWaypointGoal(SimWorldState simWorldState, Team activeTeam)
        {
            GetSimAgents(simWorldState, out var friendlies, out var enemies, activeTeam);

            if (friendlies is null) throw new Exception("Cannot create move parameters.goal without friendly agent");

            if (enemies is null)
                throw new Exception("Cannot create move (to enemy) parameters.goal without enemy agent");

            var enemyDestination = simWorldState.Get(enemies.First()).GetPositionObservedByEnemy();

            var spec = new SpecMoveToPoint
            {
                movers = friendlies.ToSimGroup(),
                destination = new Circle(enemyDestination + Vector2.one * 0.1f, 0.1f)
            };

            return spec.Any();
        }
        
        
        /// <summary>
        ///     Returned enumerables are null iff they would have been empty.
        /// </summary>
        /// <param name="simWorldState"></param>
        /// <param name="friendlies"></param>
        /// <param name="enemies"></param>
        public static void GetSimAgents(SimWorldState simWorldState, out IEnumerable<Handle<SimAgent>>? friendlies,
            out IEnumerable<Handle<SimAgent>>? enemies, Team activeTeam)
        {
            friendlies = simWorldState.GetTeamHandles(activeTeam);
            enemies = simWorldState.GetTeamHandles(simWorldState.TeamEnemy);
            if (!enemies.Any()) enemies = null;
        }

        
        public static List<SimGroup> AssignToGroups(IEnumerable<Handle<SimAgent>> agents, int numGroups, SimWorldState simWorldState)
        {
            if (numGroups < 1) throw new ArgumentException("At least one group required", nameof(numGroups));

            if (numGroups == 1) return new List<SimGroup> { new SimGroup(agents) };

            var agentList = agents.ToList();
            int agentsPerProng = agentList.Count / numGroups;
            if (agentsPerProng < 1)
                throw new Exception($"Not enough agents {agentList.Count} for {numGroups} groups.");

            var type1Agents = agentList.Where(u => simWorldState.Get(u).HighestPriorityType() == SimAgentType.TypeB).ToList();
            var type2Agents = agentList.Where(u => simWorldState.Get(u).HighestPriorityType()  == SimAgentType.TypeC).ToList();
            var type3Agents = agentList.Where(u => simWorldState.Get(u).HighestPriorityType()  == SimAgentType.TypeA).ToList();
            var type4Agents = agentList.Where(u => simWorldState.Get(u).HighestPriorityType()  == SimAgentType.TypeD).ToList();
            
            var groups = new List<SimGroup>(numGroups)
            {
                new SimGroup(type1Agents),
                new SimGroup(type2Agents),
                new SimGroup(type3Agents),
                new SimGroup(type4Agents)
            };

            return groups;
        }




    }
}