using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Internal;
using Linefy.Serialization;

namespace Linefy {
 
    public class PolygonalMesh : LinefyDrawcall {

        public class PositionEdge {
            public bool positionsDirty = true;
            public Position a;
            public Position b;

            public PositionEdge(Position a, Position b) {
                this.a = a;
                this.b = b;
            }
        }

        public class Vertex {
            public int meshIndex;
            public Position pos;
            public Normal norm;
            public UV uv;
            public VColor color;
        }

        public class Face {
            public Vertex a;
            public Vertex b;
            public Vertex c;

            Vector3 normal;

            public Vector3 RecalculateNormal() {
                Vector3 pa = a.pos;
                Vector3 pb = b.pos;
                Vector3 pc = c.pos;

                normal = Vector3.Cross(pb - pa, pc - pa);
                return normal;
            }

            public void SetVertices(Vertex a, Vertex b, Vertex c) {
                this.a = a;
                this.b = b;
                this.c = c;
            }
        }

        public class PM_Polygon {
            struct TriangulationCorner {
                public bool isUsed;
                public Vector2 position;
            }

            public int smoothingGroup;
            public int materialId;
            bool flatPolygon;
            public bool triangulationDirty;
            public bool normalsDirty;
            public Vertex[] corners;
            TriangulationCorner[] tcorners;
            public Face[] nonConvexFaces;
            public Face[] convexFaces;
            public Vector3 normal;
            public int trisArrayIndicesOffset;

            Vertex cross00;
            Vertex cross01;
            Vertex cross10;
            Vertex cross11;

            public PM_Polygon(int cornersCount, ref int trisIndicesCounter, int smoothingGroup, int materialId) {
                this.corners = new Vertex[cornersCount];
                this.tcorners = new TriangulationCorner[cornersCount];
                trisArrayIndicesOffset = trisIndicesCounter;

                nonConvexFaces = new Face[cornersCount - 2];
                convexFaces = new Face[nonConvexFaces.Length];

                for (int f = 0; f < nonConvexFaces.Length; f++) {
                    nonConvexFaces[f] = new Face();
                    convexFaces[f] = new Face();
                }
                this.smoothingGroup = smoothingGroup;
                this.materialId = materialId;
                trisIndicesCounter += nonConvexFaces.Length * 3;
                triangulationDirty = true;
                normalsDirty = true;
            }

            public void TriangulateNonConvex(int[] mTriangles) {
                triangulationDirty = false;
                Vector3 polygonCenter = Vector3.zero;
                float mult = 1f / corners.Length;
                for (int i = 0; i < corners.Length; i++) {
                    polygonCenter += corners[i].pos.positionValue * mult;
                }

                if ((polygonCenter-corners[0].pos.positionValue).magnitude<0.001f) {
                    fillTrisIndices(mTriangles, convexFaces);
                    return;
                }
 
                RecalculateNormalUnweighted();

                Vector3 pos0 = corners[0].pos.positionValue;
                Vector3 dirToPos0 = (pos0 - polygonCenter).normalized;
                Matrix4x4 basis = Matrix4x4Utility.UnscaledTRSInverse(polygonCenter, normal, dirToPos0);

                for (int i = 0; i < tcorners.Length; i++) {
                    tcorners[i].isUsed = false;
                    tcorners[i].position = basis.MultiplyPoint3x4(corners[i].pos.positionValue);
                }

                for (int i = 0; i < nonConvexFaces.Length; i++) {

                    int t0 = 0;
                    int t1 = 0;
                    int t2 = 0;
                    Face face = nonConvexFaces[i];

                    if (FindAllowedTriangleHigh(ref t0, ref t1, ref t2)) {
                        tcorners[t1].isUsed = true;
                        face.a = corners[t0];
                        face.b = corners[t1];
                        face.c = corners[t2];
                    } else {
                        fillTrisIndices(mTriangles, convexFaces);
                        return;
                    }
                }
                fillTrisIndices(mTriangles, nonConvexFaces);
             }

