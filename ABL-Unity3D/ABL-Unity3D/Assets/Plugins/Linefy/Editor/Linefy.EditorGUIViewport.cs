using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;

namespace Linefy {
    /// <summary>
    ///  <para>View area of a virtual orthographic scene that can be displayed in the editor GUI in the specified rect. </para> 
    /// For correct display, use this sequence of actions when   EventType.Repaint raised
    /// <para> 1) SetParam () </para>
    /// <para> 2) DrawLocalSpace()/DrawGUIspace .. </para>
    /// <para> 3) Render() </para>
    /// 
    ///   When finished working with the viewport, call Dispose()
    /// </summary>
    public class EditorGUIViewport : IDisposable {
        private struct DrawCommand {
            public Drawable drawable;
            public Matrix4x4 matrix;
            public bool guiSpace;
        }

        private Rect guiRect = new Rect(0, 0, 1024, 1024);
        private Vector2 guiRectCenter;
        private DrawCommand[] commands;
        private Matrix4x4 guiToLocalMatrix = Matrix4x4.identity;
        private Matrix4x4 transform = Matrix4x4.identity;
        private float cameraZOffset = 128;

        private RenderTexture _rt;
        private RenderTexture rt {
            get {
                if (_rt == null) {
                    int targetRTWidth = Mathf.CeilToInt(guiRect.width / 128) * 128;
                    int targetRTHeight = Mathf.CeilToInt(guiRect.height / 128) * 128;
                    _rt = new RenderTexture(targetRTWidth, targetRTHeight, 32);
                    _rt.name = "EditorWindowViewport Render to texture";
                    _rt.wrapMode = TextureWrapMode.Clamp;
                    _rt.format = RenderTextureFormat.ARGB32;
                    _rt.hideFlags = HideFlags.HideAndDontSave;
                    _rt.antiAliasing = 1;
                    _rt.anisoLevel = 0;
                    _rt.filterMode = FilterMode.Point;
                    _rt.useMipMap = false;
                    _rt.memorylessMode = RenderTextureMemoryless.None;
                    _rt.Create();
                    camera.targetTexture = _rt;
                }
                return _rt;
            }
        }

        private Camera _camera = null;
        private Camera camera {
            get {
                if (_camera == null) {
                    GameObject _camgo = new GameObject("Editor Window Viewport Camera Gameobhect");
                    _camgo.hideFlags = HideFlags.HideAndDontSave;
                    _camgo.transform.position = new Vector3(0f, 0f, -1.0f);
                    _camgo.transform.eulerAngles = new Vector3(0, 0, 0);
                    _camera = _camgo.AddComponent<Camera>();
                    _camera.depth = 0;
                    _camera.nearClipPlane = 0.01f;
                    _camera.farClipPlane = 100000f;
                    _camera.scene = EditorSceneManager.NewPreviewScene();
                    _camera.orthographic = true;
                    _camera.orthographicSize = 1f;
                    _camera.clearFlags = CameraClearFlags.SolidColor;
                    _camera.backgroundColor = Color.clear;
                    _camera.allowMSAA = false;
                    _camera.allowHDR = false;
                    _camera.cameraType = CameraType.Preview;
                    _camera.depthTextureMode = DepthTextureMode.None;
                    _camera.transparencySortMode = TransparencySortMode.Default;
                    _camera.enabled = false;
                    _camera.targetTexture = rt;
                    _camera.hideFlags = HideFlags.HideAndDontSave;
                }
                return _camera;
            }
        }

        /// <summary>
        /// viewport background color
        /// </summary>
        public Color backgroundColor {
            get {
                return camera.backgroundColor;
            }

            set {
                camera.backgroundColor = value;
            }
        }

        int commandsCount = 0;
        void addCommand( Drawable drawable, Matrix4x4 matrix, bool guiSpace ) {
            if (commandsCount == commands.Length) {
                System.Array.Resize(ref commands, commands.Length * 2);
            }
            commands[commandsCount].drawable = drawable;
            commands[commandsCount].matrix = matrix;
            commands[commandsCount].guiSpace = guiSpace;
            commandsCount +=1;
        }

        public EditorGUIViewport() {
            commands = new DrawCommand[32];
        }
 
