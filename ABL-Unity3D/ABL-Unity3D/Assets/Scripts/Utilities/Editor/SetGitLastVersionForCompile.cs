using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Utilities.Unity;
using Debug = UnityEngine.Debug;

namespace Utilities.Editor
{
    public class SetGitLastVersionForCompile : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!TryCallGitForVersionString(out string hash)) return;

            var textAsset = Resources.Load<TextAsset>("LastGitVersionForBuild");
            if (textAsset && textAsset.text != hash)
            {
                File.WriteAllText(AssetDatabase.GetAssetPath(textAsset), hash);
                EditorUtility.SetDirty(textAsset);
                Debug.Log("Setting git hash value for build.");
            }
        }

        [InitializeOnLoadMethod]
        private static void RegisterCallBack()
        {
            AssemblyReloadEvents.afterAssemblyReload += AssemblyReloadEventsOnAfterAssemblyReload;
        }

        private static void AssemblyReloadEventsOnAfterAssemblyReload()
        {
            if (!TryCallGitForVersionString(out string hash)) return;
            Git.LastGitVersion = hash;
        }

        private static bool TryCallGitForVersionString(out string hash)
        {
            var start = DateTime.Now;
            hash = "";

            var git = new Process();
            git.StartInfo.UseShellExecute = false;
            git.StartInfo.Arguments = "rev-parse HEAD";
            git.StartInfo.FileName = "git";
            git.StartInfo.RedirectStandardOutput = true;
            git.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            git.StartInfo.CreateNoWindow = true;

            if (!git.Start())
            {
                Debug.Log("git process not started");
                return false;
            }

            git.WaitForExit(500);

            if (!git.HasExited)
            {
                Debug.Log("git process failed");
                return false;
            }

            hash = git.StandardOutput.ReadLine();
            double secondsElapsed = (DateTime.Now - start).TotalSeconds;
            if (secondsElapsed > 0.1f) Debug.Log($"Getting git version took {secondsElapsed}s");
            return true;
        }
    }
}