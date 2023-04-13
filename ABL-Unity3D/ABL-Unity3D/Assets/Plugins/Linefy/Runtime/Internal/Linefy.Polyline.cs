using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Internal;
using Linefy.Serialization;
using System;
namespace Linefy {


    /// <summary>
    /// Incapsulates an array of PolylineVertex  connected in series by segments.
    /// </summary>
    public partial class Polyline : LinesBase {
 
        struct VertexTopology {
            public int t0;
            public int t1;
            public int t2;
            public int t3;
            public int t4;
            public int t5;
            public int t6;
            public int t7;
            public int t8;
            public int t9;

            public int n0;
            public int n1;
            public int n2;
            public int n3;
            public int n4;
            public int n5;
            public int n6;
            public int n7;
            public int n8;
            public int n9;

            public int p0;
            public int p1;
            public int p2;
            public int p3;
            public int p4;
            public int p5;
            public int p6;
            public int p7;
            public int p8;
            public int p9;

            public int a0;
            public int a1;
            public int a2;
            public int a3;
            public int a4;
            public int a5;
            public int a6;
            public int a7; 
            public int a8;
            public int a9;

            public override string ToString() {
                return string.Format("t[{0} ] p[{1} ] n[{2}]", aToString(tIndices), aToString(pIndices) , aToString(nIndices) ); 
            }

            public string aToString(int[] arr) {
                string r = "t:";
                for (int i = 0; i< arr.Length; i++) {
                    r += string.Format("{0},", arr[i]);
                }
                return r;
            }

            public int[] tIndices {
                get {
                    return new[] {t0, t1, t2, t3, t4, t5, t6, t7, t8, t9 };
                }
            }

            public int[] nIndices {
                get {
                    return new[] { n0, n1, n2, n3, n4, n5, n6, n7, n8, n9 };
                }
            }

            public int[] pIndices {
                get {
                    return new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9 };
                }
            }
        }
  
        bool _isClosed;
        /// <summary>
        /// If enabled, connects first and last vertex. 
        /// </summary>
        public bool isClosed {
            get {
                return _isClosed;
            }

            set {
                if (value != _isClosed) {
                    _isClosed = value;
                    SetCount( _count );
                }
            }
        }

        Vector3[] mpos;
        public Vector4[] mtan;
        Vector3[] mnorm;
        Color[] mcolors;
        Vector2[] muvs0; 
        Vector2[] muvs1;
        Vector2[] muvs2;
        List<Vector4> muvs3 = new List<Vector4>(); 
        int[] mtriangles;
        float _lastVertexTextureOffset;

        /// <summary>
        /// The texture offset of the last virtual vertex when the polyline is closed.
        /// </summary>
        public float lastVertexTextureOffset {
            get {
                return _lastVertexTextureOffset;
            }

            set {
                if (_lastVertexTextureOffset != value) {
                    _lastVertexTextureOffset = value;
                    if (count > 0) {
                        this.SetTextureOffset(count, _lastVertexTextureOffset);
                    } 
                }
            }
        }

        bool posDirty;
        bool colorsDirty;
        bool textureOffsetDirty;
        bool widthDirty;
        bool topologyIsDirty;
        bool visualTopologyIsDirty;

        protected override void SetDirtyAttributes() {
            boundsDirty = true;
            posDirty = true;
            colorsDirty = true;
            textureOffsetDirty = true;
            widthDirty = true;
            topologyIsDirty = true;
            visualTopologyIsDirty = true;
        }

        VertexTopology[] vtopo;
        const int vertsStride = 10;
        const int trisStride = 24;
 
        HashSet<int> dirtyTopoIndices = new HashSet<int>();

        public Polyline(int count ) {
            InternalConstructor("New polyline", count, false, 0, false, null, 1, 4 );
        }

        public Polyline(int count, bool isClosed) {
            InternalConstructor("New polyline", count, false, 0, isClosed, null, 1, 4);
        }

        [System.Obsolete("SetVisualPropertyBlock is Obsolete , use LoadSerializationData instead")]
        public Polyline(int count, VisualPropertiesBlock propertiesBlock) {
            InternalConstructor("New polyline", count, false, 0, false, null, 1, 4);
            this.SetVisualPropertyBlock(propertiesBlock);
        }

        public Polyline(int count, bool transparent, float feather, bool isClosed) {
            InternalConstructor("New polyline", count, transparent, feather, isClosed, null, 1, 4);
        }

        public Polyline(int count, int capacityChangeStep) {
            InternalConstructor("New polyline", count, false, 0, false, null, 1, capacityChangeStep );
        }

        public Polyline(int count, bool transparent, float feather, bool isClosed, Color colorMult, float widthMult ) {
            InternalConstructor("", count, transparent, feather, isClosed, null, 1, 1);
            colorMultiplier = colorMult;
            widthMultiplier = widthMult;
        }

