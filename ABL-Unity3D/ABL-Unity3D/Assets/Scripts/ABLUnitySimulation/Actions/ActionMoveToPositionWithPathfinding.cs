using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ABLUnitySimulation.Actions.Helpers;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Assertions;
using Utilities.GeneralCSharp;
using Utilities.Unity;

#nullable enable

namespace ABLUnitySimulation.Actions
{
    /// <summary>
    ///     Move to a given location using <see cref="PathfindingWrapper" />.
    /// </summary>
    [Serializable]
    public class ActionMoveToPositionWithPathfinding : SimActionPrimitive
    {
        private static readonly bool isPathfindingToPath = true;

        // Setting KmCloseEnoughToFinish to 0 may cause errors
        // To be safe, force it too be Mathf.Epsilon
        [SerializeField]
        private float _kmCloseEnoughToFinish;

        public Vector2 preferredDestination;
        [SerializeField]
        protected List<Segment2>? segments;
        public Func<SimWorldState, ActionMoveToPositionWithPathfinding, bool>? shouldCompleteBeforeFinishing;


        public ActionMoveToPositionWithPathfinding(Handle<SimAgent> oneActor, IEnumerable<Vector2> waypoints,
            Vector2 preferredDestination, float kmCloseEnoughToFinish)
        {
            this.actors = new SimGroup(oneActor);
            this.KmCloseEnoughToFinish = kmCloseEnoughToFinish;
            this.segments = ToSegments(waypoints);
            this.preferredDestination = preferredDestination;
        }

        public ActionMoveToPositionWithPathfinding(Handle<SimAgent> oneActor, Vector2 startLocation,
            Vector2 preferredDestination, float kmCloseEnoughToFinish)
        {
            var pathPoints =
                PathfindingWrapper.GetPathBlocking(startLocation, preferredDestination, CancellationToken.None);

            this.actors = new SimGroup(oneActor);
            this.KmCloseEnoughToFinish = kmCloseEnoughToFinish;
            this.segments = ToSegments(pathPoints, preferredDestination);
            this.preferredDestination = preferredDestination;
        }

        public ActionMoveToPositionWithPathfinding(Handle<SimAgent> oneActor, Vector2 preferredDestination,
            float kmCloseEnoughToFinish)
        {
            this.actors = new SimGroup(oneActor);
            this.KmCloseEnoughToFinish = kmCloseEnoughToFinish;
            this.preferredDestination = preferredDestination;
        }

        public ActionMoveToPositionWithPathfinding(SimGroup movers, Circle destination)
        {
            this.actors = movers;
            this.KmCloseEnoughToFinish = destination.kmRadius;
            this.preferredDestination = destination.center;
        }

        public ActionMoveToPositionWithPathfinding(Handle<SimAgent> oneActor, Vector2 startPosition, Circle destination)
            : this(oneActor, startPosition, destination.center, destination.kmRadius)
        {
        }

        public ActionMoveToPositionWithPathfinding(SimGroup movers, Vector2 startLocation, Vector2 preferredDestination,
            float kmCloseEnoughToFinish)
        {
            var pathPoints =
                PathfindingWrapper.GetPathBlocking(startLocation, preferredDestination, CancellationToken.None);

            this.actors = movers;
            this.KmCloseEnoughToFinish = kmCloseEnoughToFinish;
            this.segments = ToSegments(pathPoints, preferredDestination);
            this.preferredDestination = preferredDestination;
        }

        public float KmCloseEnoughToFinish
        {
            get => this._kmCloseEnoughToFinish;
            private set => this._kmCloseEnoughToFinish = value == 0 ? Mathf.Epsilon : value;
        }

        public override StatusReport GetStatus(SimWorldState state, bool useExpensiveExplanation)
        {
            this.segments ??= this.CalculateSegments(state);

            if (this.segments.Count < 1)
                return new StatusReport(ActionStatus.CompletedSuccessfully, "Path has no segments", this);

            if (this.shouldCompleteBeforeFinishing != null && this.shouldCompleteBeforeFinishing(state, this))
                return new StatusReport(ActionStatus.CompletedSuccessfully, "Triggered shouldCompleteBeforeFinishing",
                    this);

            var targetPosition = this.GetFinishPoint(state);

            var movers = state.GetGroupMembers(this.actors);
            foreach (var actor in movers)
                if (actor.CanMove && !actor.IsNearActual(targetPosition, this.KmCloseEnoughToFinish))
                {
                    var report = new StatusReport(ActionStatus.InProgress,
                        "Waiting for agents to arrive",
                        this
                    );
                    if (!useExpensiveExplanation) return report;

                    report.explanation +=
                        $"{actor.Name} at {actor.positionActual} is {actor.Distance2dActual(targetPosition)}/{this.KmCloseEnoughToFinish}km from {targetPosition}";
                    return report;
                }

            return new StatusReport(ActionStatus.CompletedSuccessfully, "All movable agents arrived", this);
        }

