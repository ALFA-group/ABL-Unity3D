using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Linefy.Serialization;
using Linefy.Internal;
using System;

namespace Linefy {

    [ExecuteInEditMode]
    [DefaultExecutionOrder(1000)]
    [HelpURL("https://polyflow.xyz/content/linefy/documentation-1-1/linefy-documentation.html#PolygonalMeshRenderer")]
    public class PolygonalMeshRenderer : MonoBehaviour {
 
        [Tooltip("Source Polygonal Mesh Asset.")]
        public PolygonalMeshAsset polygonalMeshAsset;

        [Tooltip("Polygonal Mesh properties")]
        public SerializationData_PolygonalMeshProperties polygonalMeshProperties = new SerializationData_PolygonalMeshProperties();

        [Tooltip("When enabled, the mesh will be drawn even without an attached MeshRenderer but with a default PolygonalMesh material. Default material settings are available in Polygonal Mesh Properties foldout")]
        public bool drawDefault;

        [Tooltip("Display mesh wireframe.")]
        public bool wireframeEnabled = true;

        [Tooltip("Enables automatical wireframe.viewOffset recalculation.")]
        public bool autoWireframeViewOffset = true;

        [Tooltip("Wireframe properties")]
        public SerializationData_Lines wireframeProperties = new SerializationData_Lines(2, Color.black, 1);

        MeshFilter _mf;
        MeshFilter mf {
            get {
                if (_mf == null) {
                    _mf = GetComponent<MeshFilter>();
                }
                return _mf;
            }
        }

        PolygonalMesh polygonalMesh;
        Lines wireframe;
 
        public void LateUpdate() {
            if (polygonalMeshAsset != null) {
                if (polygonalMesh == null || polygonalMesh.modificationInfo.hash != polygonalMeshAsset.serializedPolygonalMesh.modificationInfo.hash) {
                    if (polygonalMeshAsset.serializedPolygonalMesh == null) {
                        Debug.LogWarning("Something wrong with PolygonalMeshAsset serialization. serializedPolygonalMesh == null");
                    } else {
                        if (polygonalMesh == null) {
                            polygonalMesh = new PolygonalMesh(polygonalMeshAsset.serializedPolygonalMesh);
                        } else {
                            polygonalMesh.BuildFromSPM(polygonalMeshAsset.serializedPolygonalMesh);
                        }
                    }
                }  
            }  
 
            if (polygonalMesh != null) {
                polygonalMesh.LoadSerializationData(polygonalMeshProperties); 
 
                if (drawDefault) {
                    polygonalMesh.Draw(transform.localToWorldMatrix, gameObject.layer);
                }

                if (mf != null) {
                    polygonalMesh.Apply();
                    mf.sharedMesh = polygonalMesh.generatedMesh;
                }

                if (wireframeEnabled) {
                    Matrix4x4 ltw = transform.localToWorldMatrix;
                    if (wireframe == null) {
                        wireframe = new Lines(polygonalMesh.positionEdgesCount);
                    }
                    wireframe.LoadSerializationData(wireframeProperties);
                    polygonalMesh.positionEdgesWireframe = wireframe;
                    polygonalMesh.Apply();

                    if (autoWireframeViewOffset) {
                        Vector3 transformScale = transform.lossyScale;
                        float minScale = Mathf.Min(transformScale.x, Mathf.Min(transformScale.y, transformScale.z));
                        wireframeProperties.viewOffset = polygonalMesh.bounds.size.x * 0.0025f * minScale;
                    }

                    wireframe.Draw(ltw, gameObject.layer);
                } else {
                    polygonalMesh.positionEdgesWireframe = null;
                }
            }

 
        }
 
        private void OnDestroy() {
            if (wireframe != null) {
                wireframe.Dispose();
            }

            if (polygonalMesh != null) {
                polygonalMesh.Dispose();
            }
        }

        [Obsolete("wireframeWidth  is Obsolete , use wireframeProperties.widthMultiplier")]
        public float wireframeWidth;
    }
}
