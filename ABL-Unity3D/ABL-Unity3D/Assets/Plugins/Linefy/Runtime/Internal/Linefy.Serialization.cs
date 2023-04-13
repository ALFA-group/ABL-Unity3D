using UnityEngine;
 

namespace Linefy.Serialization {
    [System.Serializable]
    public class SerializationData_LinefyDrawcall {
        [HideInInspector]
        public string name;

        [Tooltip("The bound size. If value is negative then auto recalculation of bounds will performed")]
        public float boundsSize = 1000;

        [Tooltip("Render queue of material.")]
        public int renderOrder;
 
        [Tooltip("Sets opaque or transparent material. When off, an opaque material with alpha clipping is used. Note that transparent mode may affects on object sorting. ")]
        public bool transparent = false;

        [Tooltip("The main color.")]
        public Color colorMultiplier = Color.white;

        [Tooltip("The main texture. ")]
        public Texture texture;

        [Tooltip("Shifts all vertices along the view direction by this value. Useful for preventing z-fight")]
        public float viewOffset;

        [Tooltip("Material depth offset factor.")]
        public float depthOffset;

        [Tooltip("The distance to camera which transparency fading start / end")]
        public RangeFloat fadeAlphaDistance = new RangeFloat(10000, 10001);
 
        [Tooltip("How should depth testing be performed. An wrapper for shader ZTest state")]
        public UnityEngine.Rendering.CompareFunction zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

    }

    [System.Serializable]
    public class SerializationData_PrimitivesGroup : SerializationData_LinefyDrawcall {
        [HideInInspector]
        public int capacityChangeStep = 4;

        [Tooltip("Width factor. The used measuremnt units are defined by the WidthMode")]
        public float widthMultiplier = 1;

        [Tooltip("Algorithm for calculating the Width.")]
        public WidthMode widthMode = WidthMode.PixelsBillboard;
    }

    [System.Serializable]
    public class SerializationData_LinesBase : SerializationData_PrimitivesGroup {
        [Tooltip("The smoothness of the edges.  Works only when transparent material is on. This value defines the distance the color.alpha decays from the edge of the line. Can be used to draw  anti-aliased lines. When used widthMode: PixelsBillboard, WorldspaceBillboard, PercentOfScreenHeight is measured with onscreen pixels. When used  widthMode: WorldspaceXY -  in world units.")]
        public float feather = 2;

        [Tooltip("The multiplier of x texture coordinate (also known as tiling).")]
        public float textureScale = 1;

        [Tooltip("The offset of x texture coordinate.  ")]
        public float textureOffset = 0;

        [Tooltip("When on the textures offset foreach vertex will recalculated automatically when its positions changed")]
        public bool autoTextureOffset;


        public SerializationData_LinesBase(float width, Color color, float feather) {
            this.widthMultiplier = width;
            this.colorMultiplier = color;
            this.feather = feather;
            this.transparent = true;
        }

        public SerializationData_LinesBase() {
        }
    }
 
    /// <summary>
    /// Serialized representation of the Lines properties (except the lines array data)
    /// </summary>
    [System.Serializable]
    public class SerializationData_Lines : SerializationData_LinesBase {
 

        public SerializationData_Lines() { 
		    name = "new Lines";
            transparent = true;
            feather = 2;
            widthMultiplier = 20;
		}

        public SerializationData_Lines(float widthMultiplier, Color color, float feather ) {
            this.widthMultiplier = widthMultiplier;
            this.colorMultiplier = color;
            this.feather = feather;
            this.transparent = true;
        }

        public SerializationData_Lines(float widthMultiplier, Color color ) {
            this.widthMultiplier = widthMultiplier;
            this.colorMultiplier = color;
            this.transparent = false;
        }
    }
 

    /// <summary>
    /// Serialized representation of the Polyline properties 
    /// </summary>
    [System.Serializable]
    public class SerializationData_Polyline : SerializationData_LinesBase {
         
        [Tooltip("If enabled,  connects first and last vertex. ")]
        public bool isClosed = false;
        [Tooltip("The texture offset of the last virtual vertex when the polyline is closed.")]
        public float lastVertexTextureOffset = 1;

        public SerializationData_Polyline() {
            name = "new Polyline";
            transparent = true;
            feather = 2;
            widthMultiplier = 20;
            isClosed = true;
        }

        public SerializationData_Polyline(float width, Color color, float feather, bool isClosed) {
            name = "new Polyline";
            transparent = true;
            this.feather = feather;
            widthMultiplier = width;
            this.colorMultiplier = color;
            this.isClosed = isClosed;
        }

        public SerializationData_Polyline(float lastVertexTextureOffset) {
            name = "new Polyline";
            transparent = true;
            feather = 2;
            widthMultiplier = 20;
            this.lastVertexTextureOffset = lastVertexTextureOffset;
            isClosed = true;
        }
    }
 
    /// <summary>
    /// Serialized representation of the Dots propertyes  
    /// </summary>
    [System.Serializable]
    public class SerializationData_Dots : SerializationData_PrimitivesGroup {
        [Tooltip("Enables  pixel perfect rendering mode, which ensures that the screen pixel size and defined dot size are always the same. Only works for widthMode == PixelsBillboard.")]
        public bool pixelPerfect;

        [Tooltip("The used DotsAtlas. If null then used default atlas that located in Assets/Plugins/Linefy/Resources/Default DotsAtlas")]
        public DotsAtlas atlas;
		
		public SerializationData_Dots(){
			name = "new Dots";
            widthMultiplier = 64;
            transparent = true;
		}
    }
 
    /// <summary>
    /// serialized representation of the PolygonalMesh properties
    /// </summary>
    [System.Serializable]
    public class SerializationData_PolygonalMeshProperties : SerializationData_LinefyDrawcall {
        [Tooltip("Ambient lighting of internal material.  0 = backface is black   1  = backface equals main color ( unlit shading )")]
        [Range(0,1)]
        public float ambient = 1;

        [Tooltip("Defines recalculation algorithm of mesh lighting data (normals and tangens).")]
        public LightingMode lighingMode = LightingMode.Lit;

        [Tooltip("The number of corners in a polygon, greater than or equal to which the polygon will dynamically re-triangulate when its shape changes.")]
        public int dynamicTriangulationThreshold = 4;

        [Tooltip("Defines mesh normals recalculation algorithm (weighted or unweighted).")]
        public NormalsRecalculationMode normalsRecalculationMode = NormalsRecalculationMode.Unweighted;

        [Tooltip("Texture transform. xy = scale zw = offset ")]
        public Vector4 textureTransform = new Vector4(1,1,0,0);

        [Tooltip("Doublesided render mode of internal meaterial")]
        public bool doublesided = true;

        public SerializationData_PolygonalMeshProperties() {
            boundsSize = -1;
        }
    }
}
