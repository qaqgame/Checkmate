using Checkmate.Game.Controller;
using FairyGUI;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Modules.Game.UI.Component
{
    public class RolePropertyList:VirtualList
    {
        private RoleController mTargetRole;

        private List<string> mKeys;//所有待显示项

        private bool _visible;
        public bool Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                mList.visible = value;
            }
        }

        public RolePropertyList(GList list) : base(list, 0)
        {
            onRenderItem = RenderItem;
        }

        void RenderItem(int itemIdx, int childIdx, GObject obj)
        {
            //获取组件
            GComponent com = obj.asCom;
            GTextField key = com.GetChild("PropertyName").asTextField;
            GTextField value = com.GetChild("PropertyValue").asTextField;

            key.text = mKeys[itemIdx];
            string v= mTargetRole.Temp.GetValue(mKeys[itemIdx]).ToString();
            value.text = v;
        }

        public void Update(RoleController role,List<string> data)
        {
            mTargetRole = role;
            mKeys = data;
            Refresh(mKeys.Count);
        }

    }
}
