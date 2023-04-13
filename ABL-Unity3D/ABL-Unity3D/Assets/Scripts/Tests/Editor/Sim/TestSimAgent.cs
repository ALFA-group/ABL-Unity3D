using ABLUnitySimulation;
using NUnit.Framework;

namespace Tests.Editor.Sim
{
    public class TestSimAgent
    {
        [Test]
        public void TestInit()
        {
            var state = new SimWorldState(SimWorldState.PathProviderNoObstacles);
            var agent = new SimAgent(state, "testMe");
            state.Add(agent);

            Assert.That(agent.IsActive, "agent.IsActive");
            Assert.That(!agent.IsDestroyed, "!agent.IsDestroyed");
            Assert.Positive(agent.SimId.id, "SimId");
            Assert.That(agent.Handle.IsValid, "Valid handle");
            Assert.That(agent.CanMove, "agent.CanMove");
            Assert.That(agent.CanFire, "agent.CanFire");
        }
    }
}