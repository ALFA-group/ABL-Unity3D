using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy;
using System;
using Linefy.Internal;
using Linefy.Primitives;
using Linefy.Serialization;

namespace LinefyExamples {

    [ExecuteInEditMode]
    public class LinefyPlexus : MonoBehaviour {

        public Plexus plexus;

        [Header("Camera")]
        public Camera cam;
        public float cameraFocusDistance = 24;

        [Header("Volume")]
        public Vector3 size = new Vector3(10, 10, 10);
        public float maxConnectionRadius = 2;
        public bool drawSizeHandles;

        public bool drawSizeBounds = true;
        public bool drawCells;

        [Header("Points")]
        public DotsAtlas pointsAtlas;
        [Range(1, 256)]
        public int pointsCount;
        public float speed = 1;
        public float moveAmplitude = 1;
        public int splineKnots = 8;
        public int splineSegments = 8;
        public bool normalizeSpeed = true;
        public bool drawPaths;
        public bool editPointPositions;
 
        [Header("Connections")]
        public Texture2D connectionTexture;
        public int linesCapacityChangeStep = 256;
        public AnimationCurve connectionDistanceFadeCurve;
        public float connectionFadeOffset = 8;

        [Header("Misc")]
        public Gradient color = new Gradient();
        public AnimationCurve zFocusCurve;
        public float width = 1;

        void Update() {
            if (plexus == null) {
                plexus = new Plexus( size, maxConnectionRadius, pointsCount);
            }
            if (cam == null) {
                return;
            }

            plexus.size.SetValue( size );
            plexus.maxRadius.SetValue(maxConnectionRadius);
            plexus.pointsCount.SetValue(pointsCount);
            plexus.moveAmplitude.SetValue( moveAmplitude );
            plexus.pathKnotsCount.SetValue( splineKnots );
            plexus.pathSegments.SetValue(splineSegments);
            plexus.speed = speed;
            plexus.color = color;
            plexus.width = width;
            plexus.connectionTexture = connectionTexture;
            plexus.transCurve = zFocusCurve;
            plexus.editPointsPositions = editPointPositions;
            plexus.pointsAtlas = pointsAtlas;
            plexus.camera = cam;
            plexus.zFocusCurve = zFocusCurve;
            plexus.linesCapacityChangeStep = linesCapacityChangeStep;
            plexus.connectionDistanceFadeCurve = connectionDistanceFadeCurve;
            plexus.connectionFadeOffset = connectionFadeOffset;
            plexus.cameraFocusDistance = cameraFocusDistance;
            plexus.Draw(transform.localToWorldMatrix, gameObject.layer);

            if (Application.isPlaying) {
                plexus.UpdateTime();
            }
        }

