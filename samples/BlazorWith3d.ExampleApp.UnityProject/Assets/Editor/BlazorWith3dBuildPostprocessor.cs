#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class BlazorWith3dBuildPostprocessor {
    
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        var buildFilesFolder =Path.GetFullPath( Path.Combine(pathToBuiltProject, "Build"));
        var backendWwwrootFolder = Path.GetFullPath(Path.Combine("..", "BlazorWith3d.ExampleApp.Client","wwwroot"));

        foreach (var file in Directory.GetFiles(buildFilesFolder))
        {
            File.Copy(file,Path.Combine(backendWwwrootFolder,Path.GetFileName(file)),true);
        }
        Debug.Log("BlazorWith3d.ExampleApp.Client updated with fresh build!");
    }
}

#endif