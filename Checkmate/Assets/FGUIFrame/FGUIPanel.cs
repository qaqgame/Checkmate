using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using FairyGUI;

namespace QGF.Unity.FGUI
{
    public abstract class FGUIPanel
    {
        protected GComponent mRoot;
        protected GComponent mCtrlTarget;//操纵对象

        private bool mLoaded = false;
        public bool IsLoaded { get { return mLoaded; } }
        public bool IsOpened { get; protected set; }
        //加载至内存，被manager调用
        public void Load(string package,string name)
        {
            mLoaded = false;
            FGUIPackageManager.Instance.CreateUIAsync(package, name,onCreateFinished);
        }


        //销毁
        public void Destroy()
        {
            OnPanelDestroy();
            mCtrlTarget.Dispose();
        }

        //打开
        public virtual void Open(GComponent root,object arg = null)
        {
            if (root != null)
            {
                mRoot = root;
                root.AddChild(mCtrlTarget);
                IsOpened = true;
                OnOpen(arg);
            }
        }
        //关闭
        public virtual void Close(object arg = null)
        {
            if (mRoot != null)
            {
                OnClose(arg);
                mRoot.RemoveChild(mCtrlTarget);
                IsOpened = false;
                mRoot = null;
            }
        }
        

        //被加载进内存时调用
        protected virtual void OnLoad() { }
        protected virtual void OnOpen(object arg) {
            Debuger.Log("open panel with arg:{0}", arg);
        }
        protected virtual void OnClose(object arg) { }
        //在被销毁时调用
        protected virtual void OnPanelDestroy() { }
        protected virtual void OnResize(int ow, int oh, int nw, int nh) { }
    
        
        private void onCreateFinished(GObject result)
        {
            mLoaded = true;
            mCtrlTarget = result.asCom;
            if (mCtrlTarget == null)
            {
                Debuger.LogError("无法得到FGUI panel组件");
                return;
            }
            OnLoad();
        }

    }
}
