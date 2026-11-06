using System;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Owan.Editor
{
    public static class PrototypeBuild
    {
        [MenuItem("Oh Wan/Build Windows Prototype")]
        public static void Windows()
        {
            Build("Builds/Windows/OhWan.exe", BuildTarget.StandaloneWindows64);
        }

        [MenuItem("Oh Wan/Build Linux Prototype")]
        public static void Linux()
        {
            Build("Builds/Linux/OhWan.x86_64", BuildTarget.StandaloneLinux64);
        }

        private static void Build(string output, BuildTarget target)
        {
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { "Assets/Scenes/SampleScene.unity" },
                locationPathName = output,
                target = target,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Oh Wan build failed: " + report.summary.result);
        }
    }
}
