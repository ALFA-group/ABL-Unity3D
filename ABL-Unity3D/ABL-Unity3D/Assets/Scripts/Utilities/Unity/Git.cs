using UnityEngine;

namespace Utilities.Unity
{
    public static class Git
    {
        private static string _lastGitVersion = "";

        public static string LastGitVersion
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_lastGitVersion))
                {
                    var textAsset = Resources.Load<TextAsset>("LastGitVersion");
                    if (textAsset) _lastGitVersion = textAsset.text;
                }

                return _lastGitVersion;
            }
            set => _lastGitVersion = value;
        }
    }
}