using System;
using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions;
using JetBrains.Annotations;
using Linefy;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Unity;

#nullable enable

namespace UI.ABLUnitySimulation
{
    [SelectionBase]
    public class SimAgentFollower : MonoBehaviour 
    {
        [ReadOnly, EnableGUI, UsedImplicitly] public long worldStateId;

        [Tooltip("Added height to keep agent above terrain")]
        public float extraHeight = 2f;

        [ShowInInspector, PropertyOrder(-10), 
         OnStateUpdate("@_busyStatus = CalculateBusyStatus()"), 
         FoldoutGroup("$BusyStatusTitle", GroupID = "Current Action")]
        private SimAction.BusyStatusReport _busyStatus;

        private LayerMask _layerMask;

        private Lines? _lines;

        [NonSerialized] public SimWorldState? myState;

        [NonSerialized, ShowInInspector, BoxGroup]
        public SimAgent? myAgent;

        [NonSerialized] public ShowSimulation? showSimulation;

        [UsedImplicitly] private string BusyStatusTitle => $"Action: {this._busyStatus.ToHumanReadableString()}";

        private void Awake()
        {
            this._layerMask = LayerMask.GetMask("TerrainVisuals");
            this._lines = new Lines(2);
        }

        protected void LateUpdate()
        {
            if (null == this.myAgent || null == this.showSimulation || null == this.myState) return;

            UpdatePosition(this, this.showSimulation.transform);

            if (this.myAgent.IsActive) return;
            
            if (null == this._lines) throw new Exception("Polyline is undefined.");
            var bounds = this.gameObject.GetComponent<MeshRenderer>().bounds;

            var min = bounds.min;
            min.y += 0.1f;
            var max = bounds.max;
            max.y += 0.1f;
            this._lines[0] = new Line(min, max, Color.red, 4);
            this._lines[1] = new Line(new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, min.z), Color.red,
                4);
            this._lines.Draw();
        }

        private void OnDrawGizmos()
        {
            if (null == this.myAgent || this.myAgent.IsActive) return;

            Gizmos.color = Color.red;
            var bounds = this.gameObject.GetComponent<MeshRenderer>().bounds;

            var min = bounds.min;
            var max = bounds.max;
            Gizmos.DrawLine(min, max);

            Gizmos.DrawLine(new Vector3(min.x, min.y, max.z),
                new Vector3(max.x, min.y, min.z)
            );
        }

        private void OnDrawGizmosSelected()
        {
            if (null == this.myState || null == this.myAgent) return;

            this.myState.actions.OnDrawGizmos(this.myState, this.myAgent, this.SimToUnityCoordsHelper);
        }


        [UsedImplicitly]
        private SimAction.BusyStatusReport CalculateBusyStatus()
        {
            if (null == this.myAgent) return SimAction.BusyStatusReport.NotBusy;
            if (null == this.myState) return SimAction.BusyStatusReport.NotBusy;

            var busyReport = this.myState.actions.IsBusy(this.myAgent, this.myState, true);
            return busyReport;
        }

        private static void UpdatePosition(SimAgentFollower follower, Transform parentTransform)
        {
            if (null == follower.myAgent) return;

            var drawingPosition = follower.SimToUnityCoordsHelper(follower.myAgent.positionActual);
            if (follower.transform.position != drawingPosition) follower.transform.position = drawingPosition;

            if (follower.transform.parent != parentTransform) follower.transform.parent = parentTransform;
        }

        public Vector3 SimToUnityCoordsHelper(Vector2 v)
        {
            return GeneralUnityUtilities.SimToUnityCoords(v, this.extraHeight, this._layerMask);
        }

        [Button]
        private void AddArtillery()
        {
            if (null == this.myAgent) return;

            var artilleryWeapon = new SimWeapon
            {
                kmMaxRange = 20,
                kmStrikeRadius = 1,
                damage = SimDamageProfile.DefaultDamageProfile2,
                isIndirect = true,
                name = "Artillery",
                secondsOfAmmo = 3600
            };

            var artilleryEntity = new SimEntity(new SimEntityData(
                kphMaxSpeed: 10,
                name: "FakeArtillery",
                weapons: new SimWeaponSet(artilleryWeapon)
            ));

            this.myAgent.entities.Add(artilleryEntity);

            if (null != this.myState && !this.myState.actions.actions.Any(ae =>
                    ae.subAction is ActionArtillery aa && aa.shootingTeam == this.myAgent.team))
            {
                var artilleryAction = new ActionArtillery
                {
                    shootingTeam = this.myAgent.team
                };

                this.myState.actions.Add(artilleryAction);
            }
        }
    }
}