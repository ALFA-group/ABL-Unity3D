using System.Linq;
using ABLUnitySimulation;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Unity;

#nullable enable

namespace UI.ABLUnitySimulation
{
    public class SimAgentMover : MonoBehaviour
    {
        public RefSimWorldState? refSimWorldState;

        private void OnEnable()
        {
            if (null == this.refSimWorldState) this.refSimWorldState = FindObjectOfType<RefSimWorldState>();
        }

        [Button]
        public void ToMe(Team team = Team.Blue, SimAgentType simAgentType = SimAgentType.TypeC)
        {
            var state = RefSimWorldState.Fetch(this.refSimWorldState);
            if (null != state)
                foreach (var SimAgent in state.Agents)
                    if (SimAgent.team == team && SimAgent.entities.Any(entity => entity.data.simAgentType == simAgentType))
                        SimAgent.positionActual = this.transform.position.ToSimVector2();
        }
    }
}