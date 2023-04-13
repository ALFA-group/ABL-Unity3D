using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Linefy.Internal;

namespace Linefy {

    [CustomPropertyDrawer(typeof(Matrix4x4InspectorAttribute))]
    public class Matrix4x4PropertyDrawer : PropertyDrawer {

        GUIStyle _cellsStyle;
        GUIStyle cellsStyle {
            get {
                if (_cellsStyle == null) {
                    //Debug.Log("create cell style");

                    _cellsStyle = new GUIStyle(EditorStyles.helpBox);
                    _cellsStyle.fontSize = 8;
                }
                return _cellsStyle;
            }
        }

        GUIStyle _mprefixStyle;
        GUIStyle mprefixStyle {
            get {
                if (_mprefixStyle == null) {
                    //Debug.Log("create cell style");

                    _mprefixStyle = new GUIStyle(EditorStyles.helpBox);
                    _mprefixStyle.normal.background = null;
                    _mprefixStyle.fontSize = 8;
                    //_mprefixStyle.overflow = new RectOffset(0, 0, 0, 0);
                    RectOffset padding = _mprefixStyle.padding;
                    padding.left = 0;
                    padding.right = 0;
                    _mprefixStyle.padding = padding;
                }
                return _mprefixStyle;
            }
        }

        float valuesGridHeight = 86;
        float inputFieldlineHeight = 20;
        float inputFieldlineSpacing = 4;

        Matrix4x4 tm;
        SerializedObject so;
        SerializedProperty m00;
        SerializedProperty m01;
        SerializedProperty m02;
        SerializedProperty m03;

        SerializedProperty m10;
        SerializedProperty m11;
        SerializedProperty m12;
        SerializedProperty m13;

        SerializedProperty m20;
        SerializedProperty m21;
        SerializedProperty m22;
        SerializedProperty m23;

        SerializedProperty m30;
        SerializedProperty m31;
        SerializedProperty m32;
        SerializedProperty m33;

        void FillPropertyesAndMatrix(SerializedProperty property) {
            so = property.serializedObject;
            m00 = property.FindPropertyRelative("e00");
            m01 = property.FindPropertyRelative("e01");
            m02 = property.FindPropertyRelative("e02");
            m03 = property.FindPropertyRelative("e03");

            m10 = property.FindPropertyRelative("e10");
            m11 = property.FindPropertyRelative("e11");
            m12 = property.FindPropertyRelative("e12");
            m13 = property.FindPropertyRelative("e13");

            m20 = property.FindPropertyRelative("e20");
            m21 = property.FindPropertyRelative("e21");
            m22 = property.FindPropertyRelative("e22");
            m23 = property.FindPropertyRelative("e23");

            m30 = property.FindPropertyRelative("e30");
            m31 = property.FindPropertyRelative("e31");
            m32 = property.FindPropertyRelative("e32");
            m33 = property.FindPropertyRelative("e33");


            tm.m00 = m00.floatValue;
            tm.m10 = m10.floatValue;
            tm.m20 = m20.floatValue;
            tm.m30 = m30.floatValue;

            tm.m01 = m01.floatValue;
            tm.m11 = m11.floatValue;
            tm.m21 = m21.floatValue;
            tm.m31 = m31.floatValue;

            tm.m02 = m02.floatValue;
            tm.m12 = m12.floatValue;
            tm.m22 = m22.floatValue;
            tm.m32 = m32.floatValue;

            tm.m03 = m03.floatValue;
            tm.m13 = m13.floatValue;
            tm.m23 = m23.floatValue;
            tm.m33 = m33.floatValue;

        }

