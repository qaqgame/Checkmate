using Checkmate.Game.Global.Utils;
using FairyGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Modules.Game.UI.Component
{
    public class TipsPanel
    {
        private GComponent mRoot;
        private GLoader mIcon;
        private GTextField mDescription;

        public TipsPanel(GComponent root)
        {
            mRoot = root;
            mIcon = root.GetChildByPath("Icon.Icon").asLoader;
            mDescription = root.GetChild("Text").asTextField;
        }

        public void SetIcon(string icon)
        {
            if (icon != null)
            {
                mIcon.texture = new NTexture(IconManager.Instance.GetIcon(icon));
            }
        }

        public void SetText(string text)
        {
            mDescription.text = text;
        }

        public void Show()
        {
            mRoot.visible = true;
        }
        public void Hide()
        {
            mRoot.visible = false;
        }
    }
}
