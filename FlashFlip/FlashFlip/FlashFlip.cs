//using Checkmate.Game;
//using Checkmate.Game.Controller;
//using Checkmate.Game.Map;
using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.FlashFlip
{
    public class FlashFlip
    {
        public void Init() { }

        //瞬间移动
        #region 瞬间移动
        Vector3 startPos;
        Vector3 targetPos;
        GameObject moveObj;
        /// <summary>
        /// 瞬间移动
        /// </summary>
        /// <param name="role">角色</param>
        /// <param name="center">目标位置</param>
        public void FlashFlipTo(RoleController role, Position center)
        {
            if(MapManager.Instance.GetCell(center).Role != -1)
            {
                return;
            }
            Position startPosition = role.Position;
            //原位置设为-1
            MapManager.Instance.GetCell(startPosition).Role = -1;
            MapManager.Instance.GetCell(role.Position).RemoveVisibility(role);
            moveObj = role.GetGameObject();
            Vector3 target = MapManager.Instance.GetCellWorldPosition(center);
            role.FaceTo(target);
            targetPos = target;
            // 新位置设为roleid
            role.Position = center;
            MapManager.Instance.GetCell(center).SetVisibility(role);
            MapManager.Instance.GetCell(center).Role = role.RoleId;
            // 修改obj位置
            moveObj.transform.position = targetPos;
            
        }
        #endregion
    }
}
