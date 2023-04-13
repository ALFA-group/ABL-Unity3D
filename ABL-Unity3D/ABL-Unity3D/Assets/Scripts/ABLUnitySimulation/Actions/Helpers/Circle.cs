using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Utilities.GeneralCSharp;

#nullable enable

namespace ABLUnitySimulation.Actions.Helpers
{
    [HideReferenceObjectPicker]
    public class Circle
    {
        public readonly Vector2 center;
        public readonly float kmRadius;
        public readonly string? name;

        public Circle(Vector2 center, float kmRadius, string? name = null)
        {
            this.center = center;
            this.kmRadius = kmRadius;
            this.name = name;
        }

        public override int GetHashCode()
        {
            return GenericUtilities.CombineHashCodes(
                new []
                {
                    this.center.GetHashCode(),
                    this.kmRadius.GetHashCode(),
                    this.name?.GetHashCode() ?? 0
                }
            );
        }

        public bool Equals(Circle? other)
        {
            if (null == other) return false;
            return this.center == other.center && Math.Abs(this.kmRadius - other.kmRadius) < 0.00001 &&
                   this.name == other.name;
        }

        public bool IsInside(Vector2 queryPoint)
        {
            return (queryPoint - this.center).sqrMagnitude <= this.kmRadius * this.kmRadius;
        }

        public IEnumerable<Vector2> ToPolygon(int numPoints, float radiansRotation = 0)
        {
            if (numPoints < 2) throw new Exception($"Cannot have a polygon with {numPoints} points");

            float deltaAngle = Mathf.PI * 2.0f / numPoints;
            float angle = radiansRotation;
            for (var i = 0; i < numPoints; ++i, angle += deltaAngle)
            {
                var fromCenter = new Vector2(
                    this.kmRadius * Mathf.Cos(angle),
                    this.kmRadius * Mathf.Sin(angle));
                yield return this.center + fromCenter;
            }
        }

        public override string ToString()
        {
            return $"[Circle '{this.name}' at {this.center}:r{this.kmRadius}";
        }

        public Circle WithRadius(float circleKmRadius)
        {
            return new Circle(this.center, circleKmRadius);
        }

        public Circle WithCenter(Vector2 circleCenter)
        {
            return new Circle(circleCenter, this.kmRadius);
        }

        public static Circle GetCircleWithRectangleInscribedInIt(Rect r)
        {
            return new Circle(r.center, GenericUtilities.PythagoreanTheorem(r.width, r.height));
        }
    }
}