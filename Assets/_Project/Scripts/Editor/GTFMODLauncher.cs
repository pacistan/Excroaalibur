using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
public class GTFMODLauncher
{
    [MenuItem("Tools/FMOD/Open FMOD Project")]
    public static void OpenFMODProject()
    {
        string relativePath = "Croawn\\Croawn.fspro";
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

        if (File.Exists(fullPath))
        {
            var psi = new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true // important for file associations like .fspro
            };

            Process.Start(psi);
        }
        else
        {
            UnityEngine.Debug.LogError("FMOD project file not found at: " + fullPath);
        }
    }
}
