using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Internal;

namespace Linefy {
 
    [UnityEngine.PreferBinarySerialization]
    [HelpURL("https://polyflow.xyz/content/linefy/documentation-1-1/linefy-documentation.html#SerializedPolygonalMesh")]
    public partial class SerializedPolygonalMesh : ScriptableObject {

        [System.Serializable]
        public struct SPM_Position {
            public Vector3 positionValue;
            public List<int> linkedVertices;
            public List<int> adjacentPolygons;
            public List<int> adjacentEdges;

            public SPM_Position(Vector3 pos) {
                positionValue = pos;
                linkedVertices = new List<int>(4);
                adjacentPolygons = new List<int>(4);
                adjacentEdges = new List<int>(4);
            }
        }

        [System.Serializable]
        public struct SPM_Material {
            public string name;
            int hash;

            public SPM_Material(string name) {
                this.name = name;
                hash = this.name.GetHashCode();
            }

            public override int GetHashCode() {
                return hash;
            }

            public override bool Equals(object obj) {
                return ((SPM_Material)obj).name == name;
            }
        }

        [System.Serializable]
        public struct SPM_Edge {
            public int a;
            public int b;
            int hash;
            string strHash;

            public SPM_Edge(int a, int b) {
                this.a = Mathf.Min(a, b);
                this.b = Mathf.Max(a, b);
                strHash = string.Format("{0} | {1}", this.a, this.b);
                hash = strHash.GetHashCode();
            }

            public override int GetHashCode() {
                return hash;
            }

            public override bool Equals(object obj) {
                return ((SPM_Edge)obj).strHash == strHash;
            }
        }

        [System.Serializable]
        public struct SPM_Polygon {
            public int materialId;
            public int smoothingGroup;
            public int[] corners;

            public SPM_Polygon(int materialId, int smoothingGroup, int cornersCount) {
                this.materialId = materialId;
                this.smoothingGroup = smoothingGroup;
                corners = new int[cornersCount];
            }

            public void FlipNormals() {
                System.Array.Reverse(corners);
            }
        }

        [System.Serializable]
        public struct SPM_Vertex {
            public int posIdx;
            public int normIdx;
            public int uvIdx;
            public int colorIdx;
            string strHash;
            int hash;

            public SPM_Vertex(int posIdx, int normalIdx, int uvIdx, int colorIdx) {
                this.posIdx = posIdx;
                this.normIdx = normalIdx;
                this.uvIdx = uvIdx;
                this.colorIdx = colorIdx;
                strHash = string.Format("{0} {1} {2} {3}", posIdx, normIdx, uvIdx, colorIdx);
                hash = strHash.GetHashCode();
            }

            public override int GetHashCode() {
                return hash;
            }

            public override bool Equals(object obj) {
                return ((SPM_Vertex)obj).strHash == strHash;
            }
        }

        [System.Serializable]
        public struct SPM_Normal {
            public int parentPos;
            public int smoothingGroupIdx;
            public List<int> adjacentPolygons;
            public List<int> linkedVertices;
            string strHash;
            int hash;

            public SPM_Normal(int parentPos, int smoothingGroup) {
                this.parentPos = parentPos;
                this.smoothingGroupIdx = smoothingGroup;
                adjacentPolygons = new List<int>();
                linkedVertices = new List<int>();
                strHash = string.Format("{0} | {1}", parentPos, smoothingGroupIdx);
                hash = strHash.GetHashCode();
            }

            public override int GetHashCode() {
                return hash;
            }

            public override bool Equals(object obj) {
                if (smoothingGroupIdx < 0) {
                    return false;
                }
                return ((SPM_Normal)obj).strHash == strHash;
            }
        }

        [System.Serializable]
        public struct SPM_uv {
            public Vector2 uvValue;
            public List<int> linkedVertices;

            public SPM_uv(Vector2 uv) {
                uvValue = uv; 
                linkedVertices = new List<int>(4);
            }
        }

        [System.Serializable]
        public struct SPM_color {
            public Color colorValue;
            public List<int> linkedVertices;

            public SPM_color(Color color) {
                colorValue = color;
                linkedVertices = new List<int>(4);
            }
        }

