using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tests.PlayMode
{
    public static class Helpers
    {
        public static IEnumerable LoadAndWaitForScene(string sceneName)
        {
            if (!Application.isPlaying) Assert.Inconclusive("Can only run test in Play mode");

            SceneManager.LoadScene(sceneName);
            yield return null;

            if (SceneManager.GetActiveScene().name != sceneName)
                Assert.Inconclusive(
                    $"Can only run test with {sceneName} as active scene, was {SceneManager.GetActiveScene().name}");
        }
    }
}