using Checkmate.Game.Controller;
using QGF.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Modules.Game
{
    public static class GameEvent
    {
        //对象点击事件
        public static QGFEvent<BaseController> onControllerClicked;

        //地板点击事件
        public static QGFEvent<CellController> onCellClicked;

        //角色点击事件
        public static QGFEvent<RoleController> onRoleClicked;

        //角色属性改变事件
        public static QGFEvent<RoleController> onRoleChanged;
    }
}
