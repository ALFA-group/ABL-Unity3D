using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Serialization;

namespace Linefy {
    /// <summary>
    ///  Base class for Lines, Dots, Polyline, PolygonalMesh
    /// </summary>
    public abstract class LinefyDrawcall : Drawable {

 
        int id_depthOffset = Shader.PropertyToID("_DepthOffset");
        int id_viewOffset = Shader.PropertyToID("_ViewOffset");
        int id_zTest = Shader.PropertyToID("_zTestCompare");
        int id_fadeAlphaDistanceFrom = Shader.PropertyToID("_FadeAlphaDistanceFrom");
        int id_fadeAlphaDistanceTo = Shader.PropertyToID("_FadeAlphaDistanceTo");
        int id_colorMultiplier = Shader.PropertyToID("_Color");

        public string name;
        public int itemsModificationId = -1;
        public int propertiesModificationId = -1;
        protected const float defaultBoundsSize = 1000;

        protected Mesh mesh;
 
        protected int _renderOrder;

        /// <summary>
        /// Render queue of material.
        /// </summary>
        public int renderOrder {
            get {
                return _renderOrder;
            }

            set {
                if (value != _renderOrder) {
                    _renderOrder = value;
                    SetRenderQueue();
                }
            }
        }

        protected void SetRenderQueue() {
            if (transparent) {
                material.renderQueue = 3500 + _renderOrder;
            } else {
                material.renderQueue = 2450 + _renderOrder;
            }

        }

        protected bool boundsDirty = true;
        protected Bounds mBounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        public Bounds bounds {
            get {
                return mBounds;
            }
        }

        float _boundsSize = 1000;

        /// <summary>
        /// The bound size. If value is negative then auto recalculation of bounds will performed
        /// </summary>
        public float boundSize {
            get {
                return _boundsSize;
            }

            set {
                if (_boundsSize != value) {
                    boundsDirty = true;
                    _boundsSize = value;
                    if (value > 0) {
                        mBounds = new Bounds(Vector3.zero, Vector3.one * _boundsSize);
                    }
                }
            }
        }

        bool _transparent;
        /// <summary>
        ///Sets opaque or transparent material. When off, an opaque material with alpha clipping is used. Note that transparent mode may affects on object sorting. 
        /// </summary>
        public bool transparent {
            get {
                return _transparent;
            }

            set {
                if (value != _transparent) {
                    _transparent = value;
                    ResetMaterial();
                }
            }
        }

        Material _material;
        protected Material material {
            get {
                if (_disposed) {
                    Debug.LogWarning("Using the object after dispose is not allowed.");
                } else {
                    if (_material == null) {
                        Shader shader;
                        if (transparent) {
                            shader = Shader.Find(transparentShaderName());
                            if (shader == null) {
                                Debug.LogFormat("shader {0} not found", transparentShaderName());
                            }
                        } else {
                            shader = Shader.Find(opaqueShaderName());
                            if (shader == null) {
                                Debug.LogFormat("shader {0} not found", opaqueShaderName());
                            }
                        }


                        _material = new Material(shader);
                        _material.hideFlags = HideFlags.HideAndDontSave;
                        OnAfterMaterialCreated();
                    }
                }
                return _material;
            }
        }

        protected virtual string opaqueShaderName() {
            return "null";
        }

        protected virtual string transparentShaderName() {
            return "null";
        }

        protected void ResetMaterial() {
            if (_material != null) {
                Object.DestroyImmediate(_material);
            }
            _material = null;
        }

        Color _colorMultiplier = Color.white;
        /// <summary>
        /// Main color.    
        /// </summary>
        public Color colorMultiplier {
            get {
                return _colorMultiplier;
            }

            set {
                if (value != _colorMultiplier) {
                    _colorMultiplier = value;
                    material.SetColor(id_colorMultiplier, colorMultiplier);
                }
            }
        }

        protected Texture _texture;
        /// <summary>
        /// Main texture   
        /// </summary>
        public Texture texture {
            get {
                return _texture;
            }

            set {
                _texture = value;
                material.SetTexture("_MainTex", value);
            }
        }


        float _viewOffset = 0;
        /// <summary>
        ///  Shifts all vertices along the view direction by this value. Useful for preventing z-fight
        /// </summary>
        public float viewOffset {
            get {
                return _viewOffset;
            }

            set {
                if (value != _viewOffset) {
                    material.SetFloat(id_viewOffset, value);
                    _viewOffset = value;
                }
            }
        }

        float _depthOffset = 0;
        /// <summary>
        /// material depth offset factor
        /// </summary>
        public float depthOffset {
            get {
                return _depthOffset;
            }

            set {
                if (value != _depthOffset) {
                    material.SetFloat(id_depthOffset, value);
                    _depthOffset = value;
                }
            }
        }

        float _fadeAlphaDistanceFrom = 100000;
        /// <summary>
        /// The distance to camera which transparency fading start.   
        /// </summary>
        public float fadeAlphaDistanceFrom {
            get {
                return _fadeAlphaDistanceFrom;
            }

            set {
                if (value != _fadeAlphaDistanceFrom) {
                    material.SetFloat("_FadeAlphaDistanceFrom", value);
                    _fadeAlphaDistanceFrom = value;
                }
            }
        }

        float _fadeAlphaDistanceTo = 100001;
        /// <summary>
        /// The distance to camera which transparency fading end.   
        /// </summary>
        public float fadeAlphaDistanceTo {
            get {
                return _fadeAlphaDistanceTo;
            }

            set {
                if (value != _fadeAlphaDistanceTo) {
                    material.SetFloat("_FadeAlphaDistanceTo", value);
                    _fadeAlphaDistanceTo = value;
                }
            }
        }

