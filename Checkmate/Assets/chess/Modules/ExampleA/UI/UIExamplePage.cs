using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Unity.UI;

namespace Assets.chess.Modules.ExampleA.UI
{
    class UIExamplePage:UIPage
    {

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);

            this.AddUIClickListener("btmShowMsgBox", OnBtnShowMsgBox);
            this.AddUIClickListener("btmShowMsgTips", OnBtnShowMsgTips);
        }

        private void OnBtnShowMsgBox()
        {
            UIManager.Instance.OpenWindow("UIMsgBox");
        }

        private void OnBtnShowMsgTips()
        {
            UIManager.Instance.OpenWindow("UIMsgTips");
        }
    }
}
