using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using System;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif
using Sirenix.Utilities;
using Sirenix.Serialization;
using Object = UnityEngine.Object;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

namespace Utilities.Editor
{
    public class SerializedBigDataScriptableObject<T> : SerializedScriptableObject
        where T : class, new()
    {
        public static string binariesPathInResourcesFolder = "SerializedBigData";

        [HideInInspector]
        public string cachedFileName;

        static SerializedBigDataScriptableObject()
        {
            if (typeof(T).InheritsFrom<UnityEngine.Object>())
            {
                throw new Exception(typeof(T).GetNiceName());
            }
        }

        [NonSerialized]
        private T data = new T();

        [NonSerialized]
        private bool isLoadAttepted;

        [NonSerialized]
        private bool isSuccesfullyLoaded;

        [SerializeField, HideInInspector]
        private List<UnityEngine.Object> unityObjectReferences;

        [HideLabel]
        [ShowInInspector, HideReferenceObjectPicker, OnInspectorGUI(PrependMethodName = "DrawBox")]
        private T Data
        {
            get
            {
                #if UNITY_EDITOR
                // Lets wait until trying to do anything until the object is created.
                if (!AssetDatabase.Contains(this))
                {
                    return this.data;
                }
                #endif

                if (this.isLoadAttepted)
                {
                    if (this.data == null) this.data = new T();
                    return this.data;
                }

                this.data = this.LoadData();
                this.isLoadAttepted = true;
                return this.data;
            }
            set { this.data = value; }
        }

        public T LazyLoadedData
        {
            get => this.Data;
            set => this.Data = value;
        }

    #if UNITY_EDITOR
        private string GetBinaryFileDataPath()
        {
            // Lets wait until trying to do anything until the object is created.

            if (!AssetDatabase.Contains(this))
            {
                return null;
            }

            var assetPath = AssetDatabase.GetAssetPath(this);
            var folder = System.IO.Path.GetDirectoryName(assetPath);
            folder = Path.Combine(folder, "Resources", binariesPathInResourcesFolder);
            cachedFileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            Directory.CreateDirectory(folder);
            var dataPath = folder.Replace('\\', '/').TrimEnd('/') + "/" + cachedFileName + ".bytes";

            return dataPath;
        }

        public virtual void Save(T obj)
        {
            var filePath = this.GetBinaryFileDataPath();
            byte[] bytes = SerializationUtility.SerializeValue(this.data, DataFormat.Binary);
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var writer = new BinaryWriter(fs))
            {
                writer.Write(bytes);
            }
            AssetDatabase.Refresh();
        }

        private void DrawBox()
        {
            GUILayout.Space(10);
            SirenixEditorGUI.Title(typeof(T).GetNiceName().SplitPascalCase(), null, TextAlignment.Left, true);
            var rect = GUILayoutUtility.GetRect(0, 1);
            rect.y -= 12;
            rect.yMin -= 20;
            rect.xMin = rect.xMax - 120;
            rect.height += 4;

            if (GUI.Button(rect.Split(0, 2).Padding(0, 2), "Load"))
            {
                this.data = this.LoadData();
            }

            if (GUI.Button(rect.Split(1, 2).Padding(0, 2), "Save"))
            {
                this.Save(this.data);
            }

            GUILayout.Space(-5);
        }

    #endif

        public virtual T LoadData()
        {
            this.isLoadAttepted = false;
            if (string.IsNullOrEmpty(cachedFileName))
            {
                return null;
            }

            var dataPath = binariesPathInResourcesFolder + "/" + cachedFileName;

            var binary = Resources.Load(dataPath) as TextAsset;
            if (binary == null)
            {
                Debug.LogWarning($"{dataPath} does not exist");
                return null;
            }
            
            var s = new MemoryStream(binary.bytes);
            using (var reader = new BinaryReader(s))
            {
                var obj = SerializationUtility.DeserializeValue<T>(reader.BaseStream, DataFormat.Binary, this.unityObjectReferences);
                return obj;
            }
        }

    }
}