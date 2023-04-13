using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Serialization;

namespace Linefy.Primitives {
    
    public class Box : Drawable {

        float _width = 1;
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

        float _height = 1;
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

        float _length = 1;
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

        int _widthSegments = 1;
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

        int _heightSegments = 1;
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

        int _lengthSegments = 1;
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

        public Box(float width, float height, float length,  int widthSegments, int heightSegments, int lengthSegments, SerializationData_LinesBase wireframeProperties) {
            _width = width;
            _height = height;
            _length = length;
            _widthSegments = widthSegments;
            _heightSegments = heightSegments;
            _lengthSegments = lengthSegments;
            this.wireframeProperties = wireframeProperties;
        }

        public Box(float width, float height, float length, int widthSegments, int heightSegments, int lengthSegments ) {
            _width = width;
            _height = height;
            _length = length;
            _widthSegments = widthSegments;
            _heightSegments = heightSegments;
            _lengthSegments = lengthSegments;
        }

        public Box(float size, Color color) { 
            _width = size;
            _height = size;
            _length = size;
			this.wireframeProperties.colorMultiplier = color;
        }
		
		public Box() { 
 
		}

        void PreDraw() {
            if (wireframe == null) {
                wireframe = new Lines(1);
                wireframe.capacityChangeStep = 8;
            }

            if (dTopology) {
                int linesCount = (_heightSegments + 1) * 4;
                linesCount += (_widthSegments - 1) * 4;
                linesCount += (_lengthSegments - 1) * 4;
                linesCount += 4;

                wireframe.count = linesCount;
                for (int i = 0; i < linesCount; i++) {
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
                    float yPos = yPosMin + y * yStep;
                    Vector3 c0 = new Vector3(xPosMin, yPos, zPosMin);
                    Vector3 c1 = new Vector3(xPosMin, yPos, zPosMax);
                    Vector3 c2 = new Vector3(xPosMax, yPos, zPosMax);
                    Vector3 c3 = new Vector3(xPosMax, yPos, zPosMin);
                    wireframe.SetPosition(linesCounter, c0, c1);
                    linesCounter++;
                    wireframe.SetPosition(linesCounter, c1, c2);
                    linesCounter++;
                    wireframe.SetPosition(linesCounter, c2, c3);
                    linesCounter++;
                    wireframe.SetPosition(linesCounter, c3, c0);
                    linesCounter++;
                }

                for (int x = 1; x < widthSegments; x++) {
                    float xPos = xPosMin + x * xStep;
                    Vector3 c0 = new Vector3(xPos, yPosMin, zPosMin);
                    Vector3 c1 = new Vector3(xPos, yPosMin, zPosMax);
                    Vector3 c2 = new Vector3(xPos, yPosMax, zPosMax);
                    Vector3 c3 = new Vector3(xPos, yPosMax, zPosMin);
                    wireframe.SetPosition(linesCounter, c0, c1);
                    linesCounter++;
                    wireframe.SetPosition(linesCounter, c1, c2);
                    linesCounter++;
                    wireframe.SetPosition(linesCounter, c2, c3);
                    linesCounter++;
                    wireframe.SetPosition(linesCounter, c3, c0);
                    linesCounter++;
                }

                for (int z = 1; z < lengthSegments; z++) {
                    float zPos = zPosMin + z * zStep;
                    Vector3 c0 = new Vector3(xPosMin, yPosMin, zPos);
                    Vector3 c1 = new Vector3(xPosMin, yPosMax, zPos);
                    Vector3 c2 = new Vector3(xPosMax, yPosMax, zPos);
                    Vector3 c3 = new Vector3(xPosMax, yPosMin, zPos);

                    wireframe.SetPosition(linesCounter, c0, c1);
                    linesCounter++;
                    wireframe.SetPosition(linesCounter, c1, c2);
                    linesCounter++;
                    wireframe.SetPosition(linesCounter, c2, c3);
                    linesCounter++;
                    wireframe.SetPosition(linesCounter, c3, c0);
                    linesCounter++;
                }

                wireframe.SetPosition(linesCounter, new Vector3(xPosMin, yPosMin, zPosMin), new Vector3(xPosMin, yPosMax, zPosMin));
                linesCounter++;
                wireframe.SetPosition(linesCounter, new Vector3(xPosMin, yPosMin, zPosMax), new Vector3(xPosMin, yPosMax, zPosMax));
                linesCounter++;

                wireframe.SetPosition(linesCounter, new Vector3(xPosMax, yPosMin, zPosMin), new Vector3(xPosMax, yPosMax, zPosMin));
                linesCounter++;
                wireframe.SetPosition(linesCounter, new Vector3(xPosMax, yPosMin, zPosMax), new Vector3(xPosMax, yPosMax, zPosMax));
                linesCounter++;

                dSize = false;
            }
            wireframe.autoTextureOffset = true;
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
    public class LinefyBox : DrawableComponent {

        public float width = 1;
        public float height = 1;
        public float length = 1;

        [Range(1, 128)]
        public int widthSegments = 1;
        [Range(1, 128)]
        public int heightSegments = 1;
        [Range(1, 128)]
        public int lengthSegments = 1;

        public SerializationData_LinesBase wireframeProperties = new SerializationData_LinesBase(3, Color.white, 1);

        Box _box;
        Box box {
            get {
                if (_box == null) { 
                    _box = new Box(width, height, length, widthSegments, heightSegments, lengthSegments, wireframeProperties);
                }
                return _box;
            }
        }
 
        protected override void PreDraw() {
            box.width = width;
            box.height = height;
            box.length = length;
            box.widthSegments = widthSegments;
            box.heightSegments = heightSegments;
            box.lengthSegments = lengthSegments;
            box.wireframeProperties = wireframeProperties;
        }

        public override Drawable drawable {
			get {
				return box;
			}
		}  

        public static LinefyBox CreateInstance() {
            GameObject go = new GameObject("New Box");
            LinefyBox result = go.AddComponent<LinefyBox>();
            return result;
        }
    }
}