        public SPM_Position[] positions = new SPM_Position[0];
        public SPM_uv[] uvs = new SPM_uv[0];
        public SPM_color[] colors = new SPM_color[0];
        public SPM_Normal[] normals = new SPM_Normal[0];
        public SPM_Polygon[] polygons = new SPM_Polygon[0];
        public SPM_Edge[] positionEdges = new SPM_Edge[0];
        public SPM_Material[] materials = new SPM_Material[0];
        public SPM_Vertex[] vertices = new SPM_Vertex[0];
        public int trianglesCount = 0;
        public ModificationInfo modificationInfo ;

        ObjLineIdEnum GetObjLineId(string str) {
            char[] chars = str.ToCharArray();
            if (chars.Length < 2) {
                return ObjLineIdEnum.other;
            }
            if (chars[0] == 'v' && chars[1] == ' ') {
                return ObjLineIdEnum.v;
            }
            if (chars[0] == 'v' && chars[1] == 't') {
                return ObjLineIdEnum.vt;
            }
            if (chars[0] == 'f' && chars[1] == ' ') {
                return ObjLineIdEnum.f;
            }
            if (chars[0] == 's' && chars[1] == ' ') {
                return ObjLineIdEnum.s;
            }
            //usemtl
            if (chars[0] == 'u' && chars[1] == 's' && chars[2] == 'e' && chars[3] == 'm' && chars[4] == 't' && chars[5] == 'l') {
                return ObjLineIdEnum.usemtl;
            }
            if (chars[0] == 's' && chars[1] == ' ' && chars[2] == 'o' && chars[3] == 'f' && chars[4] == 'f') {
                return ObjLineIdEnum.s_off;
            }

            return ObjLineIdEnum.other;
        }

        enum ObjLineIdEnum {
            v,
            vt,
            f,
            s,
            s_off,
            usemtl,
            other
        }

        static float ToFloat(string s) {
            return float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        }

        int ToInt(string s) {
            int result = 0;
            int.TryParse(s, out result);
            return result;
        }

        void OnEOF(System.IO.TextReader sr) {
#if UNITY_WSA
                    sr.Dispose();
#else
            sr.Close();
#endif
        }

        class FaceLineParser {
            public class Corner {
                public int posIdx = -1;
                public int uvIdx = -1;
                public int normalIdx = -1;

                public List<char>[] chars = { new List<char>(), new List<char>(), new List<char>(), };
                public int ci;

                public void Parse() {
                    if (chars[0].Count > 0) {
                        int.TryParse(new string(chars[0].ToArray()), out posIdx);
                        posIdx--;
                    }

                    if (chars[1].Count > 0) {
                        int.TryParse(new string(chars[1].ToArray()), out uvIdx);
                        uvIdx--;
                    }

                    if (chars[2].Count > 0) {
                        int.TryParse(new string(chars[2].ToArray()), out normalIdx);
                        normalIdx--;
                    }
                }
            }

            public List<Corner> corners = new List<Corner>();

            public void Parse(string str) {
                corners.Clear();
                char[] ca = str.ToCharArray();

                Corner current = null;

                for (int i = 2; i < ca.Length; i++) {
                    char c = ca[i];
                    if (char.IsDigit(c)) {
                        if (current == null) {
                            current = new Corner();
                        }
                        current.chars[current.ci].Add(c);
                    }

                    if (ca[i] == '/') {
                        current.ci++;
                        continue;
                    }

                    if (ca[i] == ' ') {
                        if (current != null) {
                            corners.Add(current);
                            current = null;
                        }
                        continue;
                    }

                }
                if (current != null) {
                    corners.Add(current);
                }

                for (int c = 0; c < corners.Count; c++) {
                    corners[c].Parse();
                }
            }

            public void PrintDebug() {
                string result = string.Format("corners count {0} ", corners.Count);
                for (int i = 0; i < corners.Count; i++) {
                    result += string.Format(" {0}/{1}/{2} ", corners[i].posIdx, corners[i].uvIdx, corners[i].normalIdx);
                }
                Debug.Log(result);
            }
        }

