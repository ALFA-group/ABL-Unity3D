using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Utilities.GeneralCSharp;
using Random = System.Random;

namespace Utilities.Unity
{
    public static class Extensions
    {
        public static Vector3 WithY(this Vector3 v, float newY)
        {
            return new Vector3(v.x, newY, v.z);
        }

        public static Vector2 NextVector2(this Random random)
        {
            return new Vector2(
                random.NextFloat(),
                random.NextFloat()
            );
        }

        public static Vector3 NextVector3(this Random random)
        {
            return new Vector3(
                random.NextFloat(),
                random.NextFloat(),
                random.NextFloat()
            );
        }

        public static Vector3 ToUnityVector3(this Vector2 simVector2)
        {
            return new Vector3(simVector2.x, 0, simVector2.y);
        }

        public static Vector2 ToSimVector2(this Vector3 unityVector3)
        {
            return new Vector2(unityVector3.x, unityVector3.z);
        }

        public static Vector2 ShiftVector2IntoRange(this Vector2 v, Vector2 min, Vector2 max)
        {
            for (var i = 0; i < 2; i++) v[i] = GenericUtilities.ShiftNumberIntoRange(v[i], min[i], max[i]);

            return v;
        }

        public static Vector3 Centroid(this IEnumerable<Vector3> vectors)
        {
            var i = 0;
            var sum = Vector3.zero;
            foreach (var v in vectors)
            {
                ++i;
                sum += v;
            }

            if (i == 0) return Vector3.zero;

            return sum / i;
        }

        public static Vector2 Centroid(this IEnumerable<Vector2> vectors)
        {
            var i = 0;
            Vector2 sum = Vector3.zero;
            foreach (var v in vectors)
            {
                ++i;
                sum += v;
            }

            if (i == 0) return Vector2.zero;

            return sum / i;
        }


        public static string GetGameObjectPath(this GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            path = path.Substring(1);
            return path;
        }

        public static bool IsNear(this Vector2 v, Vector2 v2, float kmDistance)
        {
            var delta = v - v2;
            return delta.sqrMagnitude <= kmDistance * kmDistance;
        }

        public static bool IsApproximately(this Vector2 a, Vector2 b)
        {
            return Mathf.Approximately(a.x, b.x)
                   && Mathf.Approximately(a.y, b.y);
        }

        public static bool IsApproximately(this double a, double other, double epsilon)
        {
            double delta = other - a;
            return epsilon >= Math.Abs(delta);
        }

        public static float DistanceToSegmentSquared(this Vector2 point, Vector2 endPointA, Vector2 endPointB)
        {
            // https://www.randygaul.net/2014/07/23/distance-point-to-line-segment/
            var ab = endPointB - endPointA;
            var pa = endPointA - point;

            if (endPointA == endPointB) return pa.sqrMagnitude;

            float c = Vector2.Dot(ab, pa);

            if (c > 0.0f)
                // ab and pa pointing in same direction, so p is off the line in the a direction
                // Closest point is a
                return pa.sqrMagnitude;

            var bp = point - endPointB;


            if (Vector2.Dot(ab, bp) > 0.0f)
                // ab and bp pointing in same direction, so p is off the line in the b direction
                // Closest point is b
                return bp.sqrMagnitude;

            // Closest point is between a and b
            var e = pa - ab * (c / ab.sqrMagnitude);

            return e.sqrMagnitude;
        }

        public static Vector2 ClosestPointOnSegment2(this Vector2 point, Segment2 segment)
        {
            return point.ClosestPointOnSegment2(segment.endA, segment.endB);
        }

        public static Vector2 ClosestPointOnSegment2(this Vector2 point, Vector2 endPointA, Vector2 endPointB)
        {
            // https://www.randygaul.net/2014/07/23/distance-point-to-line-segment/
            var ab = endPointB - endPointA;
            var pa = endPointA - point;

            if (endPointA == endPointB) return endPointA;

            float c = Vector2.Dot(ab, pa);

            if (c > 0.0f)
                // ab and pa pointing in same direction, so p is off the line in the a direction
                // Closest point is a
                return endPointA;

            var bp = point - endPointB;


            if (Vector2.Dot(ab, bp) > 0.0f)
                // ab and bp pointing in same direction, so p is off the line in the b direction
                // Closest point is b
                return endPointB;

            // Closest point is between a and b
            var pointOnBa = ab * (c / ab.sqrMagnitude);
            return endPointA - pointOnBa;
        }

        public static bool IsCoordinateInside(this Rect areaOfOperations, Vector2 coordinate)
        {
            return coordinate.x <= areaOfOperations.xMax &&
                   coordinate.x >= areaOfOperations.xMin &&
                   coordinate.y <= areaOfOperations.yMax &&
                   coordinate.y >= areaOfOperations.yMin;
        }

        public static IEnumerable<Vector2> EnumerateCorners(this Rect rect, bool alsoListFirstPointLast)
        {
            yield return rect.min;
            yield return new Vector2(rect.xMin, rect.yMax);
            yield return rect.max;
            yield return new Vector2(rect.xMax, rect.yMin);
            if (alsoListFirstPointLast) yield return rect.min;
        }

        /// <summary>
        ///     Not great clumping, N^2 speed
        /// </summary>
        /// <param name="unClumped"></param>
        /// <param name="mergeDistance"></param>
        /// <returns></returns>
        public static List<Vector2> ClumpByProximity(this List<Vector2> unClumped, float mergeDistance)
        {
            float d2 = mergeDistance * mergeDistance;
            var clumped = new List<Vector2>(unClumped.Count);
            foreach (var point in unClumped)
            {
                bool isNearClumpedPoint = clumped.Any(clumpedPoint => (clumpedPoint - point).sqrMagnitude <= d2);
                if (!isNearClumpedPoint) clumped.Add(point);
            }

            return clumped;
        }


        public static void CreateFoldersBasedOnGameObjectPath(this string gameObjectPath, string assetPath)
        {
            var hierarchyNames = gameObjectPath.Split('/');
            var currentFolderPath = assetPath;

            foreach (var folderName in hierarchyNames.Take(hierarchyNames.Length - 1))
            {
                currentFolderPath += $"/{folderName}";
                if (!Directory.Exists(currentFolderPath))
                {
                    Directory.CreateDirectory(currentFolderPath);
                }
            }
        }
    }
}