        #region GIZMOS
        Lines cameraFocusGizmoLines;
        Box sizeBox;
        Grid3d cellGrid;


#if UNITY_EDITOR
        void OnDrawGizmos() {
            Matrix4x4 ltw = transform.localToWorldMatrix;
            if (drawPaths) {
                for (int i = 0; i < plexus.points.Length; i++) {
                    plexus.points[i].spline.DrawNow(ltw);
                }
            }
            if (cam != null) {
                if (cameraFocusGizmoLines == null) {
                    cameraFocusGizmoLines = new Lines(8, true, 1, 2, Color.yellow);
                }
                Matrix4x4 camtm = cam.transform.localToWorldMatrix;
                float hd = cameraFocusDistance / 2;

                Vector3 qz = Vector3.zero;
                Vector3 q0 = new Vector3(-hd, -hd, cameraFocusDistance);
                Vector3 q1 = new Vector3(-hd, hd, cameraFocusDistance);
                Vector3 q2 = new Vector3(hd, hd, cameraFocusDistance);
                Vector3 q3 = new Vector3(hd, -hd, cameraFocusDistance);
                cameraFocusGizmoLines.SetPosition(0, qz, q0 );
                cameraFocusGizmoLines.SetPosition(1, qz, q1);
                cameraFocusGizmoLines.SetPosition(2, qz, q2);
                cameraFocusGizmoLines.SetPosition(3, qz, q3);

                cameraFocusGizmoLines.SetPosition(4, q0, q1);
                cameraFocusGizmoLines.SetPosition(5, q1, q2);
                cameraFocusGizmoLines.SetPosition(6, q2, q3);
                cameraFocusGizmoLines.SetPosition(7, q3, q0);
                cameraFocusGizmoLines.DrawNow(camtm);
            }

            if (plexus != null) {
                if (drawSizeBounds) {
                    if (sizeBox == null) {
                        sizeBox = new Box(1,1,1,1,1,1, new SerializationData_Lines(2, Color.white, 1) );
                    }

                    sizeBox.width = size.x;
                    sizeBox.height = size.y;
                    sizeBox.length = size.z;

                    sizeBox.DrawNow(ltw);
                }

                if (drawCells) {
                     
                    float cellSize = maxConnectionRadius;
                    Vector3Int cellSegments = plexus.cellsDimensions;
                    Vector3 cellVolumeSize = (Vector3)cellSegments * cellSize;
                    if (cellGrid == null) {
                        cellGrid = new Grid3d( cellVolumeSize.x, cellVolumeSize.y, cellVolumeSize.z, cellSegments.x, cellSegments.y, cellSegments.z, new SerializationData_LinesBase(2, new Color(0,1,0,0.5f), 1)); 
                    }
                  

                    cellGrid.width = cellVolumeSize.x;
                    cellGrid.height = cellVolumeSize.y;
                    cellGrid.length = cellVolumeSize.z;

                    cellGrid.widthSegments = cellSegments.x;
                    cellGrid.heightSegments = cellSegments.y;
                    cellGrid.lengthSegments = cellSegments.z;
 
                    cellGrid.DrawNow(ltw);
                }
            }    
        }
#endif
        #endregion
    }

    public class Plexus : Drawable {
 
        public class Cell {
            public Point[]  points = new Point[128];
            public int pointsCount = 0;

            public Cell[] adjacent;
            public Vector3Int thisAdress;
            public int index;



            public Cell(int index, int coordx, int coordy, int coordz) {
                thisAdress = new Vector3Int(coordx, coordy, coordz);
                this.index = index;
            }

            public void AddPointToCell(Point point) {
                if (pointsCount < points.Length) {
                    points[pointsCount] = point;
                    pointsCount++;
                }
            }

        }

        public class Point : IVector3GetSet {
            public int index;
            Plexus parent;
            Vector3 currentPos;
            float pathPersentage;
            public Plexus.Cell parentCell = null;
            public float speedRandomizer = 1;
            public HermiteSplineClosed spline;
            public float zFocus;
            public float curAlpha;
            public float curWidth;
            public Vector2 screenPos;
            public Color color;
            public Vector3 worldspacePos;

            public Point(Plexus parent, int index) {
                this.parent = parent;
                spline = new HermiteSplineClosed(parent.pathKnotsCount, parent.pathSegments, true, -1);
                spline.properties = new SerializationData_Polyline(  2, new Color(0, 1, 1, 0.45f),1, true );
                Vector3 pathCenter = GetRandomPos(parent.moveAmplitude);
                for (int i = 0; i < spline.knotsCount; i++) {
                    spline[i] = pathCenter + UnityEngine.Random.insideUnitSphere * parent.moveAmplitude * 0.5f;
                }
                spline.ApplyKnotsPositions();

                currentPos = spline[0];
                this.index = index;
                parentCell = parent[currentPos];
            }

            public void UpdatePositionBySpline() {
                currentPos = spline.GetPoint(pathPersentage);
                UpdatePosition();
            }

            public void UpdatePosition() {
                parentCell = parent[currentPos];
                parentCell.AddPointToCell(this);
                worldspacePos = parent.ltw.MultiplyPoint3x4(currentPos);

                Vector3 localInFocusMatrix = parent.worldFocusMatrix.MultiplyPoint3x4(worldspacePos); 

                zFocus = parent.zFocusCurve.Evaluate( Mathf.Abs(localInFocusMatrix.z)/ parent.halfZSize );
                color = parent.color.Evaluate(zFocus);
                screenPos = parent.camera.WorldToScreenPoint(worldspacePos);
                curWidth = 0.5f + zFocus * 10;
                parent.dots[index] = new Dot(currentPos, curWidth, 3, color);
            }