        public override void Execute(SimWorldState state)
        {
            this.segments ??= this.CalculateSegments(state);
            if (this.segments.Count < 1) return;

            var movers = this.actors.Get(state);
            movers.ForEach(a => this.ExecuteForActor(state, a));
        }

        private List<Segment2> CalculateSegments(SimWorldState state)
        {
            Debug.Assert(this.actors.Count >= 1);
            var paths = PathfindingWrapper.GetPathBlocking(this.actors.First().Get(state).positionActual,
                this.preferredDestination,
                CancellationToken.None);
            var newSegments = ToSegments(paths, this.preferredDestination);
            Assert.IsTrue(newSegments.Count >= 1);
            return newSegments;
        }

        public void ExecuteForActor(SimWorldState state, SimAgent actor)
        {
            if (!actor.CanMove) return;
            var finishPoint = this.GetFinishPoint(state);

            if (actor.IsNearActual(finishPoint, this.KmCloseEnoughToFinish)) return;

            float kmToTravel = state.HoursSinceLastUpdate * actor.kphMaxSpeed;
            var actorPosition = actor.positionActual;

            // Determine next waypoint by finding segment closest to actor!
            this.segments ??= this.CalculateSegments(state);
            int currentSegmentIndex = this.segments.IndexOfMin(s => s.DistanceToSqr(actorPosition));
            var currentSegment = this.segments[currentSegmentIndex];
            float kmDistanceToSegment = currentSegment.DistanceTo(actorPosition);
            if (isPathfindingToPath && kmDistanceToSegment > 0.1f)
            {
                // We're off the beaten path!  Pathfind to get back to it.
                var pointOnPath = actorPosition.ClosestPointOnSegment2(currentSegment);
                var toPath = PathfindingWrapper.GetPathBlocking(actor.positionActual,
                    pointOnPath,
                    CancellationToken.None);
                var pointOnPathDestination = new Circle(pointOnPath, 0.01f);
                var segmentsToPath = ToSegments(toPath, pointOnPath);
                ExecuteMove(state, actor, pointOnPathDestination, segmentsToPath, 0, ref kmToTravel);
                return;
            }

            var destination = new Circle(finishPoint, this.KmCloseEnoughToFinish);
            ExecuteMove(state, actor, destination, this.segments, currentSegmentIndex, ref kmToTravel);
        }

        private static void ExecuteMove(SimWorldState state, SimAgent actor, Circle stopInHere, List<Segment2> segments,
            int currentSegment,
            ref float remainingKmToTravel)
        {
            // We exit the above loop either when we run out of movement, 
            //  or when we reach the final waypoint,
            //  or both.
            var actorInitialPosition = actor.positionActual;

            while (remainingKmToTravel > 0)
            {
                if (currentSegment >= segments.Count)
                {
                    Assert.IsTrue(currentSegment < segments.Count);
                    return;
                }

                if (currentSegment < 0)
                {
                    Assert.IsTrue(false);
                    return;
                }

                var segment = segments[currentSegment];
                var nextWaypoint = segment.endB;
                var toNextWaypoint = nextWaypoint - actor.positionActual;
                float distanceToNextWaypoint = toNextWaypoint.magnitude;
                if (remainingKmToTravel < distanceToNextWaypoint)
                {
                    // We can't reach the end of the segment, so use up all our movement.
                    actor.positionActual += toNextWaypoint.normalized * remainingKmToTravel;
                    if (!state.areaOfOperations.Contains(actor.positionActual)) GenericUtilities.NoOp();
                    remainingKmToTravel = 0;
                    return;
                }

                // We made it to the end of the segment!
                actor.positionActual = nextWaypoint;
                remainingKmToTravel -= distanceToNextWaypoint;
                if (!state.areaOfOperations.Contains(actor.positionActual)) GenericUtilities.NoOp();

                if (stopInHere.IsInside(actor.positionActual))
                    // Close enough!
                    return;

                // We are ON a segment endpoint, but we are NOT inside the stop circle.
                // There must be more segments to travel.
                ++currentSegment;
            }
        }

