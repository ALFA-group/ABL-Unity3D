using UnityEngine;
using Linefy.Internal;
using Linefy.Serialization;

namespace Linefy {

    public class Dots : PrimitivesGroup {

        Vector3[] mpos;
        Vector3[] mnorm;
        Color[] mcolors;
        Vector2[] muvs0;
        Vector2[] muvs2;
        int[] rectIDs;
        int[] mtriangles;

        bool posDirty;
        bool colorsDirty;
        bool normalsOrWidthDirty;
        bool uvsDirty;
        bool uvs2Dirty;
        bool topologyDirty;

        protected override void SetDirtyAttributes() {
            boundsDirty = true;
            posDirty = true;
            colorsDirty = true;
            normalsOrWidthDirty = true;
            uvsDirty = true;
            uvsDirty = true;
            topologyDirty = true;
        }

        const int vertsStride = 4;
        const int trisStride = 6;
        int atlasModificationsHash = 0;


        DotsAtlas _atlas = null;

        /// <summary>
        /// The used DotsAtlas. If null then used default atlas that located in Assets\Plugins\Linefy\Resources\Default DotsAtlas
        /// </summary>
        public DotsAtlas atlas {
            get {
                return _atlas;
            }

            set {
                if (value == null) {
                    value = DotsAtlas.Default;
                    atlasModificationsHash = value.modificationHash - 1;
                }

                if (_atlas != value) {
                    _atlas = value;
                    atlasModificationsHash = _atlas.modificationHash - 1;
                }

            }
        }
 
 
        Vector3 np0 = new Vector3(-5f, 5f, 1);
        Vector3 np1 = new Vector3(-5f, -5f, 1);
        Vector3 np2 = new Vector3(5f, -5f, 1);
        Vector3 np3 = new Vector3(5f, 5f, 1);

        Color whiteColor = Color.white;
 		
		public Dots(SerializationData_Dots data) {
            //CreateNonSerializedData();
            LoadSerializationData(data);
        }

        public Dots( int count ) {
            atlas = DotsAtlas.Default;
            //CreateNonSerializedData();
            this.count = count;
        }

        public Dots(int count, bool transparent) {
            atlas = DotsAtlas.Default;
            //CreateNonSerializedData();
            this.transparent = transparent;
            this.count = count;
        }

        public Dots(string name, int count, DotsAtlas atlas) {
            base.name = name;
            this.atlas = atlas;
            //CreateNonSerializedData();
            this.count = count;
        }

        public Dots( int count, DotsAtlas atlas) {
            this.atlas = atlas;
           // CreateNonSerializedData();
            this.count = count;
        }

        public Dots(int count, DotsAtlas atlas, bool transparent) {
            this.atlas = atlas;
            //CreateNonSerializedData();
            this.transparent = transparent;
            this.count = count;
        }
 
        public override int maxCount {
            get {
#if UNITY_2017_3_OR_NEWER
                return 160000;
#else
                return 16000;
#endif
            }
        }

        protected override void SetCount(int prevCount) {
            for (int i = 0; i< capacity; i++) {
                SetEnabledUnchecked(i, i < _count);
            }
        }

        protected override void SetCapacity(int _prevCapacity ) {
            int newVertsCapacity = capacity * vertsStride;
            System.Array.Resize(ref mpos, newVertsCapacity);
            System.Array.Resize(ref mnorm, newVertsCapacity);
            System.Array.Resize(ref mcolors, newVertsCapacity);
            System.Array.Resize(ref muvs0, newVertsCapacity);
            System.Array.Resize(ref muvs2, newVertsCapacity);
            System.Array.Resize(ref rectIDs, capacity);

            int newTriangleCapacity = capacity * trisStride;
            System.Array.Resize(ref mtriangles, newTriangleCapacity);
    
            for (int i = _prevCapacity; i< capacity; i++) {
                int vid0 = i * vertsStride;
                int vid1 = vid0+1;
                int vid2 = vid0+2;
                int vid3 = vid0+3;
 
                mnorm[vid0] = np0;
                mnorm[vid1] = np1;
                mnorm[vid2] = np2;
                mnorm[vid3] = np3;

                mcolors[vid0] = whiteColor;
                mcolors[vid1] = whiteColor;
                mcolors[vid2] = whiteColor;
                mcolors[vid3] = whiteColor;
 
                int toffset = i * trisStride;
                mtriangles[toffset] = vid0;
                mtriangles[toffset + 1] = vid1;
                mtriangles[toffset + 2] = vid2;
                mtriangles[toffset + 3] = vid0;
                mtriangles[toffset + 4] = vid2;
                mtriangles[toffset + 5] = vid3;
                SetRectIndexUnchecked(i, 0);
            }

            SetDirtyAttributes();
        }

