using FairyGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QGF.Unity.FGUI
{
    public class FGUILoading:FGUIPanel
    {
        protected GProgressBar mProgressBar;//进度条

        public double MaxValue { get { return mProgressBar.max; } }
        public double MinValue { get { return mProgressBar.min; } }
        public double Value { get { return mProgressBar.value; } }

        /// <summary>
        /// 用duration的时间来改变进度条的值至value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="transitionTime"></param>
        public void SetValue(double value,float duration = .0f)
        {
            value = value < mProgressBar.min ? mProgressBar.min : value > mProgressBar.max ? mProgressBar.max : value;

            double last = (mProgressBar.value-mProgressBar.min) / (mProgressBar.max-mProgressBar.min);
            double current = (value - mProgressBar.min) / (mProgressBar.max - mProgressBar.min);


            if (duration <= .0f)
            {
                mProgressBar.value = value;
            }
            else
            {
                mProgressBar.TweenValue(value, duration);
            }
            OnProgressUpdate(last, current);
        }

        //加载时存储进度条
        protected override void OnLoad()
        {
            base.OnLoad();
            if (mCtrlTarget.GetChild("progressBar") != null)
            {
                mProgressBar = mCtrlTarget.GetChild("progressBar").asProgress;
                
            }
        }


        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);
            mCtrlTarget.SetSize(mRoot.width, mRoot.height);
            mCtrlTarget.AddRelation(mRoot, RelationType.Size);
        }

        /// <summary>
        /// 进度条进度更新时被调用
        /// </summary>
        /// <param name="last">上一次的值（取百分比进度）</param>
        /// <param name="current">当前值（百分比)</param>
        protected virtual void OnProgressUpdate(double last,double current)
        {

        }
    }
}
