using FairyGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Unity.FGUI
{
    public class DialogParam
    {
        public string content;
        public Action onCancel = null;
        public Action onConfirm = null;
    }
    public class ExDialog:FGUIWindow
    {
        public Action  onCancel;

        protected GButton  mBtnCancel;
        protected GTextField mContent;

        protected DialogParam mCurParam;
        protected override bool IsModal { get { return true; } }
        protected override void OnLoad()
        {
            base.OnLoad();
            mBtnCancel = mCtrlTarget.GetChild("BtnCancel").asButton;
            mContent = mCtrlTarget.GetChild("Content").asTextField;
            mBtnCancel.onClick.Set(OnCancel);
            //添加位置约束：居中
            mWindow.AddRelation(GRoot.inst, RelationType.Center_Center);
        }

        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);
            mCurParam= arg as DialogParam;
            mContent.text = mCurParam.content;
            onCancel = mCurParam.onCancel;
        }

        private void OnCancel()
        {
            if (onCancel != null)
            {
                onCancel();
            }
        }
    }

    public class Dialog : ExDialog
    {
        public Action onConfirm;
        public GButton mBtnConfirm;
        protected override void OnLoad()
        {
            base.OnLoad();
            mBtnConfirm = mCtrlTarget.GetChild("BtnConfirm").asButton;
            mBtnConfirm.onClick.Set(OnConfirm);
        }

        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);
            onConfirm = mCurParam.onConfirm;
        }

        private void OnConfirm()
        {
            if (onConfirm != null)
            {
                onConfirm();
            }
        }
    }
}
