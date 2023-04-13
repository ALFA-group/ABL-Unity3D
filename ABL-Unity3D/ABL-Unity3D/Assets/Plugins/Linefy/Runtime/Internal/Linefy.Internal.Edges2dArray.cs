using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Linefy.Internal {
    public class Edges2DArray {

        int _length;
        public int length {
            get {
                return _length;
            }
        }

        Edge2D[] edges;

        public Edges2DArray(int count) {
            edges = new Edge2D[count];
            _length = count;
        }

        public void SetEdge(int index, Vector2 a, Vector2 b) {
            edges[index] = new Edge2D(a, b);
        }

        public float GetDistanceToPoint(ref Vector2 nearestA, ref Vector2 nearestB, Vector2 point) {
            float minDist = float.MaxValue;
            for (int i = 0; i < edges.Length; i++) {

                float d = edges[i].GetDistance(point);

                if (d < minDist) {
                    minDist = d;
                    nearestA = edges[i].a;
                    nearestB = edges[i].b;
                }
            }
            return minDist;
        }

        public float GetDistanceToPoint(Vector2 point) {
            float minDist = float.MaxValue;
            for (int i = 0; i < edges.Length; i++) {
                float d = edges[i].GetDistance(point);
                if (d < minDist) {
                    minDist = d;
                }
            }
            return minDist;
        }
    }

}
