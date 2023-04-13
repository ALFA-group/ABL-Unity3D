using System;
using UnityEditor;
using UnityEngine;

namespace Utilities.Editor
{
    public static class ForceUnloadAssetsBeforeRecompile
    {
        [InitializeOnLoadMethod]
        private static void RegisterCallBack()
        {
            AssemblyReloadEvents.beforeAssemblyReload += AssemblyReloadEventsOnBeforeAssemblyReload;
        }

        private static void AssemblyReloadEventsOnBeforeAssemblyReload()
        {
            var start = DateTime.Now;
            EditorUtility.UnloadUnusedAssetsImmediate(true);
            // This msg tell us the cost of doing an asset unload for every assembly reload.
            // This ?might?? help avoid the issue where Unity gets stuck on Reloading Assemblies 
            //  under normal dev usage on Windows.
            // (Seems to happen more when recompiling GP while showing experiment results in inspector)
            
            //  OR remove the whole callback if we keep seeing that bug despite doing this step 
            double elapsedSeconds = (DateTime.Now - start).TotalSeconds;
            if (elapsedSeconds > 0.1f) Debug.Log($"Done with slow Asset Unload {elapsedSeconds}s");
        }
    }
}