        public void ReadObjFromTextReader(System.IO.TextReader textReader,  SmoothingGroupsImportMode sgMode, bool flipNormals, float scaleFactor, bool swapYZ) {
            string[] subString;
            char[] spaceSeparator = @" ".ToCharArray();
            FaceLineParser faceLineParser = new FaceLineParser();

            List<Vector3> _positions = new List<Vector3>();
            List<Vector2> _uvs = new List<Vector2>();
            List<Polygon> _polygons = new List<Polygon>();

            //string currentMatName = "";
            int currentMatId = -1;
            int currentSmoothingGroup = -1;

            HashedCollection<string> materialNames = new HashedCollection<string>(null);
          

            while (true) {
                string line = textReader.ReadLine();
                if (line == null) {
                    OnEOF(textReader);
                    break;
                }
                ObjLineIdEnum lineId = GetObjLineId(line);
                if (lineId == ObjLineIdEnum.vt) {
                    subString = line.Split(spaceSeparator, System.StringSplitOptions.RemoveEmptyEntries);
                    Vector2 uv = new Vector2(ToFloat(subString[1]), ToFloat(subString[2]));
                    _uvs.Add(uv);

                } else if (lineId == ObjLineIdEnum.v) {
                    subString = line.Split(spaceSeparator, System.StringSplitOptions.RemoveEmptyEntries);
                    Vector3 pos = new Vector3(ToFloat(subString[1]), ToFloat(subString[2]), ToFloat(subString[3])) ;
                    if (swapYZ) {
                        pos.Set(pos.x, pos.z, pos.y);
                    }
                    pos *= scaleFactor;
                    _positions.Add(pos);

                } else if (lineId == ObjLineIdEnum.usemtl) {
                    string currentMatName = line.Remove(0, 7);
                    currentMatId = materialNames.FindOrAddIdx(currentMatName);

                } else if (lineId == ObjLineIdEnum.s_off) {
                    currentSmoothingGroup = -1;
                } else if (lineId == ObjLineIdEnum.s) {
                    currentSmoothingGroup = ToInt(line.Remove(0, 2));
                } else if (lineId == ObjLineIdEnum.f) {
                    faceLineParser.Parse(line);
                    int sg = currentSmoothingGroup;
                    if (sgMode == SmoothingGroupsImportMode.PerPolygon) {
                        sg = -1;
                    } else if (sgMode == SmoothingGroupsImportMode.ForceSmoothAll) {
                        sg = 0;
                    }
                    Polygon polygon = new Polygon(sg, currentMatId, faceLineParser.corners.Count);

                    for (int c = 0; c < faceLineParser.corners.Count; c++) {
                        int posIdx = faceLineParser.corners[c].posIdx;
                        int uvIdx = faceLineParser.corners[c].uvIdx;
                        polygon.corners[c] = new PolygonCorner(posIdx, uvIdx, -1);
                    }

                    if (flipNormals) {
                        System.Array.Reverse(polygon.corners);
                    }
                    _polygons.Add(polygon);
                }
            }

            BuildProcedural(_positions.ToArray(), _uvs.ToArray(), null, _polygons.ToArray());
            modificationInfo = new ModificationInfo( "imported from TextReader" );
        }

        public void ReadObjFromFile (string filePath, SmoothingGroupsImportMode sgMode, bool flipNormals, float scaleFactor, bool swapYZ) {
            System.IO.FileInfo fi = new System.IO.FileInfo(filePath);
            if (!fi.Exists) {
                Debug.LogWarningFormat("obj file not found {0} ", filePath);
                return;
            }
            using (System.IO.TextReader objFileReader = System.IO.File.OpenText(filePath)) {
                ReadObjFromTextReader(objFileReader, sgMode, flipNormals, scaleFactor, swapYZ);
            }
            modificationInfo = new ModificationInfo(string.Format("imported {0}", filePath));
        }

        public void Clear() {
            positions = new SPM_Position[0];
            normals = new SPM_Normal[0];
            uvs = new SPM_uv[0];
            polygons = new SPM_Polygon[0];
            positionEdges = new SPM_Edge[0];
            materials = new SPM_Material[0];
            vertices = new SPM_Vertex[0];
            trianglesCount = 0;
        }

