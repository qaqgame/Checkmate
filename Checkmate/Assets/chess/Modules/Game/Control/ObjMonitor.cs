using Checkmate.Game.Controller;
using Checkmate.Modules.Game.Map;
using Checkmate.Modules.Game.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Checkmate.Modules.Game.Control
{
    public class ObjMonitor
    {
        private BaseController mCurObj;//当前选择对象(优先选择role）
        private BaseController mTempObj=null;//临时点击对象


        private MapManager mMap;//地图管理
        private RoleManager mRoles;//角色管理
        public ObjMonitor(MapManager map)
        {
            mMap = map;
        }

        //外部调用的点击
        public BaseController OnClick(Vector3 position)
        {
            CellController cell=mMap.GetCell(position);
            //如果该位置存在cell
            if (cell != null)
            {
                mTempObj = cell;
                //调用事件
                GameEvent.onCellClicked.Invoke(cell);
            }

            //如果存在角色
            if (cell.HasRole)
            {
                RoleController role = mRoles.GetRole(cell.Role);
                if (role != null)
                {
                    mTempObj = role;
                    GameEvent.onRoleClicked.Invoke(role);
                }
            }

            //调用总事件
            if (mTempObj != null)
            {
                GameEvent.onControllerClicked.Invoke(mTempObj);
                mCurObj = mTempObj;
                mTempObj = null;
                return mCurObj;
            }
            return null;
        }

    }
}