        public Polyline(string name, int count, bool transparent, float feather, bool isClosed, Texture2D texture, float textureScale, int capacityChangeStep ) {
            InternalConstructor(name, count, transparent, feather, isClosed, texture, textureScale, capacityChangeStep);
        }

        public Polyline( SerializationData_Polyline data) {
            //maxCount = CreateMesh() / vertsStride;
            LoadSerializationData(data);
        }

        void InternalConstructor(string name, int count, bool transparent, float feather, bool isClosed, Texture2D texture, float textureScale, int capacityChangeStep ) {
            base.name = name;
            //maxCount = CreateMesh() / vertsStride;
            this.transparent = transparent;
            this.capacityChangeStep = capacityChangeStep;
            this.count = count;
            for (int i = 0; i< this.count; i++) {
                this[i] = new PolylineVertex(Vector3.zero, Color.white, 1, i);
            }
            this.texture = texture;
            this.textureScale = textureScale;
            this.feather = feather;
            this.isClosed = isClosed;
        }

        public override int maxCount {
            get {
#if UNITY_2017_3_OR_NEWER
                return 65000;
#else
                return 6500;
#endif
            }
        }

        protected override void SetCapacity(int prevCapacity ) {
            System.Array.Resize(ref vtopo, capacity);
            int newVertsCapacity = capacity * vertsStride;

            System.Array.Resize(ref mpos, newVertsCapacity);
            System.Array.Resize(ref mnorm, newVertsCapacity);
            System.Array.Resize(ref mtan, newVertsCapacity);
            System.Array.Resize(ref mcolors, newVertsCapacity);
            System.Array.Resize(ref muvs0, newVertsCapacity);
            System.Array.Resize(ref muvs1, newVertsCapacity);
            System.Array.Resize(ref muvs2, newVertsCapacity);
            muvs3.Resize(newVertsCapacity);

            int newTriangleCapacity = capacity * trisStride;
            System.Array.Resize(ref mtriangles, newTriangleCapacity);
 
            for (int i = prevCapacity; i < capacity; i++) {
                int _0 = i * vertsStride;
                int _1 = _0 + 1;
                int _2 = _0 + 2;
                int _3 = _0 + 3;
                int _4 = _0 + 4;
                int _5 = _0 + 5;
                int _6 = _0 + 6;
                int _7 = _0 + 7;
                int _8 = _0 + 8;
                int _9 = _0 + 9;
 
                int toffset = i * trisStride;

                muvs1[_0].x = -1;
                muvs1[_1].x = -2;
                muvs1[_2].x = -3;
                muvs1[_3].x = -4;
                muvs1[_4].x = -5;

                muvs1[_5].x = 1;
                muvs1[_6].x = 2;  
                muvs1[_7].x = 3;  
                muvs1[_8].x = 4;  
                muvs1[_9].x = 5;  

                muvs1[_0].y = 2;
                muvs1[_1].y = 2;
                muvs1[_2].y = 2;
                muvs1[_3].y = 2;
                muvs1[_4].y = 2;

                muvs1[_5].y = 2;
                muvs1[_6].y = 2;
                muvs1[_7].y = 2;
                muvs1[_8].y = 2;
                muvs1[_9].y = 2;


                 mtriangles[toffset + 0] = _0;  
                 mtriangles[toffset + 1] = _2;  
                 mtriangles[toffset + 2] = _1;  
                //mtriangles[toffset + 0] = _1;
                //mtriangles[toffset + 1] = _2;
                //mtriangles[toffset + 2] = _0;

                 mtriangles[toffset + 3] = _0;  
                 mtriangles[toffset + 4] = _1;  
                 mtriangles[toffset + 5] = _6;  
                //mtriangles[toffset + 3] = _6;
                //mtriangles[toffset + 4] = _1;
                //mtriangles[toffset + 5] = _0;

                 mtriangles[toffset + 6] = _0;  
                 mtriangles[toffset + 7] = _6; 
                 mtriangles[toffset + 8] = _5; 
                //mtriangles[toffset + 6] = _5;
                //mtriangles[toffset + 7] = _6;
                //mtriangles[toffset + 8] = _0;

                 mtriangles[toffset + 9] = _5;  
                 mtriangles[toffset + 10] = _6; 
                 mtriangles[toffset + 11] = _7; 
                //mtriangles[toffset + 9] = _7;
                //mtriangles[toffset + 10] = _6;
                //mtriangles[toffset + 11] = _5;

                 mtriangles[toffset + 12] = _3;
                 mtriangles[toffset + 13] = _4;
                 mtriangles[toffset + 14] = _0;
                //mtriangles[toffset + 12] = _0;
                //mtriangles[toffset + 13] = _4;
                //mtriangles[toffset + 14] = _3;

                 mtriangles[toffset + 15] = _3;
                 mtriangles[toffset + 16] = _0;
                 mtriangles[toffset + 17] = _5;
                //mtriangles[toffset + 15] = _5;
                //mtriangles[toffset + 16] = _0;
                //mtriangles[toffset + 17] = _3;

                 mtriangles[toffset + 18] = _3; 
                 mtriangles[toffset + 19] = _5; 
                 mtriangles[toffset + 20] = _8; 
                //mtriangles[toffset + 18] = _8;
                //mtriangles[toffset + 19] = _5;
                //mtriangles[toffset + 20] = _3;

                 mtriangles[toffset + 21] = _8; 
                 mtriangles[toffset + 22] = _5; 
                 mtriangles[toffset + 23] = _9; 
                //mtriangles[toffset + 21] = _9;
                //mtriangles[toffset + 22] = _5;
                //mtriangles[toffset + 23] = _8;
            }

 
            SetDirtyAttributes();
        }

