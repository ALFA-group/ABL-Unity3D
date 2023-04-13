using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions;
using ABLUnitySimulation.Actions.Helpers;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Planner;
using Planner.ManyWorlds;
using Planner.Methods;
using Sirenix.Serialization;
using Utilities.GeneralCSharp;

#nullable enable

namespace GP.ExecutableNodeTypes.GpPlanner
{
    public readonly struct Prong
    {
        public readonly Circle value;

        public Prong(Circle value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public bool Equals(Prong? other)
        {
            return this.value.Equals(other?.value);
        }
    }
    
    
    public readonly struct GoalWaypoint
    {
        public readonly Circle value;

        public GoalWaypoint(Circle value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public bool Equals(Prong? other)
        {
            return this.value.Equals(other?.value);
        }
    }
    

    public class ProngConstant : GpBuildingBlock<Prong>
    {
        [OdinSerialize] private readonly Prong _value;

        public ProngConstant(Circle value)
        {
            this._value = new Prong(value);
            this.symbol = this._value.ToString();
        }

        [RandomTreeConstructor]
        public ProngConstant(GpFieldsWrapper gpFieldsWrapper) : base(gpFieldsWrapper)
        {
            if (null == gpFieldsWrapper.plannerWrapper)
            {
                throw new Exception("PlannerWrapper cannot be null.");
            }
            if (null == gpFieldsWrapper.plannerWrapper.manyWorldsPlannerRunner.plannerParameters.pureData.waypointOptionsAsCirclesCache)
                throw new Exception("ManyWorldsPlannerRunner.WaypointsAsCircles cannot be null");
            var randomEntry =
                gpFieldsWrapper.plannerWrapper.manyWorldsPlannerRunner.plannerParameters.pureData
                    .waypointOptionsAsCirclesCache?.GetRandomEntry(gpFieldsWrapper.rand) ?? throw new InvalidOperationException();
            this._value = new Prong(randomEntry);
            this.symbol = this._value.ToString();
        }

        public override Prong Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this._value;
        }

        public override int GetHashCode()
        {
            return this._value.GetHashCode();
        }
    }
    
    
    public class GoalWaypointConstant : GpBuildingBlock<GoalWaypoint>
    {
        [OdinSerialize] private readonly GoalWaypoint _value;

        public GoalWaypointConstant(Circle value)
        {
            this._value = new GoalWaypoint(value);
            this.symbol = this._value.ToString();
        }

        [RandomTreeConstructor]
        public GoalWaypointConstant(GpFieldsWrapper gpFieldsWrapper) : base(gpFieldsWrapper)
        {
            if (null == gpFieldsWrapper.plannerWrapper)
            {
                throw new Exception("PlannerWrapper cannot be null. This is likely because ");
            }
            if (null == gpFieldsWrapper.plannerWrapper.manyWorldsPlannerRunner.plannerParameters.pureData.waypointOptionsAsCirclesCache)
                throw new Exception("ManyWorldsPlannerRunner.WaypointsAsCircles cannot be null");
            var randomEntry =
                gpFieldsWrapper.plannerWrapper.manyWorldsPlannerRunner.plannerParameters.pureData
                    .waypointOptionsAsCirclesCache?.GetRandomEntry(gpFieldsWrapper.rand) ?? throw new NullReferenceException();
            this._value = new GoalWaypoint(randomEntry);
            this.symbol = this._value.ToString();
        }

        public override GoalWaypoint Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this._value;
        }

        public override int GetHashCode()
        {
            return this._value.GetHashCode();
        }
    }

    
    [UsedImplicitly]
    public class NProngsSimAction : GpBuildingBlock<SimAction>
    {
        public override bool DoesChildrenOrderMatter => false;

        public GpBuildingBlock<Prong> Prong1 => (GpBuildingBlock<Prong>)this.children[0];
        public GpBuildingBlock<Prong> Prong2 => (GpBuildingBlock<Prong>)this.children[1];
        public GpBuildingBlock<Prong> Prong3 => (GpBuildingBlock<Prong>)this.children[2];

        public GpBuildingBlock<Prong>? Prong4
        {
            get
            {
                if (this.children.Count == 4)
                    return (GpBuildingBlock<Prong>)this.children[3];
                return null;
            }
        }

        public NProngsSimAction(GpBuildingBlock<Prong> prong1, GpBuildingBlock<Prong> prong2, GpBuildingBlock<Prong> prong3,
            GpBuildingBlock<Prong> prong4)
            : base(prong1, prong2, prong3, prong4)
        {
        }

