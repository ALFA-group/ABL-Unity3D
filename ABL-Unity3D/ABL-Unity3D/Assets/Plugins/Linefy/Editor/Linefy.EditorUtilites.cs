using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace Linefy.Editors.Internal{

    public class SerializedInEditorPrefs {
        protected string name;
        protected string key;
        protected const string prefix = "Polyflow.Linefy.";
        protected System.Action onChange;


        public SerializedInEditorPrefs(string name, System.Action onChange) {
            key = prefix + name;
            this.onChange = onChange;
        }
       
    }

    public class SerializedInEditorPrefs_string : SerializedInEditorPrefs {
        string _value;

        public SerializedInEditorPrefs_string(string name, string defaultValue, System.Action onChange) : base(name, onChange) {
            _value = EditorPrefs.GetString(key, defaultValue);
        }

        public string stringValue {
            get {
                return _value;
            }

            set {
                if (_value != value) {
                    _value = value;
                    EditorPrefs.SetString(key, _value);
                    if (onChange != null) {
                        onChange();
                    }
                }
            }
        }
    }

    public class SerializedInEditorPrefs_float: SerializedInEditorPrefs {
        float _value;

        public SerializedInEditorPrefs_float(string name, float defaultValue, System.Action onChange) : base (name, onChange){
            _value = EditorPrefs.GetFloat(key, defaultValue);
        }

        public float floatValue {
            get {
                return _value;
            }

            set {
                if (_value != value) {
                    _value = value;
                    EditorPrefs.SetFloat(key, _value);
                    if (onChange != null) {
                        onChange();
                    }
                }
            }
        }
    }

    public class SerializedInEditorPrefs_int : SerializedInEditorPrefs {
        int _value;

        public SerializedInEditorPrefs_int(string name, int defaultValue, System.Action onChange) : base(name, onChange) {
            _value = EditorPrefs.GetInt(key, defaultValue);
        }

        public int intValue {
            get {
                return _value;
            }

            set {
                if (_value != value) {
                    _value = value;
                    EditorPrefs.SetInt(key, _value);
                    if (onChange != null) {
                        onChange();
                    }
                }
            }
        }
    }

    public class SerializedInEditorPrefs_vector2 {
        SerializedInEditorPrefs_float fx;
        SerializedInEditorPrefs_float fy;
        System.Action onChange;


        public SerializedInEditorPrefs_vector2(string name, Vector2 defaultValue, System.Action onChange)  {
            this.onChange = onChange;
            fx = new SerializedInEditorPrefs_float(name + ".x", defaultValue.x, null);
            fy = new SerializedInEditorPrefs_float(name + ".y", defaultValue.y, null);
            _vector2Value = new Vector2(fx.floatValue, fy.floatValue);
        }

        Vector2 _vector2Value;
        public Vector2 vector2Value {
            get {
                return _vector2Value;
            }

            set {
                if (value != _vector2Value) {
   
                    _vector2Value = value;
                    fx.floatValue = value.x;
                    fy.floatValue = value.y;
                    if (onChange != null) {
                        onChange();
                    }
                }
            }
        }
    }

    public class SerializedInEditorPrefs_color {
        System.Action onChange;
        SerializedInEditorPrefs_float r;
        SerializedInEditorPrefs_float g;
        SerializedInEditorPrefs_float b;
        SerializedInEditorPrefs_float a;

        Color _colorValue;
        public SerializedInEditorPrefs_color(string name, Color defaultValue, System.Action onChange) {
            this.onChange = onChange;
            r = new SerializedInEditorPrefs_float(name + ".r", defaultValue.r, null);
            g = new SerializedInEditorPrefs_float(name + ".g", defaultValue.g, null);
            b = new SerializedInEditorPrefs_float(name + ".b", defaultValue.b, null);
            a = new SerializedInEditorPrefs_float(name + ".a", defaultValue.a, null);
            _colorValue = new Color(r.floatValue, g.floatValue, b.floatValue, a.floatValue);
        }


        public Color colorValue {
            get {
                return _colorValue;
            }

            set {
                if (value != _colorValue) {
                    _colorValue = value;
                    r.floatValue = value.r;
                    g.floatValue = value.g;
                    b.floatValue = value.b;
                    a.floatValue = value.a;
                    if (onChange != null) {
                        onChange();
                    }
                }
            }
        }
    }

    public class SerializedPropertyField {
        public SerializedProperty prp;
        public GUIContent content;

        public SerializedPropertyField(SerializedObject so, string path, string name, string tooltip) {
            prp = so.FindProperty(path);
            content = new GUIContent(name, tooltip);
        }

        public void DrawGUILayout() {
            EditorGUILayout.PropertyField(prp, content);
        }
    }

    public class AnimatedFoldout  {
        public SerializedProperty prp;
        public GUIContent content;
        AnimBool ab;

        public AnimatedFoldout(SerializedObject so, string path, string name, string tooltip, UnityEngine.Events.UnityAction repaint) {
            prp = so.FindProperty(path);
            content = new GUIContent(name, tooltip);
            ab = new AnimBool(prp.boolValue, repaint);
        }


        public bool BeginDrawFoldout() {
            prp.boolValue = EditorGUILayout.Foldout(prp.boolValue, content);
            ab.target = prp.boolValue;
            if (EditorGUILayout.BeginFadeGroup(ab.faded)) {
                return true;
            }
            return false;
        }

        public void EndDrawFoldout() {
            EditorGUILayout.EndFadeGroup();
        }
    }
 
}