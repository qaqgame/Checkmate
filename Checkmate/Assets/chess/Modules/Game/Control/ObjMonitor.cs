using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Game.Role;
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
        private ModelController mCurObj;//当前选择对象(优先选择role）
        private ModelController mTempObj=null;//临时点击对象


        public ObjMonitor()
        {
        }

        public ModelController CurrentObj
        {
            get
            {
                return mCurObj;
            }
        }

        //外部调用的点击
        public ModelController OnClick(Vector3 position)
        {
            CellController cell=MapManager.Instance.GetCell(position);
            //如果该位置存在cell
            if (cell != null)
            {
                mTempObj = cell;
                //调用事件
                GameEvent.onCellClicked.Invoke(cell);
            }

            //如果存在角色
            if (cell.HasRole&&cell.Visible)
            {
                Debug.Log("should have role");
                RoleController role = RoleManager.Instance.GetRole(cell.Role);
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