            public void UpdateTime() {
                pathPersentage += Time.deltaTime * parent.speed / spline.splineLength;
                if (pathPersentage >= 1) {
                    pathPersentage = 0;
                }
            }

            public Vector3 GetRandomPos(float boundsMargin) {
                Vector3 _size =  parent.size - new Vector3(boundsMargin, boundsMargin, boundsMargin);

                _size = _size * 0.5f;
                _size.x = Math.Max(0, _size.x);
                _size.y = Math.Max(0, _size.y);
                _size.z = Math.Max(0, _size.z);

                float randomX = UnityEngine.Random.Range(-_size.x, _size.x);
                float randomY = UnityEngine.Random.Range(-_size.y, _size.y);
                float randomZ = UnityEngine.Random.Range(-_size.z, _size.z);
                return new Vector3(randomX, randomY, randomZ);
            }

            public override string ToString() {
                return string.Format("idx:{0} ", index);
            }

            public Vector3 vector3{
                get {
                    return currentPos;
                }

                set {
                    currentPos = value;
                }
            }
        }

        public struct Connection : IEquatable<Connection> {
            public int key;
            public int pointAidx;
            public int pointBidx;
            public float distance;

            public Connection(int key, int pointA, int pointB, float distance ) {
                this.key = key;
                this.pointAidx = pointA;
                this.pointBidx = pointB;
                this.distance = distance;
            }

            public bool Equals(Connection other) {
                return other.key == key;
            }

            public override int GetHashCode() {
                return key;
            }
        }

        public int connectionsCount;

        DFlag d_size = new DFlag("Size", true);
        public DVector3Value size;
        public DFloatValue maxRadius;

        DFlag d_pointsParams = new DFlag("PointsParams", true);
        public DIntValue pointsCount;
        public DIntValue pathKnotsCount;
        public DIntValue pathSegments;
        public DFloatValue moveAmplitude;
 
        public float width;
 
        public Texture2D connectionTexture;
        public Gradient color;
        public DotsAtlas pointsAtlas;
        public Cell[] cells;
        public Point[] points;
        public bool editPointsPositions;
        public Camera camera;
        public Vector3Int cellsDimensions;
        public Vector3 cellsSpaceOffset;
        public Matrix4x4 ltw;
        public AnimationCurve zFocusCurve;
        float halfZSize;
        Matrix4x4 worldFocusMatrix;
        Dots dots;
        public int linesCapacityChangeStep = 256;
        Lines connectionsLines;
        public float speed;
        public AnimationCurve connectionDistanceFadeCurve;
        public float connectionFadeOffset;

        public int info_connectionsCount;
        public int info_linesCount;
        public int info_dotsCount;
        Color clearColor = Color.clear;
        public float cameraFocusDistance;

        public Plexus(Vector3 size, float maxRadius, int pointsCount) {
            
            this.size = new DVector3Value(size, d_size, d_pointsParams);
            this.maxRadius = new DFloatValue(maxRadius, d_size, d_pointsParams);
            this.pointsCount = new DIntValue(pointsCount, d_pointsParams);
            this.pathKnotsCount = new DIntValue(8, d_pointsParams);
            this.pathSegments = new DIntValue(8, d_pointsParams);
            this.moveAmplitude = new DFloatValue(1, d_pointsParams);

            dots = new Dots(this.pointsCount);
            
            dots.transparent = true;
            dots.widthMode = WidthMode.WorldspaceBillboard;

            connectionsLines = new Lines("c", 512, true, 0);
            connectionsLines.widthMode = WidthMode.WorldspaceBillboard;

            adjacentOffsets = new Vector3Int[27];
            int counter = 0;
            for (int x = -1; x<2; x++) {
                for (int y = -1; y < 2; y++) {
                    for (int z = -1; z < 2; z++) {
                        adjacentOffsets[counter] = new Vector3Int(x, y, z);
                        counter++;
                    }
                }
            }
        }

