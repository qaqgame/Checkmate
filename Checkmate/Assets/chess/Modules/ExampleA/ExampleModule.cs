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

namespace Chess.Module
{
    public class ExampleAModule:GeneralModule
    {
        NetManager mNet;
        
        public override void Create(object args = null)
        {
            base.Create(args);
            Debuger.Log("created!");
            Debug.Log("example created");
            mNet = new NetManager();
            mNet.Init(typeof(KCPConnect), 1, 8080);
            mNet.Connect("120.79.240.163", 8080);
            mNet.SetUserId(0);
        }


        protected override void Show(object arg)
        {
            base.Show(arg);

            FGUILoginPage login= FGUIManager.Instance.OpenPage<FGUILoginPage>("LoginPanel","Login","testwindow");
            login.Init(mNet);
            Debuger.Log("page opened");
        
        }

        protected void Update()
        {
            mNet.Update();
        }
    }
}
