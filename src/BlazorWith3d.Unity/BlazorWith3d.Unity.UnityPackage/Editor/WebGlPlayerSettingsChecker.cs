using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    public class WebGlPlayerSettingsChecker
    {
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.WebGL)
            {
                if (!PlayerSettings.WebGL.emscriptenArgs.Contains("-sMIN_SAFARI_VERSION=-1"))
                {
                    Debug.LogWarning("Building WebGL without a fix for getContext override for safari, which can cause memory leaks in all browsers! Consider adding '-sMIN_SAFARI_VERSION=-1' to emscriptenArgs in Player settings. This removes the offending code from build.");
                }
                
                if (PlayerSettings.WebGL.threadsSupport)
                {
                    Debug.LogWarning("Building WebGL with threadsSupport! This can cause memory leaks due to the thread workers not being disposed correctly. Consider disabling threadsSupport in Player settings.");
                }
                
                if (PlayerSettings.WebGL.compressionFormat!= WebGLCompressionFormat.Disabled)
                {
                    Debug.LogWarning("Building with webgl compression, in this template, ASP.NET does own compression on build.");
                }
                
                if (PlayerSettings.WebGL.nameFilesAsHashes)
                {
                    Debug.LogWarning("Building with nameFilesAsHashes, in this template, the names are relied on when updating the ASP.NET wwwroot files.");
                }
            }
        }
    }
}