        /// <summary>
        /// set the dot visiblity
        /// </summary>
        public void SetEnabled(int dotIdx, bool enabled) {
            if (validateDotIdx(dotIdx)) {
                SetEnabledUnchecked(dotIdx, enabled);
            }
        }

        void SetEnabledUnchecked(int dotIdx, bool enabled) {
            int vid0 = dotIdx * vertsStride;
            int vid1 = vid0 + 1;
            int vid2 = vid0 + 2;
            int vid3 = vid0 + 3;
            float w = enabled ? 1 : 0;
            mnorm[vid0].z = w;
            mnorm[vid1].z = w;
            mnorm[vid2].z = w;
            mnorm[vid3].z = w;
            normalsOrWidthDirty = true;
         }

        protected override void PreDraw() {
            base.PreDraw();
            if (topologyDirty) {
                mesh.Clear();
            }

            if (atlas.modificationHash != atlasModificationsHash) {
                for (int i = 0; i < count; i++) {
                    SetRectIndex(i, this[i].rectIndex);
                }
                atlasModificationsHash = atlas.modificationHash;
            }
  
            texture = atlas.texture;

            if (posDirty) {
                mesh.vertices = mpos;
                posDirty = false;
                boundsDirty = true;
            }

            if (colorsDirty) {
                mesh.colors = mcolors;
                colorsDirty = false;
            }

            if (uvsDirty) {
                mesh.uv = muvs0;
                uvsDirty = false;
            }

            if (normalsOrWidthDirty) {
                mesh.normals = mnorm;
                normalsOrWidthDirty = false;
            }

            if (uvs2Dirty) {
                mesh.uv2 = muvs2;
                uvs2Dirty = false;
            }

            if (topologyDirty) {
                mesh.triangles = mtriangles;
                topologyDirty = false;
            }

            if (boundsDirty) {
                if (boundSize <= 0) {
                    mesh.RecalculateBounds();
                    mBounds = mesh.bounds;
                }
                mesh.bounds = mBounds;
                boundsDirty = false;
            }
        }

        public void SetPosition(int dotIdx, Vector3 position ) {
            if (validateDotIdx(dotIdx)) {
                int vid0 = dotIdx * 4;
                int vid1 = vid0 + 1;
                int vid2 = vid0 + 2;
                int vid3 = vid0 + 3;
                mpos[vid0] = position;
                mpos[vid1] = position;
                mpos[vid2] = position;
                mpos[vid3] = position;
                posDirty = true;
            }
        }

        public void SetPositionAndWidth(int dotIdx, Vector3 position, float width) {
            if (validateDotIdx(dotIdx)) {
                float halfWidth = 0.5f * width;
                int vid0 = dotIdx * 4;
                int vid1 = vid0 + 1;
                int vid2 = vid0 + 2;
                int vid3 = vid0 + 3;
                mpos[vid0] = position;
                mpos[vid1] = position;
                mpos[vid2] = position;
                mpos[vid3] = position;
                mnorm[vid0].x = -halfWidth;
                mnorm[vid0].y = halfWidth;
                mnorm[vid1].x = -halfWidth;
                mnorm[vid1].y = -halfWidth;
                mnorm[vid2].x = halfWidth;
                mnorm[vid2].y = -halfWidth;
                mnorm[vid3].x = halfWidth;
                mnorm[vid3].y = halfWidth;
                normalsOrWidthDirty = true;
                posDirty = true;
            }
        }

        public void SetWidth(int dotIdx, float width ) {
            if (validateDotIdx(dotIdx)) {
                int vid0 = dotIdx * 4;
                int vid1 = vid0 + 1;
                int vid2 = vid0 + 2;
                int vid3 = vid0 + 3;
                float halfWidth = 0.5f * width;
                mnorm[vid0].x = -halfWidth;
                mnorm[vid0].y = halfWidth;
                mnorm[vid1].x = -halfWidth;
                mnorm[vid1].y = -halfWidth;
                mnorm[vid2].x =  halfWidth;
                mnorm[vid2].y =  -halfWidth;
                mnorm[vid3].x = halfWidth;
                mnorm[vid3].y = halfWidth;
                normalsOrWidthDirty = true;
            }
        }

