using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Module;
using QGF;
using UnityEngine;
using QGF.Unity.FGUI;
using QGF.Network.General.Client;
using Checkmate.Module.UI;

namespace Checkmate.Module
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

            FGUILoginPage login= FGUIManager.Instance.OpenPage<FGUILoginPage>("LoginPanel","Login","testwindow");
            Debuger.Log("page opened");
        
        }

        protected void Update()
        {
        }
    }
}
