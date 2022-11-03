using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKAnimManagement
{
    public class CustomModel
    {
        public string Name { get; set; }
        public bool IsHandleable = true;
        public Model Model { get; set; }
        public Pose Pose;
        public Bounds Bounds;
        public bool DrawHandle = false;
        public float Scale = 1.0f;

        public IAnimationManager AnimationManager;

        public CustomModel(string name, string filename, Shader shader = null)
        {
            Name = name;
            Model = Model.FromFile(filename, shader);
            Bounds = Model.Bounds;
            Pose = Pose.Identity;
            AnimationManager = new AnimationManager(Model);
        }

        public CustomModel(string name, string filename, Bounds bounds, Pose initialPose, Shader shader = null) 
        {
            Name = name;
            Model = Model.FromFile(filename, shader);
            Pose = initialPose;
            Bounds = bounds;
            AnimationManager = new AnimationManager(Model);
        }

        public CustomModel(string name, string filename, Pose initialPose, Shader shader = null) 
        {
            Name = name;
            Model = Model.FromFile(filename, shader);
            Pose = initialPose;
            Bounds = Model.Bounds;
            AnimationManager = new AnimationManager(Model);
        }



        public void Update()
        {
            if (IsHandleable)
            {
                UI.Handle(Name, ref Pose, Bounds * Scale, DrawHandle);
            }
            AnimationManager.UpdateAnimations();
            Model.Draw(Pose.ToMatrix(Scale));
        }
    }
}
