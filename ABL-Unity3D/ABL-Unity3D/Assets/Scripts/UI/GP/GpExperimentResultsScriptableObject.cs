#nullable enable

using System.IO;
using GP.Experiments;
using UnityEditor;
using Utilities.Editor;
using Utilities.Unity;

namespace UI.GP
{
    public class GpExperimentResultsScriptableObject : SerializedBigDataScriptableObject<GpExperimentResults>
    {
        private const string AssetPath = "Assets/Data/Results"; 

        public static GpExperimentResultsScriptableObject Create(GpExperimentResults results)
        {
            var so = CreateInstance<GpExperimentResultsScriptableObject>();
            so.LazyLoadedData = results;
            so.name = results.resultString;
            
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