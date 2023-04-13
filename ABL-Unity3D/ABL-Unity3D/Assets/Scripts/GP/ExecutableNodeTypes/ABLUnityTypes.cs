using System;
using System.Linq;
using System.Reflection;
using ABLUnitySimulation;
using Sirenix.Serialization;
using Utilities.GeneralCSharp;

#nullable enable

namespace GP.ExecutableNodeTypes.ABLUnityTypes
{
    public class BooleanAttribute : GpBuildingBlock<bool>
    {
        [OdinSerialize] private readonly MemberInfo _memberInfo;
        [OdinSerialize] private readonly Handle<SimAgent> _simAgent; 

        public BooleanAttribute(MemberInfo memberInfo, Handle<SimAgent> simAgent)
        {
            this._memberInfo = memberInfo;
            this._simAgent = simAgent;
            this.symbol = memberInfo.Name;
        }

        [RandomTreeConstructor]
        public BooleanAttribute(GpFieldsWrapper gpFieldsWrapper) : base(gpFieldsWrapper)
        {
            this._simAgent = gpFieldsWrapper.worldState.GetRandom<SimAgent>();
            this._memberInfo = gpFieldsWrapper.worldState.GetRandomMemberInfo<bool>();
            this.symbol = $"{this._simAgent.simId}.{this._memberInfo.Name}";
        }

        public override bool Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return gpFieldsWrapper.worldState.GetAttributeValueOfType<bool>(this._simAgent, this._memberInfo);
        }
    }

    public class SimAgentVariable : GpBuildingBlock<Handle<SimAgent>> 
    {
        public int argIndex;

        [RandomTreeConstructor]
        public SimAgentVariable(GpFieldsWrapper gpFieldsWrapper) : base(gpFieldsWrapper)
        {
            if (null == gpFieldsWrapper.positionalArguments) throw new Exception("Positional arguments is null");
            this.argIndex = gpFieldsWrapper.positionalArguments.PopNextIndex();
        }

        public SimAgentVariable(int argIndex)
        {
            this.argIndex = argIndex;
        }

        public override Handle<SimAgent> Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            if (null == gpFieldsWrapper.positionalArguments) throw new Exception("Positional arguments is null");
            // ReSharper disable once InvertIf
            if (gpFieldsWrapper.positionalArguments.MapToTypedArgument(this.argIndex, out Handle<SimAgent>? arg))
                if (arg.HasValue)
                    return arg.Value;

            return
                Handle<SimAgent>
                    .Invalid; 
            // this._agent ??= gpFieldsWrapper.positionalArguments.PopNextArgument<Handle<SimAgent>>();
            // return (Handle<SimAgent>)this._agent;
        }
    }

    [Friendly]
    public class SimAgentVariableFriendly : SimAgentVariable
    {
        [RandomTreeConstructor]
        public SimAgentVariableFriendly(GpFieldsWrapper gpFieldsWrapper) : base(gpFieldsWrapper)
        {
            if (gpFieldsWrapper.positionalArguments == null)
                throw new Exception("Positional arguments cannot be null with this genome type.");
            
            // This is a temp fix for filtering enemy vs friendly when getting positional arguments
            var SimAgentHandles =
                gpFieldsWrapper.positionalArguments.positionalArguments.Get<Handle<SimAgent>>().ToList();
            var SimAgents = SimAgentHandles.Select(gpFieldsWrapper.worldState.Get);
            var friendlies = SimAgents.Where(u => u.team == gpFieldsWrapper.simEvaluationParameters.friendlyTeam).ToList();
            var randomFriendly = friendlies.GetRandomEntry(gpFieldsWrapper.rand);
            this.argIndex = SimAgentHandles.IndexOf(randomFriendly);
            // var 
        }

        public SimAgentVariableFriendly(int argIndex) : base(argIndex)
        {
        }
    }

    [Enemy]
    public class SimAgentVariableEnemy : SimAgentVariable
    {
        [RandomTreeConstructor]
        public SimAgentVariableEnemy(GpFieldsWrapper gpFieldsWrapper) : base(gpFieldsWrapper)
        {
            if (null == gpFieldsWrapper.positionalArguments) throw new Exception("Positional arguments is null");
            
            
            var SimAgentHandles =
                gpFieldsWrapper.positionalArguments.positionalArguments.Get<Handle<SimAgent>>().ToList();
            var SimAgents = SimAgentHandles.Select(gpFieldsWrapper.worldState.Get);
            var enemies = SimAgents.Where(u => u.team == gpFieldsWrapper.simEvaluationParameters.enemyTeam).ToList();
            var randomEnemy = enemies.GetRandomEntry(gpFieldsWrapper.rand);
            this.argIndex = SimAgentHandles.IndexOf(randomEnemy);
        }

        public SimAgentVariableEnemy(int argIndex) : base(argIndex)
        {
        }
    }

    public class SimAgentConstant : GpBuildingBlock<Handle<SimAgent>>
    {
        [OdinSerialize] private readonly Handle<SimAgent> _simAgent;

        public SimAgentConstant(Handle<SimAgent> simAgent)
        {
            this._simAgent = simAgent;
            this.symbol += $" {simAgent.simId.ToString()}";
        }

        [RandomTreeConstructor]
        public SimAgentConstant(GpFieldsWrapper gpFieldsWrapper, Team? team) : base(gpFieldsWrapper)
        {
            this._simAgent = gpFieldsWrapper.worldState
                .GetAll<SimAgent>()
                .Where(u => team == null || u.team == team)
                .GetRandomEntry(gpFieldsWrapper.rand);
            this.symbol += $" {this._simAgent.simId.ToString()}";
        }

        public override Handle<SimAgent> Evaluate(GpFieldsWrapper gpFieldsWrapper)
        {
            return this._simAgent;
        }
    }

    [Friendly]
    public class SimAgentConstantFriendly : SimAgentConstant
    {
        [RandomTreeConstructor]
        public SimAgentConstantFriendly(GpFieldsWrapper gpFieldsWrapper) :
            base(gpFieldsWrapper, gpFieldsWrapper.simEvaluationParameters.friendlyTeam)
        {
        }

        public SimAgentConstantFriendly(Handle<SimAgent> simAgent) : base(simAgent)
        {
        }
    }

    [Enemy]
    public class SimAgentConstantEnemy : SimAgentConstant
    {
        [RandomTreeConstructor]
        public SimAgentConstantEnemy(GpFieldsWrapper gpFieldsWrapper) :
            base(gpFieldsWrapper, gpFieldsWrapper.simEvaluationParameters.enemyTeam)
        {
        }

        public SimAgentConstantEnemy(Handle<SimAgent> simAgent) : base(simAgent)
        {
        }
    }
}