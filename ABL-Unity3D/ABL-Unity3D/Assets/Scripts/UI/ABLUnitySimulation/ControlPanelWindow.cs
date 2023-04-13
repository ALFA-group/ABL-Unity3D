using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UI.ABLUnitySimulation.Runners;
using UI.Planner;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

#nullable enable

namespace UI.ABLUnitySimulation
{
    public class ControlPanelWindow : OdinEditorWindow
    {
        private SimRunner? _simRunnerAll;

        /// <summary>
        ///     Used by Odin for PlayOrPauseSim()
        /// </summary>
        [UsedImplicitly]
        private string PlayOrPauseSimText =>
            this._simRunnerAll
                ? $"Play/Pause Sim ({this._simRunnerAll!.status})"
                : "Play/Pause Sim (error- no SimRunner Found)";

        private void OnInspectorUpdate()
        {
            this._simRunnerAll = Application.isPlaying ? GetObjectOfTypeAndAssertThereIsOnlyOne<SimRunner>() : null;
            this.Repaint();
        }

        [MenuItem("ABLUnity/Control Panel")]
        private static void OpenWindow()
        {
            GetWindow<ControlPanelWindow>().Show();
        }

        /// <summary>
        ///     Returns null or singleton.  Null is allowed in case we're between scenes (often happens when running Unit tests)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static T? GetObjectOfTypeAndAssertThereIsOnlyOne<T>() where T : MonoBehaviour
        {
            var objectsOfType = FindObjectsOfType<T>();
            if (objectsOfType.Length < 1) return null;

            Assert.IsTrue(objectsOfType.Length < 2, $"objectsOfType.Length < 2 {typeof(T).Name}");
            return objectsOfType.First();
        }

        [Button(ButtonSizes.Large)]
        [DisableIf("@!UnityEngine.Application.isPlaying")]
        private static void StartOrResetSim()
        {
            var simCreator = GetObjectOfTypeAndAssertThereIsOnlyOne<SimCreator>();
            if (simCreator) simCreator!.CreateNow();
        }

        [Button(ButtonSizes.Large)]
        [DisableIf("@!UnityEngine.Application.isPlaying")]
        private static void GenerateManyWorldsPlans()
        {
            var manyWorldsPlannerRunner = GetObjectOfTypeAndAssertThereIsOnlyOne<ManyWorldsPlannerRunner>();
            if (manyWorldsPlannerRunner) manyWorldsPlannerRunner!.CreateManyWorldsPlans();
        }

        [Button(ButtonSizes.Large, Name = "$PlayOrPauseSimText")]
        private void PlayOrPauseSim()
        {
            if (this._simRunnerAll) this._simRunnerAll!.ToggleRunStatus();
        }

        [Button(ButtonSizes.Large)]
        [HorizontalGroup("Split", 0.5f)]
        private static void ShowGizmos()
        {
            ToggleGizmos(true);
        }

        [Button(ButtonSizes.Large)]
        [VerticalGroup("Split/right")]
        private static void HideGizmos()
        {
            ToggleGizmos(false);
        }


        private static void ToggleGizmos(bool gizmosOn)
        {
            int val = gizmosOn ? 1 : 0;
            var asm = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = asm.GetType("UnityEditor.AnnotationUtility");
            if (type != null)
            {
                var getAnnotations = type.GetMethod("GetAnnotations", BindingFlags.Static | BindingFlags.NonPublic)
                                     ?? throw new InvalidOperationException();
                var setGizmoEnabled = type.GetMethod("SetGizmoEnabled", BindingFlags.Static | BindingFlags.NonPublic)
                                      ?? throw new InvalidOperationException();
                var setIconEnabled = type.GetMethod("SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic)
                                     ?? throw new InvalidOperationException();
                object? annotations = getAnnotations.Invoke(null, null);
                foreach (object annotation in (IEnumerable)annotations)
                {
                    var annotationType = annotation.GetType();
                    var classIdField = annotationType.GetField("classID", BindingFlags.Public | BindingFlags.Instance);
                    var scriptClassField =
                        annotationType.GetField("scriptClass", BindingFlags.Public | BindingFlags.Instance);
                    if (classIdField != null && scriptClassField != null)
                    {
                        var classId = (int)classIdField.GetValue(annotation);
                        var scriptClass = (string)scriptClassField.GetValue(annotation);
                        setGizmoEnabled.Invoke(null, new object[] { classId, scriptClass, val, false });
                        setIconEnabled.Invoke(null, new object[] { classId, scriptClass, val });
                    }
                }
            }
        }
    }
}