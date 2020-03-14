using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Unity.UI;
using QGF;
namespace Assets.chess.Modules.ExampleA.UI
{
    public class UIExamplePage:UIPage
    {

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);

            this.AddUIClickListener("btnShowMsgBox", OnBtnShowMsgBox);
            this.AddUIClickListener("btnShowMsgTips", OnBtnShowMsgTips);
        }

        private void OnBtnShowMsgBox()
        {
            Debuger.Log("btn msg clicked");
            UIManager.Instance.OpenWindow("ExampleA/UIMsgBox","这是一个测试window");
        }

        private void OnBtnShowMsgTips()
        {
            
            UIManager.Instance.OpenWidget("ExampleA/UIMsgTips");
        }
    }
}
