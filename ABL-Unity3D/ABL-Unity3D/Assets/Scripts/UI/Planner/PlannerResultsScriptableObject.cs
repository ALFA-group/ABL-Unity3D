#nullable enable
using System;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Utilities.Editor;
using Utilities.Unity;

namespace UI.Planner
{
    
    public class PlannerResultsScriptableObject : SerializedBigDataScriptableObject<PlannerResults>
    {
        private const string AssetPath = "Assets/Data/Results/"; 

        public static PlannerResultsScriptableObject Create(PlannerResults results)
        {
            var so = CreateInstance<PlannerResultsScriptableObject>();
            so.LazyLoadedData = results;
            
            var sceneNameFolderPath = $"{AssetPath}/{results.sceneName}";
            if (!Directory.Exists(sceneNameFolderPath))
            {
                Directory.CreateDirectory(sceneNameFolderPath);
            }
            results.objectPathInScene.CreateFoldersBasedOnGameObjectPath(sceneNameFolderPath);
            
            var assetFilePath = $"{sceneNameFolderPath}/{results.objectPathInScene}.asset";

            string path = AssetDatabase.GenerateUniqueAssetPath(assetFilePath);
            
            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.SaveAssets();

            so.Save(results);

            return so;
        }
        
        
    }
}