using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Pathfinding;
using UnityEngine;
using UnityEngine.Assertions;

#nullable enable

namespace Utilities.Unity
{
    public static class PathfindingWrapper
    {
        private static readonly ConcurrentDictionary<(GraphNode, GraphNode), List<Vector3>> DPathCache =
            new ConcurrentDictionary<(GraphNode, GraphNode), List<Vector3>>();

        public static TimeSpan timeTaken = TimeSpan.Zero;

        
        private static readonly bool shouldCache = false;

        public static void ClearCache()
        {
            DPathCache.Clear();
        }

        public static IEnumerable<Vector2> GetPathBlocking(Vector2 start, Vector2 stop, CancellationToken cancel)
        {
            var startTime = DateTime.UtcNow;
            var unityVectors = GetPathBlocking(start.ToUnityVector3(), stop.ToUnityVector3(), cancel);
            timeTaken += DateTime.UtcNow - startTime;
            return unityVectors.Select(v3 => v3.ToSimVector2());
        }

        private static IEnumerable<Vector3> GetPathBlocking(Vector3 start, Vector3 stop, CancellationToken cancel)
        {
            // AStar singleton can be null if we are running in a thread when Unity leaves play mode.
            if (!AstarPath.active) return Enumerable.Empty<Vector3>();
            AstarPath.active.logPathResults = PathLog.None;

            // Check if the path has already been cached
            var nearestStartNode = AstarPath.active.GetNearest(start).node;
            var nearestEndNode = AstarPath.active.GetNearest(stop).node;

            if (nearestStartNode == nearestEndNode)
                // Same node, no further pathfinding needed.
                return new[] { start, stop };

            var key = (nearestStartNode, nearestEndNode);

            if (shouldCache && DPathCache.TryGetValue(key, out var cachedPath))
            {
                // The cached path will likely have the wrong start/stop points.
                // However, they will be in the same graph nodes.
                var copy = cachedPath.ToList();
                Assert.IsTrue(copy.Count >= 2);
                copy[0] = start;
                copy[copy.Count - 1] = stop;
                return copy;
            }

            var path = ABPath.Construct(start, stop);
            AstarPath.StartPath(path);

            if (PlayerLoopHelper.IsMainThread)
                AstarPath.BlockUntilCalculated(path);
            else
                do
                {
                    UniTask.Yield(cancel);
                    if (cancel.IsCancellationRequested) return Enumerable.Empty<Vector3>();
                    if (!AstarPath.active) return Enumerable.Empty<Vector3>();
                } while (!path.IsDone());

            if (!path.IsDone() || path.error) return Enumerable.Empty<Vector3>();

            var points = new List<Vector3>();
            if (path.path.Count < 3)
            {
                // Only 1 or 2 nodes involved.  The path is clear; skip smoothing.
                points.Add(start);
                points.Add(stop);
            }
            else
            {
                var richPath = new RichPath();
                richPath.Initialize(null, path, true, true);

                richPath.GetRemainingPath(points, start, out bool _);
                points.Add(path.endPoint);
            }

            if (shouldCache) DPathCache[key] = points;

            return points;
        }
    }
}