        void setPointType(int vertIdx, int side, int typeId) {
            int vertOffset = vertIdx * vertsStride;
            if (side == 1) {
                vertOffset += 5;
            }
            muvs1[vertOffset].y = typeId;
            muvs1[vertOffset+1].y = typeId;
            muvs1[vertOffset+2].y = typeId;
            muvs1[vertOffset+3].y = typeId;
            muvs1[vertOffset+4].y = typeId;
        }

        void setPointType(int vertIdx,  int typeId) {
            int vertOffset = vertIdx * vertsStride;
            muvs1[vertOffset].y = typeId;
            muvs1[vertOffset + 1].y = typeId;
            muvs1[vertOffset + 2].y = typeId;
            muvs1[vertOffset + 3].y = typeId;
            muvs1[vertOffset + 4].y = typeId;
            muvs1[vertOffset + 5].y = typeId;
            muvs1[vertOffset + 6].y = typeId;
            muvs1[vertOffset + 7].y = typeId;
            muvs1[vertOffset + 8].y = typeId;
            muvs1[vertOffset + 9].y = typeId;
        }

        protected override void SetCount(int _prevCount) {
            FillDirtyTopoIndices(_prevCount);
            foreach (int i in dirtyTopoIndices) {
                SetVTopo(i);
                setPointType(i, i < _count ? 0 : 2);
            }

            if (_count > 0) {
                if (!_isClosed) {
                    setPointType(0, 0, 1);
                    setPointType(Mathf.Max(0, _count - 1), 2);
                    setPointType(Mathf.Max(0, _count - 2), 1, 1);
                }
            }
            if (_prevCount < _count) {
                if (_prevCount > 0) {
                    PolylineVertex last = this[_prevCount - 1];
                    for (int i = _prevCount; i < _count; i++) {
                        this[i] = last;
                    }
                    this[0] = this[0];

                } else {
                    for (int i = Mathf.Max(0, _prevCount); i < _count; i++) {
                        this[i] = new PolylineVertex(Vector3.zero, Color.white, 1);
                    }
                }
            }

            foreach (int i in dirtyTopoIndices) {
                if (i < _count) {
                    this[i] = this[i];
                }
            }
            float cashedLastVertexTextureOffset = _lastVertexTextureOffset;
            _lastVertexTextureOffset -= 1;
            lastVertexTextureOffset = cashedLastVertexTextureOffset;
            visualTopologyIsDirty = true;
        }

        void FillDirtyTopoIndices(int _pCount ) {
             dirtyTopoIndices.Clear();
            if (_count > 0) {
                dirtyTopoIndices.Add(0);
            }
            if (_count > 1) {
                dirtyTopoIndices.Add(1);
            }
  
            int _from = Mathf.Min(_pCount, _count);
            int _to = Mathf.Max(_pCount, _count);

 
            int _ifrom = Mathf.Clamp(_from - 3, 0, capacity);
            int _ito = Mathf.Clamp(_to + 2, 0, capacity);

            for (int i = _ifrom; i < _ito; i++) {
                dirtyTopoIndices.Add(i);
            }
 
        }

        void SetVTopo(int vertIdx) {
            VertexTopology vt = vtopo[vertIdx];
            getIndices(vertIdx , 1, ref vt.t0, ref vt.t1, ref vt.t2, ref vt.t3, ref vt.t4);
            getIndices(vertIdx + 1, 0, ref vt.t5, ref vt.t6, ref vt.t7, ref vt.t8, ref vt.t9);

            getIndices(vertIdx - 1, 1, ref vt.p0, ref vt.p1, ref vt.p2, ref vt.p3, ref vt.p4);
            getIndices(vertIdx, 0, ref vt.p5, ref vt.p6, ref vt.p7, ref vt.p8, ref vt.p9);

            getIndices(vertIdx - 2, 1,  ref vt.n0, ref vt.n1, ref vt.n2, ref vt.n3, ref vt.n4);
            getIndices(vertIdx - 1, 0, ref vt.n5, ref vt.n6, ref vt.n7, ref vt.n8, ref vt.n9);

            getIndices(vertIdx - 1, 0, ref vt.a0, ref vt.a1, ref vt.a2, ref vt.a3, ref vt.a4);
            getIndices(vertIdx , 1, ref vt.a5, ref vt.a6, ref vt.a7, ref vt.a8, ref vt.a9);

            vtopo[vertIdx] = vt;
        }

