using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Unity.UI;
using QGF;
using UnityEngine.UI;
namespace Assets.chess.Modules.ExampleA.UI
{
    public class UIMsgBox:UIWindow
    {
        public Text textContent;
        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            this.AddUIClickListener("btnOk",OnBtnOk);
            textContent.text = arg as string;
            
        }

        private void OnBtnOk()
        {
            Debuger.Log("clicked ok!");
        }
    }
}
