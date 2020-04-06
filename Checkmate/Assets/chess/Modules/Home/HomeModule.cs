using Checkmate.Module.UI;
using QGF.Module;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Module
{
    public class HomeModule:GeneralModule
    {
        protected override void Show(object arg)
        {
            base.Show(arg);
            FGUIManager.Instance.OpenPage<HomePage>("Root", "Home");
        }
    }
}