        public void SetSize(int dotIdx, Vector2 size) {
            if (validateDotIdx(dotIdx)) {
                int vid0 = dotIdx * 4;
                int vid1 = vid0 + 1;
                int vid2 = vid0 + 2;
                int vid3 = vid0 + 3;
                float halfWidth = 0.5f * size.x;
                float halfHeight = 0.5f * size.y;
                mnorm[vid0].x = -halfWidth;
                mnorm[vid0].y = halfHeight;
                mnorm[vid1].x = -halfWidth;
                mnorm[vid1].y = -halfHeight;
                mnorm[vid2].x = halfWidth;
                mnorm[vid2].y = -halfHeight;
                mnorm[vid3].x = halfWidth;
                mnorm[vid3].y = halfHeight;
                normalsOrWidthDirty = true;
            }
        }

        public void SetPixelOffset(int dotIdx, Vector2 pixelOffset) {
            if (validateDotIdx(dotIdx)) {
                int vid0 = dotIdx * 4;
                int vid1 = vid0 + 1;
                int vid2 = vid0 + 2;
                int vid3 = vid0 + 3;
                muvs2[vid0]  = pixelOffset;
                muvs2[vid1]  = pixelOffset;
                muvs2[vid2]  = pixelOffset;
                muvs2[vid3]  = pixelOffset;
                uvs2Dirty = true;
            }
        }

        public void SetColor(int dotIdx, Color color) {
            if (validateDotIdx(dotIdx)) {
                int vid0 = dotIdx * 4;
                int vid1 = vid0 + 1;
                int vid2 = vid0 + 2;
                int vid3 = vid0 + 3;
                mcolors[vid0] = color;
                mcolors[vid1] = color;
                mcolors[vid2] = color;
                mcolors[vid3] = color;
                colorsDirty = true;
            }
        }

        public Color GetColor(int dotIdx) {
            return mcolors[dotIdx * 4];
        }

        public void SetRectIndex(int dotIdx, int rectIndex) {
            if (validateDotIdx(dotIdx)) {
                int vid0 = dotIdx * 4;
                int vid1 = vid0 + 1;
                int vid2 = vid0 + 2;
                int vid3 = vid0 + 3;
                rectIDs[dotIdx] = rectIndex;
                DotsAtlas.Rect ri = atlas.rects[MathUtility.RoundedArrayIdx(rectIndex, atlas.rects.Length)];
                muvs0[vid0] = ri.v0;
                muvs0[vid1] = ri.v1;
                muvs0[vid2] = ri.v2;
                muvs0[vid3] = ri.v3;
                uvsDirty = true;
            }
        }

        public void SetRectIndexUnchecked(int dotIdx, int rectIndex) {
            int vid0 = dotIdx * 4;
            int vid1 = vid0 + 1;
            int vid2 = vid0 + 2;
            int vid3 = vid0 + 3;
            rectIDs[dotIdx] = rectIndex;
            DotsAtlas.Rect ri = atlas.rects[rectIndex % atlas.rects.Length];
            muvs0[vid0] = ri.v0;
            muvs0[vid1] = ri.v1;
            muvs0[vid2] = ri.v2;
            muvs0[vid3] = ri.v3;
            uvsDirty = true;
        }

        public Dot this [int dotIdx]{
            get {
                if (validateDotIdx(dotIdx)) {
                    Dot result;
                    int ida = dotIdx * 4;
                    result.position = mpos[ida];
                    result.color = mcolors[ida];
                    result.rectIndex = rectIDs[dotIdx];
                    
                    result.size2d = mnorm[ida+3];
                    result.enabled = mnorm[ida].z == 1;
                    result.offset = muvs2[ida];
                    return result;
                } else {
                    return new Dot();
                }
            }

            set {
                if (validateDotIdx(dotIdx)) {
                    int vid0 = dotIdx * 4;
                    int vid1 = vid0 + 1;
                    int vid2 = vid0 + 2;
                    int vid3 = vid0 + 3;
                    float w =   value.enabled?  1 : 0;
                    Vector2 halfSize = value.size2d * 0.5f;
                    mnorm[vid0] = new Vector3( -halfSize.x, +halfSize.y, w);
                    mnorm[vid1] = new Vector3( -halfSize.x, -halfSize.y, w);
                    mnorm[vid2] = new Vector3( halfSize.x, -halfSize.y, w);
                    mnorm[vid3] = new Vector3( halfSize.x, halfSize.y, w);
                    mpos[vid0] = value.position;
                    mpos[vid1] = value.position;
                    mpos[vid2] = value.position;
                    mpos[vid3] = value.position;
                    rectIDs[dotIdx] = value.rectIndex;
                    DotsAtlas.Rect ri = atlas.rects[ MathUtility.RoundedArrayIdx(value.rectIndex, atlas.rects.Length) ];
					//atlas.rects[ value.rectIndex % atlas.rects.Length ];
                    muvs0[vid0] = ri.v0;
                    muvs0[vid1] = ri.v1;
                    muvs0[vid2] = ri.v2;
                    muvs0[vid3] = ri.v3;
                    muvs2[vid0] = value.offset;
                    muvs2[vid1] = value.offset;
                    muvs2[vid2] = value.offset;
                    muvs2[vid3] = value.offset;
                    mcolors[vid0] = value.color;
                    mcolors[vid1] = value.color;
                    mcolors[vid2] = value.color;
                    mcolors[vid3] = value.color;
                    posDirty = true;
                    colorsDirty = true;
                    normalsOrWidthDirty = true;
                    uvsDirty = true;
                    uvs2Dirty = true;
                }
            }
        }

