using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Internal;
using Linefy.Serialization;

namespace Linefy {
    public class Lines : LinesBase {
        int id_autoTextureOffset = Shader.PropertyToID("_AutoTextureOffset");
        Vector3[] mpos;
        Vector3[] mnorm;
        Color[] mcolors;
        Vector2[] muvs0;
        Vector2[] muvs2;
        int[] mtriangles;

        bool posDirty;
        bool colorsDirty;
        bool widthDirty;
        bool topologyIsDirty;
        bool visualTopologyIsDirty;

        public override int maxCount {
            get {
#if UNITY_2017_3_OR_NEWER
                return 160000;
#else
                return 16000;
#endif
            }
        }

        protected override void SetDirtyAttributes() {
            boundsDirty = true;
            posDirty = true;
            colorsDirty = true;
            widthDirty = true;
            topologyIsDirty = true;
            visualTopologyIsDirty = true;
        }

        const int vertsStride = 4;
        const int trisStride = 6;

        public Lines(int count) {
            InternalConstructor("", count, false, 0);
        }

        public Lines(int count, bool transparent) {
            InternalConstructor("", count, transparent, 1);
        }

        public Lines(string name, int count, bool transparent, float feather) {
            InternalConstructor(name, count, transparent, feather);
        }

        public Lines( int count, bool transparent, float feather, float widthMult, Color colorMult) {
            InternalConstructor(name, count, transparent, feather);
            this.widthMultiplier = widthMult;
            this.colorMultiplier = colorMult;
        }

        public Lines(string name, Line[] lines, bool transparent, float feather) {
            InternalConstructor(name, lines.Length, transparent, feather);
            for (int i = 0; i < lines.Length; i++) {
                this[i] = lines[i];
            }
        }

        public Lines(SerializationData_Lines data) {
            LoadSerializationData(data);
        }

        void InternalConstructor(string name, int count, bool transparent, float feather) {
            base.name = name;
            base.transparent = transparent;
            this.feather = feather;
            this.count = count;
            for (int i = 0; i< this.count; i++) {
                this[i] = new Line(1, Color.white);
            }
        }

        protected override void SetCapacity(int prevCapacity) {

            int newVerticesLength = capacity * vertsStride;

            System.Array.Resize(ref mpos, newVerticesLength);
            System.Array.Resize(ref mnorm, newVerticesLength);
            System.Array.Resize(ref mcolors, newVerticesLength);
            System.Array.Resize(ref muvs0, newVerticesLength);
            System.Array.Resize(ref muvs2, newVerticesLength);

            int newTrianglesLength = capacity * trisStride;
            System.Array.Resize(ref mtriangles, newTrianglesLength);

            for (int i = prevCapacity; i < capacity; i++) {
                int vid0 = i * vertsStride;
                int vid1 = vid0 + 1;
                int vid2 = vid0 + 2;
                int vid3 = vid0 + 3;

                muvs2[vid0].x = 0;
                muvs2[vid1].x = 1;
                muvs2[vid2].x = 2;
                muvs2[vid3].x = 3;

                int toffset = i * trisStride;
                mtriangles[toffset + 2] = vid3;
                mtriangles[toffset + 1] = vid1;
                mtriangles[toffset] = vid0;
                mtriangles[toffset + 5] = vid2;
                mtriangles[toffset + 4] = vid1;
                mtriangles[toffset + 3] = vid3;
            }

            SetDirtyAttributes();
        }

        protected override void SetCount(int prevCount) {
            int _from = Mathf.Min(prevCount, _count);
            int _to = Mathf.Max(prevCount, _count);
            _from = Mathf.Clamp(_from-1, 0, capacity);
            _to = Mathf.Clamp(_to+1, 0, capacity);
 
            for (int i = _from; i < _to; i++) {
                int enabledId = i < _count ? 0 : 1;
                if (enabledId == 0) {
                    this[i] = new Line(1, Color.white);
                }  
                 int vid0 = i * vertsStride;
                 int vid1 = vid0 + 1;
                 int vid2 = vid0 + 2;
                 int vid3 = vid0 + 3;
                 muvs2[vid0].y = enabledId;
                 muvs2[vid1].y = enabledId;
                 muvs2[vid2].y = enabledId;
                 muvs2[vid3].y = enabledId;
            }
            visualTopologyIsDirty = true;
        }