            bool FindAllowedTriangle(ref int t0, ref int t1, ref int t2) {
                 for (int i = 0; i < tcorners.Length; i++) {
                    if (tcorners[i].isUsed) {
                        continue;
                    }
                    int prevIdx = GetUnusedAdjacentTCorner(i, -1);
                    int nextIdx = GetUnusedAdjacentTCorner(i, +1);
                    Vector2 prevPos = tcorners[prevIdx].position;
                    Vector2 thisPos = tcorners[i].position;
                    Vector2 nextPos = tcorners[nextIdx].position;
                    float a = Vector2Unility.SignedAngle(prevPos - thisPos, nextPos - thisPos);
                    if (a < 180) {
                        //Debug.LogFormat("a {0}", a);
                        if (isValidTriangulationFace(prevIdx, i, nextIdx)) {
                            t0 = prevIdx;
                            t1 = i;
                            t2 = nextIdx;
                            return true;
                        }
                    }
                }
                return false;
            }

            bool FindAllowedTriangleHigh(ref int t0, ref int t1, ref int t2) {
                float minAngle = float.MaxValue;
                bool result = false;
                for (int i = 0; i < tcorners.Length; i++) {
                    if (tcorners[i].isUsed) {
                        continue;
                    }
                    int prevIdx = GetUnusedAdjacentTCorner(i, -1);
                    int nextIdx = GetUnusedAdjacentTCorner(i, +1);
                    Vector2 prevPos = tcorners[prevIdx].position;
                    Vector2 thisPos = tcorners[i].position;
                    Vector2 nextPos = tcorners[nextIdx].position;
                    float a = Vector2Unility.SignedAngle(prevPos - thisPos, nextPos - thisPos);
                    if (a < minAngle) {
                        //Debug.LogFormat("a {0}", a);
                        if (isValidTriangulationFace(prevIdx, i, nextIdx)) {
                            t0 = prevIdx;
                            t1 = i;
                            t2 = nextIdx;
                            minAngle = a;
                            result = true;
                        }
                    }
                }
                return result;
            }

            int GetUnusedAdjacentTCorner(int idx, int sign) {
                for (int i = 1; i < tcorners.Length; i++) {
                    int ti = idx + i * sign;
                    ti = MathUtility.RoundedArrayIdx(ti, tcorners.Length);
                    if (tcorners[ti].isUsed == false) {
                        return ti;
                    }
                }
                Debug.Log("Not found!");
                return 0;
            }

            bool isValidTriangulationFace(int ta, int tb, int tc) {
                //return true;
                Vector2 tpA = tcorners[ta].position;
                Vector2 tpB = tcorners[tb].position;
                Vector2 tpC = tcorners[tc].position;

                for (int c = 0; c < tcorners.Length; c++) {
                    if (c == ta || c == tb || c == tc) {
                        continue;
                    }

                    Vector2 testPoint = tcorners[c].position;
                    if (Triangle2D.PointTestDoublesided(tpA, tpB, tpC, testPoint)) {
                        return false;
                    }
                }
                return true;
            }

            public void RecalculateNormal(NormalsRecalculationMode mode) {
                if (mode == NormalsRecalculationMode.Unweighted) {
                     RecalculateNormalUnweighted();
                } else {
                    RecalculateNormalWeighted();
                }
                normalsDirty = false;
            }

            void RecalculateNormalUnweighted() {
                normal = Vector3.Cross(cross01.pos.positionValue - cross00.pos.positionValue, cross11.pos.positionValue - cross10.pos.positionValue).normalized;
            }

            void RecalculateNormalWeighted() {
                normal.Set(0, 0, 0);
                for (int i = 0; i < nonConvexFaces.Length; i++) {
                    normal += convexFaces[i].RecalculateNormal();
                }
            }
 
