using StereoKit;
using System;

namespace SKAnimManagement
{
    public enum AnimAction { PlayOnceReset, PlayOnce, Loop, Pause, Stop, }
    public class AnimationStartedEventArgs : EventArgs
    {

    }

    public class AnimationStoppedEventArgs : EventArgs
    {

    }

    public class AnimationEndedEventArgs : EventArgs
    {

    }
    public class CustomAnimation
    {
        public Anim Anim { get; set; }
        public bool CanStack { get; protected set; }

        protected AnimAction currentAction = AnimAction.Stop;
        public AnimAction CurrentAction
        {
            get => currentAction;
            set
            {
                if (value == AnimAction.PlayOnce || value == AnimAction.PlayOnceReset || value == AnimAction.Loop)
                {
                    PlayMode = value;
                }
                currentAction = value;
            }
        }
        public AnimAction PlayMode { get; protected set; } = AnimAction.PlayOnceReset;

        private float beginPlayTime = 0f;
        public float BeginPlayTime { get => beginPlayTime; set => beginPlayTime = Math.Clamp(value, 0, Duration); }

        protected float endPlayTime = 0f;
        public float EndPlayTime { get => endPlayTime; set => endPlayTime = Math.Clamp(value, 0, Duration); }

        protected float speed = 1f;
        public float Speed { get => speed; set => speed = value < 0 ? 0 : value; }
        public float Duration { get; protected set; }

        protected float animTime = 0f;
        public float AnimCompletion { get => animTime / Duration; }
        public float AnimTime { get => animTime; }

        public CustomAnimation(Anim anim)
        {
            Anim = anim;
            Duration = anim.Duration;
            CanStack = true;
        }

        public event EventHandler<AnimationStartedEventArgs> AnimationStart;
        public event EventHandler<AnimationStoppedEventArgs> AnimationStopped;
        public event EventHandler<AnimationEndedEventArgs> AnimationEnded;


        public void Update(Model model)
        {
            if (BeginPlayTime == 0 && EndPlayTime == 0)
                EndPlayTime = Duration;

            model.PlayAnim(Anim, AnimMode.Manual);
            if (CurrentAction == AnimAction.Pause)
            {
                model.AnimTime = animTime;
                return;
            }
            if (CurrentAction == AnimAction.Stop)
            {
                animTime = BeginPlayTime;
                model.AnimTime = animTime;
                model.StepAnim();
                FireStop(this, new AnimationStoppedEventArgs());
                return;
            }
            if (animTime >= EndPlayTime)
            {
                if (CurrentAction == AnimAction.PlayOnceReset)
                {
                    animTime = 0f;
                    model.AnimTime = animTime;
                }
                if (CurrentAction == AnimAction.Loop)
                {
                    animTime = BeginPlayTime;
                    model.AnimTime = animTime;
                }
                if (CurrentAction == AnimAction.PlayOnceReset || CurrentAction == AnimAction.PlayOnce)
                {
                    CurrentAction = AnimAction.Stop;
                    FireEnd(this, new AnimationEndedEventArgs());
                    return;
                }
            }
            animTime = Math.Clamp(Time.Elapsedf + animTime * Speed, BeginPlayTime, EndPlayTime);
            model.AnimTime = animTime;
            model.StepAnim();
        }


        protected void FireStart(object sender, AnimationStartedEventArgs args)
        {
            AnimationStart?.Invoke(sender, args);
        }

        protected void FireStop(object sender, AnimationStoppedEventArgs args)
        {
            AnimationStopped?.Invoke(sender, args);
        }

        protected void FireEnd(object sender, AnimationEndedEventArgs args)
        {
            AnimationEnded?.Invoke(sender, args);
        }
    }
}