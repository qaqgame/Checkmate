using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Unity.UI;
using QGF.Module;
using QGF;
using UnityEngine;
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

            UIManager.Instance.OpenPage("ExampleA/UIExamplePage");
            Debuger.Log("page opened");
        
        }
    }
}
