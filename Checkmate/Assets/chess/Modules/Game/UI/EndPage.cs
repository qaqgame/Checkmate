using Assets.Chess;
using FairyGUI;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Modules.Game.UI
{
    public class EndPage:FGUIPage
    {
        GImage mSuc, mFail;//胜利与失败的图片
        GButton mConfirm;//确认

        protected override void OnLoad()
        {
            base.OnLoad();
            mSuc = mCtrlTarget.GetChild("SucBg").asImage;
            mFail = mCtrlTarget.GetChild("FailBg").asImage;

            mConfirm = mConfirm.GetChild("ConfirmBtn").asButton;
            mConfirm.onClick.Add(OnButtonClicked);
        }

        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);
            bool result = (bool)arg;
            //如果是胜利
            if (result)
            {
                mSuc.visible = true;
                mFail.visible = false;
            }
            else
            {
                mSuc.visible = false;
                mFail.visible = true;
            }
        }


        private void OnButtonClicked()
        {
            GlobalEvent.onGameEnd.Invoke();
        }
    }
}
