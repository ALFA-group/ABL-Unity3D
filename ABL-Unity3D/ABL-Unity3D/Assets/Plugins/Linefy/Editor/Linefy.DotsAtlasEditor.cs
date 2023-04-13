using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy.Internal;
using Linefy.Primitives;

namespace Linefy {

    [CustomEditor(typeof(DotsAtlas))]
    public class DotsAtlasEditor : Editor {
        SerializedProperty prp_xcount;
        SerializedProperty prp_ycount;
        SerializedProperty prp_flipVertical;
        SerializedProperty prp_texture;
        SerializedProperty prp_settingsHash;
        SerializedProperty prp_statisticString;
        SerializedProperty prp_isFontAtlas;

        //font settings
        SerializedProperty prp_whitespaceWidth;
        SerializedProperty prp_horizontalSpacing;
        SerializedProperty prp_verticalSpacing;
        SerializedProperty prp_monowidth;
        SerializedProperty prp_enableRemappingIndexTable;
        SerializedProperty prp_remappingIndexTable;
        SerializedProperty prp_background9SliseIndices;
        SerializedProperty prp_fontSettingsFoldout;
        SerializedProperty prp_resetIndexOffset;

        //Apperance
        SerializedProperty prp_apperance;
        SerializedProperty prp_backgroundBrightness;
        SerializedProperty prp_labelSize;
        SerializedProperty prp_labelsColor;
        float inspectorWidth =200;
        LabelsRenderer indicesLabels;
        EditorGUIViewport viewport;
        Grid2d grid;
        PolygonalMesh texturePlane;
        bool rectsTopologyDirty = true;

        private void OnEnable() {
            prp_xcount = serializedObject.FindProperty("xCount");
            prp_ycount = serializedObject.FindProperty("yCount");
            prp_texture = serializedObject.FindProperty("texture");
            prp_flipVertical = serializedObject.FindProperty("flipVertical");
            prp_statisticString = serializedObject.FindProperty("statisticString");
            prp_settingsHash = serializedObject.FindProperty("modificationHash");

            //font settings
            prp_fontSettingsFoldout = serializedObject.FindProperty("fontSettingsFoldout");
            prp_isFontAtlas = serializedObject.FindProperty("isFontAtlas");
            prp_whitespaceWidth = serializedObject.FindProperty("whitespaceWidth");
            prp_horizontalSpacing = serializedObject.FindProperty("horizontalSpacing");
            prp_monowidth = serializedObject.FindProperty("monowidth");
            prp_enableRemappingIndexTable = serializedObject.FindProperty("enableRemapping");
            prp_remappingIndexTable = serializedObject.FindProperty("remappingIndexTable");
            prp_background9SliseIndices = serializedObject.FindProperty("background9SliseIndices");
            prp_resetIndexOffset = serializedObject.FindProperty("resetIndexOffset");

            //apperance
            prp_apperance = serializedObject.FindProperty("apperance");
            prp_backgroundBrightness = prp_apperance.FindPropertyRelative("backgroundBrightness");
            prp_labelSize = prp_apperance.FindPropertyRelative("labelSize");
            prp_labelsColor = prp_apperance.FindPropertyRelative("labelsColor");
        

            viewport = new EditorGUIViewport();
            Vector2[] uvData = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
            Polygon[] polygons = new Polygon[] { 
                new Polygon(
                        new PolygonCorner(0, 0, 0), 
                        new PolygonCorner(1, 1, 0),
                        new PolygonCorner(2, 2, 0),
                        new PolygonCorner(3, 3, 0)
                    )
                };
            texturePlane = new PolygonalMesh(new Vector3[4], uvData, polygons);
            Repaint();
        }