            public void TriangulateConvex( int[] mTriangles ) {
                int cc = 0;
                int sc = 1;
                int ec = corners.Length-1;
                bool dir = true;
                for (int f = 0; f<convexFaces.Length; f++) {
                    Face face = convexFaces[f];
                    face.c = corners[sc];
                    face.b = corners[cc];
                    face.a = corners[ec];
                    if (dir) {
                        cc = sc;
                        sc++;
                    } else {
                        cc = ec;
                        ec--;
                    }
                    dir = !dir;
                }
                fillTrisIndices(mTriangles, convexFaces);
            }

            void fillTrisIndices(int[] mTriangles, Face[] _faces ) {
                for (int i = 0; i < _faces.Length; i++) {
                    Face face = _faces[i];
                    int fa = i * 3;
                    int fb = fa + 1;
                    int fc = fa + 2;
                    mTriangles[trisArrayIndicesOffset + fa] = face.a.meshIndex;
                    mTriangles[trisArrayIndicesOffset + fb] = face.b.meshIndex;
                    mTriangles[trisArrayIndicesOffset + fc] = face.c.meshIndex;
                }
            }

            public void SetNormalCross() {
                if (corners.Length == 3) {
                    cross00 = corners[0];
                    cross01 = corners[1];
                    cross10 = corners[1];
                    cross11 = corners[2];
                } else if (corners.Length == 4 || corners.Length == 5) {
                    cross00 = corners[0];
                    cross01 = corners[2];
                    cross10 = corners[1];
                    cross11 = corners[3];
                } else if (corners.Length == 6 || corners.Length == 7) {
                    cross00 = corners[0];
                    cross01 = corners[3];
                    cross10 = corners[2];
                    cross11 = corners[5];
                } else {
                    int num = corners.Length / 4;
                    cross00 = corners[0];
                    cross01 = corners[num * 2];
                    cross10 = corners[num ];
                    cross11 = corners[num * 3];
                }
            }

            string info() {
                string result = string.Format("polygon cornersCount:{0} positions: ", corners.Length);
                for (int c = 0; c<corners.Length; c++) {
                    result += string.Format("{0}, ", corners[c].pos.positionValue);
                }
                return result;
            }
        }

        public class Position {
            public int idx;
            public Vector3 positionValue;
            public Vertex[] linkedVertices;
            internal PM_Polygon[] adjacentPolygons ;
            public PositionEdge[] adjacentEdges;

            public Position( int id ) {
                idx = id;
            }

            public static implicit operator Vector3(Position pv) {
                return pv.positionValue;
            }
        }

        public class Normal {
            public bool dirty;
            public PM_Polygon[] adjacentPolygons;
            public Vertex[] linkedVertices ;
            public Vector3 normal;

            public Normal(int adjacentCount) {
                adjacentPolygons = new PM_Polygon[adjacentCount];
            }

            public void Recalculate() {
                normal.Set(0,0,0);
                for (int i = 0; i < adjacentPolygons.Length; i++) {
                    normal += adjacentPolygons[i].normal ;
                }
                dirty = false;
            }

            public static implicit operator Vector3(Normal pv) {
                return pv.normal;
            }
        }

        public class UV {
            public int idx;
            public Vertex[] linkedVertices;
            public Vector2 uvValue;

            public UV(int idx, Vector2 value) {
                this.idx = idx;
                this.uvValue = value;
            }
        }

        public class VColor {
            public int idx;
            public Vertex[] linkedVertices;
            public Color colorValue;

            public VColor(int idx, Color color) {
                this.idx = idx;
                this.colorValue = color;
            }
        }

        bool formIsDirty;
        bool mTrianglesDirty;
        bool mPositionsDirty;
        bool mUVDirty;
        bool mColorDirty;
        bool mNormalsDirty;

        protected override void SetDirtyAttributes() {
            boundsDirty = true;
            mTrianglesDirty = true;
            mPositionsDirty = true;
            mUVDirty = true;
            mNormalsDirty = true;
            mColorDirty = true;
            formIsDirty = true;
        }

        int[] mTriangles;
        Vector3[] mPositions;
        Vector2[] mUVs;
        Color[] mColors;
        Vector3[] mNormals;
 
