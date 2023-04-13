using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using Linefy.Internal;
using Linefy.Editors.Internal;

namespace Linefy{

    [CustomEditor(typeof(PolygonalMeshAsset))]
    public class PolygonalMeshAssetEditor : Editor {

        AnimatedFoldout importFromObjFoldout;
 
        SerializedPropertyField scaleFactor;
        SerializedPropertyField swapZAxis;
        SerializedPropertyField flipNormals;
        SerializedPropertyField smoothingGroupsMode;
 

        SerializedProperty prp_path;
        GUIContent pathButtonContent = new GUIContent("obj", "objFile");

        GUIStyle _statisticStyle;
        GUIStyle statisticStyle { 
            get {
                if (_statisticStyle == null) {
                    if (EditorStyles.whiteBoldLabel != null) {
                        _statisticStyle = new GUIStyle(EditorStyles.label);
                        _statisticStyle.fontSize = 10;
                    }  
                    _statisticStyle.wordWrap = true;
                }
                return _statisticStyle;
            }
        }
   
        string statistic;
        string lastImportStatistic;

        void OnEnable() {
            SerializedObject so = serializedObject;
            importFromObjFoldout = new AnimatedFoldout(so, "importFromObjFoldout", "Import from .obj", null, Repaint);
            prp_path = serializedObject.FindProperty("pathToObjFile");
            scaleFactor = new SerializedPropertyField(so, "scaleFactor", "Scale factor", "How much to scale the model comapered what in the sources file.");
            swapZAxis = new SerializedPropertyField(so, "swapYZAxis", "Swap YZ axis", "Swap YZ axis");
            flipNormals = new SerializedPropertyField(so, "flipNormals", "Flip Normals", "Flip normals");
            smoothingGroupsMode = new SerializedPropertyField(so, "smoothingGroupsImportMode", "Smoothing groups", "How deal with SG");
            UpdateStatistic();
        }

        void UpdateStatistic() {
            PolygonalMeshAsset t = target as PolygonalMeshAsset;
            if (t.serializedPolygonalMesh == null) {
                statistic = "empty";
            } else {
                statistic = string.Format(" Modification: \n");
                statistic += string.Format("      name: {0}\n", t.serializedPolygonalMesh.modificationInfo.name);
                statistic += string.Format("      date:  {0}\n", t.serializedPolygonalMesh.modificationInfo.date);
                statistic += string.Format(" Polygonal Mesh: \n");
                statistic += string.Format("      vertices: {0}\n",  t.serializedPolygonalMesh.positions.Length);
                statistic += string.Format("      normals: {0}\n", t.serializedPolygonalMesh.normals.Length);
                statistic += string.Format("      polygons: {0}\n", t.serializedPolygonalMesh.polygons.Length);
                statistic += string.Format("      edges: {0}\n",  t.serializedPolygonalMesh.positionEdges.Length);
                statistic += string.Format(" Unity Mesh: \n");
                statistic += string.Format("     vertices:  {0}\n", t.serializedPolygonalMesh.vertices.Length);
                statistic += string.Format("     triangles: {0}", t.serializedPolygonalMesh.trianglesCount);
            }
            lastImportStatistic =  string.Format("Last import took {0}ms\n", t.lastImportMS.ToString("F2"));
        }

        public override void OnInspectorGUI() {
            PolygonalMeshAsset t = target as PolygonalMeshAsset;
 
            EditorGUILayout.LabelField(statistic, statisticStyle);
            EditorGUILayout.Space();

            if (importFromObjFoldout.BeginDrawFoldout()) {

                pathButtonContent.text = prp_path.stringValue;
                if (string.IsNullOrEmpty(pathButtonContent.text)) {
                    pathButtonContent.text = "none";
                }

                EditorGUILayout.LabelField("Path to .obj file");

                EditorGUI.BeginChangeCheck();

                if (GUILayout.Button(pathButtonContent)) {
                    string dir = EditorPrefs.GetString("Linefy.lastOpenedObjDirectory");
                    string nselect = EditorUtility.OpenFilePanelWithFilters("Select .obj file", dir, new string[2] { "obj", "OBJ" });
                    if (System.IO.File.Exists(nselect)) {
                        EditorPrefs.SetString("Linefy.lastOpenedObjDirectory", new System.IO.FileInfo(nselect).Directory.FullName);
                    }
                    prp_path.stringValue = nselect;
                }

                scaleFactor.DrawGUILayout();
                swapZAxis.DrawGUILayout();
                flipNormals.DrawGUILayout();
                smoothingGroupsMode.DrawGUILayout();

                if (EditorGUI.EndChangeCheck()) {
                    ApplyChanges(false);
                }

                if (GUILayout.Button("Import")) {
                    t.ImportObjLocal();
                    UpdateStatistic();
                    ApplyChanges(true);
                }

                EditorGUILayout.LabelField(lastImportStatistic, statisticStyle);
            }

            importFromObjFoldout.EndDrawFoldout();
        }
 
