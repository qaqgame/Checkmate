using FairyGUI;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Unity.FGUI
{
    public class FGUIPage : FGUIPanel
    {
        GButton mBtnBack;//返回按钮

        protected override void OnLoad()
        {
            base.OnLoad();

            if (mCtrlTarget.GetChild("Back") != null)
            {
                mBtnBack = mCtrlTarget.GetChild("Back").asButton;
                mBtnBack.onClick.Add(OnBackClicked);
            }
            
        }
        //保持page为全屏
        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);
            mCtrlTarget.SetSize(mRoot.width, mRoot.height);
            mCtrlTarget.SetPosition(0, 0, 0);
            mCtrlTarget.AddRelation(mRoot, RelationType.Size);
        }

        private void OnBackClicked()
        {
            FGUIManager.Instance.GoBackPage();
            OnPageGoBack();
        }
        protected virtual void OnPageGoBack()
        {

        }
    }
}
