using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;


namespace QGF.Unity.UI
{
    public class UILoadingArg
    {
        public string title = "";
        public string tips = "";
        public float progress = 0;//进度0-1

        public override string ToString()
        {
            return string.Format("title:{0}, tips:{1}, progress:{2}", title, tips, progress);
        }
    }
    public class UILoading:UIPanel
    {
        public override UITypeDef UIType { get { return UITypeDef.Loading; } }

        public Text txtTitle;
        public Text txtTips;
        //参数
        private UILoadingArg mArg;
        public UILoadingArg arg { get { return mArg; } }

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);

            mArg = arg as UILoadingArg;
            if (mArg == null)
            {
                mArg = new UILoadingArg();
            }
            UpdateText();
        }

        //显示进度
        public void ShowProgress(string title,float progress)
        {
            mArg.title = title;
            mArg.progress = progress;
        }

        public void ShowProgress(float progress)
        {
            mArg.progress = progress;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (mArg != null)
            {
                //更新text以及进度条
                UpdateText();
                UpdateProgress();
            }
        }

        protected virtual void UpdateProgress()
        {

        }


        private void UpdateText()
        {
            if (txtTitle != null)
            {
                txtTitle.text = mArg.title + "(" + (int)(mArg.progress * 100) + "%)";
            }
            if (txtTips != null)
            {
                txtTips.text = mArg.tips;
            }
        }
    }
}
