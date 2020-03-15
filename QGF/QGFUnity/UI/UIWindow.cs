using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace QGF.Unity.UI
{
    public abstract class UIWindow:UIPanel
    {
        public override UITypeDef UIType { get { return UITypeDef.Window; } }

        [SerializeField]
        private Button mBtnClose;//关闭按钮

        //在激活时
        protected void OnEnable()
        {
            Debuger.Log();
            //给关闭按钮添加事件
            this.AddUIClickListener(mBtnClose, OnBtnClose);
        }


        private void OnDisable()
        {
            Debuger.Log();
            RemoveUIClickListeners(mBtnClose);
        }


        private void OnBtnClose()
        {
            Close(0);
        }
    }
}