        void DrawValuesGrid(ref Rect position) {
            if (Event.current.type == EventType.Repaint) {
                float mprefixspace = 13;
                Rect indentedRect = EditorGUI.IndentedRect(position);
                Vector2 gridPosition = indentedRect.position;

                float spacing = 4;

                float inspectorWidthSpace = indentedRect.width;
                Vector2 cellPositionStep = new Vector2(inspectorWidthSpace / 4, 21);
                Rect cellRect = new Rect(0, 0, cellPositionStep.x - spacing, cellPositionStep.y - spacing);

                for (int r = 0; r < 4; r++) {
                    for (int c = 0; c < 4; c++) {
                        cellRect.x = gridPosition.x + c * cellPositionStep.x;
                        cellRect.y = gridPosition.y + r * cellPositionStep.y;
                        string str = string.Format("{0: 0.000;-0.000}", tm[r, c]);

                        Rect mPrefixRect = cellRect;
                        mPrefixRect.width = mprefixspace;
                        Rect valueRect = cellRect;
                        valueRect.x += mprefixspace;
                        valueRect.width -= mprefixspace;
                        GUI.Label(mPrefixRect, string.Format("{0}{1}", r, c), mprefixStyle);
                        GUI.Label(valueRect, str, cellsStyle);
                    }
                }

            }
            position.y += valuesGridHeight;
        }

        void ResetPosition() {
            m03.floatValue = 0;
            m13.floatValue = 0;
            m23.floatValue = 0;
            m33.floatValue = 1;
            m03.serializedObject.ApplyModifiedProperties();
        }

        void ApplyAxis(Matrix4x4 ntm) {
            m00.floatValue = ntm.m00;
            m10.floatValue = ntm.m10;
            m20.floatValue = ntm.m20;
            m30.floatValue = 0;

            m01.floatValue = ntm.m01;
            m11.floatValue = ntm.m11;
            m21.floatValue = ntm.m21;
            m31.floatValue = 0;

            m02.floatValue = ntm.m02;
            m12.floatValue = ntm.m12;
            m22.floatValue = ntm.m22;
            m32.floatValue = 0;
        }

        void ResetEuler() {
            Vector3 _position = tm.GetColumn(3);
            Vector3 _right = tm.GetColumn(0);
            Vector3 _up = tm.GetColumn(1);
            Vector3 _fwd = tm.GetColumn(2);
            Vector3 _scale = new Vector3(_right.magnitude, _up.magnitude, _fwd.magnitude);
            Matrix4x4 ntm = Matrix4x4.TRS(_position, Quaternion.Euler(Vector3.zero), _scale);
            ApplyAxis(ntm);
            so.ApplyModifiedProperties();
        }

        void ResetScale() {
            Vector4 _position = tm.GetColumn(3);
            Vector3 _right = tm.GetColumn(0).normalized;
            Vector3 _up = tm.GetColumn(1).normalized;
            Vector3 _fwd = tm.GetColumn(2).normalized;
            Matrix4x4 ntm = new Matrix4x4();
            ntm.SetColumn(0, _right);
            ntm.SetColumn(1, _up);
            ntm.SetColumn(2, _fwd);
            ntm.SetColumn(3, _position);
            ApplyAxis(ntm);
            so.ApplyModifiedProperties();
        }

        void ResetToIdentity() {
            m00.floatValue = 1;
            m10.floatValue = 0;
            m20.floatValue = 0;
            m30.floatValue = 0;

            m01.floatValue = 0;
            m11.floatValue = 1;
            m21.floatValue = 0;
            m31.floatValue = 0;

            m02.floatValue = 0;
            m12.floatValue = 0;
            m22.floatValue = 1;
            m32.floatValue = 0;

            m03.floatValue = 0;
            m13.floatValue = 0;
            m23.floatValue = 0;
            m33.floatValue = 1;
            so.ApplyModifiedProperties();
        }

        void MirrorX() {
            m00.floatValue = -m00.floatValue;
            m10.floatValue = -m10.floatValue;
            m20.floatValue = -m20.floatValue;
            m30.floatValue = -m30.floatValue;
            so.ApplyModifiedProperties();
        }

        void MirrorY() {
            m01.floatValue = -m01.floatValue;
            m11.floatValue = -m11.floatValue;
            m21.floatValue = -m21.floatValue;
            m31.floatValue = -m31.floatValue;
            so.ApplyModifiedProperties();
        }

        void MirrorZ() {
            m02.floatValue = -m02.floatValue;
            m12.floatValue = -m12.floatValue;
            m22.floatValue = -m22.floatValue;
            m32.floatValue = -m32.floatValue;
            so.ApplyModifiedProperties();
        }

