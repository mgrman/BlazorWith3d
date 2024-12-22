#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BlazorWith3dBuildPreprocessor: IPreprocessBuildWithReport
{
    public int callbackOrder { get; }
    public void OnPreprocessBuild(BuildReport report)
    { 
        var args = Environment.GetCommandLineArgs();
        Debug.Log($"GetCommandLineArgs {string.Join(" ",args)}");
        if (args.Any(o => o == "isCiBuild" ||  o == "-isCiBuild"))
        {
            // Set Platform Settings to optimize for disk size (LTO)
            UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;
        }
    }
}

#endif