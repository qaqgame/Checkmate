﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using QGF.Unity.Common;

namespace QGF.Unity.UI
{
    /// <summary>
    /// 为UI操作提供基础封装，使UI操作更方便
    /// </summary>
    public static class UIUtils
    {
        /// <summary>
        /// 设置一个UI元素是否可见
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="value"></param>
        public static void SetActive(UIBehaviour ui, bool value)
        {
            if (ui != null && ui.gameObject != null)
            {
                GameObjectUtils.SetActiveRecursively(ui.gameObject, value);
            }
        }


        public static void SetButtonText(Button btn, string text)
        {
            Text objText = btn.transform.GetComponentInChildren<Text>();
            if (objText != null)
            {
                objText.text = text;
            }
        }

        public static string GetButtonText(Button btn)
        {
            Text objText = btn.transform.GetComponentInChildren<Text>();
            if (objText != null)
            {
                return objText.text;
            }
            return "";
        }
        //给UI控件设置文本
        public static void SetChildText(UIBehaviour ui, string text)
        {
            Text objText = ui.transform.GetComponentInChildren<Text>();
            if (objText != null)
            {
                objText.text = text;
            }
        }


    }
}