        /// <summary>
        /// Submits Drawable for rendering in local space
        /// </summary>
        /// <param name="drawable"></param>
        /// <param name="matrix"> transformation matrix </param>
        public void DrawLocalSpace( Drawable drawable, Matrix4x4 matrix ) {
            addCommand(drawable, matrix, false);
        }

        /// <summary>
        /// Submits Drawable for rendering in local space with Matrix4x4.Identity 
        /// </summary>
        /// <param name="entity"></param>
        public void DrawLocalSpace(Drawable drawable) {
            addCommand(drawable, Matrix4x4.identity, false);
        }

        /// <summary>
        /// Submits Drawable for rendering in GUI space   
        /// </summary>
        public void DrawGUIspace(Drawable drawable, Matrix4x4 matrix) {
            addCommand(drawable, matrix, true);
        }

        /// <summary>
        /// Convert GUI position to viewporl local position 
        /// </summary>
        /// <param name="guiPoint"></param>
        /// <returns></returns>
        public Vector2 GUItoLocalSpace(Vector2 guiPoint) {
            Vector2 scenePos = guiPoint - guiRect.center;
            return guiToLocalMatrix.MultiplyPoint3x4(scenePos);
        }

        /// <summary>
        /// Setup viewport.
        /// </summary>
        /// <param name="rect"> GUI rect  </param>
        /// <param name="zoom"> view scale </param>
        /// <param name="pan"> view offset </param>
        public void SetParams (Rect rect, float zoom, Vector2 pan ) {
            this.guiRect = rect;
            guiRectCenter = rect.center;
            zoom = Mathf.Max(zoom, 0.1f);
            transform =  Matrix4x4.TRS(pan, Quaternion.identity, Vector3.one * zoom);
            guiToLocalMatrix = transform.inverse * Matrix4x4.Scale(new Vector3(1, -1, 1));
        }

        /// <summary>
        /// Setup viewport.
        /// </summary>
        /// <param name="guiRect"> GUI rect  </param>
        public void SetParams(Rect guiRect) {
            SetParams(guiRect, 1, Vector2.zero);
        }

        /// <summary>
        /// Actualy draw GUI rect with rendered scene
        /// </summary>
        public void Render(  ) {
            if (Event.current.type == EventType.Repaint) {
                int targetRTWidth = Mathf.CeilToInt(guiRect.width / 128) * 128;
                int targetRTHeight = Mathf.CeilToInt(guiRect.height / 128) * 128;

                if (targetRTWidth != rt.width || targetRTHeight != rt.height) {
                    _rt.Release();
                    _rt = null;
                }
 
                float rtwidth = rt.width;
                float rtheight = rt.height;
                float drawScaleWidth = this.guiRect.width / rtwidth;
                float drawScaleHeight = this.guiRect.height / rtheight;

                camera.orthographicSize = (rt.height / 2f);
                Vector3 cameraPos = new Vector3(-(this.guiRect.width - rt.width) / 2  , -(this.guiRect.height - rt.height) / 2  , -cameraZOffset)  ;
                camera.transform.position = cameraPos;
                for (int i = 0; i<commandsCount; i++) {
                    if (commands[i].guiSpace) {
                        Matrix4x4 tm = commands[i].matrix;
                        Vector4 pos = tm.GetColumn(3);
                        pos.x = pos.x - guiRectCenter.x;
                        pos.y = -(pos.y - guiRectCenter.y);
                        tm.SetColumn(3, pos);
                        commands[i].drawable.Draw( tm, camera);
                    } else {
                        commands[i].drawable.Draw(transform * commands[i].matrix, camera);
                    }
                }
                commandsCount = 0;
                camera.Render();
                Graphics.DrawTexture(this.guiRect, rt, new Rect(0, 0, drawScaleWidth, drawScaleHeight), 0, 0, 0, 0);
            }
        }

        public void Dispose() {
            if (_camera != null) {
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_camera.scene);
                EditorSceneManager.ClosePreviewScene(_camera.scene);
           
                _camera.targetTexture = null;
                GameObject cameraGO = _camera.gameObject;
                UnityEngine.Object.DestroyImmediate(_camera);
                UnityEngine.Object.DestroyImmediate(cameraGO);
            }
            if (_rt != null) {
                _rt.Release();
                UnityEngine.Object.DestroyImmediate(_rt);
                _rt = null;
            }
            System.GC.SuppressFinalize(this);
        }
    }
}