        public void BuildProcedural(Vector3[] posData, Vector2[] uvsData, Color[] colorsData, Polygon[] _polygonsData) {
            Clear();

            if (colorsData == null || colorsData.Length == 0) {
                colorsData = new Color[] { Color.white};
            }

            if (uvsData == null || uvsData.Length == 0) {
                uvsData = new Vector2[] { Vector2.zero };
            }

 
            if (posData == null || posData.Length == 0) {
                posData = new Vector3[] { Vector3.zero };
            }

            List<Polygon> polygonsDataList = new List<Polygon>();
            for (int i = 0; i<_polygonsData.Length; i++) {
                if (_polygonsData[i].isValid) {
                    polygonsDataList.Add(_polygonsData[i]);
                }
            }
            Polygon[] polygonsData = polygonsDataList.ToArray();

            for (int p = 0; p< polygonsData.Length; p++) {
                polygonsData[p].ClampCornerIndices(posData.Length-1, colorsData.Length-1, uvsData.Length-1);
            }

            this.positions = new SPM_Position[posData.Length];
            for (int i = 0; i < posData.Length; i++) {
                this.positions[i] = new SPM_Position(posData[i]);
            }

            polygons = new SPM_Polygon[polygonsData.Length];
            HashedCollection<SPM_Normal> _normals = new HashedCollection<SPM_Normal>(null);
            HashedCollection<SPM_Vertex> _vertices = new HashedCollection<SPM_Vertex>(null);
 
            for (int p = 0; p < polygonsData.Length; p++) {
                Polygon poly = polygonsData[p];
                SPM_Polygon _spmPolygon = new SPM_Polygon(poly.materialId, poly.smoothingGroup, poly.CornersCount); 
                for (int c = 0; c < poly.CornersCount; c++) {
                    int posIdx = poly[c].position;
                    SPM_Normal norm = new SPM_Normal(posIdx, poly.smoothingGroup);
                    int normIdx = _normals.FindOrAddIdx(norm);
                    norm = _normals[normIdx];
                    norm.adjacentPolygons.Add(p);
                    _normals[normIdx] = norm;
                    SPM_Vertex _vertex = new SPM_Vertex(posIdx, normIdx, poly[c].uv, poly[c].color);
                    int vertIdx = _vertices.FindOrAddIdx(_vertex);
                    positions[posIdx].adjacentPolygons.Add(p);
                    positions[posIdx].linkedVertices.Add(vertIdx);
                    _spmPolygon.corners[c] = vertIdx;
                }
                this.polygons[p] = _spmPolygon;
            }

            this.normals = _normals.ToArray();
            this.vertices = _vertices.ToArray();

            for (int i = 0; i<this.vertices.Length; i++) {
                this.normals[this.vertices[i].normIdx].linkedVertices.Add(i);
            }

 
            this.uvs = new SPM_uv[uvsData.Length];
            for (int i = 0; i < uvsData.Length; i++) {
                this.uvs[i] = new SPM_uv(uvsData[i]);
            }

            for (int i = 0; i < vertices.Length; i++) {
                this.uvs[ this.vertices[i].uvIdx ].linkedVertices.Add(i);
            }
 
 
            this.colors = new SPM_color[colorsData.Length];
            for (int i = 0; i<colorsData.Length; i++) {
                this.colors[i] = new SPM_color(colorsData[i]);
            }

            for (int i = 0; i < vertices.Length; i++) {
                this.colors[ vertices[i].colorIdx].linkedVertices.Add(i);
            }
 
            trianglesCount = 0;
            
            HashedCollection<SPM_Edge> _edges = new HashedCollection<SPM_Edge>(null);
            for (int p = 0; p < polygonsData.Length; p++) {
                int cornersCount = this.polygons[p].corners.Length;
                trianglesCount  += (cornersCount - 2);
                for (int c = 0; c < cornersCount; c++) {
                    int edgePosA = vertices[this.polygons[p].corners[c]].posIdx;
                    int edgePosB = vertices[this.polygons[p].corners[(c + 1) % cornersCount]].posIdx;
                    int eidx = _edges.FindOrAddIdx(new SPM_Edge(edgePosA, edgePosB));
                    positions[edgePosA].adjacentEdges.Add(eidx);
                    positions[edgePosB].adjacentEdges.Add(eidx);
                }
            }
            positionEdges = _edges.ToArray();
            modificationInfo = new ModificationInfo("Built procedurally" );
        }

        public static SerializedPolygonalMesh GetProcedural(Vector3[] positions, Vector2[] uvs, Color[] colors, Polygon[] polygons) {
            SerializedPolygonalMesh spm = ScriptableObject.CreateInstance<SerializedPolygonalMesh>();
            spm.BuildProcedural(positions, uvs, colors, polygons);
            return spm;
        }
 
    }
}
