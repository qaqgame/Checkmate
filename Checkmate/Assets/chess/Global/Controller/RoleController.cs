using Checkmate.Game.UI;
using Checkmate.Global.Data;
using FairyGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Game.Controller
{
    public enum RoleState
    {
        Idle,
        PreMove,//预移动
        Move,//移动中
        EndMove,//结束移动
        PreSpell,//预施法
        Spell,//施法中
        EndSpell//结束施法
    }
    public class RoleController : ModelController
    {
        private GameObject mObj;//根物体
        private RolePanel mPanel;//头顶面板

        private RoleState mCurState = RoleState.Idle;//当前状态

        private Action<RoleController> onRoleChanged;//角色属性发生改变的事件

        public void SetRoleChangeListener(Action<RoleController> listener)
        {
            onRoleChanged = listener;
        }

        public RoleState CurrentState
        {
            get { return mCurState; }
        }

        public void SetState(RoleState state)
        {
            mCurState = state;
        }

        //该角色当前是否可操作
        public bool CanOperate
        {
            get
            {
                return mCurState == RoleState.Idle||mCurState==RoleState.PreMove;
            }
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
        private List<Int32> mStandMask;//可站立的掩码，1位代表一类地形或特征

        public RoleController(RoleData data,GameObject obj):base(data.extraData)
        {
            Id = data.id;
            Current = new RoleAttributeController(data.props);
            Current.onAttributeChanged = OnAttributeChanged;
            Origin = new RoleAttributeController(data.props);
            Position = new Position(data.position.x, data.position.y, data.position.z);
            Team = data.team;
            Status = data.status;
            mStandMask = data.mask;
            mExtraMove = data.extraMove;
            Name = data.name;
            Model = data.model;

            mObj = obj;

            //初始化面板
            mPanel = new RolePanel(obj.GetComponentInChildren<UIPanel>(), Origin.Hp, Current.Hp,Name);
        }

        //=============================
        //外部接口
        //设置当前属性
        public void SetCurProperty(RoleProperty property)
        {
            Current.Set(property);
        }

        //获取额外的移动力
        public int GetExtraMove(int terrain)
        {
            if (mExtraMove.ContainsKey(terrain))
            {
                return mExtraMove[terrain];
            }
            return 0;
        }

        //判断能否站立
        public bool CanStand(int terrain)
        {
            
            int idx = terrain / 32;
            int offset = terrain - idx * 32;
            return (mStandMask[idx] & (1 << offset)) != 0;
        }


        //==========================
        //继承的接口
        public override int Type { get { return (int)ControllerType.Role; } }

        public override GameObject GetGameObject()
        {
            return mObj;
        }

        public override Position GetPosition()
        {
            return Position;
        }



        //========================
        //内置处理
        private void OnAttributeChanged(string param,object value)
        {
            if (param == "Hp")
            {
                //通知生命值改变
                mPanel.SetHP((int)value);
            }
            //通知外部
            onRoleChanged(this);


        }
    }
}
