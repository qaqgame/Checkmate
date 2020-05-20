using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Checkmate.Game.Global.UI
{
    /// <summary>
    /// 控制伤害显示
    /// </summary>
    public class FloatText : MonoBehaviour
    {
        /// <summary>
        /// 滚动速度
        /// </summary>
        private float speed = 1.5f;

        /// <summary>
        /// 计时器
        /// </summary>
        private float timer = 0f;

        /// <summary>
        /// 销毁时间
        /// </summary>
        private float time = 2.0f;

        private float mUpdateSize;
        private void Start()
        {
            mUpdateSize = GetComponent<TextMeshProUGUI>().fontSize / time;
        }

        private void Update()
        {
            Scroll();
        }

        /// <summary>
        /// 冒泡效果
        /// </summary>
        private void Scroll()
        {
            //字体滚动
            this.transform.Translate(Vector3.up * speed * Time.deltaTime);
            timer += Time.deltaTime;
            //字体缩小
            this.GetComponent<TextMeshProUGUI>().fontSize-=mUpdateSize;
            //字体渐变透明
            this.GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0, 1 - timer/time);
            Destroy(gameObject, time);
        }

    }
}