        void DrawInputFields(ref Rect position) {
            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 80;
            EditorGUIUtility.wideMode = true;
            Vector3 _position = tm.GetColumn(3);

            Rect fieldRect = position;

            fieldRect.height = inputFieldlineHeight;

            //POS FIELD

            Vector3 _nposition = EditorGUI.Vector3Field(fieldRect, "Position", _position);
            if (_nposition != _position) {
                m03.floatValue = _nposition.x;
                m13.floatValue = _nposition.y;
                m23.floatValue = _nposition.z;
            }

            Vector3 up = tm.GetColumn(1);
            Vector3 fwd = tm.GetColumn(2);
#if UNITY_2017_2_OR_NEWER
            //Vector3 _prevScale = tm.lossyScale;
            Vector3 _prevScale = new Vector3(tm.GetColumn(0).magnitude, tm.GetColumn(1).magnitude, tm.GetColumn(2).magnitude);
#else
            Vector3 _prevScale = new Vector3(tm.GetColumn(0).magnitude, tm.GetColumn(1).magnitude, tm.GetColumn(2).magnitude );
#endif



            //EULER FIELD
            fieldRect.y += (inputFieldlineHeight + inputFieldlineSpacing);
            bool validFwdScale = true;
            bool validUpScale = true;
            if (Mathf.Approximately(fwd.magnitude, 0)) {
                validFwdScale = false;
            }
            Vector3 _prevEuler = Vector3.zero;
            Vector3 _neuler = Vector3.zero;

            if (Mathf.Approximately(up.magnitude, 0)) {
                validUpScale = false;
            }

            if (validFwdScale && validUpScale) {
                _prevEuler = Quaternion.LookRotation(fwd, up).eulerAngles;
                _neuler = EditorGUI.Vector3Field(fieldRect, "Euler", _prevEuler);
            } else {
                GUI.Label(fieldRect, "Euler   Extract rotation failed. Matrix has zero length column.");
            }

            //EULER FIELD
            fieldRect.y += (inputFieldlineHeight + inputFieldlineSpacing);
            Vector3 _nscale = EditorGUI.Vector3Field(fieldRect, "Scale", _prevScale);

            if (_neuler != _prevEuler || _nscale != _prevScale) {
                Matrix4x4 ntm = Matrix4x4.TRS(_nposition, Quaternion.Euler(_neuler), _nscale);
                m00.floatValue = ntm.m00;
                m10.floatValue = ntm.m10;
                m20.floatValue = ntm.m20;
                m30.floatValue = 0;

                m01.floatValue = ntm.m01;
                m11.floatValue = ntm.m11;
                m21.floatValue = ntm.m21;
                m31.floatValue = 0;

                m02.floatValue = ntm.m02;
                m12.floatValue = ntm.m12;
                m22.floatValue = ntm.m22;
                m32.floatValue = 0;
            }

            if (EditorGUI.EndChangeCheck()) {
                m00.serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Matrix4x4InspectorAttribute a = attribute as Matrix4x4InspectorAttribute;
            Rect headerRect = new Rect(position.x, position.y, position.width, 18);
            EditorGUI.PropertyField(headerRect, property);

            if (property.isExpanded) {
                if (Event.current.type == EventType.ContextClick && position.Contains(Event.current.mousePosition)) {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Reset to Identity"), false, ResetToIdentity);
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Reset position"), false, ResetPosition);
                    menu.AddItem(new GUIContent("Reset rotation"), false, ResetEuler);
                    menu.AddItem(new GUIContent("Reset scale"), false, ResetScale);
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Mirror X "), false, MirrorX);
                    menu.AddItem(new GUIContent("Mirror Y"), false, MirrorY);
                    menu.AddItem(new GUIContent("Mirror Z"), false, MirrorZ);
                    menu.ShowAsContext();
                }
                position.y += 18;

                FillPropertyesAndMatrix(property);

                if (a.showValuesGrid) {
                    DrawValuesGrid(ref position);
                }

                if (a.showInputFields) {
                    DrawInputFields(ref position);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            Matrix4x4InspectorAttribute a = attribute as Matrix4x4InspectorAttribute;
            float result = 14;
            if (property.isExpanded) {
                if (a.showValuesGrid) {
                    result += valuesGridHeight;
                }
                if (a.showInputFields) {
                    result += (inputFieldlineHeight + inputFieldlineSpacing) * 3 + inputFieldlineSpacing;
                }

            }
            return result;
        }
    }

    [CustomPropertyDrawer(typeof(PolygonCorner))]
    public class PolygonCornerDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label.text = string.Format("  #{0}", label.text.Remove(0, 7));
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            float widthNum = position.width / 3f - 4;
            EditorGUI.indentLevel = 0;
            Rect positionPropertyRect = new Rect(position.x, position.y, widthNum, position.height);
            Rect uvPropertyRect = new Rect(position.x + widthNum + 4, position.y, widthNum, position.height);
            Rect colorPropertyRect = new Rect(position.x + widthNum * 2 + 8, position.y, widthNum, position.height);
            SerializedProperty prp_pos = property.FindPropertyRelative("position");
            SerializedProperty prp_uv = property.FindPropertyRelative("uv");
            SerializedProperty prp_color = property.FindPropertyRelative("color");
            EditorGUIUtility.labelWidth = 28;
            EditorGUIUtility.wideMode = true;
            prp_pos.intValue = EditorGUI.IntField(positionPropertyRect, "pos", prp_pos.intValue);
            prp_uv.intValue = EditorGUI.IntField(uvPropertyRect, "uv", prp_uv.intValue);
            prp_color.intValue = EditorGUI.IntField(colorPropertyRect, "col", prp_color.intValue);
            EditorGUI.EndProperty();
        }
    }

