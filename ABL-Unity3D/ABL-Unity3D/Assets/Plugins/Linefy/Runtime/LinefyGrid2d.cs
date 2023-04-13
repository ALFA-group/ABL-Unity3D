using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Internal;
using Linefy.Serialization;

namespace Linefy.Primitives {

    public class Grid2d : Drawable {


        float _width;
        public float width {
            get {
                return _width;
            }

            set {
                if (_width != value) {
                    _width = value;
                    dSize = true;
                }
            }
        }

        float _height;
        public float height {
            get {
                return _height;
            }

            set {
                if (_height != value) {
                    _height = value;
                    dSize = true;
                }
            }
        }

        int _widthSegments;
        public int widthSegments {
            get {
                return _widthSegments;
            }

            set {
                value = Mathf.Max(value, 1);
                if (_widthSegments != value) {
                    _widthSegments = value;
                    dTopology = true;
                }
            }
        }
 
        int _heightSegments;
        public int heightSegments {
            get {
                return _heightSegments;
            }

            set {
                value = Mathf.Max(value, 1);
                if (_heightSegments != value) {
                    _heightSegments = value;
                    dTopology = true;
                }
            }
        }

        bool _linePerSegment;
        public bool linePerSegment {
            get {
                return _linePerSegment;
            }

            set {
                if (_linePerSegment != value) {
                    _linePerSegment = value;
                    dTopology = true;
                }
            }
        }

        bool dTopology = true;
        bool dSize = true;

        Lines wireframe;
        public SerializationData_LinesBase wireframeProperties = new SerializationData_LinesBase();

        public Grid2d(float width, float height, int widthSegments, int heightSegments, bool linePerSegment, SerializationData_LinesBase wireframeProperties) {
            _width = width;
            _height = height;
            _widthSegments = widthSegments;
            _heightSegments = heightSegments;
            _linePerSegment = linePerSegment;
            this.wireframeProperties = wireframeProperties;
        }

        void PreDraw() {
            if (wireframe == null) {
                wireframe = new Lines(1);
                wireframe.capacityChangeStep = 8;
            }

            if (dTopology) {
                int linesCount = 0;
                if (_linePerSegment) {
                    int xc = widthSegments + 1;
                    int zc = heightSegments + 1;
                    linesCount = xc * heightSegments +  zc * widthSegments;
                } else {
                    int xc = widthSegments + 1;
                    int zc = heightSegments + 1;
                    linesCount = xc  +  zc;
                }
 
                wireframe.count = linesCount;

                for (int i = 0; i < wireframe.count; i++) {
                    wireframe.SetTextureOffset(i, 0, 0);
                }

                dTopology = false;
                dSize = true;
            }

            wireframe.autoTextureOffset = true;

            if (dSize) {
                float xPosMin = -width / 2f;
                float zPosMin = -height / 2f;
                float xPosMax = -xPosMin;
                float zPosMax = -zPosMin;
                float xStep = width / widthSegments;
                float zStep = height / heightSegments;
                int linesCounter = 0;

                if (linePerSegment) {
                    for (int z = 0; z <= heightSegments; z++) {
                        float zpos = zPosMin + zStep * z;
                        for (int x = 0; x < widthSegments; x++) {
                            float xpos = xPosMin + xStep * x;

                            Vector3 c0 = new Vector3(xpos,   zpos, 0);
                            Vector3 c1 = new Vector3(xpos + xStep,   zpos, 0);
                            wireframe.SetPosition(linesCounter, c0, c1);
                            linesCounter++;
                        }
                    }

                    for (int x = 0; x <= widthSegments; x++) {
                        float xpos = xPosMin + xStep * x;
                        for (int z = 0; z < heightSegments; z++) {
                            float zpos = zPosMin + zStep * z;

                            Vector3 c0 = new Vector3(xpos,   zpos, 0);
                            Vector3 c1 = new Vector3(xpos,   zpos + zStep, 0);
                            wireframe.SetPosition(linesCounter, c0, c1);
                            linesCounter++;
                        }
                    }
                } else {
                    for (int z = 0; z <= heightSegments; z++) {
                        float zpos = zPosMin + zStep * z;
                        Vector3 c0 = new Vector3(xPosMin,  zpos, 0);
                        Vector3 c1 = new Vector3(xPosMax,   zpos, 0);
                        wireframe.SetPosition(linesCounter, c0, c1);
                        linesCounter++;
                    }
                    for (int x = 0; x <= widthSegments; x++) {
                        float xpos = xPosMin + xStep * x;
                        Vector3 c0 = new Vector3(xpos, zPosMin, 0);
                        Vector3 c1 = new Vector3(xpos, zPosMax, 0);
                        wireframe.SetPosition(linesCounter, c0, c1);
                        linesCounter++;
                    }
                }
 
                dSize = false;
            }
            wireframe.LoadSerializationData(wireframeProperties);
        }

        public override void DrawNow(Matrix4x4 matrix) {
            PreDraw();
            wireframe.DrawNow(matrix);
        }

        public override void Draw(Matrix4x4 tm, Camera cam, int layer) {
            PreDraw();
            wireframe.Draw(tm, cam, layer);
        }

        public override void Dispose() {
            if (wireframe != null) {
                wireframe.Dispose();
            }
        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            if (wireframe != null) {
                linesCount += 1;
                totallinesCount += wireframe.count;
            }
        }
    }

    [ExecuteInEditMode]
    public class LinefyGrid2d : DrawableComponent {

        public float width = 1;
        public float length = 1;

        [Range(1, 128)]
        public int widthSegments = 4;
 
        [Range(1, 128)]
        public int lengthSegments = 4;

        public bool linePerSegment = false;

        public SerializationData_LinesBase wireframeProperties = new SerializationData_LinesBase(3,Color.white, 1);

        Grid2d _grid;
        Grid2d grid {
            get {
                if (_grid == null) {
                    _grid = new Grid2d(width, length, widthSegments, lengthSegments, linePerSegment, wireframeProperties);
                }
                return _grid;
            }
        }

        public override Drawable drawable {
			get{
				return grid;
			}
		}  

        protected override void PreDraw() {
            grid.width = width;
            grid.height = length;
            grid.widthSegments = widthSegments;
            grid.heightSegments = lengthSegments;
            grid.linePerSegment = linePerSegment;
            grid.wireframeProperties = wireframeProperties ;
        }

        public static LinefyGrid2d CreateInstance() {
            GameObject go = new GameObject("New Grid2d");
            LinefyGrid2d result = go.AddComponent<LinefyGrid2d>();
            return result;
        }
    }


}