        protected override void PreDraw() {
            base.PreDraw();

            if (topologyIsDirty) {
                mesh.Clear();
            }

            if (posDirty) {
                mesh.vertices = mpos;
                mesh.normals = mnorm;
                posDirty = false;
                boundsDirty = true;
            }

            if (colorsDirty) {
                mesh.colors = mcolors;
                colorsDirty = false;
            }

            if (widthDirty) {
                mesh.uv =  muvs0;
                widthDirty = false;
            }

            if (visualTopologyIsDirty) {
                mesh.uv2 = muvs2;
                visualTopologyIsDirty = false;
            }

            if (topologyIsDirty) {

                mesh.triangles = mtriangles;
                topologyIsDirty = false;
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

        /// <summary>
        /// Sets start end positions of line  
        /// </summary>
        public void SetPosition(int lineIdx, Vector3 positionA, Vector3 positionB) {
            if (validateLineIdx(lineIdx)) {
                int ida = lineIdx * vertsStride;
                int ida0 = ida + 1;
                int idb = ida + 2;
                int idb0 = ida + 3;
                mpos[ida] = positionA;
                mpos[ida0] = positionA;
                mpos[idb] = positionB;
                mpos[idb0] = positionB;
                mnorm[ida] = positionB;
                mnorm[ida0] = positionB;
                mnorm[idb] = positionA;
                mnorm[idb0] = positionA;
                posDirty = true;
            }
        }

        /// <summary>
        /// Sets the x texure coordinates offset of line start and end  
        /// </summary>
        public void SetTextureOffset(int lineIdx, float textureOffsetA, float textureOffsetB) {
            if (validateLineIdx(lineIdx)) {
                int ida = lineIdx * vertsStride;
                int ida0 = ida + 1;
                int idb = ida + 2;
                int idb0 = ida + 3;
                muvs0[ida].x = textureOffsetA;
                muvs0[ida0].x = textureOffsetA;
                muvs0[idb].x = textureOffsetB;
                muvs0[idb0].x = textureOffsetB;
                widthDirty = true;
            }
        }

        /// <summary>
        /// Sets the line color 
        /// </summary>
        public void SetColor(int lineIdx, Color color) {
            if (validateLineIdx(lineIdx)) {
                int ida = lineIdx * vertsStride;
                int ida0 = ida + 1;
                int idb = ida + 2;
                int idb0 = ida + 3;
                mcolors[ida] = color;
                mcolors[ida0] = color;
                mcolors[idb] = color;
                mcolors[idb0] = color;
                colorsDirty = true;
            }
        }

        /// <summary>
        /// Sets the colors of line start and end  
        /// </summary>
        public void SetColor(int lineIdx, Color colorA, Color colorB) {
            if (validateLineIdx(lineIdx)) {
                int ida = lineIdx * vertsStride;
                int ida0 = ida + 1;
                int idb = ida + 2;
                int idb0 = ida + 3;
                mcolors[ida] = colorA;
                mcolors[ida0] = colorA;
                mcolors[idb] = colorB;
                mcolors[idb0] = colorB;
                colorsDirty = true;
            }
        }

        public void SetColorAlpha(int lineIdx, float alphaA, float alphaB) {
            if (validateLineIdx(lineIdx)) {
                int ida = lineIdx * vertsStride;
                int ida0 = ida + 1;
                int idb = ida + 2;
                int idb0 = ida + 3;
                mcolors[ida].a = alphaA;
                mcolors[ida0].a = alphaA;
                mcolors[idb].a = alphaB;
                mcolors[idb0].a = alphaB;
                colorsDirty = true;
            }
        }

        /// <summary>
        /// Sets the start and end widths of line 
        /// </summary>
        public void SetWidth(int lineIdx, float widthA, float widthB) {
             int ida = lineIdx * vertsStride;
            int ida0 = ida + 1;
            int idb = ida + 2;
            int idb0 = ida + 3;
            muvs0[ida].y = widthA;
            muvs0[ida0].y = widthA;
            muvs0[idb].y = widthB;
            muvs0[idb0].y = widthB;
            widthDirty = true;
        }

        /// <summary>
        /// Sets the width of line
        /// </summary>
        public void SetWidth(int lineIdx, float width) {
            if (validateLineIdx(lineIdx)) {
                int ida = lineIdx * vertsStride;
                int ida0 = ida + 1;
                int idb = ida + 2;
                int idb0 = ida + 3;
                muvs0[ida].y = width;
                muvs0[ida0].y = width;
                muvs0[idb].y = width;
                muvs0[idb0].y = width;
                widthDirty = true;
            }
        }

        /// <summary>
        /// Access the individual Line by index  
        /// </summary>
        /// <param name="lineIdx">index of line</param>
        public Line this[int lineIdx] {
            get {
                Line result = new Line();
                if (validateLineIdx(lineIdx)) {
 
                    int ida = lineIdx * 4;
                    int idb = ida + 2;
                    result.positionA = mpos[ida];
                    result.positionB = mpos[idb];
                    result.colorA = mcolors[ida];
                    result.colorB = mcolors[idb];
                    result.widthA = muvs0[ida].y;
                    result.widthB = muvs0[idb].y;
                    result.textureOffsetA = muvs0[ida].x;
                    result.textureOffsetB = muvs0[ida].x;
                }
                return result;
            }

            set {
                if (validateLineIdx(lineIdx)) {
                    int ida = lineIdx * 4;
                    int ida0 = ida + 1;
                    int idb = ida + 2;
                    int idb0 = ida + 3;
                    mpos[ida] = value.positionA;
                    mpos[ida0] = value.positionA;
                    mpos[idb] = value.positionB;
                    mpos[idb0] = value.positionB;

                    mnorm[ida] = value.positionB;
                    mnorm[ida0] = value.positionB;
                    mnorm[idb] = value.positionA;
                    mnorm[idb0] = value.positionA;

                    mcolors[ida] = value.colorA;
                    mcolors[ida0] = value.colorA;
                    mcolors[idb] = value.colorB;
                    mcolors[idb0] = value.colorB;
                    Vector2 uv0a = new Vector3( value.textureOffsetA, value.widthA );
                    Vector2 uv0b = new Vector3( value.textureOffsetB, value.widthB );
                    muvs0[ida] = uv0a;
                    muvs0[ida0] = uv0a;
                    muvs0[idb] = uv0b;
                    muvs0[idb0] = uv0b;
                    posDirty = true;
                    colorsDirty = true;
                    widthDirty = true;
                }
            }
        }

        protected override string opaqueShaderName() {
            if (widthMode == WidthMode.WorldspaceBillboard) { 
                return "Hidden/Linefy/LinesWorldspaceBillboard";
            }
            if (widthMode == WidthMode.WorldspaceXY) {
                return "Hidden/Linefy/LinesWorldspaceXY";
            }
            return "Hidden/Linefy/LinesPixelBillboard";
        }

        protected override string transparentShaderName() {
            if (widthMode == WidthMode.WorldspaceBillboard) {
                return "Hidden/Linefy/LinesTransparentWorldspaceBillboard";
            }
            if (widthMode == WidthMode.WorldspaceXY) {
                return "Hidden/Linefy/LinesTransparentWorldspaceXY";
            }
            return "Hidden/Linefy/LinesTransparentPixelBillboard";
        }

  
        public override bool autoTextureOffset {
            get {
                return _autoTextureOffset;
            }

            set {
                if (_autoTextureOffset != value) {
                    _autoTextureOffset = value;
                    material.SetFloat(id_autoTextureOffset, _autoTextureOffset?1:0);
                }
            }
        }

        protected override void OnAfterMaterialCreated() {
            base.OnAfterMaterialCreated();
            material.SetFloat(id_autoTextureOffset, _autoTextureOffset ? 1 : 0);
        }

        /// <summary>
        /// Reads and apply inputData to this Lines instance  (deserialization)
        /// </summary>
        public void LoadSerializationData(SerializationData_Lines inputData) {
            if (inputData == null) {
                Debug.LogError("Lines.SetSerializableData (inputData)  inputData  == null");
            } else {
                base.LoadSerializationData(inputData);
                autoTextureOffset = inputData.autoTextureOffset;
            }
        }

        /// <summary>
        /// Writes the current Lines state to the outputData (serialization)
        /// </summary>
        public void SaveSerializationData(ref SerializationData_Lines outputData) {
            if (outputData == null) {
                Debug.LogError("Lines.GetSerializableData (outputData)  outputData == null");
            } else {
                base.SaveSerializationData(outputData);
                outputData.autoTextureOffset = autoTextureOffset;
            }
        }

        bool validateLineIdx( int vertexIdx) {
             bool result = true;
#if UNITY_EDITOR
            if (_count == 0 || vertexIdx < 0 || vertexIdx >= _count) {
                result = false;
                Debug.LogWarningFormat("Index {0} is out of range. Lines count {1}", vertexIdx, _count);
            }
#endif
            return result;
 
        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            linesCount += 1;
            totallinesCount += count; 
        }
    }
}
