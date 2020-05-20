using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Checkmate.Game.Global.UI
{
    public class HUDText : MonoBehaviour
    {
        /// <summary>
        /// 文字预制体
        /// </summary>
        public GameObject hudText;


        private void Start()
        {
            GetComponent<Canvas>().worldCamera = Camera.main;
        }

        /// <summary>
        /// 生成伤害文字
        /// </summary>
        public void HUD(string text)
        {
            GameObject hud = Instantiate(hudText, transform) as GameObject;
            hud.GetComponent<TextMeshProUGUI>().text = text;
        }

    }
}