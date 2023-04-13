using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Unity
{
    public struct Segment2
    {
        public Vector2 endA;
        public Vector2 endB;


        public Segment2(Vector2 endA, Vector2 endB)
        {
            this.endA = endA;
            this.endB = endB;
        }

        public float Length => (this.endA - this.endB).magnitude;

        public float DistanceToSqr(Vector2 point)
        {
            return point.DistanceToSegmentSquared(this.endA, this.endB);
        }

        public float DistanceTo(Vector2 point)
        {
            return Mathf.Sqrt(this.DistanceToSqr(point));
        }


        public IEnumerable<Vector2> EnumeratePoints()
        {
            yield return this.endA;
            yield return this.endB;
        }
    }
}