        public Vector2 GetFinishPoint(SimWorldState state)
        {
            this.segments ??= this.CalculateSegments(state);
            if (this.segments.Count < 1) return this.preferredDestination;

            var finish = this.segments.Last();
            if (!finish.endB.IsApproximately(this.preferredDestination))
            {
                if (finish.endB.IsNear(this.preferredDestination, this.KmCloseEnoughToFinish))
                    return this.preferredDestination;

                GenericUtilities.NoOp();
            }

            return finish.endB;
        }

        public override string GetUsefulInspectorInformation(SimWorldState simWorldState)
        {
            var sb = new StringBuilder();

            this.segments ??= this.CalculateSegments(simWorldState);
            foreach (var segment in this.segments) sb.AppendLine($"{segment.endA} to {segment.endB}");

            return sb.ToString();
        }

        public IEnumerable<Segment2> EnumerateSubPathInUse(SimWorldState state)
        {
            this.segments ??= this.CalculateSegments(state);
            var used = new HashSet<Segment2>();

            foreach (var mover in this.actors.Get(state))
            {
                var closestSegment = this.segments.ArgMin(s => s.DistanceToSqr(mover.positionActual));
                used.Add(closestSegment);
            }

            return used;
        }

        public static List<Segment2> ToSegments(IEnumerable<Vector2> waypoints, Vector2? finalPoint = null)
        {
            var segments = new List<Segment2>();
            Vector2? lastWaypoint = null;

            if (finalPoint.HasValue) waypoints = waypoints.Append(finalPoint.Value);

            foreach (var waypoint in waypoints)
            {
                if (!lastWaypoint.HasValue)
                {
                    lastWaypoint = waypoint;
                    continue;
                }

                var segment = new Segment2
                {
                    endA = lastWaypoint.Value,
                    endB = waypoint
                };

                if (!segment.endA.IsApproximately(segment.endB)) segments.Add(segment);

                lastWaypoint = waypoint;
            }

            if (segments.Count < 1)
                if (lastWaypoint.HasValue)
                    // No segments, so add a zero length one, I guess.
                    segments.Add(new Segment2(lastWaypoint.Value, lastWaypoint.Value));

            Debug.Assert(segments.Count > 0);
            return segments;
        }


        public override void OnDrawGizmos(SimWorldState state, Handle<SimAgent> agentHandle,
            Func<Vector2, Vector3> simToUnityCoords)
        {
            if (!this.actors.Contains(agentHandle)) return;

            if (null == this.segments) return;
            if (this.segments.Count < 1) return;

            var agent = state.Get(agentHandle);
            if (!agent.CanMove) return;

            var darkGreen = Color.Lerp(Color.green, Color.black, .5f);
            //int minNextWaypointIndex = this.dNextWaypointIndex.Values.Max();

            foreach (var segment2 in this.segments)
            {
                //Gizmos.color = minNextWaypointIndex < index ? darkGreen : Color.green;
                Gizmos.color = darkGreen;
                Gizmos.DrawLine(simToUnityCoords(segment2.endA), simToUnityCoords(segment2.endB));
            }
        }

        public override void DrawIntentDestructive(SimWorldState throwawayState, IIntentDrawer drawer)
        {
            var actor = throwawayState.Get(this.actors).FirstOrDefault();
            if (null == actor) return;

            this.segments ??= this.CalculateSegments(throwawayState);

            if (this.segments.Count < 1) return;

            var points = this.segments.First().endA.ToEnumerable();
            points = points.Concat(this.segments.Select(s => s.endB));

            drawer.DrawPath(actor.team, points);

            foreach (var mover in this.actors.Get(throwawayState)) mover.positionActual = this.preferredDestination;
        }

        public override SimAction DeepCopy()
        {
            var copy = (ActionMoveToPositionWithPathfinding)this.MemberwiseClone();
            return copy;
        }

        public IReadOnlyList<Segment2> GetSegments(SimWorldState state)
        {
            if (null != this.segments) return this.segments;
            return this.CalculateSegments(state);
        }
    }
}