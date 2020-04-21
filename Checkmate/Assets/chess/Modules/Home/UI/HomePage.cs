﻿using FairyGUI;
using QGF.Module;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Module.UI
{
    public class HomePage:FGUIPage
    {
        GButton mStartBtn;

        protected override void OnLoad()
        {
            base.OnLoad();
            mStartBtn = mCtrlTarget.GetChild("StartBtn").asButton;
            mStartBtn.onClick.Add(OnStartBtnClicked);
        }

        //开始按钮点击事件
        private void OnStartBtnClicked()
        {
            //跳转到房间页面
            ModuleManager.Instance.ShowModule("RoomModule");
        }
    }
}