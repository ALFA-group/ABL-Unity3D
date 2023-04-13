using System;
using UnityEditor;
using Utilities.Unity;
using Object = UnityEngine.Object;

namespace UI.ABLUnitySimulation.Editor
{
    [InitializeOnLoad]
    public static class SimAgentFollowerEditHack
    {
        static SimAgentFollowerEditHack()
        {
            EditorApplication.pauseStateChanged += EditorApplicationOnpauseStateChanged;
        }

        private static void EditorApplicationOnpauseStateChanged(PauseState pause)
        {
            switch (pause)
            {
                case PauseState.Paused:
                    break;
                case PauseState.Unpaused:
                    var followers = Object.FindObjectsOfType<SimAgentFollower>();
                    foreach (var simAgentFollower in followers)
                        if (simAgentFollower.myAgent != null)
                            simAgentFollower.myAgent.positionActual = simAgentFollower.transform.position.ToSimVector2();

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pause), pause, null);
            }
        }
    }
}