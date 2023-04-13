using System.Linq;
using ABLUnitySimulation.Actions.Helpers;
using NUnit.Framework;
using UnityEngine;
using Range = NUnit.Framework.RangeAttribute;


namespace Tests.Editor.Utility
{
    public static class TestCircle
    {
        [Test]
        public static void CheckPolygon([NUnit.Framework.Range(.1f, 2.1f, .2f)] float kmRadius,
            [NUnit.Framework.Range(2, 9)] int numPoints)
        {
            var centers = new[]
            {
                Vector2.down, Vector2.one, Vector2.zero, new Vector2(3.42f, 50), new Vector2(-31, -4.5f)
            };

            foreach (var center in centers)
            {
                var circle = new Circle(center, kmRadius);
                var points = circle.ToPolygon(numPoints).ToList();
                Assert.That(points.Count, Is.EqualTo(numPoints));
                Assert.That(points.Count, Is.EqualTo(points.Distinct().Count()));

                foreach (var point in points)
                {
                    // Should be on circle
                    var offset = point - circle.center;
                    Assert.That(offset.magnitude, Is.EqualTo(circle.kmRadius).Within(0.1f).Percent);
                }
            }
        }
    }
}