        void DrawFontSettings() {
            prp_fontSettingsFoldout.boolValue = EditorGUILayout.Foldout(prp_fontSettingsFoldout.boolValue, "Font settings");
            if (prp_fontSettingsFoldout.boolValue) {
             
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(prp_isFontAtlas);
                GUI.enabled = prp_isFontAtlas.boolValue;
                EditorGUILayout.PropertyField(prp_whitespaceWidth);
                EditorGUILayout.PropertyField(prp_horizontalSpacing);
                EditorGUILayout.PropertyField(prp_monowidth);
                EditorGUILayout.PropertyField(prp_background9SliseIndices, true);
                EditorGUILayout.PropertyField(prp_enableRemappingIndexTable);

                if (prp_enableRemappingIndexTable.boolValue) {

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(prp_resetIndexOffset,   GUILayout.ExpandWidth(false));

                    GUILayout.Space(4);

                    if (GUILayout.Button("Reset indices", EditorStyles.miniButton, GUILayout.MaxWidth(120))) {
                        prp_remappingIndexTable.arraySize = (prp_xcount.intValue * prp_ycount.intValue);
                        int arraySize = prp_remappingIndexTable.arraySize;
                        for (int i = 0; i < arraySize; i++) {
                            prp_remappingIndexTable.GetArrayElementAtIndex(i).intValue = prp_resetIndexOffset.intValue+i;
                        }
                    }

                    GUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(prp_remappingIndexTable, true);
 
                }

                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Apply Font settings", GUILayout.MaxWidth(200))) {
                    RecalculateRects(true);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                GUI.enabled = true;
            }
        }

        public override void OnInspectorGUI() {
    
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(prp_texture);
            EditorGUILayout.PropertyField(prp_xcount);
            EditorGUILayout.PropertyField(prp_ycount);
            EditorGUILayout.PropertyField(prp_flipVertical);

            GUI.enabled = false;
            EditorGUILayout.LabelField(prp_statisticString.stringValue);
            GUI.enabled = true;

            GUILayout.Space(3);

            EditorGUILayout.PropertyField(prp_apperance, true);

            GUILayout.Space(3);

            DrawFontSettings();

            if (EditorGUI.EndChangeCheck()) {
                RecalculateRects(false);
                serializedObject.ApplyModifiedProperties();
                rectsTopologyDirty = true;
            }

            GUILayout.Space(2); 

            if (prp_texture.objectReferenceValue != null) {
                DrawPreviewArea();
            }
 
            GUILayout.Space(3);
        }