        int roundedVertexIdx(int idx) {
            if (_count == 0) {
                return 0;
            }
            idx = idx % _count;
            if (idx < 0) {
                idx = (_count + idx) % _count;
            }
            return idx;
        }

        void getIndices(int pointIdx, int side, ref int i0, ref int i1, ref int i2, ref int i3, ref int i4 ) {
 
            pointIdx = roundedVertexIdx(pointIdx);
 
            int offset = pointIdx * vertsStride;
            if (side == 1) {
                offset += vertsStride / 2;
            }
            i0 = offset;
            i1 = offset + 1;
            i2 = offset + 2;
            i3 = offset + 3;
            i4 = offset + 4;
        }
 
        public PolylineVertex this[int idx] {
            get {
                if (validateVertexIdx(idx)) {
                    int vidx = idx * vertsStride;
                    return new PolylineVertex(mpos[vidx], mcolors[vidx], muvs2[vidx].x, muvs0[vidx].x);
                } else {
                    return new PolylineVertex();
                }
            }

            set {
                if (validateVertexIdx(idx)) {
                    Vector3 pos = value.position;
                    Vector4 posTan = value.position;
                    Color color = value.color;

                    muvs2[vtopo[idx].p0].x = value.width;
                    muvs2[vtopo[idx].p1].x = value.width;
                    muvs2[vtopo[idx].p2].x = value.width;
                    muvs2[vtopo[idx].p3].x = value.width;
                    muvs2[vtopo[idx].p4].x = value.width;
                    muvs2[vtopo[idx].p5].x = value.width;
                    muvs2[vtopo[idx].p6].x = value.width;
                    muvs2[vtopo[idx].p7].x = value.width;
                    muvs2[vtopo[idx].p8].x = value.width;
                    muvs2[vtopo[idx].p9].x = value.width;

                    //muvs2[vtopo[idx].n0].y = value.width;
                    muvs2[vtopo[idx].n1].y = value.width;
                    muvs2[vtopo[idx].n2].y = value.width;
                    muvs2[vtopo[idx].n3].y = value.width;
                    muvs2[vtopo[idx].n4].y = value.width;
                    //muvs2[vtopo[idx].n5].y = value.width;
                    muvs2[vtopo[idx].n6].y = value.width;
                    muvs2[vtopo[idx].n7].y = value.width;
                    muvs2[vtopo[idx].n8].y = value.width;
                    muvs2[vtopo[idx].n9].y = value.width;

                    //mtan[vtopo[idx].t0] = posTan;
                    mtan[vtopo[idx].t1] = posTan;
                    mtan[vtopo[idx].t2] = posTan;
                    mtan[vtopo[idx].t3] = posTan;
                    mtan[vtopo[idx].t4] = posTan;
                    //mtan[vtopo[idx].t5] = posTan;
                    mtan[vtopo[idx].t6] = posTan;
                    mtan[vtopo[idx].t7] = posTan;
                    mtan[vtopo[idx].t8] = posTan;
                    mtan[vtopo[idx].t9] = posTan;

                    mpos[vtopo[idx].p0] = pos;
                    mpos[vtopo[idx].p1] = pos;
                    mpos[vtopo[idx].p2] = pos;
                    mpos[vtopo[idx].p3] = pos;
                    mpos[vtopo[idx].p4] = pos;
                    mpos[vtopo[idx].p5] = pos;
                    mpos[vtopo[idx].p6] = pos;
                    mpos[vtopo[idx].p7] = pos;
                    mpos[vtopo[idx].p8] = pos;
                    mpos[vtopo[idx].p9] = pos;

                    //mnorm[vtopo[idx].n0] = pos;
                    mnorm[vtopo[idx].n1] = pos;
                    mnorm[vtopo[idx].n2] = pos;
                    mnorm[vtopo[idx].n3] = pos;
                    mnorm[vtopo[idx].n4] = pos;
                    //mnorm[vtopo[idx].n5] = pos;
                    mnorm[vtopo[idx].n6] = pos;
                    mnorm[vtopo[idx].n7] = pos;
                    mnorm[vtopo[idx].n8] = pos;
                    mnorm[vtopo[idx].n9] = pos;

                    mcolors[vtopo[idx].p0] = color;
                    mcolors[vtopo[idx].p1] = color;
                    mcolors[vtopo[idx].p2] = color;
                    mcolors[vtopo[idx].p3] = color;
                    mcolors[vtopo[idx].p4] = color;
                    mcolors[vtopo[idx].p5] = color;
                    mcolors[vtopo[idx].p6] = color;
                    mcolors[vtopo[idx].p7] = color;
                    mcolors[vtopo[idx].p8] = color;
                    mcolors[vtopo[idx].p9] = color;

                    Vector4 ac = color;
                    muvs3[vtopo[idx].a0] = ac;
                    muvs3[vtopo[idx].a1] = ac;
                    muvs3[vtopo[idx].a2] = ac;
                    muvs3[vtopo[idx].a3] = ac;
                    muvs3[vtopo[idx].a4] = ac;
                    muvs3[vtopo[idx].a5] = ac;
                    muvs3[vtopo[idx].a6] = ac;
                    muvs3[vtopo[idx].a7] = ac;
                    muvs3[vtopo[idx].a8] = ac;
                    muvs3[vtopo[idx].a9] = ac;

                    muvs0[vtopo[idx].p0].x = value.textureOffset;
                    muvs0[vtopo[idx].p1].x = value.textureOffset;
                    muvs0[vtopo[idx].p2].x = value.textureOffset;
                    muvs0[vtopo[idx].p3].x = value.textureOffset;
                    muvs0[vtopo[idx].p4].x = value.textureOffset;
                    muvs0[vtopo[idx].p5].x = value.textureOffset;
                    muvs0[vtopo[idx].p6].x = value.textureOffset;
                    muvs0[vtopo[idx].p7].x = value.textureOffset;
                    muvs0[vtopo[idx].p8].x = value.textureOffset;
                    muvs0[vtopo[idx].p9].x = value.textureOffset;

                    //muvs0[vtopo[idx].a0].y = value.textureOffset;
                    muvs0[vtopo[idx].a1].y = value.textureOffset;
                    //muvs0[vtopo[idx].a2].y = value.textureOffset;
                    muvs0[vtopo[idx].a3].y = value.textureOffset;
                    //muvs0[vtopo[idx].a4].y = value.textureOffset;

                    //muvs0[vtopo[idx].a5].y = value.textureOffset;
                    muvs0[vtopo[idx].a6].y = value.textureOffset;
                    //muvs0[vtopo[idx].a7].y = value.textureOffset;
                    muvs0[vtopo[idx].a8].y = value.textureOffset;
                    //muvs0[vtopo[idx].a9].y = value.textureOffset;

                    posDirty = true;
                    colorsDirty = true;
                    textureOffsetDirty = true;
                    widthDirty = true;
                }

            }
        }