        protected override string opaqueShaderName() {
            if (widthMode == WidthMode.WorldspaceBillboard) {
                return "Hidden/Linefy/DotsWorldspaceBillboard";
            }
            if (widthMode == WidthMode.WorldspaceXY) {
                return "Hidden/Linefy/DotsWorldspaceXY";
            }
            if (_pixelPerfect) {
                return "Hidden/Linefy/DotsPixelPerfectBillboard";
            }
            return "Hidden/Linefy/DotsPixelBillboard";
        }

        protected override string transparentShaderName() {
            if (widthMode == WidthMode.WorldspaceBillboard) {
                return "Hidden/Linefy/DotsTransparentWorldspaceBillboard";
            }
            if (widthMode == WidthMode.WorldspaceXY) {
                return "Hidden/Linefy/DotsTransparentWorldspaceXY";
            }
            if (_pixelPerfect) {
                return "Hidden/Linefy/DotsTransparentPixelPerfectBillboard";
            }
            return "Hidden/Linefy/DotsTransparentPixelBillboard";

        }

        bool _pixelPerfect;
        /// <summary>
        /// Enables pixel perfect rendering mode, which ensures that the onscreen size and defined dot size are always the same. Only works for widthMode == PixelsBillboard.
        /// </summary>
        public bool pixelPerfect {
            get {
                return _pixelPerfect;
            }

            set {
                if (value != _pixelPerfect) {
                    _pixelPerfect = value;
                    ResetMaterial();
                }
            }
        }

        /// <summary>
        /// Reads and apply inputData to this Dots instance (deserialization)
        /// </summary>
        public void LoadSerializationData(SerializationData_Dots inputData) {
            if (inputData == null) {
                Debug.LogError("Dots.SetSerializableData (inputData)  data == null");
            } else {
                base.LoadSerializationData( inputData );
                atlas = inputData.atlas;
                texture = atlas.texture;
                pixelPerfect = inputData.pixelPerfect;
            }
        }

        /// <summary>
        /// Writes the current Dots properties to the outputData (serialization)
        /// </summary>
        public void SaveSerializationData(SerializationData_Dots outputData ) {
            if (outputData == null) {
                Debug.LogError("Dots.GetSerializableData (outputData)  data == null");
            } else {
                base.SaveSerializationData(outputData);
                outputData.atlas = atlas;
                outputData.pixelPerfect = pixelPerfect;
            }
        }

        bool validateDotIdx(int dotIdx) {
            bool result = true;
#if UNITY_EDITOR
            if (_count == 0 || dotIdx < 0 || dotIdx >= _count) {
                result = false;
                Debug.LogWarningFormat("Index {0} is out of range. Dots count {1}", dotIdx, _count);
            }

#endif
            return result;
        }
 
        protected override void OnAfterMaterialCreated() {
            base.OnAfterMaterialCreated();
 
        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            dotsCount += 1;
            totalDotsCount += count;
        }

        public int GetNearestXY(Vector2 point, ref float dist) {
            float minDist = float.MaxValue;
            int result = 0;
            for (int i = 0; i < _count; i++) {
                float d = Vector2.Distance(point, mpos[i*vertsStride]);
                if (d < minDist) {
                    result = i;
                    dist = d;
                    minDist = dist;
                }
            }
            return result;
        }
    }
}