        public void UpdateTime() { 
            for(int i = 0; i<points.Length; i++){
                points[i].UpdateTime();
            }
        }

        public string gridInfo;

        Vector3Int[] adjacentOffsets;

        public AnimationCurve transCurve;

        HashSet<Connection> connections = new HashSet<Connection>();

        void PreDraw() {
   
            dots.atlas = pointsAtlas;
            Vector3 _size = size;
            float _maxConnectionRadius = this.maxRadius;
            halfZSize = _size.z / 2;
            if (d_size) {
                cellsDimensions = new Vector3Int();
                for (int i = 0; i < 3; i++) {
                    cellsDimensions[i] = Mathf.CeilToInt(_size[i] / _maxConnectionRadius);
                    cellsDimensions[i] = Mathf.Max(cellsDimensions[i], 1);
                }

                cellsSpaceOffset = (Vector3)cellsDimensions * _maxConnectionRadius * 0.5f;

                int cellsCount = cellsDimensions.x * cellsDimensions.y * cellsDimensions.z;
                cells = new Cell[cellsCount];
 
                int cellCounter = 0;
                for (int y = 0; y < cellsDimensions.y; y++) {
                    for (int z = 0; z < cellsDimensions.z; z++) {
                         for (int x = 0; x < cellsDimensions.x; x++) {
                            cells[cellCounter] = new Cell(cellCounter, x, y, z);
                            cellCounter++;
                         }
                    }
                }

                List<Cell> tl = new List<Cell>();
                for (int i = 0; i<cells.Length; i++) {
                    Cell c = cells[i];
                    for (int a = 0; a<adjacentOffsets.Length; a++) {
                        Vector3Int aadress = c.thisAdress + adjacentOffsets[a];
                        int acellIdx = this[aadress.x, aadress.y, aadress.z];
                        if (acellIdx >= 0) {
                            tl.Add(cells[acellIdx]);
                        }
                    }

                    c.adjacent = tl.ToArray();
                    tl.Clear();
                }

                gridInfo = string.Format("{0}x{1}x{2}, {3} cells {4}", cellsDimensions.x, cellsDimensions.y, cellsDimensions.z, cells.Length, (Vector3)cellsDimensions * _maxConnectionRadius);
                d_size.Reset();
            }

            if (d_pointsParams) {
 
                points = new Point[(int)pointsCount];
                for (int i = 0; i<points.Length; i++) {
                    points[i] = new Point(this, i);
                }
 
                dots.count = (int)pointsCount;
 
                d_pointsParams.Reset();
            }

            for (int i = 0; i < cells.Length; i++) {
                cells[i].pointsCount = 0;
            }

             

            Vector3 focusMatrixPos = camera.transform.position + camera.transform.forward * cameraFocusDistance;
            worldFocusMatrix = Matrix4x4.TRS(focusMatrixPos, camera.transform.rotation, Vector3.one);
            worldFocusMatrix = worldFocusMatrix.inverse;

            if (editPointsPositions) {
                for (int i = 0; i < points.Length; i++) {
                    points[i].UpdatePosition();
                }
            } else {
                for (int i = 0; i < points.Length; i++) {
                    points[i].UpdatePositionBySpline();
                }
            }
 
            connections.Clear();

            foreach (Point point in points) {
                foreach (Cell ac in point.parentCell.adjacent) {
                    for (int i = 0; i<ac.pointsCount; i++) {
                        Point adjacentPoint = ac.points[i];
                        float distance = Vector3.Distance(point.vector3, adjacentPoint.vector3);
                        if (adjacentPoint != point && distance < maxRadius) {
                            int key = ConnectionKey(point.index, adjacentPoint.index);
                            connections.Add(new Connection(key, point.index, adjacentPoint.index, distance));
                        }
                    }
                }
            }

            connectionsCount = connections.Count;
            connectionsLines.count = connections.Count*3;
            info_connectionsCount = connectionsCount;
            info_linesCount = connectionsLines.count;
            info_dotsCount = points.Length;

            int idxCounter = 0;
            
            foreach (Connection c in connections) {
                Point pA = points[c.pointAidx];
                Point pB = points[c.pointBidx];
 
                Vector3 pa = pA.vector3;
                Vector3 pb = pB.vector3;
 
                float screenDistance = Vector2.Distance(pA.screenPos, pB.screenPos);
                float ip0lv = (pA.curWidth * connectionFadeOffset) / screenDistance;
                ip0lv = Mathf.Min(ip0lv, 0.5f);

                float ip1lv =  1f -((pB.curWidth* connectionFadeOffset) / screenDistance);
                ip1lv = Mathf.Max(ip1lv, 0.5f);

                Vector3 ip0 = Vector3.LerpUnclamped(pa, pb, ip0lv);
                Vector3 ip1 = Vector3.LerpUnclamped(pa, pb, ip1lv);

                float magnitudeFadeFactor = connectionDistanceFadeCurve.Evaluate(  c.distance / maxRadius );

                Color ca = pA.color;
                ca.a *= magnitudeFadeFactor;
                Color cb = pB.color;
                cb.a *= magnitudeFadeFactor;

                connectionsLines[idxCounter] = new Line(pa, ip0, clearColor, ca, pA.curWidth, pA.curWidth, 0, 1);
                idxCounter++;
                connectionsLines[idxCounter] = new Line(ip0,ip1, ca, cb, pA.curWidth , pB.curWidth , 0,1);
                idxCounter++;
                connectionsLines[idxCounter] = new Line(ip1, pb, cb, clearColor, pB.curWidth, pB.curWidth, 0, 1f);
                idxCounter++;
            }

            connectionsLines.texture = connectionTexture;
            connectionsLines.widthMultiplier = width;
            dots.widthMultiplier = width;
        }