        public void SetWidth(int idx, float width) {
            if (idx == count) {
                if (isClosed) {
                    int v = count * vertsStride;
                    muvs2[v - 1].x = width;
                    muvs2[v - 2].x = width;
                    muvs2[v - 3].x = width;
                    muvs2[v - 4].x = width;
                    muvs2[v - 5].x = width;
                    muvs2[v - 6].y = width;
                    muvs2[v - 7].y = width;
                    muvs2[v - 8].y = width;
                    muvs2[v - 9].y = width;
                    muvs2[v - 10].y = width;
                    textureOffsetDirty = true;
                }
                return;
            }

            if (validateVertexIdx(idx)) {
                //muvs2[vtopo[idx].p0].x = width;
                muvs2[vtopo[idx].p1].x = width;
                muvs2[vtopo[idx].p2].x = width;
                muvs2[vtopo[idx].p3].x = width;
                muvs2[vtopo[idx].p4].x = width;
                //muvs2[vtopo[idx].p5].x = width;
                muvs2[vtopo[idx].p6].x = width;
                muvs2[vtopo[idx].p7].x = width;
                muvs2[vtopo[idx].p8].x = width;
                muvs2[vtopo[idx].p9].x = width;

                //muvs2[vtopo[idx].n0].y = width;
                muvs2[vtopo[idx].n1].y = width;
                muvs2[vtopo[idx].n2].y = width;
                muvs2[vtopo[idx].n3].y = width;
                muvs2[vtopo[idx].n4].y = width;
                //muvs2[vtopo[idx].n5].y = width;
                muvs2[vtopo[idx].n6].y = width;
                muvs2[vtopo[idx].n7].y = width;
                muvs2[vtopo[idx].n8].y = width;
                muvs2[vtopo[idx].n9].y = width;
                textureOffsetDirty = true;
            }
        }

