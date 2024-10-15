using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    public class WebGlPlayerSettingsChecker: IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.WebGL)
            {
                if (!PlayerSettings.WebGL.emscriptenArgs.Contains("-sMIN_SAFARI_VERSION=-1"))
                {
                    Debug.LogWarning("Building WebGL without a fix for getContext override for safari, which can cause memory leaks in all browsers! Consider adding '-sMIN_SAFARI_VERSION=-1' to emscriptenArgs in Player settings. This removes the offending code from build.");
                }
                
                if (PlayerSettings.WebGL.threadsSupport)
                {
                   Debug.LogWarning("Building WebGL with threadsSupport! This can cause memory leaks due to the thread workers not being disposed correctly. Consider disabling threadsSupport in Player settings.");
                }
            }
        }
    }
}