        public int ConnectionKey(int pointIdxA, int pointIdxB ) {
            int summ = pointIdxA + pointIdxB;
            int min = Mathf.Min(pointIdxA, pointIdxB);
            return min * 10000 + (summ - min);
        }

        public override void DrawNow(Matrix4x4 matrix) {
            //PreDraw();
        }

        public override void Draw(Matrix4x4 matrix, Camera cam, int layer) {
            ltw = matrix;
            PreDraw();
            dots.Draw(matrix);
            connectionsLines.Draw(matrix);
        }

        public Vector3Int LocalCoordsToCellAdress(Vector3 localCoords ) {
            Vector3 ccords = (localCoords + cellsSpaceOffset)/maxRadius;
            return new Vector3Int(Mathf.FloorToInt(ccords.x), Mathf.FloorToInt(ccords.y), Mathf.FloorToInt(ccords.z));
        }

        /// <summary>
        /// returns index in cell array
        /// </summary>
        public int this[int x, int y, int z] {
            get {
                if (x < 0 || x >= cellsDimensions.x) {
                    return -1;
                }
                if (y < 0 || y >= cellsDimensions.y) {
                    return -1;
                }
                if (z < 0 || z >= cellsDimensions.z) {
                    return -1;
                }
 
                return  (y * cellsDimensions.x * cellsDimensions.z) + (z * cellsDimensions.x) + x ;
            }
        }

        public Cell this[Vector3 localPoint] {
            get {
                Vector3Int a = LocalCoordsToCellAdress(localPoint);
                a.x = Mathf.Clamp(a.x, 0, cellsDimensions.x-1);
                a.y = Mathf.Clamp(a.y, 0, cellsDimensions.y-1);
                a.z = Mathf.Clamp(a.z, 0, cellsDimensions.z-1);

                return cells[(a.y * cellsDimensions.x * cellsDimensions.z) + (a.z * cellsDimensions.x) + a.x];
            }
        }

        public override void Dispose() {

        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            linesCount += 1;
            totallinesCount += connectionsLines.count;

            dotsCount += 1;
            totalDotsCount += dots.count;
        }

    }
}


