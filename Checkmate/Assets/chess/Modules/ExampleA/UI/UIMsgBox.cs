using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Unity.UI;
using QGF;
namespace Assets.chess.Modules.ExampleA.UI
{
    class UIMsgBox:UIWindow
    {
        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            this.AddUIClickListener("btnOK",OnBtnOk);
        }

        private void OnBtnOk()
        {
            Debuger.Log("clicked ok!");
        }
    }
}