        public override SimAction Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            var prongsPreEvaluation = new List<GpBuildingBlock<Prong>> { this.Prong1, this.Prong2, this.Prong3 };
            if (null != this.Prong4) prongsPreEvaluation.Add(this.Prong4);
            var prongs = //new List<GpBuildingBlock<Prong>> { this.Prong1, this.Prong2, this.Prong3, this.Prong4 }
                prongsPreEvaluation
                .Select(p => p.Evaluate(gpFieldsWrapper).value).ToList();

            var friendlyAgents = gpFieldsWrapper.worldState.GetTeamHandles(gpFieldsWrapper.worldState.teamFriendly).ToList();
            var groups = PlannerGoalBuilder.AssignToGroups(
                friendlyAgents, prongs.Count(), gpFieldsWrapper.worldState);

            var allClears = new ActionParallel();
            for (int i = 0; i < prongs.Count(); i++)
            {
                var circleSpec = new SpecClearCircleWith()
                {
                    circle = prongs[i],
                    friendlies = groups[i],
                    occupyCircle = true
                };

                var method = new MethodClearCircleOfKnownEnemiesWith(circleSpec);
                var action = method.GetActionForSim(gpFieldsWrapper.worldState);
                allClears.Add(action);
            }
            
            if (null == gpFieldsWrapper.plannerWrapper) throw new Exception("Planner Wrapper is null");
                
            if (null == gpFieldsWrapper.plannerWrapper.manyWorldsPlannerRunner.plannerParameters.pureData.goalWaypointAsCircleCache) 
                throw new Exception("goalWaypointAsCircleCache is null");

            
            var attackCity = new ActionClearCircle(
                friendlyAgents, 
                gpFieldsWrapper.worldState.teamFriendly, 
                gpFieldsWrapper.worldState.TeamEnemy,
                gpFieldsWrapper.plannerWrapper.manyWorldsPlannerRunner.plannerParameters.pureData.goalWaypointAsCircleCache ?? throw new InvalidOperationException(), 
                true);

            var seq = new ActionSequential(allClears, attackCity);

            return seq;
        }
    }

    [UsedImplicitly]
    public class NProngsPlan : GpBuildingBlock<UniTask<Plan>>
    {
        public override bool DoesChildrenOrderMatter => false;

        public GpBuildingBlock<Prong> Prong1 => (GpBuildingBlock<Prong>)this.children[0];
        public GpBuildingBlock<Prong> Prong2 => (GpBuildingBlock<Prong>)this.children[1];
        public GpBuildingBlock<Prong> Prong3 => (GpBuildingBlock<Prong>)this.children[2];
        public GpBuildingBlock<Prong> Prong4 => (GpBuildingBlock<Prong>)this.children[3];

        public NProngsPlan(GpBuildingBlock<Prong> prong1, GpBuildingBlock<Prong> prong2, GpBuildingBlock<Prong> prong3,
            GpBuildingBlock<Prong> prong4) :
            base(prong1, prong2, prong3, prong4)
        {
        }


        public override async UniTask<Plan> Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            var prongs = new List<GpBuildingBlock<Prong>> { this.Prong1, this.Prong2, this.Prong3, this.Prong4 }
                .Select(p => p.Evaluate(gpFieldsWrapper).value).ToList();

            if (null == gpFieldsWrapper.plannerWrapper?.manyWorldsPlannerRunner.plannerParameters.pureData.goalWaypointAsCircleCache)
            {
                throw new Exception("ManyWorldsPlannerRunner.goalWaypointAsCircleCache can not be null");
            }

            var goalMethod = PlannerGoalBuilder.MakeNProngGoal(
                gpFieldsWrapper.worldState,
                gpFieldsWrapper.plannerWrapper.manyWorldsPlannerRunner.plannerParameters.pureData.numProngs,
                gpFieldsWrapper.plannerWrapper.manyWorldsPlannerRunner.plannerParameters.pureData.goalWaypointAsCircleCache ?? throw new InvalidOperationException(),
                prongs,
                gpFieldsWrapper.plannerWrapper.manyWorldsPlannerRunner.plannerParameters.pureData.activeTeam);

            
            var bestPlan = await gpFieldsWrapper.plannerWrapper.manyWorldsPlannerRunner.GetBestPlan(
                gpFieldsWrapper.simEvaluationParameters?.primaryScoringFunction ??
                throw new InvalidOperationException(
                    $"{nameof(gpFieldsWrapper.simEvaluationParameters.primaryScoringFunction)} is null"),
                gpFieldsWrapper.timeoutInfo.cancelTokenSource.Token,
                goalMethod,
                shouldLogDebugInformation: gpFieldsWrapper.verbose,
                gpFieldsWrapper.timeoutInfo);

            return bestPlan;
        }
    }
}