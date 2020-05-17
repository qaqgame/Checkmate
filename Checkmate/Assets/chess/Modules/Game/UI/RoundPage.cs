using FairyGUI;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Modules.Game.UI
{
    public enum RoundAnim
    {
        Begin,
        End
    }
    public class RoundPage:FGUIPage
    {
        GImage mBegin, mEnd;

        Transition mBeginAnim, mEndAnim;

        public Action onOpen = null;
        public Action onPlayFinished = null;

        public bool IsPlaying
        {
            get
            {
                return mBeginAnim.playing || mEndAnim.playing;
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            mBegin = mCtrlTarget.GetChild("Begin").asImage;
            mEnd = mCtrlTarget.GetChild("End").asImage;
            mBeginAnim = mCtrlTarget.GetTransition("BeginAnim");
            mEndAnim = mCtrlTarget.GetTransition("EndAnim");
        }

        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);
            RoundAnim anim = (RoundAnim)arg;
            switch (anim)
            {
                case RoundAnim.Begin:
                    {
                        PlayBegin();
                        break;
                    }
                case RoundAnim.End:
                    {
                        PlayEnd();
                        break;
                    }
            }
        }


        public void PlayBegin()
        {
            mEnd.visible = false;
            mBegin.visible = true;
            mBeginAnim.Play(OnPlayFinished);
        }

        public void PlayEnd()
        {
            mBegin.visible = false;
            mEnd.visible = true;
            mEndAnim.Play(OnPlayFinished);
        }


        private void OnPlayFinished()
        {
            if (onPlayFinished != null)
            {
                onPlayFinished.Invoke();
            }
        }

        public void Show()
        {
            mCtrlTarget.visible = true;
        }

        public void Hide()
        {
            mCtrlTarget.visible = false;
        }
    }
}
