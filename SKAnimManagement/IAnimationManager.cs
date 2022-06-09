using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKAnimManagement
{
    public interface IAnimationManager
    {
        void UpdateAnimations();
        bool AddAnimation(string name, CustomAnimation animation);
        bool RemoveAnimation(string name);
        CustomAnimation GetAnimation(string name);
        bool HasAnimation(string name);
        bool HasAnimation(CustomAnimation animation);

        void PlayOnce(CustomAnimation animation, float begin = 0, float end = 0, float speed = 1f);
        void PlayOnceAndReset(CustomAnimation animation, float begin = 0, float end = 0, float speed = 1f);
        void PlayLoop(CustomAnimation animation, float begin = 0, float end = 0, float speed = 1f);
        void Stop(CustomAnimation animation);
        void Pause(CustomAnimation animation);
        void Resume(CustomAnimation animation);
        void PlayOnce(string animation, float begin = 0, float end = 0, float speed = 1f);
        void PlayOnceAndReset(string animation, float begin = 0, float end = 0, float speed = 1f);
        void PlayLoop(string animation, float begin = 0, float end = 0, float speed = 1f);
        void Stop(string animation);
        void Pause(string animation);
        void Resume(string animation);
    }
}