   // [CustomPropertyDrawer(typeof(PolylineVertex))]
    

    //public class PolygonxCornerDrawer : PropertyDrawer {
     


    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    //        label.text = string.Format(" #{0}", label.text.Remove(0, 8));
    //        float lineWidth = position.width;
    //        EditorGUI.BeginProperty(position, label, property);
    //        Rect cPos = position;
    //        cPos.width = 10;
    //        GUI.Label(position, label.text);
    //        cPos.x += cPos.width + 4;
    //        cPos.width = 200;

    //        //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
    //        //Rect positionPropertyRect = new Rect(position.x, position.y, position.width/4, position.height);
    //        SerializedProperty prp_pos = property.FindPropertyRelative("position");
    //         EditorGUIUtility.labelWidth = 28;
    //        //GUI.Box(positionPropertyRect, EditorStyles.helpBox.normal.background);
    //         EditorGUIUtility.wideMode = true;
    //         prp_pos.vector3Value = EditorGUI.Vector3Field(cPos, "", prp_pos.vector3Value);
    //         EditorGUI.EndProperty();
    //    }
    //}

    [CustomPropertyDrawer(typeof(InfoStringAttribute))]
    public class InfoStringDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            string str = property.stringValue;
            //EditorGUI.BeginProperty(position, label, property);