        public void SetTextureOffset(int idx, float textureOffset) {
            if ( idx == count  && _count>0 ) {
                if (isClosed) {
                    int v = count * vertsStride;
                    muvs0[v - 1].x = textureOffset;
                    muvs0[v - 2].x = textureOffset;
                    muvs0[v - 3].x = textureOffset;
                    muvs0[v - 4].x = textureOffset;
                    muvs0[v - 5].x = textureOffset;
                    muvs0[v - 6].y = textureOffset;
                    muvs0[v - 7].y = textureOffset;
                    muvs0[v - 8].y = textureOffset;
                    muvs0[v - 9].y = textureOffset;
                    muvs0[v - 10].y = textureOffset;
                    textureOffsetDirty = true;
                }
                return;
            }

            if (validateVertexIdx(idx)) {
                muvs0[vtopo[idx].p0].x =  textureOffset;
                muvs0[vtopo[idx].p1].x =  textureOffset;
                muvs0[vtopo[idx].p2].x =  textureOffset;
                muvs0[vtopo[idx].p3].x =  textureOffset;
                muvs0[vtopo[idx].p4].x =  textureOffset;
                muvs0[vtopo[idx].p5].x =  textureOffset;
                muvs0[vtopo[idx].p6].x =  textureOffset;
                muvs0[vtopo[idx].p7].x =  textureOffset;
                muvs0[vtopo[idx].p8].x =  textureOffset;
                muvs0[vtopo[idx].p9].x =  textureOffset;

                //muvs0[vtopo[idx].a0].y =  textureOffset;
                muvs0[vtopo[idx].a1].y =  textureOffset;
                //muvs0[vtopo[idx].a2].y =  textureOffset;
                muvs0[vtopo[idx].a3].y =  textureOffset;
                //muvs0[vtopo[idx].a4].y =  textureOffset;
                                            
                //muvs0[vtopo[idx].a5].y =  textureOffset;
                muvs0[vtopo[idx].a6].y =  textureOffset;
                //muvs0[vtopo[idx].a7].y =  textureOffset;
                muvs0[vtopo[idx].a8].y =  textureOffset;
               // muvs0[vtopo[idx].a9].y =  textureOffset;
                textureOffsetDirty = true;
            }
        }

        public void SetPosition(int idx, Vector3 position) {
            if (validateVertexIdx(idx)) {
                //VertexTopology vt = vtopo[idx];
                Vector3 pos = position;
                Vector4 posTan = position;

                //mtan[vtopo[idx].t0] = posTan;
                mtan[vtopo[idx].t1] = posTan;
                mtan[vtopo[idx].t2] = posTan;
                mtan[vtopo[idx].t3] = posTan;
                mtan[vtopo[idx].t4] = posTan;
               // mtan[vtopo[idx].t5] = posTan;
                mtan[vtopo[idx].t6] = posTan;
                mtan[vtopo[idx].t7] = posTan;
                mtan[vtopo[idx].t8] = posTan;
                mtan[vtopo[idx].t9] = posTan;

                //mnorm[vtopo[idx].n0] = pos;
                mnorm[vtopo[idx].n1] = pos;
                mnorm[vtopo[idx].n2] = pos;
                mnorm[vtopo[idx].n3] = pos;
                mnorm[vtopo[idx].n4] = pos;
                //mnorm[vtopo[idx].n5] = pos;
                mnorm[vtopo[idx].n6] = pos;
                mnorm[vtopo[idx].n7] = pos;
                mnorm[vtopo[idx].n8] = pos;
                mnorm[vtopo[idx].n9] = pos;

                mpos[vtopo[idx].p0] = pos;
                mpos[vtopo[idx].p1] = pos;
                mpos[vtopo[idx].p2] = pos;
                mpos[vtopo[idx].p3] = pos;
                mpos[vtopo[idx].p4] = pos;
                mpos[vtopo[idx].p5] = pos;
                mpos[vtopo[idx].p6] = pos;
                mpos[vtopo[idx].p7] = pos;
                mpos[vtopo[idx].p8] = pos;
                mpos[vtopo[idx].p9] = pos;
 
                posDirty = true;
            }
        }

        /// <summary>
        /// Sets the vertex color 
        /// </summary>
        public void SetColor(int idx, Color color) {
            if (validateVertexIdx( idx)) {
 
                mcolors[vtopo[idx].p0] = color;
                mcolors[vtopo[idx].p1] = color;
                mcolors[vtopo[idx].p2] = color;
                mcolors[vtopo[idx].p3] = color;
                mcolors[vtopo[idx].p4] = color;
                mcolors[vtopo[idx].p5] = color;
                mcolors[vtopo[idx].p6] = color;
                mcolors[vtopo[idx].p7] = color;
                mcolors[vtopo[idx].p8] = color;
                mcolors[vtopo[idx].p9] = color;
                colorsDirty = true;
            }
        }