        bool _autoRecalculateBounds = true;
        public bool autoRecalculateBounds {
            get {
                return _autoRecalculateBounds;
            }

            set {
                if (_autoRecalculateBounds != value) {
                    if (_autoRecalculateBounds) {
                        formIsDirty = true;
                    }                    
                    _autoRecalculateBounds = value;
                }
            }
        }

        Lines _positionWireframe;
        public Lines positionEdgesWireframe {
            get {
                return _positionWireframe;
            }

            set {
                if (value != null && value != _positionWireframe) {
                    formIsDirty = true;
                }
                _positionWireframe = value;
            }
        }

        protected PM_Polygon[] polygons = new PM_Polygon[0];
        PM_Polygon[] dynamicalyTriangulatedPolygons = new PM_Polygon[0];

        protected Position[] positions = new Position[0];
        protected Normal[] normals;
        protected UV[] uvs;
        protected VColor[] colors;
        protected Vertex[] vertices;
        protected PositionEdge[] positionEdges = new PositionEdge[0];
        public ModificationInfo modificationInfo;
 
        int _dynamicTriangulationThreshold = 5;
        /// <summary>
        /// The number of corners in a polygon, greater than or equal to which the polygon will dynamically re-triangulate when its shape changes.
        /// </summary>
        public int dynamicTriangulationThreshold { 
            get {
                return _dynamicTriangulationThreshold;
            }

            set {
                if (_dynamicTriangulationThreshold != value) {
                    _dynamicTriangulationThreshold = Mathf.Max(4, value);
                    OnTriangulationParamsChanged();
                }
            }
        }

        void OnTriangulationParamsChanged() {
            //if (_dynamicTriangulationThreshold >= 3) {
                dynamicalyTriangulatedPolygons = System.Array.FindAll(polygons, (PM_Polygon p) => (p.corners.Length >= _dynamicTriangulationThreshold) );
                foreach (PM_Polygon p in dynamicalyTriangulatedPolygons) {
                    p.triangulationDirty = true;
                }
            //}
            formIsDirty = true;
        }

        float _ambient = 1;
        /// <summary>
        /// Ambient lighting of internal material.  0 = backface is black   1  = backface equals main color ( unlit shading ) 
        /// </summary>
        public float ambient {
            get {
                return _ambient;
            }


            set {
                if (_ambient != value) {
                    _ambient = value;
                    material.SetFloat("_Ambient", _ambient);
                }
            }
        }

        bool _doublesided = true;

        /// <summary>
        /// Doublesided render mode of internal meaterial
        /// </summary>
        public bool doublesided {
            get {
                return _doublesided;
            }


            set {
                if (_doublesided != value) {
                    _doublesided = value;
                    material.SetFloat("_Culling", _doublesided?0:1);
                }
            }
        }

        Vector4 _textureTransform = new Vector4(1,1,0,0);
        /// <summary>
        /// Texture transform. xy = scale zw = offset
        /// </summary>
        public Vector4 textureTransform {
            get {
                return _textureTransform;
            }

            set {
                if (_textureTransform != value) {
                    _textureTransform = value;
                    material.SetVector("_TextureTransform", _textureTransform);
                }
            }
        }

        LightingMode _lightingMode = LightingMode.Lit;
        /// <summary>
        /// Defines recalculation algorithm of mesh lighting data (normals and tangens).
        /// </summary>
        public LightingMode lighingMode { 
            get {
                return _lightingMode;
            }
        
            set {
                if (value != _lightingMode) {
                    _lightingMode = value;
                    formIsDirty = true;
                    mNormalsDirty = true;
                    if (_lightingMode > 0) {
                        foreach (PM_Polygon p in polygons) {
                            p.normalsDirty = true;
                        }
                    }
                }
            }
        }

        NormalsRecalculationMode _normalsRecalculationMode;
        /// <summary>
        /// Defines mesh normals recalculation algorithm (weighted or unweighted).
        /// </summary>
        public NormalsRecalculationMode normalsRecalculationMode { 
            get {
                return _normalsRecalculationMode;
            }

            set {
                if (value != _normalsRecalculationMode) {
                    _normalsRecalculationMode = value;
                    foreach (PM_Polygon p in polygons) {
                        p.normalsDirty = true;
                    }
                    formIsDirty = true;
                }
            }
        }