        void RecalculateRects(bool recalculateFont) {
            DotsAtlas t = target as DotsAtlas;
            prp_settingsHash.intValue = DotsAtlas.GetSettingsHash(prp_xcount.intValue, prp_ycount.intValue, prp_flipVertical.boolValue);

            int xCount = prp_xcount.intValue;
            int yCount = prp_ycount.intValue;

            if (prp_texture.objectReferenceValue != null) {
                Texture tex = prp_texture.objectReferenceValue as Texture;
                prp_statisticString.stringValue = string.Format("{0} rects  {1}x{2} pixels", xCount * yCount, tex.width / xCount, tex.height / yCount);
            } else {
                prp_statisticString.stringValue = string.Format("{0} rects no texture selected", xCount * yCount);
            }

            serializedObject.ApplyModifiedProperties();

            t.RecalculateRectsCoordinates();
            if (recalculateFont) {
                string[] resultMessage = t.ApplyFontSettings();
                if ( resultMessage != null) {
                    EditorUtility.DisplayDialog(resultMessage[0], resultMessage[1], "OK");
                }
            }
            
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        void DrawPreviewArea() {
            Texture tex = prp_texture.objectReferenceValue as Texture;
            EditorGUILayout.GetControlRect(false, 2);

            if (Event.current.type == EventType.Repaint) {
                inspectorWidth = GUILayoutUtility.GetLastRect().width;
                inspectorWidth = Screen.width;
            }

            Rect textureRect = EditorGUILayout.GetControlRect(false, inspectorWidth * 2);
            textureRect.height = textureRect.width * tex.height / tex.width;
            int xCount = prp_xcount.intValue;
            int yCount = prp_ycount.intValue;
            int totalRectsCount = xCount * yCount;

             if (grid == null) {
                grid = new Grid2d(textureRect.width - 1, textureRect.height - 1, xCount, yCount, false,  new Serialization.SerializationData_Lines(1, Color.black, 0));
                grid.wireframeProperties.transparent = true;
 
            }

            grid.wireframeProperties.renderOrder = 102;
            grid.width = textureRect.width - 1;
            grid.height = textureRect.height - 1;
            grid.widthSegments = xCount;
            grid.heightSegments = yCount;

            if (indicesLabels == null) {
                indicesLabels = new LabelsRenderer(totalRectsCount);
                indicesLabels.drawBackground = true;
                indicesLabels.transparent = true;
                indicesLabels.renderOrder = 101;
                indicesLabels.backgroundExtraSize = new Vector2(-10, -12);
                indicesLabels.horizontalAlignment = TextAlignment.Left;
            }

            indicesLabels.size = prp_labelSize.floatValue;
 
            if (rectsTopologyDirty) {
                indicesLabels.count = totalRectsCount;
                for (int i = 0; i < indicesLabels.count; i++) {
                    indicesLabels.SetText(i, i.ToString());
                }
                rectsTopologyDirty = false;
            }

            int indexCounter = 0;
            float cellSizeX = textureRect.width / xCount;
            float cellSizeY = textureRect.height / yCount;
            Vector2 labelOffset = new Vector2(0, -8);
            Vector2 posOffset = new Vector2(-textureRect.width / 2, -textureRect.height / 2 + cellSizeY) + labelOffset;

            for (int y = yCount - 1; y >= 0; y--) {
                for (int x = 0; x < xCount; x++) {
                    Vector2 labelPos = posOffset + new Vector2(x * cellSizeX, y * cellSizeY -  indicesLabels.size);
                    indicesLabels.SetPosition(indexCounter, labelPos);
                    indexCounter++;
                }
            }
 
            grid.wireframeProperties.colorMultiplier = prp_labelsColor.colorValue;
            indicesLabels.textColor = prp_labelsColor.colorValue;

            texturePlane.texture = tex;
            texturePlane.transparent = true;
            texturePlane.ambient = 1;
            texturePlane.SetPosition(0, new Vector3(-textureRect.width / 2, -textureRect.height / 2, 0));
            texturePlane.SetPosition(1, new Vector3(-textureRect.width / 2, textureRect.height / 2, 0));
            texturePlane.SetPosition(2, new Vector3(textureRect.width / 2, textureRect.height / 2, 0));
            texturePlane.SetPosition(3, new Vector3(textureRect.width / 2, -textureRect.height / 2, 0));

            if (Event.current.type == EventType.Repaint) {
                viewport.SetParams(textureRect, 1, Vector2.zero);
                viewport.DrawLocalSpace( texturePlane );
                viewport.DrawLocalSpace( grid );
                viewport.DrawLocalSpace(indicesLabels);
                float brightness = prp_backgroundBrightness.floatValue;
                viewport.backgroundColor = new Color(brightness, brightness, brightness, 1);
                viewport.Render();
            }
        }

        protected override void OnHeaderGUI() {
            base.OnHeaderGUI();
            if (Event.current.type == EventType.Repaint) {
                Rect r = GUILayoutUtility.GetLastRect();
                r.position += new Vector2(44, 22);
                GUI.Label(r, "Dots Atlas", EditorStyles.miniLabel);
            }
        }

        private void OnDestroy() {
            if (viewport != null) {
                viewport.Dispose();
                viewport = null;
            }

            if (grid != null) {
                grid.Dispose();
                grid = null;
            }

            if (texturePlane != null) {
                texturePlane.Dispose();
                texturePlane = null;
            }

            if (indicesLabels != null) {
                indicesLabels.Dispose();
                indicesLabels = null;
            }
        }

    }
}
