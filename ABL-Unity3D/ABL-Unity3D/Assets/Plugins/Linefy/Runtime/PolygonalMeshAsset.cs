using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
 
namespace Linefy{
    [HelpURL("https://polyflow.xyz/content/linefy/documentation-1-1/linefy-documentation.html#PolygonalMeshAsset")]
    public class PolygonalMeshAsset : ScriptableObject {

        public bool importFromObjFoldout = true;
        public string pathToObjFile;
        public float scaleFactor = 1;
        public bool swapYZAxis;

        public bool flipNormals;
        public SmoothingGroupsImportMode smoothingGroupsImportMode = SmoothingGroupsImportMode.FromSource;
        public SerializedPolygonalMesh serializedPolygonalMesh ;
        public float lastImportMS;
 

        public bool ImportObjLocal() {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            if (serializedPolygonalMesh == null) {
                Debug.LogWarning("Something wrong with PolygonalMeshAsset serialization. serializedPolygonalMesh == null");
                return false;
            }
            serializedPolygonalMesh.ReadObjFromFile(pathToObjFile, smoothingGroupsImportMode, flipNormals, scaleFactor, swapYZAxis);
            sw.Stop();
            lastImportMS = sw.ElapsedTicks / (float)TimeSpan.TicksPerMillisecond;
            return true;
        }

        public PolygonalMeshRenderer InstantiateRenderer(Material material) {
            GameObject go = new GameObject(string.Format("{0} Polygonal Mesh Renderer", name));
            PolygonalMeshRenderer pmr = go.AddComponent<PolygonalMeshRenderer>();
            go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;
            pmr.polygonalMeshAsset = this;
            //mf.sharedMesh = sharedMesh;
            pmr.LateUpdate();
            return pmr;
        }
    }
}
