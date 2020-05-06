using Checkmate.Global.Data;
using QGF.Common;
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

        public delegate void AttributeListener(string param, ref object value, object origin);

        public AttributeListener onAttributeChanged;//属性改变事件 

        [GetProperty]
        [SetProperty]
        public int Hp
        {
            get { return _hp; }
            set 
            {
                object v = value;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("Hp", ref v, _hp);
                }
                _hp = (int)v<0?0:(int)v;
                
            }
        }

        [GetProperty]
        [SetProperty]
        public int Mp
        {
            get { return _mp; }
            set {
                object v = value;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("Mp", ref v, _mp);
                }
                _mp = (int)v; 
            }
        }

        [GetProperty]
        [SetProperty]
        public float Miss
        {
            get { return _miss; }
            set
            {
                object v = value;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("Miss", ref v, _miss);
                }
                _miss = (float)v > 1.0f ? 1.0f : (float)v;
            }
        }

        [GetProperty]
        [SetProperty]
        public int PhysicalRes
        {
            get { return _phyRes; }
            set {
                object v = value;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("PhysicalRes", ref v, _phyRes);
                }
                _phyRes = (int)v; 
            }
        }

        [GetProperty]
        [SetProperty]
        public int MagicRes
        {
            get { return _magicRes; }
            set {
                object v = value;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("MagicRes", ref v, _magicRes);
                }
                _magicRes =(int)v; 
            }
        }

        [GetProperty]
        [SetProperty]
        public int Attack
        {
            get { return _attack; }
            set {
                object v = value;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("Attack", ref v, _attack);
                }
                _attack = (int)v;
            }
        }

        [GetProperty]
        [SetProperty]
        public int MoveRange
        {
            get { return _moveRange; }
            set {
                object v = value;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("MoveRange", ref v, _moveRange);
                }
                _moveRange = (int)v>10?10:(int)v; 
            }
        }

        [GetProperty]
        [SetProperty]
        public float AttackSpeed
        {
            get { return _atkSpeed; }
            set {
                object v = value;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("AttackSpeed", ref v, _atkSpeed);
                }
                _atkSpeed = (float)v>3.0f?3.0f:(float)v; 
            }
        }

        [GetProperty]
        [SetProperty]
        public float PhysicalIgnore
        {
            get { return _phyIgn; }
            set {
                object v = value;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("PhysicalIgnore", ref v, _phyIgn);
                }
                _phyIgn = (float)v > 1.0f ? 1.0f : (float)v;
            }
        }

        [GetProperty]
        [SetProperty]
        public float MagicIgnore
        {
            get { return _magicIgn; }
            set {
                object v = value;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("MagicIgnore", ref v, _magicIgn);
                }
                _magicIgn = (float)v > 1.0f ? 1.0f : (float)v;
            }
        }

        [GetProperty]
        [SetProperty]
        public int ViewRange
        {
            get { return _viewRange; }
            set {
                object v = value;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("ViewRange", ref v, _viewRange);
                }
                _viewRange = (int)v > 5 ? 5 : (int)v; 
            }
        }

        [GetProperty]
        [SetProperty]
        public int ViewHeight
        {
            get { return _viewHeight; }
            set {
                object v = value;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("ViewHeight", ref v, _viewHeight);
                }
                _viewHeight = (int)v > 3 ? 3 : (int)v;
            }
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

        public void Copy(RoleAttributeController data)
        {
            _hp = data._hp;
            _mp = data._mp;
            _miss = data._miss;
            _phyRes = data._phyRes;
            _magicRes = data._magicRes;
            _attack = data._attack;
            _moveRange = data._moveRange;
            _atkSpeed = data._atkSpeed;
            _phyIgn = data._phyIgn;
            _magicIgn = data._magicIgn;
            _viewRange = data._viewRange;
            _viewHeight = data._viewHeight;

            mExtraData = new Dictionary<string, object>(data.mExtraData);
        }


        public override int Type { get { return (int)ControllerType.RoleAttribute; } }

    }
}
