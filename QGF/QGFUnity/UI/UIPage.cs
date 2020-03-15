using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


namespace QGF.Unity.UI
{
    public abstract class UIPage:UIPanel
    {

        public override UITypeDef UIType { get { return UITypeDef.Page; } }

        [SerializeField]
        private Button mBtnBack;//返回按钮

        //在激活时
        protected void OnEnable()
        {
            Debuger.Log();
            //给返回按钮添加事件
            this.AddUIClickListener(mBtnBack, OnPageGoBack);
        }


        private void OnDisable()
        {
            Debuger.Log();
            RemoveUIClickListeners(mBtnBack);
        }

        //点击返回时调用
        private void OnPageGoBack()
        {
            Debuger.Log();
            UIManager.Instance.GoBackPage();
        }
    }
}
