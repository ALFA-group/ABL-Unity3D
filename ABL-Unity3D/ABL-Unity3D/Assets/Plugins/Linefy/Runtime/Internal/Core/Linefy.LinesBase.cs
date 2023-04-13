using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Serialization;


namespace Linefy {

    /// <summary>
    ///  Base class for Lines and Polylines
    /// </summary>
    public abstract class LinesBase : PrimitivesGroup {
        int id_feather = Shader.PropertyToID("_Feather");
        int id_textureScale = Shader.PropertyToID("_TextureScale");
        int id_textureOffset = Shader.PropertyToID("_TextureOffset");

        float _feather = 1;
        /// <summary>
        //The smoothness of the edges.Works only when transparent material is on.This value defines the distance the color.alpha decays from the edge of the line.Can be used to draw  anti-aliased lines.   
        //When used widthMode: PixelsBillboard, WorldspaceBillboard , PercentOfScreenHeight is measured with onscreen pixels.  
        //When used  widthMode: WorldspaceXY -  in world units.
        /// </summary>
        public float feather {
            get {
                return _feather;
            }

            set {
                if (_feather != value) {
                    _feather = value;
                    if (transparent) {
                        material.SetFloat(id_feather, _feather);
                        //material.SetFloat("_Feather", value);
                    }
                }

            }
        }

        float _textureScale = 1;
        /// <summary>
        /// The multiplier of x texture coordinate (also known as tiling).  
        /// </summary>
        public float textureScale {
            get {
                return _textureScale;
            }

            set {
                if (value != _textureScale) {
                    _textureScale = value;
                    material.SetFloat(id_textureScale, _textureScale);
                }
            }
        }

        float _textureOffset;
        /// <summary>
        /// The offset of x texture coordinate.  
        /// </summary>
        public float textureOffset {
            get {
                return _textureOffset;
            }

            set {
                if (_textureOffset != value) {
                    _textureOffset = value;
                    material.SetFloat(id_textureOffset, _textureOffset);
                }
            }
        }

        protected bool _autoTextureOffset;

        /// <summary>
        /// When on the textures offset foreach vertex will recalculated automatically when its positions changed
        /// </summary>
        public virtual bool autoTextureOffset {
            get {
                return _autoTextureOffset;
            }

            set {
                if (_autoTextureOffset != value) {
                    _autoTextureOffset = value;
                }
            }

        }

        protected virtual void OnAutoTextureOffsetChanged() { 
            
        }

        protected override void OnAfterMaterialCreated() {
            base.OnAfterMaterialCreated();
			material.SetTexture("_MainTex", _texture);
            material.SetFloat(id_feather, _feather);
            material.SetFloat(id_textureScale, _textureScale);
            material.SetFloat(id_textureOffset, _textureOffset);
        }

        [System.Obsolete("SetVisualPropertyBlock is Obsolete , use LoadSerializationData instead")]
        public override void SetVisualPropertyBlock(VisualPropertiesBlock block) {
            base.SetVisualPropertyBlock(block);
            this.feather = block.feather;
            this.widthMultiplier = block.widthMuliplier;
            this.texture = block.texture;
        }

        /// <summary>
        /// Read and apply LinesBase data (deserialization)
        /// </summary>
        public void LoadSerializationData(SerializationData_LinesBase inputData) {
            base.LoadSerializationData((SerializationData_PrimitivesGroup)inputData);
            feather = inputData.feather;
            textureScale = inputData.textureScale;
            textureOffset = inputData.textureOffset;
            autoTextureOffset = inputData.autoTextureOffset;
        }

        /// <summary>
        ///  Save LinesBase data (serialization)
        /// </summary>
        public void SaveSerializationData(SerializationData_LinesBase outputData) {
            base.SaveSerializationData((SerializationData_PrimitivesGroup)outputData);
            outputData.feather = feather;
            outputData.textureScale = textureScale;
            outputData.textureOffset = textureOffset;
            outputData.autoTextureOffset = autoTextureOffset;
        }

        public virtual float GetDistanceXY( Vector2 point, ref int segmentIdx, ref float segmentPersentage) {
            Debug.LogFormat("Not implemented");
            return float.MaxValue;
        }
    }

}
