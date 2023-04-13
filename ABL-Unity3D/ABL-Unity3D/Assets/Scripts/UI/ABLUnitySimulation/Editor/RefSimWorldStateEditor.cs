using System.Linq;
using ABLUnitySimulation;
using ABLUnitySimulation.Actions;
using Planner;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace UI.ABLUnitySimulation.Editor
{
    [CustomEditor(typeof(RefSimWorldState))]
    public class RefSimWorldStateEditor : UnityEditor.Editor
    {
        public bool showWorldState;
        public bool showSimAgents;

        private static void ReadOnlyField(string label, string value)
        {
            var style = new GUIStyle(GUI.skin.textField) { normal = { textColor = Color.gray } };

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUILayout.TextField(value,
                style); // editable which is silly, but importantly allows selecting all text for when it gets too long and we need to view it in a different editor
            EditorGUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            DrawPropertiesExcluding(this.serializedObject, "data");

            this.showWorldState = EditorGUILayout.Foldout(this.showWorldState, "Sim World State");

            if (this.showWorldState) this.DrawSimGui();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawSimGui()
        {
            var state = this.GetSimWorldState();
            if (null == state)
            {
                EditorGUILayout.LabelField("Sim World State is NULL");
            }
            else
            {
                ReadOnlyField("Is Replan Requested?", state.isReplanRequested.ToString());
                ReadOnlyField("World State Id", state.WorldStateId.ToString());
                ReadOnlyField("Seconds Elapsed", state.SecondsElapsed.ToString());

                var agentList = state.Agents.ToList();
                this.showSimAgents = EditorGUILayout.Foldout(this.showSimAgents, $"{agentList.Count} Agents");
                if (this.showSimAgents)
                    foreach (var agent in agentList)
                        DrawSimAgent(agent);

                this.DrawSimAction(state.actions, "");
            }
        }

        private SimWorldState? GetSimWorldState()
        {
            var refState = this.serializedObject.targetObject as RefSimWorldState;
            // ReSharper disable once NullableWarningSuppressionIsUsed
            // This should be fine because we're explicitly checking if it's not null.
            var state = refState ? refState!.data : null;
            return state;
        }

        private static void DrawSimAgent(SimAgent agent)
        {
            EditorGUILayout.LabelField($"{agent.Name}: {agent.team} {agent.SimId}");
            EditorGUI.indentLevel++;
            ReadOnlyField("Health", agent.TotalCurrentHealth().ToString());
            EditorGUI.indentLevel--;
        }

        private void DrawSimAction(SimAction? action, string suffix)
        {
            if (null == action)
            {
                EditorGUILayout.LabelField("null");
                return;
            }

            var type = action.GetType();
            switch (action)
            {
                case ActionSequential sequential:
                    this.DrawActionSequential(sequential);
                    return;
                case ActionParallel parallel:
                    this.DrawActionParallel(parallel);
                    return;
                case SimActionPrimitive primitive:
                    EditorGUILayout.LabelField($"{action.name} ({type.Name}) {suffix}");
                    EditorGUI.indentLevel++;
                    ReadOnlyField("Agents", primitive.actors.ToString());
                    var state = this.GetSimWorldState();
                    // At this point, if the state is null, something crazy is going on. So forcing non-null is fine.
                    // ReSharper disable once NullableWarningSuppressionIsUsed
                    ReadOnlyField("Extra Info", primitive.GetUsefulInspectorInformation(state!));
                    EditorGUI.indentLevel--;
                    break;
                case PlannerSimAction plannerAction:
                    this.DrawPlannerSimAction(plannerAction);
                    break;

                default:
                    EditorGUILayout.LabelField($"{action.name} ({type.Name})");
                    break;
            }
        }

        private void DrawPlannerSimAction(PlannerSimAction plannerAction)
        {
            EditorGUILayout.LabelField($"PlannerSimAction {plannerAction.name}");
            EditorGUI.indentLevel++;
            this.DrawSimAction(plannerAction.GetCurrentAction(), "");
            EditorGUI.indentLevel--;
        }

        private void DrawActionSequential(ActionSequential sequential)
        {
            EditorGUILayout.LabelField($"ActionSequential {sequential.name}");
            EditorGUI.indentLevel++;
            for (var i = 0; i < sequential.actionQueue.Count; i++)
            {
                var simAction = sequential.actionQueue[i];
                this.DrawSimAction(simAction, i < sequential.actionsCompletedSuccessfully ? "Completed" : "");
            }

            EditorGUI.indentLevel--;
        }

        private void DrawActionParallel(ActionParallel parallel)
        {
            EditorGUILayout.LabelField($"ActionParallel {parallel.name}");
            EditorGUI.indentLevel++;
            foreach (var actionEntry in parallel.actions)
                this.DrawSimAction(actionEntry.subAction, actionEntry.ToHumanReadableString());
            EditorGUI.indentLevel--;
        }
    }
}