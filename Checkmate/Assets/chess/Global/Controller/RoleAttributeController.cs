using Checkmate.Global.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Game.Controller
{
    public class RoleAttributeController : BaseController
    {
        private int _hp;
        private int _mp;
        private float _miss;//闪避率
        private int _phyRes;//物抗
        private int _magicRes;//魔抗
        private int _attack;//攻击
        private int _moveRange;//移动力
        private float _atkSpeed;//攻速
        private float _phyIgn;//物穿
        private float _magicIgn;//法穿
        private int _viewRange;//视距
        private int _viewHeight;//视高

        [GetProperty]
        [SetProperty]
        public int Hp
        {
            get { return _hp; }
            set { _hp = value; }
        }

        [GetProperty]
        [SetProperty]
        public int Mp
        {
            get { return _mp; }
            set { _mp = value; }
        }

        [GetProperty]
        [SetProperty]
        public float Miss
        {
            get { return _miss; }
            set { _miss = value > 1.0f ? 1.0f : value; }
        }

        [GetProperty]
        [SetProperty]
        public int PhysicalRes
        {
            get { return _phyRes; }
            set { _phyRes = value; }
        }

        [GetProperty]
        [SetProperty]
        public int MagicRes
        {
            get { return _magicRes; }
            set { _magicRes = value; }
        }

        [GetProperty]
        [SetProperty]
        public int Attack
        {
            get { return _attack; }
            set { _attack = value; }
        }

        [GetProperty]
        [SetProperty]
        public int MoveRange
        {
            get { return _moveRange; }
            set { _moveRange = value>10?10:value; }
        }

        [GetProperty]
        [SetProperty]
        public float AttackSpeed
        {
            get { return _atkSpeed; }
            set { _atkSpeed = value>3.0f?3.0f:value; }
        }

        [GetProperty]
        [SetProperty]
        public float PhysicalIgnore
        {
            get { return _phyIgn; }
            set { _phyIgn= value>1.0f?1.0f:value; }
        }

        [GetProperty]
        [SetProperty]
        public float MagicIgnore
        {
            get { return _magicIgn; }
            set { _magicIgn = value > 1.0f ? 1.0f : value; }
        }

        [GetProperty]
        [SetProperty]
        public int ViewRange
        {
            get { return _viewRange; }
            set { _viewRange = value > 5 ? 5 : value; }
        }

        [GetProperty]
        [SetProperty]
        public int ViewHeight
        {
            get { return _viewHeight; }
            set { _viewHeight = value > 3 ? 13 : value; }
        }

        public RoleAttributeController(RoleProperty data):base(data.extraData)
        {
            _hp = data.hp;
            _mp = data.mp;
            _miss = data.miss;
            _phyRes = data.physicalRes;
            _magicRes = data.magicRes;
            _attack = data.attack;
            _moveRange = data.moveRange;
            _atkSpeed = data.attackSpeed;
            _phyIgn = data.physicalIgnore;
            _magicIgn = data.magicIgnore;
            _viewRange = data.viewRange;
            _viewHeight = data.viewHeight;
        }

        public void Set(RoleProperty data)
        {
            _hp = data.hp;
            _mp = data.mp;
            _miss = data.miss;
            _phyRes = data.physicalRes;
            _magicRes = data.magicRes;
            _attack = data.attack;
            _moveRange = data.moveRange;
            _atkSpeed = data.attackSpeed;
            _phyIgn = data.physicalIgnore;
            _magicIgn = data.magicIgnore;
            _viewRange = data.viewRange;
            _viewHeight = data.viewHeight;

            SetExtra(data.extraData);
        }


        public override int Type { get { return 2; } }

        public override GameObject GetGameObject()
        {
            throw new NotImplementedException();
        }

        public override Position GetPosition()
        {
            throw new NotImplementedException();
        }
    }
}