        public PolygonalMesh( SerializedPolygonalMesh serializableData )   {
            BuildFromSPM( serializableData );
            //Apply();
        }

        public PolygonalMesh(Vector3[] posData, Vector2[] uvsData, Color[] colorsData, Polygon[] polygonsData ) {
            SerializedPolygonalMesh spm = SerializedPolygonalMesh.GetProcedural(posData, uvsData, colorsData, polygonsData);
            BuildFromSPM(spm);
        }

        public PolygonalMesh(Vector3[] posData, Vector2[] uvsData, Polygon[] polygonsData) {
            SerializedPolygonalMesh spm = SerializedPolygonalMesh.GetProcedural(posData, uvsData, null, polygonsData);
            BuildFromSPM(spm);
            Object.DestroyImmediate(spm);
        }
		
		public PolygonalMesh(Vector3[] posData, Polygon[] polygonsData) {
            SerializedPolygonalMesh spm = SerializedPolygonalMesh.GetProcedural(posData, null, null, polygonsData);
            BuildFromSPM(spm);
            Object.DestroyImmediate(spm);
        }

        public void BuildFromSPM ( SerializedPolygonalMesh spm ) {
			if(mesh == null){
			} else {
				mesh.Clear();
			}
			
            modificationInfo = spm.modificationInfo;
            positions = new Position[spm.positions.Length];
            for (int i = 0; i < positions.Length; i++) {
                positions[i] = new Position(i);
            }

            //if (spm.uvs != null && spm.uvs.Length > 0) {
            uvs = new UV[spm.uvs.Length];
            for (int i = 0; i< spm.uvs.Length; i++) {
                uvs[i] = new UV(i, spm.uvs[i].uvValue);
            }

            colors = new VColor[spm.colors.Length];    
            for (int i = 0; i < spm.colors.Length; i++) {
                 colors[i] = new VColor(i,spm.colors[i].colorValue); ;
            }

            vertices = new Vertex[spm.vertices.Length];
            for (int i = 0; i < vertices.Length; i++) {
                SerializedPolygonalMesh.SPM_Vertex serializedVertex = spm.vertices[i];
                Vertex vertex = new Vertex();
                vertex.meshIndex = i;
                vertex.pos = positions[serializedVertex.posIdx];
                vertex.uv = uvs[serializedVertex.uvIdx];
                vertex.color = colors[serializedVertex.colorIdx];
                vertices[i] = vertex;
            }
 
            for (int i = 0; i < uvs.Length; i++) {
                SerializedPolygonalMesh.SPM_uv sUv = spm.uvs[i];
                UV uv = uvs[i];
                uv.linkedVertices = new Vertex[sUv.linkedVertices.Count];
                for (int v = 0; v < uv.linkedVertices.Length; v++) {
                    uv.linkedVertices[v] = vertices[sUv.linkedVertices[v]];
                }
            }
   
            for (int i = 0; i < colors.Length; i++) {
                SerializedPolygonalMesh.SPM_color sColor = spm.colors[i];
                VColor color = colors[i];
                color.linkedVertices = new Vertex[sColor.linkedVertices.Count];
                for (int v = 0; v < color.linkedVertices.Length; v++) {
                    color.linkedVertices[v] = vertices[sColor.linkedVertices[v]];
                }
            }

            int trisIndicesCounter = 0;
            polygons = new PM_Polygon[spm.polygons.Length];
 
            for (int i = 0; i < polygons.Length; i++) {
                SerializedPolygonalMesh.SPM_Polygon serializedPolygon = spm.polygons[i];
                PM_Polygon polygon = new PM_Polygon(serializedPolygon.corners.Length, ref trisIndicesCounter, spm.polygons[i].smoothingGroup, spm.polygons[i].materialId);
                for (int c = 0; c<polygon.corners.Length; c++) {
                    Vertex vert = vertices[serializedPolygon.corners[c]];
                    polygon.corners[c] = vert;
                }
                polygon.SetNormalCross();
                polygons[i] = polygon;
            }

            normals = new Normal[spm.normals.Length];
 
            for (int i = 0; i<normals.Length; i++) {
                SerializedPolygonalMesh.SPM_Normal serializedNormal = spm.normals[i];
                Normal normal = new Normal(serializedNormal.adjacentPolygons.Count);
                for (int a = 0; a<normal.adjacentPolygons.Length; a++) {
                    normal.adjacentPolygons[a] = polygons[serializedNormal.adjacentPolygons[a]];
                }
                normals[i] = normal;
            }

            for (int i = 0; i < vertices.Length; i++) {
                SerializedPolygonalMesh.SPM_Vertex serializedVertex = spm.vertices[i];
                Normal normal = normals[serializedVertex.normIdx];
                vertices[i].norm = normal;
            }
            
            for (int i = 0; i<normals.Length; i++) {
                SerializedPolygonalMesh.SPM_Normal sNormal = spm.normals[i];
                Normal normal = normals[i];
                normal.linkedVertices = new Vertex[sNormal.linkedVertices.Count];
                for (int v = 0; v<normal.linkedVertices.Length; v++) {
                    normal.linkedVertices[v] = vertices[sNormal.linkedVertices[v]];
                }
            }

            mNormals = new Vector3[vertices.Length];
            mPositionsDirty = true;
            mColorDirty = true;
            mUVDirty = true;
            mTrianglesDirty = true;
            mPositions = new Vector3[vertices.Length];
 
            mColors = new Color[vertices.Length];
            for (int c = 0; c < spm.colors.Length; c++) {
                SetColor(c, spm.colors[c].colorValue);
            }
   
            mUVs = new Vector2[vertices.Length];
            for (int i = 0; i < spm.uvs.Length; i++) {
                SetUV(i, spm.uvs[i].uvValue);
            }
 
            mTriangles = new int[trisIndicesCounter];
            foreach (PM_Polygon p in polygons) {
                p.TriangulateConvex(mTriangles);
            }
 
            positionEdges = new PositionEdge[spm.positionEdges.Length];
            for (int i = 0; i<spm.positionEdges.Length; i++) {
                SerializedPolygonalMesh.SPM_Edge se = spm.positionEdges[i];
                Position pa = positions[se.a];
                Position pb = positions[se.b];
                positionEdges[i] = new PositionEdge(pa, pb);
            }

            for (int p = 0; p < spm.positions.Length; p++) {
                SerializedPolygonalMesh.SPM_Position spmPosition = spm.positions[p];
                Position position = positions[p];
                position.adjacentPolygons = new PM_Polygon[spmPosition.adjacentPolygons.Count];
                for (int i = 0; i < position.adjacentPolygons.Length; i++) {
                    position.adjacentPolygons[i] = polygons[spmPosition.adjacentPolygons[i]];
                }
                position.linkedVertices = new Vertex[spmPosition.linkedVertices.Count];
                for (int i = 0; i < position.linkedVertices.Length; i++) {
                    position.linkedVertices[i] = vertices[spmPosition.linkedVertices[i]];
                }

                position.adjacentEdges = new PositionEdge[spmPosition.adjacentEdges.Count];
                for (int i = 0; i < position.adjacentEdges.Length; i++) {
                    position.adjacentEdges[i] = positionEdges[spmPosition.adjacentEdges[i]];
                }

                SetPosition(p, spmPosition.positionValue);
            }
            SetDirtyAttributes();
            OnTriangulationParamsChanged();
            Apply();
        }

