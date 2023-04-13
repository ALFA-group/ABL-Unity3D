using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions;
using ABLUnitySimulation.Actions.Helpers;
using JetBrains.Annotations;
using UnityEngine;
// ReSharper disable SuggestBaseTypeForParameterInConstructor
// ReSharper disable MemberCanBePrivate.Global

#nullable enable

namespace GP.ExecutableNodeTypes.ABLUnityActionTypes
{
    
    public class MoveToVector2 : GpBuildingBlock<SimAction?>
    {
        public MoveToVector2(
            GpBuildingBlock<Vector2> target,
            [Friendly] GpBuildingBlock<Handle<SimAgent>> friendly,
            GpBuildingBlock<float> closeEnough) : base(target, friendly, closeEnough)
        {
        }

        public GpBuildingBlock<Vector2> TargetLocation => (GpBuildingBlock<Vector2>)this.children[0];

        [Friendly] public GpBuildingBlock<Handle<SimAgent>> Friendly => (GpBuildingBlock<Handle<SimAgent>>)this.children[1];

        public GpBuildingBlock<float> CloseEnough => (GpBuildingBlock<float>)this.children[2];


        public override SimAction Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            var agent = gpFieldsWrapper.worldState.Get(this.Friendly.Evaluate(gpFieldsWrapper));

            var targetPosition = this.TargetLocation.Evaluate(gpFieldsWrapper);
            float kmCloseEnough = this.CloseEnough.Evaluate(gpFieldsWrapper);

            var moveToPosition = new ActionMoveToPositionWithPathfinding(agent, targetPosition, kmCloseEnough);
            return moveToPosition;
        }
    }

    public class MoveToSimAgent : GpBuildingBlock<SimAction?>
    {
        public MoveToSimAgent(
            [Enemy] GpBuildingBlock<Handle<SimAgent>> target,
            [Friendly] GpBuildingBlock<Handle<SimAgent>> friendly,
            GpBuildingBlock<float> closeEnough) : base(target, friendly, closeEnough)
        {
        }

        [Enemy]
        public GpBuildingBlock<Handle<SimAgent>> TargetEnemy =>
            (GpBuildingBlock<Handle<SimAgent>>)this.children[0];

        [Friendly] public GpBuildingBlock<Handle<SimAgent>> Friendly => (GpBuildingBlock<Handle<SimAgent>>)this.children[1];

        public GpBuildingBlock<float> CloseEnough => (GpBuildingBlock<float>)this.children[2];

        public override SimAction Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            var agent = gpFieldsWrapper.worldState.Get(this.Friendly.Evaluate(gpFieldsWrapper));
            var enemyAgent = this.TargetEnemy.Evaluate(gpFieldsWrapper);

            var target = new Circle(enemyAgent.Get(gpFieldsWrapper.worldState).GetObservedPosition((SimAgent)agent),
                this.CloseEnough.Evaluate(gpFieldsWrapper));

            var moveToPosition = new ActionMoveToPositionWithPathfinding(
                agent, target.center, this.CloseEnough.Evaluate(gpFieldsWrapper));
            return moveToPosition;
        }
    }

    public class AttackToDestroy : GpBuildingBlock<SimAction?>
    {
        public AttackToDestroy(
            [Enemy] GpBuildingBlock<Handle<SimAgent>> enemy,
            [Friendly] GpBuildingBlock<Handle<SimAgent>> friendly) : base(enemy, friendly)
        {
        }

        [Enemy] public GpBuildingBlock<Handle<SimAgent>> Enemy => (GpBuildingBlock<Handle<SimAgent>>)this.children[0];

        [Friendly] public GpBuildingBlock<Handle<SimAgent>> Friendly => (GpBuildingBlock<Handle<SimAgent>>)this.children[1];

        public override SimAction Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            var friendlyAgent = gpFieldsWrapper.worldState.Get(this.Friendly.Evaluate(gpFieldsWrapper));
            var enemyAgent = gpFieldsWrapper.worldState.Get(this.Enemy.Evaluate(gpFieldsWrapper));

            var action = new ActionAttackToDestroy
            {
                actors = new SimGroup(friendlyAgent),
                target = enemyAgent
            };

            return action;
        }
    }

    public class Sequence : GpBuildingBlock<SimAction?>
    {
        // [RandomTreeConstructor]
        
        public Sequence(GpBuildingBlock<SimAction?> s1, GpBuildingBlock<SimAction?> s2) : base(s1, s2)
        {
        }

        public override SimAction Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            var sequentialAction = new ActionSequential();

            foreach (var child in this.children.Cast<GpBuildingBlockClosed>())
            {
                object? val = child.EvaluateToObject(gpFieldsWrapper);
                if (val is SimAction subAction) sequentialAction.actionQueue?.Add(subAction);
            }

            return sequentialAction;
        }
    }

    public class Conditional : GpBuildingBlock<SimAction?>
    {
        public Conditional(
            GpBuildingBlock<bool> cond,
            GpBuildingBlock<SimAction?> trueBranch,
            GpBuildingBlock<SimAction?> falseBranch) : base(cond, trueBranch, falseBranch)
        {
        }

        public GpBuildingBlock<bool> Cond => (GpBuildingBlock<bool>)this.children[0];
        public GpBuildingBlock<SimAction?> TrueBranch => (GpBuildingBlock<SimAction?>)this.children[1];
        public GpBuildingBlock<SimAction?> FalseBranch => (GpBuildingBlock<SimAction?>)this.children[2];

        public override SimAction? Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this.Cond.Evaluate(gpFieldsWrapper)
                ? this.TrueBranch.Evaluate(gpFieldsWrapper)
                : this.FalseBranch.Evaluate(gpFieldsWrapper);
        }
    }

    [UsedImplicitly]
    public class NoOp : GpBuildingBlock<SimAction?>
    {
        public override SimAction? Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return null;
        }
    }
}