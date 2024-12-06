#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

public class BlazorWith3dBuildPostprocessor : IPostprocessBuildWithReport
{
    public int callbackOrder { get; }

    public void OnPostprocessBuild(BuildReport report)
    {
        var pathToBuiltProject = report.summary.outputPath;
        var buildFilesFolder = Path.GetFullPath(Path.Combine(pathToBuiltProject, "Build"));
        var backendWwwrootFolder =
            Path.GetFullPath(Path.Combine("..", "BlazorWith3d.ExampleApp.Client.Unity", "wwwroot"));

        if (!Directory.Exists(backendWwwrootFolder))
        {
            Directory.CreateDirectory(backendWwwrootFolder);
        }

        var buildPrefixName = Path.GetFileName(pathToBuiltProject) + ".";

        foreach (var file in Directory.GetFiles(buildFilesFolder))
        {
            var destinationFileName = Path.GetFileName(file);
            if (destinationFileName.StartsWith(buildPrefixName))
                destinationFileName = "Build." + destinationFileName.Substring(buildPrefixName.Length);

            File.Copy(file, Path.Combine(backendWwwrootFolder, destinationFileName), true);
        }

        Debug.Log("BlazorWith3d.ExampleApp.Client updated with fresh build!");
    }
}

#endif