        void ApplyChanges(bool databaseChanged) {
            PolygonalMeshAsset t = target as PolygonalMeshAsset;
            prp_path.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(t);
            if (databaseChanged) {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                serializedObject.Update();
            }
        }

        public class PolygonalMeshAssetFactory {
            [MenuItem("Assets/Create/Linefy/Polygonal Mesh Asset", priority = 202)]
            public static void MenuCreate() {
                var icon = FindPolygonalMeshAssetIcon();
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreatePolygonalMeshAsset>(), "New Polygonal Mesh Asset.asset", icon, null);
            }

            public static PolygonalMeshAsset CreateAssetAtPath(string path, int id) {
                var polygonalMeshAsset = ScriptableObject.CreateInstance<PolygonalMeshAsset>();
                polygonalMeshAsset.serializedPolygonalMesh = ScriptableObject.CreateInstance<SerializedPolygonalMesh>();
                polygonalMeshAsset.serializedPolygonalMesh.name = "Serialized Polygonal Mesh";
                //CtorInit();
                AssetDatabase.CreateAsset(polygonalMeshAsset, path);
                AssetDatabase.AddObjectToAsset(polygonalMeshAsset.serializedPolygonalMesh, polygonalMeshAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return polygonalMeshAsset;
            }
        }

        class DoCreatePolygonalMeshAsset : EndNameEditAction {
            public override void Action(int instanceId, string pathName, string resourceFile) {
                PolygonalMeshAsset data = PolygonalMeshAssetFactory.CreateAssetAtPath(pathName, instanceId);
                ProjectWindowUtil.ShowCreatedAsset(data);
            }
        }

        static Texture2D FindPolygonalMeshAssetIcon() {
            string[] guids = AssetDatabase.FindAssets("LinefyPolygonalMeshIcon");
            if (guids == null || guids.Length == 0) {
                Debug.LogWarningFormat("Texture LinefyPolygonalMeshIcon not found. Please reinstall Linefy");
                return null;
            }
            return (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(Texture2D));
        }
        
        internal void OnSceneDrag(SceneView sceneView) {

            Event e = Event.current;
            GameObject go = HandleUtility.PickGameObject(e.mousePosition, false);

            if (e.type == EventType.DragUpdated) {
                if (go) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                } else {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }
 
                e.Use();
            } else if (e.type == EventType.DragPerform) {
                DragAndDrop.AcceptDrag();
                e.Use();

                PolygonalMeshRenderer pmRendererComponent = go ? go.GetComponent<PolygonalMeshRenderer>() : null;
                if (pmRendererComponent == null) {
                    Plane floor = new Plane(Vector3.up, Vector3.zero);
                    Ray r = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    Vector3 pos = r.GetPoint(1);
                    floor.RaycastDoublesided(r, ref pos);
                    PolygonalMeshAsset t = target as PolygonalMeshAsset;
                    pmRendererComponent = t.InstantiateRenderer(AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat"));
                    Selection.activeGameObject = pmRendererComponent.gameObject;
                    pmRendererComponent.transform.position = pos; 
                }
                pmRendererComponent.polygonalMeshAsset = target as PolygonalMeshAsset;
                pmRendererComponent.LateUpdate();
            }



        }

        protected override void OnHeaderGUI() {
            base.OnHeaderGUI();
            if (Event.current.type == EventType.Repaint) {
                Rect r =  GUILayoutUtility.GetLastRect();
                r.position += new Vector2(44,22);
                GUI.Label(r, "Polygonal Mesh Asset", EditorStyles.miniLabel);
            }
        }

 
    }
}