        UnityEngine.Rendering.CompareFunction _zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        /// <summary>
        /// How should depth testing be performed. 
        /// An wrapper for ZTest state  
        /// </summary>
        public UnityEngine.Rendering.CompareFunction zTest {
            get {
                return _zTest;
            }

            set {
                if (_zTest != value) {
                    material.SetInt(id_zTest, (int)value);
                    _zTest = value;
                }
            }
        }

        /// <summary>
        /// Actually sends objects to render. Use inside OnDrawGizmos() and  OnDrawGizmosSelected()  only.
        /// </summary>
        /// <param name="matrix">transformation matrix</param>
        public override void DrawNow(Matrix4x4 matrix) {
            if (_disposed) {
                return;
            }
            if (  material != null ) {
                PreDraw();
                material.SetPass(0);
                Graphics.DrawMeshNow(mesh, matrix);
            } else {
#if UNITY_EDITOR
                //CreateMesh();
                ResetMaterial();
                SetDirtyAttributes();
                DrawNow(matrix);
#endif
            }
        }

        /// <summary>
        /// Actually sends objects to render. Use inside Update() and LateUpdate() only.
        /// </summary>
        /// <param name="matrix">transformation matrix</param>
        /// <param name="cam"> If null (default), the mesh will be drawn in all cameras. Otherwise it will be rendered in the given camera only. </param>
        /// <param name="layer">Layer to use.</param>
        public sealed override void Draw(Matrix4x4 matrix, Camera cam, int layer) {
            if (_disposed) {
                return;
            }
            PreDraw();
            Graphics.DrawMesh(mesh, matrix, material, layer, cam);
        }

        protected virtual void SetDirtyAttributes() {
            Debug.LogWarningFormat("not implemented SetDirtyAttributes() {0}", this.GetType());
        }


 
        protected virtual void PreDraw() {
            if (_disposed) {
                Debug.LogErrorFormat("{0} {1} is disposed. Yon have not to use this instance",  GetType(), name);
            }
            if (mesh == null) {
                mesh = new Mesh();
                mesh.MarkDynamic();
                mesh.hideFlags = HideFlags.HideAndDontSave;
#if UNITY_2017_3_OR_NEWER
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#else
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
#endif
                SetDirtyAttributes();
            }
        }

        protected virtual void OnAfterMaterialCreated() {
            SetRenderQueue();
            material.SetFloat(id_depthOffset, depthOffset);
            material.SetFloat(id_viewOffset, viewOffset);
            material.SetInt(id_zTest, (int)zTest);
            material.SetFloat(id_fadeAlphaDistanceFrom, fadeAlphaDistanceFrom);
            material.SetFloat(id_fadeAlphaDistanceTo, fadeAlphaDistanceTo);
            material.SetColor(id_colorMultiplier, colorMultiplier);
            material.SetTexture("_MainTex", _texture);
        }

        /// <summary>
        /// Destroys incapsulated unmanaged objects. Calling other methods after calling Dispose() will leads to unexpected behaviour. 
        /// </summary>
        public override void Dispose() {
            if (_disposed) {
                // Debug.LogWarning("Using the object after dispose is not allowed.");
            } else {
                if (Application.isPlaying) {
                    Object.Destroy(mesh);
                    Object.Destroy(material);
                } else {
                    Object.DestroyImmediate(mesh);
                    Object.DestroyImmediate(material);
                }
            }
        }

        [System.Obsolete("SetVisualPropertyBlock is Obsolete , use LoadSerializationData instead")]
        public virtual void SetVisualPropertyBlock(VisualPropertiesBlock block) {
            this.colorMultiplier = block.colorMuliplier;
            this.transparent = block.transparent;
            this.zTest = block.zTest;
            this.depthOffset = block.depthOffset;
            this.viewOffset = block.viewOffset;
            this.renderOrder = block.renderOrder;
        }

        /// <summary>
        /// Reads and apply inputData to this LinefyDrawcall instance  (deserialization)
        /// </summary>
        public void LoadSerializationData(SerializationData_LinefyDrawcall inputData) {
            name = inputData.name;
            boundSize = inputData.boundsSize;
            renderOrder = inputData.renderOrder;
            colorMultiplier = inputData.colorMultiplier;
            texture = inputData.texture;
            viewOffset = inputData.viewOffset;
            depthOffset = inputData.depthOffset;
            fadeAlphaDistanceFrom = inputData.fadeAlphaDistance.from;
            fadeAlphaDistanceTo = inputData.fadeAlphaDistance.to;
            zTest = inputData.zTest;
            transparent = inputData.transparent;
        }

        /// <summary>
        /// Writes the current LinefyDrawcall properties to the outputData (serialization)
        /// </summary>
        public void SaveSerializationData(SerializationData_LinefyDrawcall outputData) {
            outputData.name = name;
            outputData.renderOrder = renderOrder;
            outputData.transparent = transparent;
            outputData.colorMultiplier = colorMultiplier;
            outputData.texture = texture;
            outputData.viewOffset = viewOffset;
            outputData.depthOffset = depthOffset;
            outputData.fadeAlphaDistance.from = fadeAlphaDistanceFrom;
            outputData.fadeAlphaDistance.to = fadeAlphaDistanceTo;
            outputData.zTest = zTest;
             
        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            Debug.LogFormat("not implemented LinefyEntity.GetStatistic(...) {0}", GetType());
        }

 
    }
}
