using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using FairyGUI;

namespace QGF.Unity.FGUI
{
    public class FGUIWindow : FGUIPanel
    {
        protected class ExWindow : Window
        {
            private Action onShow, onHide;
            public ExWindow(Action onShow, Action onHide) : base()
            {
                this.onShow = onShow;
                this.onHide = onHide;
            }

            protected override void DoShowAnimation()
            {
                onShow();
            }

            protected override void DoHideAnimation()
            {
                onHide();
            }
        }


        protected ExWindow mWindow;//窗口
        protected GButton mCloseBtn;//关闭按钮

        protected virtual bool IsModal{get{return false;} }

        protected override void OnLoad()
        {
            base.OnLoad();
            mWindow = new ExWindow(OnShow,OnHide);
            mWindow.contentPane = mCtrlTarget;
            mWindow.modal = IsModal;
            mCloseBtn = mCtrlTarget.GetChild("btnClose").asButton;
        }

        public override void Open(GComponent root, object arg = null)
        {
            IsOpened = true;
            mWindow.Show();
            OnOpen(arg);
        }

        public override void Close(object arg)
        {
            IsOpened = false;
            OnClose(arg);
            mWindow.Hide();
        }

        protected override void OnPanelDestroy()
        {
            base.OnPanelDestroy();
            mWindow.contentPane = null;
            mWindow.Dispose();
        }

        //=================
        //重写动画控制部分
        //=================
        protected virtual void OnShow()
        {

        }
        protected virtual void OnHide()
        {

        }
    }
}
