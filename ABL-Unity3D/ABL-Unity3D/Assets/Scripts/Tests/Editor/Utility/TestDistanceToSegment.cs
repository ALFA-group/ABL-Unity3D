using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Utilities.Unity;

namespace Tests.Editor.Utility
{
    public static class TestDistanceToSegment
    {
        private static IEnumerable<DistanceTest> SimpleTestCases()
        {
            // Check values gathered from random js snippet:  https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment

            var test = new DistanceTest
            {
                point = Vector2.zero,
                endA = Vector2.zero,
                endB = Vector2.zero,
                expectedDistanceSquared = 0
            };
            yield return test;
            yield return test.Reversed();

            test.point = new Vector2(1, 0);
            test.expectedDistanceSquared = 1;
            yield return test;
            yield return test.Reversed();

            test.endA = test.point;
            test.expectedDistanceSquared = 0;
            yield return test;
            yield return test.Reversed();

            var f1 = 3472.214f;
            float f2 = -925.45789234f;
            test.point = new Vector2(f1, f2);
            test.endA = new Vector2(f1, 0);
            test.expectedDistanceSquared = f2 * f2;
            yield return test;
            yield return test.Reversed();

            test = new DistanceTest
            {
                point = new Vector2(-3, 4),
                endA = new Vector2(56, 23),
                endB = new Vector2(-13, -99),
                expectedDistanceSquared = 1764.15215067f
            };
            yield return test;
            yield return test.Reversed();

            test = new DistanceTest
            {
                point = new Vector2(-3, 4),
                endA = new Vector2(23, 56),
                endB = new Vector2(-13, -99),
                expectedDistanceSquared = 183.917099f
            };
            yield return test;
            yield return test.Reversed();

            test = new DistanceTest
            {
                point = new Vector2(-.3f, 4.1f),
                endA = new Vector2(3, 5),
                endB = new Vector2(-3, -99),
                expectedDistanceSquared = 10.5150055289f
            };
            yield return test;
            yield return test.Reversed();


            test = new DistanceTest
            {
                point = new Vector2(1, 4),
                endA = new Vector2(-1, -9),
                endB = new Vector2(3, 6),
                expectedDistanceSquared = 2.0082984f
            };
            yield return test;
            yield return test.Reversed();


            test = new DistanceTest
            {
                point = new Vector2(0.1968777f, 0.1612742f),
                endA = new Vector2(0.1132975f, 0.895304f),
                endB = new Vector2(0.04247115f, 0.5251149f),
                expectedDistanceSquared = 0.156221405f
            };
            yield return test;
            yield return test.Reversed();


            test = new DistanceTest
            {
                point = new Vector2(0.02349756f, 0.3308737f),
                endA = new Vector2(0.06591804f, 0.3825535f),
                endB = new Vector2(0.2423148f, 0.02254854f),
                expectedDistanceSquared = 0.00370061956f
            };
            yield return test;
            yield return test.Reversed();


            test = new DistanceTest
            {
                point = new Vector2(0.1477125f, 0.02565237f),
                endA = new Vector2(0.626892f, 0.390325f),
                endB = new Vector2(0.4478875f, 0.9198651f),
                expectedDistanceSquared = 0.362599015f
            };
            yield return test;
            yield return test.Reversed();


            test = new DistanceTest
            {
                point = new Vector2(0.4474164f, 0.1226061f),
                endA = new Vector2(0.1213116f, 0.167178f),
                endB = new Vector2(0.9129248f, 0.9007638f),
                expectedDistanceSquared = 0.0646939725f
            };
            yield return test;
            yield return test.Reversed();

            test = new DistanceTest
            {
                point = new Vector2(0.5470062f, 0.1810627f),
                endA = new Vector2(0.8076299f, 0.2800769f),
                endB = new Vector2(0.962414f, 0.401712f),
                expectedDistanceSquared = 0.0777285323f
            };
            yield return test;
            yield return test.Reversed();


            test = new DistanceTest
            {
                point = new Vector2(0.6931362f, 0.03251201f),
                endA = new Vector2(0.963657f, 0.9074725f),
                endB = new Vector2(0.2609349f, 0.7817808f),
                expectedDistanceSquared = 0.662044346f
            };
            yield return test;
            yield return test.Reversed();

            test = new DistanceTest
            {
                point = new Vector2(0.6578689f, 0.1869001f),
                endA = new Vector2(0.9845617f, 0.8015093f),
                endB = new Vector2(0.2120622f, 0.1025134f),
                expectedDistanceSquared = 0.0559514463f
            };
            yield return test;
            yield return test.Reversed();


            test = new DistanceTest
            {
                point = new Vector2(0.7756585f, 0.1153096f),
                endA = new Vector2(0.7404682f, 0.0227499f),
                endB = new Vector2(0.4229793f, 0.828153f),
                expectedDistanceSquared = 0.00444664247f
            };
            yield return test;
            yield return test.Reversed();


            test = new DistanceTest
            {
                point = new Vector2(0.8731609f, 0.4172631f),
                endA = new Vector2(0.6631747f, 0.1712253f),
                endB = new Vector2(0.5343823f, 0.7229725f),
                expectedDistanceSquared = 0.0678171143f
            };
            yield return test;
            yield return test.Reversed();


            test = new DistanceTest
            {
                point = new Vector2(0.8642631f, 0.7449878f),
                endA = new Vector2(0.8340937f, 0.8971569f),
                endB = new Vector2(0.6191966f, 0.8617211f),
                expectedDistanceSquared = 0.02406561f
            };
            yield return test;
            yield return test.Reversed();
        }

        [Test]
        [TestCaseSource(nameof(SimpleTestCases))]
        public static void TestSimple(DistanceTest test)
        {
            float d2 = test.point.DistanceToSegmentSquared(test.endA, test.endB);
            Assert.That(d2, Is.EqualTo(test.expectedDistanceSquared).Within(.1).Percent,
                $"{test.ToJsCall()}\n{test.ToConstructorString()}");
        }

        public struct DistanceTest
        {
            public Vector2 point;
            public Vector2 endA;
            public Vector2 endB;
            public float expectedDistanceSquared;

            public override string ToString()
            {
                return
                    $"DistanceTest{{ point={this.point} endA={this.endA} endB={this.endB} d2={this.expectedDistanceSquared}}}";
            }

            public string ToArrayString()
            {
                return
                    $"[{this.point.x},{this.point.y}, {this.endA.x},{this.endA.y}, {this.endB.x},{this.endB.y},{this.expectedDistanceSquared}]";
            }

            public string ToJsCall()
            {
                return
                    $"distToSegmentSquared([{this.point.x},{this.point.y}], [{this.endA.x},{this.endA.y}], [{this.endB.x},{this.endB.y}]) - {this.expectedDistanceSquared};";
            }

            public string ToConstructorString()
            {
                return $@"
test = new DistanceTest
{{
  point = new Vector2({this.point.x},{this.point.y}),
  endA = new Vector2({this.endA.x},{this.endA.y}),
  endB = new Vector2({this.endB.x},{this.endB.y}),
  expectedDistanceSquared = {this.expectedDistanceSquared}f
}};";
            }

            public DistanceTest Reversed()
            {
                return new DistanceTest
                {
                    point = this.point,
                    endA = this.endB,
                    endB = this.endA,
                    expectedDistanceSquared = this.expectedDistanceSquared
                };
            }
        }
    }
}