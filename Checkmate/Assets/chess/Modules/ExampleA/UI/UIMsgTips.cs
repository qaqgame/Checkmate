using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Unity.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.chess.Modules.ExampleA.UI
{
    public class UIMsgTips:UIWidget
    {
        public Text textContent;
        public Image imgBackground;
        private float mAlpha = 1;//透明度
        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            textContent.text = arg as string;

            mAlpha = 1;
            UpdateView();
        }


        protected override void OnUpdate()
        {
            base.OnUpdate();

            mAlpha -= 0.01f;
            if (mAlpha <= 0)
            {
                mAlpha = 0;
                this.Close();
            }
            UpdateView();
        }

        private void UpdateView()
        {
            Color c = imgBackground.color;
            c.a = mAlpha;
            imgBackground.color = c;
        }
    }
}
