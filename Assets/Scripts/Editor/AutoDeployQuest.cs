using System.Diagnostics;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class AutoDeployQuest
{
    [PostProcessBuild(1)] // Higher number means it runs later in the build chain
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        // Check for Android (Quest)
        if (target == BuildTarget.Android)
        {
            UnityEngine.Debug.Log("Build Complete: Starting Android Deployment...");
            RunScript("deploy_android.ps1");
        }
        // Check for Server (Adjust 'StandaloneWindows64' to your server target)
        else
        {
            UnityEngine.Debug.Log("Build Complete: Starting Server Deployment...");
            RunScript("deploy_server.ps1");
        }
    }

    private static void RunScript(string scriptName)
    {
        try
        {
            // This gets the path to your Project Root
            string projectRoot = System.IO.Directory.GetCurrentDirectory();

            // Update this path to where your scripts actually live!
            // Example: "Assets/Scripts/Deploy/yourscript.ps1"
            string fullPath = System.IO.Path.Combine(projectRoot, "Assets", "Scripts", scriptName);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                // We use quotes around the path in case you have spaces in your folder names
                Arguments = $"-ExecutionPolicy Bypass -File \"{fullPath}\"",
                UseShellExecute = true,
                CreateNoWindow = false,
            };

            UnityEngine.Debug.Log($"Executing: {fullPath}");
            Process.Start(startInfo);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to run {scriptName}: {e.Message}");
        }
    }
}
