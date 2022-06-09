using StereoKit;
using System;

namespace SKAnimManagement
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Initialize StereoKit
            SKSettings settings = new SKSettings
            {
                appName = "SKAnimManagement",
                assetsFolder = "Assets",
            };
            if (!SK.Initialize(settings))
                Environment.Exit(1);


            Pose cubePose = new Pose(0, 0, -0.5f, Quat.Identity);
            CustomModel cube = new CustomModel("theCube", "CUBE.gltf");
            cube.Pose = cubePose;

            // don't forget to call a Play/stop somewhere, this cube has "translate"
            // and "rotate" animations


            // Core application loop
            while (SK.Step(() =>
            {
                
                cube.Update();

            })) ;
            SK.Shutdown();
        }
    }
}
