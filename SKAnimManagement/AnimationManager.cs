using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKAnimManagement
{

    public class AnimationCommand
    {
        public CustomAnimation Animation { get; set; }
        public AnimAction Action { get; set; }

        private float beginAnim = 0f;
        public float BeginAnim { get => beginAnim; set => beginAnim = Math.Clamp(value, 0, Animation.Duration); }

        private float endAnim = 0f;
        public float EndAnim { get => endAnim; set => endAnim = Math.Clamp(value, 0, Animation.Duration); }

        private float playSpeed = 1f;
        public float PlaySpeed { get => playSpeed; set => playSpeed = value < 0 ? 0 : value; }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static bool operator ==(AnimationCommand a, AnimationCommand b)
        {

            return a?.Animation == b?.Animation && a?.Action == b?.Action && a?.BeginAnim == b?.BeginAnim && a?.EndAnim == b?.EndAnim;
        }

        public static bool operator !=(AnimationCommand a, AnimationCommand b)
        {

            return !(a?.Animation == b?.Animation && a?.Action == b?.Action && a?.BeginAnim == b?.BeginAnim && a?.EndAnim == b?.EndAnim);
        }

    }
    public class AnimationManager : IAnimationManager
    {
        public Model model;
        List<AnimationCommand> animationQueue = new List<AnimationCommand>();
        List<AnimationCommand> animationQueueBin = new List<AnimationCommand>();
        Dictionary<string, CustomAnimation> animationDict = new Dictionary<string, CustomAnimation>();

        public AnimationManager(Model model)
        {
            this.model = model;
            foreach (Anim anim in model.Anims)
            {
                // WARNING : doesn't take into account that animations don't have IDs, be careful to have
                // different names for each animation
                animationDict[anim.Name] = new CustomAnimation(anim);
                animationDict[anim.Name].AnimationStart += OnAnimStart;
                animationDict[anim.Name].AnimationStopped += OnAnimStop;
                animationDict[anim.Name].AnimationEnded += OnAnimEnd;

            }
        }

        public bool AddAnimation(string name, CustomAnimation animation)
        {
            if (!animationDict.ContainsKey(name))
            {
                animationDict[name] = animation;
                return true;
            }
            return false;
        }

        public bool RemoveAnimation(string name)
        {
            CustomAnimation res;
            if (!animationDict.TryGetValue(name, out res))
                return false;
            if (res is CustomAnimation)
            { Log.Warn("SK Animations may not be removed"); return false; }
            return animationDict.Remove(name);

        }

        public CustomAnimation GetAnimation(string name)
        {
            CustomAnimation anim = null;
            animationDict.TryGetValue(name, out anim);
            return anim;
        }


        #region Animation Commands
        public void Pause(CustomAnimation animation)
        {
            // Pausing is useful only if there was something playing the instant before
            AnimationCommand cmd = animationQueue.Where(command => command.Animation == animation).FirstOrDefault();
            if (cmd != null)
            {
                cmd.Action = AnimAction.Pause;
            }
        }

        public void Pause(string animation)
        {
            CustomAnimation anim;
            if (animationDict.TryGetValue(animation, out anim))
                Pause(anim);
        }

        public void PlayLoop(CustomAnimation animation, float begin = 0, float end = 0, float speed = 1f)
        {
            if (!animationDict.ContainsValue(animation))
                return;
            var cmd = new AnimationCommand() { Action = AnimAction.Loop, Animation = animation, BeginAnim = begin, EndAnim = end, PlaySpeed = speed };
            if (TryUpdateAction(cmd))
                return;
            var concurrent = AddStackableOrNoConcurrent(cmd);
            if (concurrent == null)
                return;
            ReplaceConcurrentByRequested(concurrent, cmd);

        }

        public void PlayLoop(string animation, float begin = 0, float end = 0, float speed = 1f)
        {
            CustomAnimation anim;
            if (animationDict.TryGetValue(animation, out anim))
                PlayLoop(anim, begin, end, speed);
        }

        public void PlayOnce(CustomAnimation animation, float begin = 0, float end = 0, float speed = 1f)
        {
            if (!animationDict.ContainsValue(animation))
                return;
            var cmd = new AnimationCommand() { Action = AnimAction.PlayOnce, Animation = animation, BeginAnim = begin, EndAnim = end, PlaySpeed = speed };
            if (TryUpdateAction(cmd))
                return;
            var concurrent = AddStackableOrNoConcurrent(cmd);
            if (concurrent == null)
                return;
            ReplaceConcurrentByRequested(concurrent, cmd);
        }

        public void PlayOnce(string animation, float begin = 0, float end = 0, float speed = 1f)
        {
            CustomAnimation anim;
            if (animationDict.TryGetValue(animation, out anim))
                PlayOnce(anim, begin, end, speed);
        }

        public void PlayOnceAndReset(CustomAnimation animation, float begin = 0, float end = 0, float speed = 1f)
        {
            if (!animationDict.ContainsValue(animation))
                return;
            var cmd = new AnimationCommand() { Action = AnimAction.PlayOnceReset, Animation = animation, BeginAnim = begin, EndAnim = end, PlaySpeed = speed };
            if (TryUpdateAction(cmd))
                return;
            var concurrent = AddStackableOrNoConcurrent(cmd);
            if (concurrent == null)
                return;
            ReplaceConcurrentByRequested(concurrent, cmd);
        }

        public void PlayOnceAndReset(string animation, float begin = 0, float end = 0, float speed = 1f)
        {
            CustomAnimation anim;
            if (animationDict.TryGetValue(animation, out anim))
                PlayOnceAndReset(anim, begin, end, speed);
        }



        public void Stop(CustomAnimation animation)
        {
            AnimationCommand cmd = animationQueue.Where(command => command.Animation == animation).FirstOrDefault();
            if (cmd != null)
            {
                cmd.Action = AnimAction.Stop;
                cmd.BeginAnim = 0f;
                cmd.EndAnim = 0f;
                cmd.PlaySpeed = 0f;
            }
        }

        public void Stop(string animation)
        {
            CustomAnimation anim;
            if (animationDict.TryGetValue(animation, out anim))
                Stop(anim);
        }

        public void Resume(CustomAnimation animation)
        {
            AnimationCommand cmd = animationQueue.Where(command => command.Animation == animation).FirstOrDefault();
            if (cmd != null)
            {
                cmd.Action = cmd.Animation.PlayMode;
            }
        }

        public void Resume(string animation)
        {
            CustomAnimation anim;
            if (animationDict.TryGetValue(animation, out anim))
                Resume(anim);
        }

        #endregion

        public void UpdateAnimations()
        {
            foreach (var toDelete in animationQueueBin)
            {
                animationQueue.Remove(toDelete);
            }
            animationQueueBin.Clear();
            foreach (var act in animationQueue)
            {
                if (act.Action != act.Animation.CurrentAction)
                    act.Animation.CurrentAction = act.Action;

                if (act.BeginAnim != act.Animation.BeginPlayTime)
                    act.Animation.BeginPlayTime = act.BeginAnim;

                if (act.EndAnim != act.Animation.EndPlayTime)
                    act.Animation.EndPlayTime = act.EndAnim;

                act.Animation.Update(model);
            }
        }

        /// <summary>
        /// Checks if the animation exists, if so, updates it
        /// </summary>
        /// <param name="requestedAnimationCommand"></param>
        /// <returns>true if enqueued successfully</returns>
        protected bool TryUpdateAction(AnimationCommand requestedAnimationCommand)
        {
            // is the animation in the animation queue ?
            var currentAnimationCommand = animationQueue.Where(a => a.Animation == requestedAnimationCommand.Animation).FirstOrDefault();
            if (currentAnimationCommand != null)
            {
                // is the requested action the same as the current one? (including animation bounds)
                if (currentAnimationCommand.Action != requestedAnimationCommand.Action
                   || currentAnimationCommand.BeginAnim != requestedAnimationCommand.BeginAnim
                   || currentAnimationCommand.EndAnim != requestedAnimationCommand.EndAnim)
                {
                    // updating the current animation
                    animationQueueBin.Add(currentAnimationCommand);
                    animationQueue.Add(requestedAnimationCommand);

                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the animation can be stacked, if so adds it to the animation queue
        /// </summary>
        /// <param name="requestedAnimationCommand"></param>
        /// <returns>null if enqueud successfully, otherwise it meas there's a concurrent command</returns>
        protected AnimationCommand AddStackableOrNoConcurrent(AnimationCommand requestedAnimationCommand)
        {
            // can this animation type be stacked with one of the same type? ?
            if (!requestedAnimationCommand.Animation.CanStack)
            {
                // if that's the case, is ther one in the animation queue?
                var concurrentAnimationCommand = animationQueue.Where(a => a.Animation.GetType() == requestedAnimationCommand.Animation.GetType()).FirstOrDefault();
                if (concurrentAnimationCommand == null)
                {
                    animationQueue.Add(requestedAnimationCommand);
                    return null;
                }
                return concurrentAnimationCommand;

            }
            animationQueue.Add(requestedAnimationCommand);
            return null;
        }

        /// <summary>
        /// Replaces a concurrent command with a requested one. Recommended to use only when the requested command is a play
        /// </summary>
        /// <param name="concurrentCommand"></param>
        /// <param name="requestedCommand"></param>
        protected void ReplaceConcurrentByRequested(AnimationCommand concurrentCommand, AnimationCommand requestedCommand)
        {
            // on remplace l'action courante du concurrent par un Stop, la prochaine frame déclenchera le OnStop qui retirera l'animation de la file
            int index = animationQueue.IndexOf(concurrentCommand);
            var stopAction = new AnimationCommand() { Action = AnimAction.Stop, Animation = concurrentCommand.Animation };
            animationQueueBin.Add(animationQueue[index]);
            animationQueue.Add(stopAction);
            animationQueue.Add(requestedCommand);
        }


        #region AnimationEventManagement
        private void OnAnimEnd(object sender, AnimationEndedEventArgs e)
        {
            OnAnimStop(sender, null);
        }

        private void OnAnimStop(object sender, AnimationStoppedEventArgs e)
        {
            CustomAnimation anim = (CustomAnimation)sender;
            var a = animationQueue.Where(animTupl => animTupl.Animation == anim).FirstOrDefault();
            animationQueueBin.Add(a);

        }

        private void OnAnimStart(object sender, AnimationStartedEventArgs e)
        {
        }


        #endregion
        public bool HasAnimation(string name)
        {
            return animationDict.ContainsKey(name);
        }

        public bool HasAnimation(CustomAnimation animation)
        {
            return animationDict.ContainsValue(animation);
        }
    }
}
