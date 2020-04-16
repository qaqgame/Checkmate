using Checkmate.Global.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Game.Controller
{
    public class RoleController : BaseController
    {
        private GameObject mObj;

        //该角色当前是否可操作
        public bool CanOperate
        {
            get;
        }

        //角色id
        [GetProperty]
        public int Id
        {
            get;
            private set;
        }


        [GetProperty]
        public RoleAttributeController Current
        {
            get;
        }

        [GetProperty]
        public RoleAttributeController Origin
        {
            get;
        }

        //显示姓名
        [GetProperty]
        public string Name
        {
            get;
            private set;
        }

        //当前位置
        [GetProperty]
        [SetProperty]
        public Position Position
        {
            get;
            set;
        }

        //所属队伍
        [GetProperty]
        [SetProperty]
        public int Team
        {
            get;
            set;
        }

        //状态
        [GetProperty]
        [SetProperty]
        public int Status
        {
            get;
            set;
        }

        [GetProperty]
        public string Model
        {
            get;
            set;
        }

        private Dictionary<int, int> mExtraMove;//不同地形对应的额外行动力
        private int mStandMask;//可站立的掩码

        public RoleController(RoleData data,GameObject obj):base(data.extraData)
        {
            Id = data.id;
            Current = new RoleAttributeController(data.props);
            Origin = new RoleAttributeController(data.props);
            Position = new Position(data.position.x, data.position.y, data.position.z);
            Team = data.team;
            Status = data.status;
            mStandMask = data.mask;
            mExtraMove = data.extraMove;
            Name = data.name;
            Model = data.model;

            mObj = obj;
        }

        public void SetCurProperty(RoleProperty property)
        {
            Current.Set(property);
        }

        public override int Type { get { return 3; } }

        public override GameObject GetGameObject()
        {
            return mObj;
        }

        public override Position GetPosition()
        {
            return Position;
        }
    }
}
