using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Module;
using QGF;
using UnityEngine;
using QGF.Unity.FGUI;
namespace Chess.Module
{
    public class ExampleAModule:GeneralModule
    {
        public override void Create(object args = null)
        {
            base.Create(args);
            Debuger.Log("created!");
            Debug.Log("example created");
        }

        protected override void Show(object arg)
        {
            base.Show(arg);

            FGUIManager.Instance.OpenPage<FGUIPage>("LoginPanel","Login","testwindow");
            Debuger.Log("page opened");
        
        }
    }
}
