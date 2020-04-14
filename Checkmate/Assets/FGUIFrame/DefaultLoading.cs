using FairyGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Unity.FGUI
{
    public class DefaultLoading:FGUILoading
    {
        GTextField mTitle;
        protected override void OnLoad()
        {
            base.OnLoad();
            mTitle = mProgressBar.GetChild("name").asTextField;
        }

        public void SetName(string name)
        {
            mTitle.text = name;
        }
    }
}