        /// <summary>
        /// Updates the generated mesh. Automatically called before any drawing. Use only if you want to update the mesh, but don't draw it.
        /// </summary>
        public void Apply() {
            base.PreDraw();

            if (formIsDirty) {
                foreach (PM_Polygon p in dynamicalyTriangulatedPolygons) {
                    if (p.triangulationDirty) {
                        p.TriangulateNonConvex(mTriangles);
                        mTrianglesDirty = true;
                    }
                }

                if (lighingMode > 0) {
                    foreach (PM_Polygon p in polygons) {
                        if (p.normalsDirty) {
                            p.RecalculateNormal(normalsRecalculationMode);
                            foreach (Vertex v in p.corners) {
                                v.norm.dirty = true;
                            }
                        }
                    }

                    foreach (Normal n in normals) {
                        if (n.dirty) {
                            n.Recalculate();
                            foreach (Vertex v in n.linkedVertices) {
                                mNormals[v.meshIndex] = n;
                            }
                            mNormalsDirty = true;
                        }
                    }
                }

                FillMeshBuffers();

                if (positionEdgesWireframe != null) {
                    positionEdgesWireframe.count = positionEdges.Length;
                    for (int i = 0; i < positionEdges.Length; i++) {
                        PositionEdge edge = positionEdges[i];
                        if (edge.positionsDirty) {
                            positionEdgesWireframe.SetPosition(i, edge.a, edge.b);
                            edge.positionsDirty = false;
                        }
                    }
                }

                formIsDirty = false;
                boundsDirty = true;
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

        private void FillMeshBuffers() {

            if (mPositionsDirty) {
                mesh.vertices = mPositions;
                mPositionsDirty = false;
            }

            if (mUVDirty) {
                mesh.uv = mUVs;
                mUVDirty = false;
            }

            if ( mColorDirty) {
                mesh.colors = mColors;
                mColorDirty = false;
            }

            if (_lightingMode == 0) {
                mesh.normals = null;
                mesh.tangents = null;
 
            } else if (_lightingMode == LightingMode.Lit) {
                if (mNormalsDirty) {
                    mesh.normals = mNormals;
                    mesh.tangents = null;
                    mNormalsDirty = false;
                }
            } else {
                if (mNormalsDirty) {
                    mesh.normals = mNormals;
                    mesh.RecalculateTangents();
                    mNormalsDirty = false;
                }
            }

            if (mTrianglesDirty) {
                mesh.triangles = mTriangles;
                mTrianglesDirty = false;
            }
        }

        public virtual Vector3 GetPosition(int idx) {
            return positions[idx].positionValue;
        }

        public virtual void SetPosition(int idx, Vector3 positionValue) {
#if UNITY_EDITOR
            if (idx < 0 || idx >= positions.Length) {
                return;
            }
#endif
            Position p = positions[idx];
            p.positionValue = positionValue;

            foreach (Vertex v in p.linkedVertices) {
                mPositions[v.meshIndex] = positionValue;
            }

            foreach (PM_Polygon polygon in p.adjacentPolygons ) {
                polygon.triangulationDirty = true;
                polygon.normalsDirty = true;
            }

            foreach (PositionEdge edge in p.adjacentEdges) {
                edge.positionsDirty = true;
            }
            mPositionsDirty = true;
            formIsDirty = true;
        }

        public virtual void SetUV(int idx, Vector2 uvValue) {
            UV uv = uvs[idx];
            uv.uvValue = uvValue;
            for (int i = 0; i < uv.linkedVertices.Length; i++) {
                mUVs[uv.linkedVertices[i].meshIndex] = uvValue;
            }
            mUVDirty = true;
        }

        public virtual Vector2 GetUV(int idx) {
            return uvs[idx].uvValue;
        }

        public virtual void SetColor(int idx, Color colorValue) {
            VColor color = colors[idx];
            color.colorValue = colorValue;
            for (int i = 0; i < color.linkedVertices.Length; i++) {
                mColors[color.linkedVertices[i].meshIndex] = colorValue;
            }
            mColorDirty = true;
        }

        public virtual Color GetColor(int idx) {
            return colors[idx].colorValue;
        }

        protected override void PreDraw() {
            Apply();
        }

        protected override void OnAfterMaterialCreated() {
            base.OnAfterMaterialCreated();
            material.SetFloat("_Ambient", _ambient);
            material.SetVector("_TextureTransform", _textureTransform);
            material.SetFloat("_Culling", _doublesided ? 0 : 1);
        }

        protected override string transparentShaderName() {
            return "Hidden/Linefy/DefaultPolygonalMeshTransparent";
        }

        protected override string opaqueShaderName() {
            return "Hidden/Linefy/DefaultPolygonalMesh";
        }
    
        public int positionsCount { 
            get {
                return positions.Length;
            }
        }

        public int positionEdgesCount {
            get {
                return positionEdges.Length;
            }
        }

        public static PolygonalMesh BuildProcedural(Vector3[] posData, Vector2[] uvsData, Color[] colorsData, Polygon[] polygonsData) {
            SerializedPolygonalMesh spm = SerializedPolygonalMesh.GetProcedural(posData, uvsData, colorsData, polygonsData);
            PolygonalMesh result = new PolygonalMesh(spm);
            if (Application.isPlaying) {
                Object.Destroy(spm);
            } else {
                Object.DestroyImmediate(spm);
            }
            return result;
        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            //if (_positionEdgesWireframe != null) {
            //    linesCount += 1;
            //    totallinesCount += _positionEdgesWireframe.count;
            //}
        }

        public override void Dispose() {
            base.Dispose();
        }

        public void SaveToSPM( SerializedPolygonalMesh spm ) {
            Vector3[] posData = new Vector3[positions.Length];
            for (int i = 0; i<posData.Length; i++) {
                posData[i] = positions[i].positionValue;
            }

            Vector2[] uvData = new Vector2[uvs.Length];
            for (int i = 0; i<uvs.Length; i++) {
                uvData[i] = uvs[i].uvValue;
            }

            Color[] colorData = new Color[colors.Length];
            for (int i = 0; i<colorData.Length; i++) {
                colorData[i] = colors[i].colorValue;
            }

            Polygon[] polygonsData = new Polygon[polygons.Length];
            for (int p = 0; p<polygons.Length; p++) {
                Polygon polygon = new Polygon(polygons[p].smoothingGroup, polygons[p].materialId, polygons[p].corners.Length);
                for (int i = 0; i<polygon.corners.Length; i++) {
                    polygon.corners[i].position = polygons[p].corners[i].pos.idx;
                    polygon.corners[i].uv = polygons[p].corners[i].uv.idx;
                    polygon.corners[i].color = polygons[p].corners[i].color.idx;
                }
                polygonsData[p] = polygon;
            }

            spm.BuildProcedural(posData, uvData, colorData, polygonsData);
        }

        /// <summary>
        /// Reads and apply properties from inputData to this PolygonalMesh instance  (deserialization)
        /// </summary>
        public void LoadSerializationData(SerializationData_PolygonalMeshProperties inputData) {
            base.LoadSerializationData(inputData);
            this.ambient = inputData.ambient;
            this.lighingMode = inputData.lighingMode;
            this.dynamicTriangulationThreshold = inputData.dynamicTriangulationThreshold;
            this.normalsRecalculationMode = inputData.normalsRecalculationMode;
            this.textureTransform = inputData.textureTransform;
            this.doublesided = inputData.doublesided;
        }

        /// <summary>
        /// Writes the current PolygonalMesh properties to the outputData (serialization)
        /// </summary>
        public void SaveSerializationData(ref SerializationData_PolygonalMeshProperties outputData) {
            base.SaveSerializationData(outputData);
            outputData.ambient = this.ambient;
            outputData.lighingMode = this.lighingMode;
            outputData.dynamicTriangulationThreshold = this.dynamicTriangulationThreshold;
            outputData.textureTransform = this.textureTransform;
            this.normalsRecalculationMode = this.normalsRecalculationMode;
        }

        int dynamiclyTriangulatedPolygonsCount { 
            get {
                return dynamicalyTriangulatedPolygons.Length;
            }
        }

        /// <summary>
        ///  The current state of internal mesh. You should not modify it, use it on read-only mode..
        /// </summary>
        public Mesh generatedMesh {
            get {
                return base.mesh;
            }
        }    
    }

  
}
