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
            private Action onWindowShow, onWindowHide;
            public ExWindow(Action onShow, Action onHide) : base()
            {
                this.onWindowShow = onShow;
                this.onWindowHide = onHide;
            }

            protected override void DoShowAnimation()
            {
                onWindowShow();
                OnShown();
                
            }

            protected override void DoHideAnimation()
            {
                onWindowHide();
                HideImmediately();
            }
        }


        protected ExWindow mWindow;//窗口
        protected GButton mCloseBtn;//关闭按钮
        private GObject mDragArea;//拖动区域
        protected virtual bool IsModal{get{return false;} }

        protected override void OnLoad()
        {
            base.OnLoad();
            mWindow = new ExWindow(OnShow,OnHide);
            
            mWindow.contentPane = mCtrlTarget;
            mWindow.modal = IsModal;
            mCloseBtn = mCtrlTarget.GetChild("BtnClose")!=null?mCtrlTarget.GetChild("BtnClose").asButton:null;
            if (mCloseBtn != null)
            {
                mWindow.closeButton = mCloseBtn;
            }
            mDragArea = mCtrlTarget.GetChild("DragArea");
            if (mDragArea != null)
            {
                mWindow.dragArea = mDragArea;
            }
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