            GUI.Label(position, string.Format("{0}: {1}", label.text, str), EditorStyles.helpBox);
            //EditorGUI.EndProperty();
        }
    }


    //public class LinefySerializedClassPropertyDrawerBase : PropertyDrawer {

    //    protected GUIContent c_renderOrder = new GUIContent("Render Order", "Determine in which order objects are renderered.");
    //    protected GUIContent c_transparent = new GUIContent("Transparent", "If true will be use the unlit, transparent shader, otherwise the unlit, opaque with alpha clipping.");
    //    protected GUIContent c_colorMultiplier = new GUIContent("Color Multiplier", "The main color. An color of each encapsulated item will multiplied by this color.");
        
    //    protected GUIContent c_widthMode = new GUIContent("Width Mode", "Worldspace: Billboarded orientation, width measured in worldspace units , respects an perspective distortion  \n\n  Pixels: Billboarded orientation, constant width measured in pixels, perspective distortions are ignored.  \n\n  PersentOfScreenHeight: Billboarded orientation, constant width measured in persents of Screen.height , perspective distortions are ignored. ");


    //    protected void DrawIntendedProperty(SerializedProperty root, string prpname,   string tooltip, ref Rect position) {
    //        SerializedProperty p = root.FindPropertyRelative(prpname);
    //        float height = EditorGUI.GetPropertyHeight(p, false);
    //        position.height = height;
    //        GUIContent c = new GUIContent(p.displayName, tooltip);
    //        //Debug.Log(pos);
    //        EditorGUI.PropertyField(position, p, c, false);

    //        position.y += height;
    //        position.y += 2;
    //    }

    //    protected void DrawIntendedProperty(SerializedProperty root, string prpname, GUIContent c, ref Rect position) {
    //        SerializedProperty p = root.FindPropertyRelative(prpname);
    //        float height = EditorGUI.GetPropertyHeight(p, false);
    //        position.height = height;
 
    //        //Debug.Log(pos);
    //        EditorGUI.PropertyField(position, p, c, false);

    //        position.y += height;
    //        position.y += 2;
    //    }

    //}

    //[CustomPropertyDrawer(typeof(LinesSerializable))]
    //public class LinesSerializableDrawer : LinefySerializedClassPropertyDrawerBase {
    //    //public 

    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            
    //        //string str = property.stringValue;
    //        EditorGUI.BeginProperty(position, label, property);
    //        Rect headerRect = position;
    //        headerRect.height = 18;
    //        EditorGUI.PropertyField(headerRect, property, label, false);
    //        if (property.isExpanded) {
    //            Rect r = position;
    //            r.x += 15;
    //            r.width -= 15;
    //            r.y += EditorGUI.GetPropertyHeight(property, false);
    //            DrawIntendedProperty(property, "renderOrder", c_renderOrder, ref r);
    //            DrawIntendedProperty(property, "transparent", "If true will be use the unlit, transparent shader, otherwise the unlit, opaque with alpha clipping.", ref r);
    //            DrawIntendedProperty(property, "colorMultiplier", "The main color. An each encapsulated item will multiplied by this color.", ref r);
    //            DrawIntendedProperty(property, "viewOffset", "Shifts all vertices along the view direction by this value. Useful for preventing z-fight of surfaces", ref r);
    //            DrawIntendedProperty(property, "depthOffset", "A wrapper for shader Offset", ref r);
    //            DrawIntendedProperty(property, "fadeAlphaDistanceFrom", "The distance to camera which transparency fading start", ref r);
    //            DrawIntendedProperty(property, "fadeAlphaDistanceTo", "The distance to camera which transparency fading end", ref r);
    //            DrawIntendedProperty(property, "zTest", "How should depth testing be performed. An wrapper shader ZTest property", ref r);
    //            DrawIntendedProperty(property, "capacityChangeStep", "Determines how often the internal arrays capacity will change when count changes. " +
    //            "A lower value saves the GPU performance, but leads to a frequent allocation of memory when the count changing. " +
    //            "Set CapacityChangeStep = 1 in case of you do not plan to dynamically change the count. ", ref r);
 
    //            DrawIntendedProperty(property, "widthMultiplier", "The main multiplier for the width of all elements. Note that the units of this value are controlled by the widthMode.", ref r);
                 
    //            DrawIntendedProperty(property, "widthMode", c_widthMode, ref r);
    //            //a wrapper  for shader Offset
    //            //viewOffset - shifts all vertices along the direction of the camera view. Useful for preventing z-fight of surfaces.
    //            // DrawIntendedProperty(property, "transparent", "r rrr", ref r);
    //        }

    //        //   Worldspace : Billboarded orientation, width measured in worldspace units , respects an perspective distortion Pixels: Billboarded orientation, constant width measured in pixels, perspective distortions are ignored. PersentOfScreenHeight: Billboarded orientation, constant width measured in persents of Screen.height , perspective distortions are ignored.

    //        //EditorGUI.PropertyField(position, property, label, true );
    //        //position.y += EditorGUI.GetPropertyHeight(SerializedPropertyType.Float, new GUIContent());
    //        //EditorGUI.PropertyField(position, property.FindPropertyRelative("renderOrder"), new GUIContent("text", "tooltip"));
    //        //GUI.Label(position, string.Format("{0}: {1}", label.text, str), EditorStyles.helpBox);
    //        EditorGUI.EndProperty();
    //    }

    //    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
    //        Matrix4x4InspectorAttribute a = attribute as Matrix4x4InspectorAttribute;
    //        float result = EditorGUI.GetPropertyHeight(property, true);
    //        //EditorGUI.GetPropertyHeight(SerializedPropertyType.Float, new GUIContent());

    //        //Debug.Log(result);
    //        //if (property.isExpanded) {
    //        //    result *= 2;

    //        //}
    //         return 300;
    //    }
    //}
}
