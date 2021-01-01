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
        private int _maxHp;
        private int _maxMp;
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
        private int _atkRange;//攻击距离
        private bool _magicAtk;//是否是魔法攻击
        private bool _canMiss;//攻击能否被闪避

        public delegate void AttributeListener(string param, ref object value, object origin);

        public AttributeListener onAttributeChanging=null;//属性改变前事件
        public AttributeListener onAttributeChanged= null;//属性改变后事件

        public void SetHP(int value) { _hp = value>_maxHp?_maxHp:value; }
        public void SetMP(int value) { _mp = value>_maxMp?_maxMp:value; }

        [GetProperty]
        [SetProperty]
        public int MaxHp {
            get
            {
                return _maxHp;
            }
            set
            {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("MaxHp", ref v, _maxHp);
                }
                int temp = _maxHp;
                _maxHp = (int)v < 0 ? 0 : (int)v;
                v = _maxHp;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("MaxHp", ref v, temp);
                }
                //如果当前生命值大于上限
                if (Hp > _maxHp)
                {
                    Hp = _maxHp;
                }
            }
        
        }

        [GetProperty]
        [SetProperty]
        public int MaxMp
        {
            get
            {
                return _maxMp;
            }
            set
            {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("MaxMp", ref v, _maxMp);
                }
                int temp = _maxMp;
                _maxMp = (int)v;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("MaxMp", ref v, temp);
                }
                //如果当前生命值大于上限
                if (Mp > _maxMp)
                {
                    Mp = _maxMp;
                }
            }

        }

        [GetProperty]
        [SetProperty]
        public int Hp
        {
            get { return _hp; }
            set 
            {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("Hp", ref v, _hp);
                }
                int temp = _hp;
                _hp = (int)v<0?0:(int)v;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("Hp", ref v, temp);
                }

            }
        }

        [GetProperty]
        [SetProperty]
        public int Mp
        {
            get { return _mp; }
            set {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("Mp", ref v, _mp);
                }
                int temp = _mp;
                _mp = (int)v;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("Mp", ref v, temp);
                }
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
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("Miss", ref v, _miss);
                }
                float temp = _miss;
                _miss = (float)v > 1.0f ? 1.0f : (float)v;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("Miss", ref v, temp);
                }
            }
        }

        [GetProperty]
        [SetProperty]
        public int PhysicalRes
        {
            get { return _phyRes; }
            set {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("PhysicalRes", ref v, _phyRes);
                }
                int temp = _phyRes;
                _phyRes = (int)v;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("PhysicalRes", ref v, temp);
                }
            }
        }

        [GetProperty]
        [SetProperty]
        public int MagicRes
        {
            get { return _magicRes; }
            set {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("MagicRes", ref v, _magicRes);
                }
                int temp = _magicRes;
                _magicRes =(int)v;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("MagicRes", ref v, temp);
                }
            }
        }

        [GetProperty]
        [SetProperty]
        public int Attack
        {
            get { return _attack; }
            set {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("Attack", ref v, _attack);
                }
                int temp = _attack;
                _attack = (int)v;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("Attack", ref v, temp);
                }
            }
        }

        [GetProperty]
        [SetProperty]
        public int MoveRange
        {
            get { return _moveRange; }
            set {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("MoveRange", ref v, _moveRange);
                }
                int temp = _moveRange;
                _moveRange = (int)v>10?10:(int)v;
                v = _moveRange;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("MoveRange", ref v, temp);
                }
            }
        }

        [GetProperty]
        [SetProperty]
        public float AttackSpeed
        {
            get { return _atkSpeed; }
            set {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("AttackSpeed", ref v, _atkSpeed);
                }
                float temp = _atkSpeed;
                _atkSpeed = (float)v>3.0f?3.0f:(float)v;
                v = _atkSpeed;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("AttackSpeed", ref v, temp);
                }
            }
        }

        [GetProperty]
        [SetProperty]
        public float PhysicalIgnore
        {
            get { return _phyIgn; }
            set {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("PhysicalIgnore", ref v, _phyIgn);
                }
                float temp = _phyIgn;
                _phyIgn = (float)v > 1.0f ? 1.0f : (float)v;
                v = _phyIgn;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("PhysicalIgnore", ref v, temp);
                }
            }
        }

        [GetProperty]
        [SetProperty]
        public float MagicIgnore
        {
            get { return _magicIgn; }
            set {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("MagicIgnore", ref v, _magicIgn);
                }
                float temp = _magicIgn;
                _magicIgn = (float)v > 1.0f ? 1.0f : (float)v;
                v = _magicIgn;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("MagicIgnore", ref v, temp);
                }
            }
        }

        [GetProperty]
        [SetProperty]
        public int ViewRange
        {
            get { return _viewRange; }
            set {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("ViewRange", ref v, _viewRange);
                }
                int temp = _viewRange;
                _viewRange = (int)v > 5 ? 5 : (int)v;
                v = _viewRange;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("ViewRange", ref v, temp);
                }
            }
        }

        [GetProperty]
        [SetProperty]
        public int ViewHeight
        {
            get { return _viewHeight; }
            set {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("ViewHeight", ref v, _viewHeight);
                }
                int temp = _viewHeight;
                _viewHeight = (int)v > 3 ? 3 : (int)v;
                v = _viewHeight;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("ViewHeight", ref v, temp);
                }
            }
        }

        [GetProperty]
        [SetProperty]
        public int AttackRange
        {
            get { return _atkRange; }
            set
            {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("AttackRange", ref v, _atkRange);
                }
                int temp = _atkRange;
                _atkRange = (int)v <0 ? 0 : (int)v;
                v = _atkRange;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("AttackRange", ref v, temp);
                }
            }
        }

        [GetProperty]
        [SetProperty]
        public bool IsMagicAttack
        {
            get { return _magicAtk; }
            set
            {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("IsMagicAttack", ref v, _magicAtk);
                }
                bool temp = _magicAtk;
                _magicAtk = (bool)v;
                v = _magicAtk;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("IsMagicAttack", ref v, temp);
                }
            }
        }

        [GetProperty]
        [SetProperty]
        public bool CanMiss
        {
            get { return _canMiss; }
            set
            {
                object v = value;
                if (onAttributeChanging != null)
                {
                    onAttributeChanging("CanMiss", ref v, _canMiss);
                }
                bool temp = _canMiss;
                _canMiss = (bool)v;
                v = _canMiss;
                if (onAttributeChanged != null)
                {
                    onAttributeChanged("CanMiss", ref v, temp);
                }
            }
        }

        public RoleAttributeController(RoleProperty data):base(data.extraData)
        {
            _maxHp = data.hp;
            _maxMp = data.mp;
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
            _atkRange = data.attackRange;
            _magicAtk = data.magicAttack;
            _canMiss = data.canMiss;
        }

        public RoleAttributeController()
        {
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
            _atkRange = data.attackRange;
            _magicAtk = data.magicAttack;
            _canMiss = data.canMiss;
            SetExtra(data.extraData);
        }

        public void Copy(RoleAttributeController data)
        {
            _maxHp = data._maxHp;
            _maxMp = data._maxMp;
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
            _atkRange = data._atkRange;
            _magicAtk = data._magicAtk;
            _canMiss = data._canMiss;
            mExtraData = new Dictionary<string, object>(data.mExtraData);
        }


        public override int Type { get { return (int)ControllerType.RoleAttribute; } }

    }
}