        /// <summary>
        /// Sets the vertex color alpha
        /// </summary>
         public void SetAlpha(int idx, float alpha) {
            if (validateVertexIdx(idx)) {
                mcolors[vtopo[idx].p0].a = alpha;
                mcolors[vtopo[idx].p1].a = alpha;
                mcolors[vtopo[idx].p2].a = alpha;
                mcolors[vtopo[idx].p3].a = alpha;
                mcolors[vtopo[idx].p4].a = alpha;  
                mcolors[vtopo[idx].p5].a = alpha;
                mcolors[vtopo[idx].p6].a = alpha;
                mcolors[vtopo[idx].p7].a = alpha;
                mcolors[vtopo[idx].p8].a = alpha;
                mcolors[vtopo[idx].p9].a = alpha;
                colorsDirty = true;
            }
        }

        protected override void PreDraw() {
            base.PreDraw();
 
            if (topologyIsDirty) {
                mesh.Clear();
            }
         

            if (posDirty) {
                mesh.vertices = mpos;
                mesh.normals = mnorm;
                mesh.tangents = mtan;
                posDirty = false;
                boundsDirty = true;
                if (autoTextureOffset) {
                    RecalculateTextureOffsets();
                }
            }

            if (colorsDirty) {
                mesh.colors = mcolors;
                mesh.SetUVs(3, muvs3);
                colorsDirty = false;
            }

            if (textureOffsetDirty) {
 
                mesh.uv = muvs0;
                textureOffsetDirty = false;
            }

            if (widthDirty) {

                mesh.uv3 = muvs2;
                widthDirty = false;
            }
             
            if (topologyIsDirty) {
                mesh.uv2 = muvs1;
                mesh.triangles = mtriangles;
                topologyIsDirty = false;
                visualTopologyIsDirty = false;
            }

            if (visualTopologyIsDirty) {
                mesh.uv2 = muvs1;
                visualTopologyIsDirty = false;
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
 
        public void AddVertex(PolylineVertex pv) {
            int vertIdx = count;
            count++;
            this[vertIdx] = pv;
        }

        /// <summary>
        /// Adds a new vertex to the end and sets its textureOffset to the textureOffset of 
        /// the previous vertex plus the distance in world coordinates to the new one. 
        /// This function is convenient for creating polylines with proportional of length texture coordinates 
        /// (such as trails, paint strokes) by adding new vertices and avoids recalculating all distances with each addition .
        /// </summary>
        /// <param name="vertex"></param>
        public void AddWithDistance(PolylineVertex vertex) {
            int vertIdx = count;
            count++;
            if (vertIdx > 0) {
                int prevIdx = vertIdx - 1;
                PolylineVertex prevVertex = this[prevIdx];
                Vector3 prevPos = prevVertex.position;
                float distToPrev = Vector3.Distance(prevPos, vertex.position);
 
                vertex.textureOffset = prevVertex.textureOffset + distToPrev;
            }

            this[vertIdx] = vertex;
        }

        bool validateVertexIdx(int vertexIdx) {
            bool result = true;
            #if UNITY_EDITOR
            if (count == 0 || vertexIdx < 0 || vertexIdx >= _count) {
                result = false;
                Debug.LogWarningFormat("Index {0} is out of range. Vertices count {1}", vertexIdx, count);
            }
            #endif
            return result;
        }


        public void RecalculateDistances(float distanceMultiplier) {
            float dist = 0;
            Vector3 prevPos = mpos[0];
  
            for (int v = 0; v < count; v++) {
                int vidx = v * vertsStride;
                Vector3 pos = mpos[vidx];
                dist += Vector3.Distance(prevPos, pos) * distanceMultiplier;
                SetTextureOffset(v, dist);
 
                prevPos = pos;
            }
            if (_isClosed) {
                Vector3 pos = mpos[0];
                dist += Vector3.Distance(prevPos, pos) * distanceMultiplier;
                SetTextureOffset(count, dist);
            }
            textureOffsetDirty = true;
        }


        /// <summary>
        /// Recalculates the texture offset for each point to equal distance of that point from the first point.
        /// </summary>
        public void RecalculateTextureOffsets() {
            float dist = 0;
            Vector3 prevPos = mpos[0];

            for (int v = 0; v < count; v++) {
                int vidx = v * vertsStride;
                Vector3 pos = mpos[vidx];
                dist += Vector3.Distance(prevPos, pos) ;
                SetTextureOffset(v, dist);
                prevPos = pos;
            }
            if (_isClosed) {
                Vector3 pos = mpos[0];
                dist += Vector3.Distance(prevPos, pos) ;
                SetTextureOffset(count, dist);
            }

             textureOffsetDirty = true;
        }


        protected override void OnAutoTextureOffsetChanged() {
            posDirty = true;
        }

        public Color GetColor(int vertexIndex) {
            if (validateVertexIdx(vertexIndex)) {
                int mVertIdx = vertexIndex * vertsStride;
                return mcolors[mVertIdx];
            } else {
                return Color.magenta;
            }
        }

        public float GetDistance(int vertexIndex) {
            if (validateVertexIdx(vertexIndex)) {
                int mVertIdx = vertexIndex * vertsStride;
                return muvs0[mVertIdx].x;
            } else {
                return 0;
            }
        }

        public Vector3 GetPosition(int vertexIndex) {
            if (validateVertexIdx(vertexIndex)) {
                int mVertIdx = vertexIndex * vertsStride;
                return mpos[mVertIdx];
            } else {
                return Vector3.zero;
            }
        }

        protected override string opaqueShaderName() {
            if (widthMode == WidthMode.WorldspaceBillboard) {
                return "Hidden/Linefy/PolylineWorldspaceBillboard";
            }
            if (widthMode == WidthMode.WorldspaceXY) {
                return "Hidden/Linefy/PolylineWorldspaceXY";
            }
            return "Hidden/Linefy/PolylinePixelBillboard";
        }

        protected override string transparentShaderName() {
            if (widthMode == WidthMode.WorldspaceBillboard) {
                return "Hidden/Linefy/PolylineTransparentWorldspaceBillboard";
            }
            if (widthMode == WidthMode.WorldspaceXY) {
                return "Hidden/Linefy/PolylineTransparentWorldspaceXY";
            }
            return "Hidden/Linefy/PolylineTransparentPixelBillboard";
        }

        /// <summary>
        /// Reads and apply inputData to this Polyline instance  (deserialization)
        /// </summary>
        public void LoadSerializationData(SerializationData_Polyline inputData) {
            if (inputData == null) {
                Debug.LogError("Polyline.SetSerializableData (inputData)  inputData == null )");
            } else {
                base.LoadSerializationData(inputData);
                this.textureOffset = inputData.textureOffset;
                this.isClosed = inputData.isClosed;
                 _lastVertexTextureOffset -= 0.1f;
                this.lastVertexTextureOffset = inputData.lastVertexTextureOffset;
            }
        }

        /// <summary>
        /// Writes the current Polyline state to the outputData (serialization)
        /// </summary>
        public void SaveSerializationData(ref SerializationData_Polyline outputData) {
            if (outputData == null) {
                Debug.LogError("Polyline.GetSerializableData (outputData)  outputData == null");
            } else {
                base.SaveSerializationData(outputData);
                outputData.isClosed = isClosed;
                outputData.textureOffset = textureOffset;
                outputData.lastVertexTextureOffset = _lastVertexTextureOffset;
            }
        }

        protected override void OnAfterMaterialCreated() {
            base.OnAfterMaterialCreated();
        }

        public void DrawInstanced(Matrix4x4[] matrices) {
            PreDraw();
            material.enableInstancing = true;
            Graphics.DrawMeshInstanced(mesh, 0, material, matrices);
            //material.enableInstancing = _enableInstancing;
        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            polylinesCount += 1;
            totalPolylineVerticesCount += count;
        }

        Vector3 getPosition(int idx) {
            int mVertIdx = idx * vertsStride;
            if (idx >= _count) {
                mVertIdx = (idx-1) * vertsStride+ vertsStride/2;
            }
            return mpos[mVertIdx];
        }

        public override float GetDistanceXY(Vector2 point, ref int segmentIdx, ref float segmentPersentage) {
            float minDist = float.MaxValue;
            int minus = _isClosed ? 0 : 1;
            for (int i = 0; i<count - minus; i++) {
                float lv = 0;
                Vector2 a = getPosition(i);
                Vector2 b = getPosition(i+1);
                float d = Edge2D.GetDistance(a, b, point, ref lv);
                if (d < minDist) {
                    segmentIdx = i;
                    segmentPersentage = lv;
                    minDist = d;
                }
            }

            return minDist;
        }

        [Obsolete("TransparentPropertyBlock is Obsolete , use Linefy.Serialization.SerializationData_Polyline and Linefy.Serialization.SerializationDataFull_Polyline instead")]
        public Polyline(TransparentPropertyBlock t) { 
        
        }

        [Obsolete("PolylineSerializableData is Obsolete , use Linefy.Serialization.SerializationData_Polyline and Linefy.Serialization.SerializationDataFull_Polyline instead")]
        public Polyline(PolylineSerializableData t) {

        }


        [Obsolete("TransparentPropertyBlock is Obsolete , use Linefy.Serialization.SerializationData_Polyline and Linefy.Serialization.SerializationDataFull_Polyline instead")]
        public void SetTransparentProperty(TransparentPropertyBlock t) { 
        
        }

        [Obsolete("PolylineSerializableData is Obsolete , use Linefy.Serialization.SerializationData_Polyline and Linefy.Serialization.SerializationDataFull_Polyline instead")]
        public PolylineSerializableData GetSerializableData() {
            return null;
        }
    }
}
