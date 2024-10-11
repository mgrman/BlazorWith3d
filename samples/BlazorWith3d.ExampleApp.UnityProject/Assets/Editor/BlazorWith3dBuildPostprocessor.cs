#if UNITY_EDITOR
using System;
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

        var buildPrefixName = Path.GetFileName(pathToBuiltProject)+".";
        
        foreach (var file in Directory.GetFiles(buildFilesFolder))
        {
            var destinationFileName = Path.GetFileName(file);
            if (destinationFileName.StartsWith(buildPrefixName))
            {
                destinationFileName = "Build."+ destinationFileName.Substring(buildPrefixName.Length);
            }
            
            File.Copy(file, Path.Combine(backendWwwrootFolder, destinationFileName),true);
        }
        Debug.Log("BlazorWith3d.ExampleApp.Client updated with fresh build!");
    }
}

#endif