using System;
using System.Collections.Generic;
using System.Linq;
using ABLUnitySimulation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.ABLUnitySimulation
{
    public class ShowSimulation : MonoBehaviour
    {
        public RefSimWorldState stateRef;

        [Header("Visualization")]
        public SimAgentFollower redPrefab; 

        public SimAgentFollower bluePrefab;

        private List<SimAgentFollower> _followersBlue = new List<SimAgentFollower>();

        private List<SimAgentFollower> _followersRed = new List<SimAgentFollower>();


        private void Start()
        {
            if (!this.redPrefab || !this.bluePrefab) Debug.LogWarning("Missing prefab in ShowSimulation", this);
        }

        protected void LateUpdate()
        {
            var state = RefSimWorldState.Fetch(this.stateRef);
            this.DrawSimulation(state);
        }

        private SimAgentFollower MakeFollower(SimAgent agent, SimAgentFollower prefab)
        {
            var follower = Instantiate(prefab, this.transform);
            follower.myAgent = agent;
            return follower;
        }

        private void DrawSimulation(SimWorldState state)
        {
            if (null == state)
            {
                this.DestroyAllFollowers();
                return;
            }

            this.Draw(state, state.GetTeam(Team.Blue), this._followersBlue, this.bluePrefab);
            this.Draw(state, state.GetTeam(Team.Red), this._followersRed, this.redPrefab);
        }

        private void Draw(SimWorldState state, IEnumerable<SimAgent> agents, IList<SimAgentFollower> currentFollowers,
            SimAgentFollower prefab)
        {
            // Remove excess followers
            var simAgents = agents as SimAgent[] ?? agents.ToArray();
            int numAgents = simAgents.Length;
            while (currentFollowers.Count > numAgents)
            {
                int last = currentFollowers.Count - 1;
                var simAgentFollower = currentFollowers[last];

                Destroy(simAgentFollower.gameObject);
                currentFollowers.RemoveAt(last);
            }

            if (!prefab) return;

            // Add missing followers
            var currentUnitIndex = 0;
            foreach (var agent in simAgents)
            {
                if (currentUnitIndex >= currentFollowers.Count) currentFollowers.Add(this.MakeFollower(agent, prefab));

                var simAgentFollower = currentFollowers[currentUnitIndex];
                simAgentFollower.showSimulation = this;
                simAgentFollower.myAgent = agent;
                simAgentFollower.worldStateId = state.WorldStateId;
                simAgentFollower.gameObject.name = agent.ToString();
                simAgentFollower.myState = state;

                ++currentUnitIndex;
            }
        }

        private void DestroyAllFollowers()
        {
            var destroy = new Action<SimAgentFollower>(f => Destroy(f.gameObject));

            this._followersBlue.ForEach(destroy);
            this._followersBlue.Clear();

            this._followersRed.ForEach(destroy);
            this._followersRed.Clear();
        }
    }
}