using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Internal;
using Linefy.Serialization;

namespace Linefy.Primitives {

    public class Grid3d : Drawable {
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

        float _length;
        public float length {
            get {
                return _length;
            }

            set {
                if (_length != value) {
                    _length = value;
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

        int _lengthSegments;
        public int lengthSegments {
            get {
                return _lengthSegments;
            }

            set {
                value = Mathf.Max(value, 1);
                if (_lengthSegments != value) {
                    _lengthSegments = value;
                    dTopology = true;
                }
            }
        }


        bool dTopology = true;
        bool dSize = true;

        Lines wireframe;
        public SerializationData_LinesBase wireframeProperties = new SerializationData_LinesBase(3, Color.white, 1);

        public Grid3d(float width, float height, float length, int widthSegments, int heightSegments, int lengthSegments, SerializationData_LinesBase wireframeProperties) {
            _width = width;
            _height = height;
            _length = length;

            _widthSegments = widthSegments;
            _heightSegments = heightSegments;
            _lengthSegments = lengthSegments;

            this.wireframeProperties = wireframeProperties;
        }

        void PreDraw() {


            if (wireframe == null) {
                wireframe = new Lines(1);
                wireframe.capacityChangeStep = 8;
            }
            wireframe.autoTextureOffset = true;

            if (dTopology) {
                int xc = widthSegments + 1;
                int yc = heightSegments + 1;
                int zc = lengthSegments + 1;

                int zo = xc * yc;
                int xo = zc * yc;
                int yo = zc * xc;

                int linesCount = zo + xo + yo;


                wireframe.count = linesCount;
                for (int i = 0; i<wireframe.count; i++) {
                    wireframe.SetTextureOffset(i, 0,0);
                }
                dTopology = false;
                dSize = true;
            }

            if (dSize) {
                float xPosMin = -width / 2f;
                float zPosMin = -length / 2f;
                float yPosMin = -height / 2f;

                float xPosMax = -xPosMin;
                float yPosMax = -yPosMin;
                float zPosMax = -zPosMin;

                float xStep = width / widthSegments;
                float zStep = length / lengthSegments;
                float yStep = height / heightSegments;

                int linesCounter = 0;
                for (int y = 0; y <= heightSegments; y++) {
                    for (int x = 0; x <= widthSegments; x++) {
                        float xpos = xPosMin + xStep * x;
                        float ypos = yPosMin + yStep * y;
                        Vector3 c0 = new Vector3(xpos, ypos, zPosMin);
                        Vector3 c1 = new Vector3(xpos, ypos, zPosMax);
                        wireframe.SetPosition(linesCounter, c0, c1);
                        linesCounter++;
                    }
                }

                for (int z = 0; z <= lengthSegments; z++) {
                    for (int y = 0; y <= heightSegments; y++) {
                        float zpos = zPosMin + zStep * z;
                        float ypos = yPosMin + yStep * y;
                        Vector3 c0 = new Vector3(xPosMin, ypos, zpos);
                        Vector3 c1 = new Vector3(xPosMax, ypos, zpos);
                        wireframe.SetPosition(linesCounter, c0, c1);
                        linesCounter++;
                    }
                }

                for (int z = 0; z <= lengthSegments; z++) {
                    for (int x = 0; x <= widthSegments; x++) {
                        float xpos = xPosMin + xStep * x;
                        float zpos = zPosMin + zStep * z;
                        Vector3 c0 = new Vector3(xpos, yPosMin, zpos);
                        Vector3 c1 = new Vector3(xpos, yPosMax, zpos);
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
            wireframe.Dispose();
        }

        public override void GetStatistic(ref int linesCount, ref int totallinesCount, ref int dotsCount, ref int totalDotsCount, ref int polylinesCount, ref int totalPolylineVerticesCount) {
            if (wireframe != null) {
                linesCount += 1;
                totallinesCount += wireframe.count;
            }
        }
    }

    [ExecuteInEditMode]
    public class LinefyGrid3d : DrawableComponent {

        public float width = 1;
        public float height = 1;
        public float length = 1;

        [Range(1, 128)]
        public int widthSegments = 4;
        [Range(1, 128)]
        public int heightSegments = 4;
        [Range(1, 128)]
        public int lengthSegments = 4;

        public SerializationData_LinesBase wireframeProperties = new SerializationData_LinesBase(3, Color.white, 1);

        Grid3d _grid;
        Grid3d grid {
            get {
                if (_grid == null) {
                    _grid = new Grid3d(width, height, length, widthSegments, heightSegments, lengthSegments, wireframeProperties);
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
            grid.height = height;
            grid.length = length;
            grid.widthSegments = widthSegments;
            grid.heightSegments = heightSegments;
            grid.lengthSegments = lengthSegments;
            grid.wireframeProperties = wireframeProperties ;
        }

        public static LinefyGrid3d CreateInstance() {
            GameObject go = new GameObject("New Grid3d");
            LinefyGrid3d result = go.AddComponent<LinefyGrid3d>();
            return result;
        }
    }
}
