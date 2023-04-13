using UnityEngine;
using Linefy;
using Linefy.Serialization;
 
namespace LinefyExamples {
    [ExecuteInEditMode]
    public class PolygonalCube : MonoBehaviour {
        PolygonalMesh _cube;
        public PolygonalMesh cube {
            get {
                if (_cube == null) {
                    Vector3[] positions = new Vector3[8];
                    positions[0] = new Vector3(-1, -1, -1);
                    positions[1] = new Vector3(1, -1, -1);
                    positions[2] = new Vector3(1, 1, -1);
                    positions[3] = new Vector3(-1, 1, -1);
                    positions[4] = new Vector3(-1, -1, 1);
                    positions[5] = new Vector3(1, -1, 1);
                    positions[6] = new Vector3(1, 1, 1);
                    positions[7] = new Vector3(-1, 1, 1);

                    Vector2[] uvs = new Vector2[4];
                    uvs[0] = new Vector2(0, 0);
                    uvs[1] = new Vector2(0, 1);
                    uvs[2] = new Vector2(1, 1);
                    uvs[3] = new Vector2(1, 0);

                    Color[] colors = new Color[2];
                    colors[0] = new Color32(255, 0, 118, 255);
                    colors[1] = new Color32(0, 171, 255, 255);

                    Polygon[] polygons = new Polygon[6];
                    polygons[0] = new Polygon(0, 0, new PolygonCorner(0, 0, 0), new PolygonCorner(1, 1, 0), new PolygonCorner(2, 2, 0), new PolygonCorner(3, 3, 0));
                    polygons[1] = new Polygon(1, 0, new PolygonCorner(7, 0, 1), new PolygonCorner(6, 1, 1), new PolygonCorner(5, 2, 1), new PolygonCorner(4, 3, 1));
                    polygons[2] = new Polygon(2, 0, new PolygonCorner(4, 0, 1), new PolygonCorner(5, 1, 1), new PolygonCorner(1, 2, 0), new PolygonCorner(0, 3, 0));
                    polygons[3] = new Polygon(3, 0, new PolygonCorner(5, 0, 1), new PolygonCorner(6, 1, 1), new PolygonCorner(2, 2, 0), new PolygonCorner(1, 3, 0));
                    polygons[4] = new Polygon(4, 0, new PolygonCorner(7, 0, 1), new PolygonCorner(3, 1, 0), new PolygonCorner(2, 2, 0), new PolygonCorner(6, 3, 1));
                    polygons[5] = new Polygon(5, 0, new PolygonCorner(4, 0, 1), new PolygonCorner(0, 1, 0), new PolygonCorner(3, 2, 0), new PolygonCorner(7, 3, 1));

                    _cube = new PolygonalMesh(positions, uvs, colors, polygons);
                }
                return _cube;
            }
        }

        Lines _wireframe;
        Lines wireframe {
            get {
                if (_wireframe == null) {
                    _wireframe = new Lines(12);
                }
                return _wireframe;
            }
        }

        public SerializationData_PolygonalMeshProperties polygonalMeshProperties = new SerializationData_PolygonalMeshProperties();
        public SerializationData_Lines wireframePropertyes = new SerializationData_Lines(2, Color.black, 1);

        private void Update() {
            cube.LoadSerializationData(polygonalMeshProperties);
            cube.positionEdgesWireframe = wireframe;
            wireframe.LoadSerializationData(wireframePropertyes);
            cube.Draw(transform.localToWorldMatrix);
            wireframe.Draw(transform.localToWorldMatrix);    
        }
    }
}
