using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions;
using ABLUnitySimulation.Actions.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode.Sim
{
    public class TestActionMoveToWhileEngaging
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Debug.Log("Setup!");
            SceneManager.LoadScene("NProngs");
            yield return new WaitForFixedUpdate();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Debug.Log("Tear down!");
            yield break;
        }

        /// <summary>
        ///     Wraps straightforward test case to work around issue where UnityTest and TestCaseSource don't work together well
        /// </summary>
        public static IEnumerable<TestCaseData> GenerateTestKillBluesCases()
        {
            foreach (var testKillBlue in KillBluesInternal()) yield return new TestCaseData(testKillBlue).Returns(null);
        }

        public static IEnumerable<TestKillBlue> KillBluesInternal()
        {
            var nobody = Array.Empty<Vector2>();

            yield return new TestKillBlue
            {
                redStartLocation = new Vector2(10, 0),
                redDestination = Vector2.zero,
                kmKillRadius = .1f,
                bluesToKill = new[] { new Vector2(5, 0) },
                bluesToLeave = nobody,
                testDescription = "Kill 1 blue on the line",
                randomSeed = 1
            };


            // Make sure default range of simAgents is less than blue offset from line
            var defaultUnit = SimAgent.Create("tmp", Team.Blue, Vector2.zero, 1);
            Assert.That(defaultUnit.KmMaxRange(), Is.LessThan(3));

            yield return new TestKillBlue
            {
                redStartLocation = new Vector2(10, 0),
                redDestination = Vector2.zero,
                kmKillRadius = 3,
                bluesToKill = new[] { new Vector2(5, 2.5f) },
                bluesToLeave = nobody,
                testDescription =
                    "Kill 1 blue off the line, inside kill radius but outside weapon range - fails due to pathfinding, fix after merging zero pathfinding option",
                randomSeed = 2
            };

            Assert.That(defaultUnit.KmMaxRange(), Is.GreaterThanOrEqualTo(1));
            yield return new TestKillBlue
            {
                redStartLocation = new Vector2(10, 0),
                redDestination = Vector2.zero,
                kmKillRadius = 0.5f,
                bluesToKill = nobody,
                bluesToLeave = new[] { new Vector2(5, 0.6f) },
                testDescription = "Ignore 1 blue off the line, outside kill radius but inside weapon range",
                randomSeed = 3
            };

            var killUs = new[]
            {
                new Vector2(5, 0.6f),
                new Vector2(5, 1.6f),
                new Vector2(5, 2.6f),
                new Vector2(2, -0.6f),
                new Vector2(11, 0f),
                new Vector2(3, 2f)

                // this blue is past the destination,
                //so the action evals to Completed before it dies.
                //That's an acceptable place for undefined behavior
                //new Vector2(-1, 0.6f),  
            };

            var dontKillUs = new[]
            {
                // Don't kill
                new Vector2(-3, 0.2f),
                new Vector2(13, -.12f),
                new Vector2(3, 4f)
            };

            yield return new TestKillBlue
            {
                redStartLocation = new Vector2(10, 0),
                redDestination = Vector2.zero,
                kmKillRadius = 3f,
                bluesToKill = killUs,
                bluesToLeave = dontKillUs,
                testDescription =
                    "Kill some blues, but not the ones off path - - fails due to pathfinding, fix after merging zero pathfinding option",
                randomSeed = 3
            };
        }

        [UnityTest]
        public IEnumerator TestNoBlues([NUnit.Framework.Range(0, 2)] int randomSeed)
        {
            Assert.That(Application.isPlaying, "Not in play mode, needed for pathfinding");

            var rStart = new Rect(10, 1, 1, 10);
            var state = SimScenarios.CreateTestSim(4, rStart, 0, rStart, randomSeed);
            var destination = new Circle(Vector2.one, 1);

            TestMoveReds(state, destination, 2);

            yield return null;
        }


        [UnityTest]
        [TestCaseSource(nameof(GenerateTestKillBluesCases))]
        public IEnumerator TestKillBlues(TestKillBlue killBlue)
        {
            var rStart = new Rect(killBlue.redStartLocation, Vector2.zero);
            var state = SimScenarios.CreateTestSim(4, rStart, 0, rStart, killBlue.randomSeed);

            foreach (var red in state.GetTeam(Team.Red)) red.kmVisualRange = 10;

            foreach (var blueLocation in killBlue.bluesToKill.Concat(killBlue.bluesToLeave))
            {
                var blue = SimAgent.Create("victim", Team.Blue, blueLocation, 1);
                state.Add(blue);
                Assert.That(blue.IsWorthShooting(), "Created a blue not worth shooting!");
            }

            var destination = new Circle(killBlue.redDestination, 1);

            var finalState = TestMoveReds(state, destination, killBlue.kmKillRadius);

            var blues = finalState.GetTeam(Team.Blue).ToList();

            foreach (var deadBlueLocation in killBlue.bluesToKill)
            {
                var blue = blues.FirstOrDefault(b => b.positionActual == deadBlueLocation);
                var whichBlue = $"Blue At {deadBlueLocation}";
                Assert.NotNull(blue, whichBlue + " not found: " + killBlue.testDescription);
                Assert.That(!blue.IsActive, whichBlue + " should be dead, but isn't: " + killBlue.testDescription);
            }

            foreach (var liveBlueLocation in killBlue.bluesToLeave)
            {
                var blue = blues.FirstOrDefault(b => b.positionActual == liveBlueLocation);
                var whichBlue = $"Blue At {liveBlueLocation}";
                Assert.NotNull(blue, whichBlue + " not found: " + killBlue.testDescription);
                Assert.That(blue.IsActive, whichBlue + " should be alive, but isn't: " + killBlue.testDescription);
            }

            yield return null;
        }

        private static SimWorldState TestMoveReds(SimWorldState startState, Circle destination, float kmClearRadius)
        {
            var localState = startState.DeepCopy();
            var clear = new ActionMoveToWhileEngaging(new SimGroup(localState.GetTeam(Team.Red)))
            {
                destination = destination,
                kmEngagementWidth = kmClearRadius,
                name = "UnitTest",
                targetTeam = Team.Blue
            };
            Assert.Positive(clear.attackers.Count);
            Assert.AreEqual(ActionStatus.InProgress, clear.GetStatus(localState, false).status, "Pre-run status");

            localState.actions.Add(clear);

            var maxSeconds = 10000;
            localState.ExecuteUntil(
                worldState => clear.GetStatus(worldState, false).status == ActionStatus.CompletedSuccessfully,
                100,
                maxSeconds,
                CancellationToken.None);

            Assert.Less(localState.SecondsElapsed, maxSeconds,
                $"First red at {localState.GetTeam(Team.Red).First().positionActual}");

            foreach (var agent in localState.GetTeam(Team.Red))
                Assert.That(destination.IsInside(agent.positionActual),
                    $"{agent.positionActual} should be inside {destination}");

            return localState;
        }

        public struct TestKillBlue
        {
            public Vector2 redStartLocation;
            public Vector2 redDestination;
            public float kmKillRadius;
            public Vector2[] bluesToKill;
            public Vector2[] bluesToLeave;
            public int? randomSeed;
            public string testDescription;
        }
    }
}