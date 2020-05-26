using Checkmate.Game.Buff;
using Checkmate.Game.Global.UI;
using Checkmate.Game.Map;
using Checkmate.Game.Role;
using Checkmate.Game.Skill;
using Checkmate.Game.UI;
using Checkmate.Game.Utils;
using Checkmate.Global.Data;
using FairyGUI;
using QGF;
using QGF.Math;
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
        EndSpell,//结束施法
        PreAttack,//预攻击
        Attack,
    }
    public class RoleController : ModelController
    {
        private GameObject mObj;//根物体
        private RolePanel mPanel;//头顶面板

        private RoleState mCurState = RoleState.Idle;//当前状态

        private Action<RoleController> onRoleChanged;//角色属性发生改变的事件
        private Action<RoleController,List<int>> onBuffChanged;//buff栏发生改变的事件
        private Action<List<int>> onSkillChanged;//技能发生更新
        public void SetRoleChangeListener(Action<RoleController> listener)
        {
            onRoleChanged = listener;
        }
        public void SetBuffChangeListener(Action<RoleController,List<int>> listener)
        {
            onBuffChanged = listener;
        }
        public void SetSkillChangeListener(Action<List<int>> listener)
        {
            onSkillChanged = listener;
        }

        public void ClearAllListener()
        {
            onRoleChanged = null;
            onBuffChanged = null;
            onSkillChanged = null;
        }
        //当前状态
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
                return mCurState == RoleState.Idle||mCurState==RoleState.PreMove||mCurState==RoleState.PreSpell||mCurState==RoleState.PreAttack;
            }
        }

        //数据改变的列表（current为持续，temp为临时）
        public DataMap CurrentMap, TempMap;

        //技能
        [GetProperty]
        public List<int> Skills
        {
            get;
            private set;
        }

        //普通攻击的效果
        [GetProperty]
        public int AttackAction
        {
            get;
            private set;
        }

        //角色id
        [GetProperty]
        public int RoleId
        {
            get;
            private set;
        }

        //该部分为战斗临时属性
        [GetProperty]
        public RoleAttributeController Temp
        {
            get;
        }

        //该部分为直接使用属性（buff修改后）
        [GetProperty]
        public RoleAttributeController Current
        {
            get;
        }
        //该部分为原始属性（基础值）
        [GetProperty]
        public RoleAttributeController Origin
        {
            get;
        }

        //攻击资源文件
        [GetProperty]
        public string AtkSource
        {
            get;
            private set;
        }

        //攻击特效文件
        [GetProperty]
        public string AtkEffect
        {
            get;
            private set;
        }


        //显示姓名
        [GetProperty]
        public string Name
        {
            get;
            private set;
        }

        //当前位置
        private Position _position;
        [GetProperty]
        [SetProperty]
        public Position Position
        {
            get { return _position; }
            set
            {
                _position = value;
                CellController cell = MapManager.Instance.GetCell(value);
                if (cell != null)
                {
                    //可见则设置可见
                    if (cell.Visible)
                    {

                        Visible = true;
                    }
                    else
                    {
                        Visible = false;
                    }
                }
            }
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

        private bool _visible=true;
        [GetProperty]
        public bool Visible
        {
            get { return mObj.activeSelf; }
            set
            {
                if (value != _visible)
                {
                    mObj.SetActive(value);
                    _visible = value;
                }
            }
        }


        private Dictionary<int, int> mExtraMove;//不同地形对应的额外行动力
        private List<Int32> mStandMask;//可站立的掩码，1位代表一类地形或特征
        public SkillAction AtkAction;//攻击的具体活动
        public RoleController(RoleData data,GameObject obj):base(data.extraData)
        {
            RoleId = data.id;
            Current = new RoleAttributeController(data.props);
            Current.onAttributeChanged = OnCurrentAttributeChanged;
            Origin = new RoleAttributeController(data.props);
            Temp = new RoleAttributeController(data.props);
            Temp.onAttributeChanged = OnTempAttributeChanged;
            //Position = new Position(data.position.x, data.position.y, data.position.z);
            Team = data.team;
            Status = data.status;
            mStandMask = data.mask;
            mExtraMove = data.extraMove;
            Name = data.name;
            Model = data.model;

            mObj = obj;

            CurrentMap = new DataMap();
            CurrentMap.onChanged = OnCurrentAttrChanged;
            TempMap = new DataMap();
            TempMap.onChanged = OnTempAttrChanged;

            //初始化面板
            mPanel = new RolePanel(obj.GetComponentInChildren<UIPanel>(), Origin.Hp, Current.Hp,Name);

            Skills = new List<int>();
            //添加技能
            foreach(var skill in data.skills)
            {
                int sid = SkillManager.Instance.GetSkill(skill);
                Skills.Add(sid);
            }

            AtkSource = data.attackSource;
            AtkEffect = data.attackEffect;

            string content;
            if (data.nearAttack)
            {
                content = Resources.Load<TextAsset>("Attack_near").text;
            }
            else
            {
                content = Resources.Load<TextAsset>("Attack_far").text;
            }
            AtkAction = new SkillAction(content);

            mHud = mObj.GetComponentInChildren<HUDText>();
        }


        //===================================
        //buff部分
        public List<int> Buffs=new List<int>();//所有的buff

        //添加buff
        public int AddBuff(string file)
        {
            int bid = BuffManager.Instance.InstanceBuff(file);
            Checkmate.Game.Buff.Buff buff = BuffManager.Instance.GetBuff(bid);
            //调用onAddBuff
            OnAddBuff(ref buff);
            Buffs.Add(bid);

            Debuger.Log("buff {0} added to {1}", buff.Name, Name);
            UpdatePanel();
            return bid;
        }

        //移除buff
        public void RemoveBuff(int id)
        {
            //移除前执行buff的OnRemove
            Checkmate.Game.Buff.Buff buff = BuffManager.Instance.RemoveBuff(id);
            EnvVariable env = new EnvVariable();
            env.Src = buff.Src;
            env.Obj = buff.Obj;
            env.Main = buff;
            env.Center = Position;
            env.Dst = null;
            GameEnv.Instance.PushEnv(env);
            buff.Execute(TriggerType.OnRemoved);
            GameEnv.Instance.PopEnv();
            Buffs.Remove(id);

            Debuger.Log("remove buff {0} of {1}, with changed roles:{2}", buff.Name, Name,buff.Current.mUsedRoles.Count.ToString());
            //移除buff的所有temp属性加成
            foreach (var role in buff.Current.mUsedRoles)
            {
                role.CurrentMap.RemoveTrack(buff.Current);
            }
            buff.Current.Clear();

            UpdatePanel();
        }

        public void RemoveBuff(string name)
        {
            int i = -1;
            foreach(var id in Buffs)
            {
                Checkmate.Game.Buff.Buff buff = BuffManager.Instance.GetBuff(id);
                if (buff.Name == name)
                {
                    i = id;
                    break;

                }
            }
            if (i != -1)
            {
                RemoveBuff(i);
            }
        }

        //受到伤害
        private void OnDamaged(int dmg)
        {
            EnvVariable env = new EnvVariable();
            env.Copy(GameEnv.Instance.CurrentExe);
            env.Dst = this;
            GameEnv.Damage = dmg;
            GameEnv.Instance.PushEnv(env);
            //执行target的ondamaged
            BuffManager.Instance.ExecuteWithEnv(TriggerType.OnBeDamaged,this,env.Main);
            //执行src的ondamage
            if (env.Src.Type == (int)ControllerType.Role)
            {
                RoleController src = env.Src as RoleController;
                BuffManager.Instance.ExecuteWithEnv(TriggerType.OnDamage, src,env.Main);
            }
            //添加伤害函数
            GameExecuteManager.Instance.Add(_Damage);

            GameEnv.Instance.PopEnv();
        }

        private void _Damage()
        {
            RoleController target = GameEnv.Instance.CurrentExe.Dst as RoleController;
            if (target != null)
            {
                target.Temp.Hp -= GameEnv.Damage;
                ShowText(GameEnv.Damage.ToString());
            }
        }
        //判断是否闪避成功
        private bool CheckMiss()
        {
            float miss = Temp.Miss;
            float temp = QGFRandom.Default.Range(0.0f, 1.0f);
            if (temp < miss)
            {
                EnvVariable env = new EnvVariable();
                env.Copy(GameEnv.Instance.CurrentExe);
                env.Dst = this;
                GameEnv.Instance.PushEnv(env);
                //如果闪避判定成功
                BuffManager.Instance.ExecuteWithEnv(TriggerType.OnMiss, this,env.Main);
                //执行src的onbemissed
                if (env.Src.Type == (int)ControllerType.Role)
                {
                    RoleController src = env.Src as RoleController;
                    BuffManager.Instance.ExecuteWithEnv(TriggerType.OnBeMissed, src,env.Main);
                }

                GameEnv.Instance.PopEnv();
                Debuger.Log("{0} missed", Name);
                ShowText("Miss");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 对该role造成物理伤害
        /// </summary>
        /// <param name="dmg">伤害值</param>
        /// <param name="canMiss">能否闪避</param>
        public void DamagePhysically(int dmg, bool canMiss)
        {
            
            //判断闪避
            if (canMiss&&CheckMiss())
            {
                return;
            }

            int phy = Temp.PhysicalRes;
            float res = 100 / (Mathf.Log(5, phy + 1) + 1);
            int realDmg =(int)( res * dmg);
            //执行伤害
            OnDamaged(realDmg);
        }

        /// <summary>
        /// 对该role造成魔法伤害
        /// </summary>
        /// <param name="dmg">伤害值</param>
        /// <param name="canMiss">能否闪避</param>
        public void DamageMagically(int dmg, bool canMiss)
        {

            //判断闪避
            if (canMiss && CheckMiss())
            {
                return;
            }

            int phy = Temp.MagicRes;
            float res = 100 / (Mathf.Log(5, phy + 1) + 1);
            int realDmg = (int)(res * dmg);
            //执行伤害
            OnDamaged(realDmg);

            //执行伤害
            OnDamaged(dmg);
        }

        /// <summary>
        /// 被攻击时调用
        /// </summary>
        /// <param name="isMagic">攻击类型是否是魔法</param>
        public void Attacked(bool isMagic,bool canMiss=false)
        {
            if (GameEnv.Instance.CurrentExe.Src.Type != (int)ControllerType.Role)
            {
                //如果不是角色，直接返回，不允许攻击
                return;
            }
            EnvVariable env = new EnvVariable();
            env.Copy(GameEnv.Instance.CurrentExe);
            env.Dst = this;
            GameEnv.Instance.PushEnv(env);
            //执行攻击者的onattack
            RoleController src = env.Src as RoleController;
            BuffManager.Instance.ExecuteWithEnv(TriggerType.OnAttack,src,env.Main);

            //执行被攻击者的onbeattacked
            BuffManager.Instance.ExecuteWithEnv(TriggerType.OnBeAttacked, this,env.Main);
            GameEnv.Instance.PopEnv();

            int damage=src.Temp.Attack;

            if (isMagic)
            {
                DamageMagically(damage, canMiss);
            }
            else
            {
                DamagePhysically(damage, canMiss);
            }
        }

        /// <summary>
        /// 该角色被击杀时调用
        /// </summary>
        private void OnKilled()
        {
            Debug.Log("onKilled invoke");
            EnvVariable env = new EnvVariable();
            env.Copy(GameEnv.Instance.CurrentExe);
            env.Dst = this;
            GameEnv.Instance.PushEnv(env);
            //执行src的onkill
            if (env.Src.Type == (int)ControllerType.Role)
            {
                RoleController src = env.Src as RoleController;
                BuffManager.Instance.ExecuteWithEnv(TriggerType.OnKill, src, env.Main);
            }
            //执行该角色的onbekilled
            BuffManager.Instance.ExecuteWithEnv(TriggerType.OnBekilled, this, env.Main);
            //添加结束事件
            GameExecuteManager.Instance.Add(OnKillBuffFinished);
            
            GameEnv.Instance.PopEnv();

        }
        //在击杀相关buff执行结束后
        private void OnKillBuffFinished()
        {
            Debug.Log("onKillBuffFinished");
            //如果生命值不足，直接销毁
            if (Temp.Hp <= 0)
            {
                RoleManager.Instance.RemoveRole(RoleId);
            }
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
            if (mExtraMove!=null&&mExtraMove.ContainsKey(terrain))
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

        private HUDText mHud;
        public override GameObject GetGameObject()
        {
            return mObj;
        }
        //获取模型的对象
        public GameObject GetModel()
        {
            return mObj.transform.Find("Model").GetChild(0).gameObject;
        }

        public void UpdateSkill()
        {
            if (onSkillChanged != null)
            {
                onSkillChanged.Invoke(Skills);
            }
        }

        public void FaceTo(Vector3 position)
        {
            Vector3 target = position;
            target.y = mObj.transform.position.y;
            mObj.transform.LookAt(target);
        }

        public void ShowText(string text)
        {
            mHud.HUD(text);
        }

        public override Position GetPosition()
        {
            return Position;
        }



        //========================
        //内置处理

        //=============
        //属性变化
        //临时属性的增减变化
        private void OnTempAttrChanged()
        {
            Debuger.Log("{0} temp attr update", Name);
            Debuger.Log("temp hp:{0},Current:{1}", Temp.Hp,Current.Hp) ;
            Temp.Copy(Current);
            Debuger.Log("temp after hp:{0}", Temp.Hp);
            DataUtil.Execute(Temp, TempMap.Tracks);
            //若未触发属性监听，手动通知外部
            if (TempMap.Tracks.Count == 0)
            {
                UpdatePanel();
                //通知外部
                if (onRoleChanged != null)
                {
                    onRoleChanged(this);
                }
            }
        }

        //当前属性的增减发生了变化
        private void OnCurrentAttrChanged()
        {
            Debuger.Log("{0} current attr update", Name);
            int hp = Current.Hp;
            int mp = Current.Mp;
            //赋予原值
            Current.Copy(Origin);
            Current.SetHP(hp);
            Current.SetMP(mp);
            //进行更改
            DataUtil.Execute(Current, CurrentMap.Tracks);
            //如果大于上限
            if (Current.Hp > Current.MaxHp)
            {
                //设置为上限
                Current.SetHP(Current.MaxHp);
            }
            if (Current.Mp > Current.MaxMp)
            {
                Current.SetMP(Current.MaxMp);
            }

            //如果未触发current的变化监听，则手动触发temp的执行
            if (CurrentMap.Tracks.Count == 0)
            {
                OnTempAttrChanged();
            }
        }
        //temp改变时通知外部改变
        private void OnTempAttributeChanged(string param,ref object value,object origin)
        {
            Debuger.Log("temp param {0} changed", param);
            if (param == "Hp")
            {
                //如果低于当前生命值
                if ((int)value < Current.Hp)
                {
                    Current.Hp = (int)value;
                }
                //如果当前生命值小于等于0,调用onkill
                Debuger.Log("Current HP: {0}", Current.Hp);
                if (Current.Hp <= 0)
                {
                    OnKilled();
                }
                Debuger.Log("HP changed");
                //通知生命值改变
                mPanel.SetHP((int)value);
            }
            //通知外部
            if (onRoleChanged != null)
            {
                onRoleChanged(this);
            }
        }
        //当current值发生变化时通知temp改变
        private void OnCurrentAttributeChanged(string param, ref object value, object origin)
        {
            Debuger.Log("current {0} updated:{1}", param, value.ToString());
            OnTempAttrChanged();
        }

        //更新所有面板
        private void UpdatePanel()
        {
            mPanel.SetHP(Temp.Hp);
            mPanel.UpdateBuff(Buffs);
            OnBuffUpdate();
            if (onRoleChanged != null)
            {
                onRoleChanged.Invoke(this);
            }
        }
        //=====================

        //添加buff时的回调
        private void OnAddBuff(ref Checkmate.Game.Buff.Buff buff)
        {
            
            //设置环境变量
            EnvVariable env = new EnvVariable();
            env.Copy(GameEnv.Instance.CurrentExe);
            env.Dst = this;
            env.Data = buff;
            GameEnv.Instance.PushEnv(env);
            //调用源的OnBuff
            //前提是角色（只有角色拥有buff)
            if (GameEnv.Instance.CurrentExe.Src!=null&&GameEnv.Instance.CurrentExe.Src.Type == (int)ControllerType.Role)
            {
                RoleController src = GameEnv.Instance.CurrentExe.Src as RoleController;
                BuffManager.Instance.Execute(TriggerType.OnBuff, src);
            }
            //调用现有buff的OnBuffed
            BuffManager.Instance.Execute(TriggerType.OnBeBuffed, this);
            GameEnv.Instance.PopEnv();

            //绑定至该角色
            buff.AttachTo(this);
            //调用该buff的onAttach
            env.Src = buff.Src;
            env.Obj = buff.Obj;
            env.Main = buff;
            env.Center = Position;
            env.Dst = null;
            GameEnv.Instance.PushEnv(env);
            Debuger.Log("buff execute env:obj:{0}", GameEnv.Instance.Current.Obj == null ? "null" :GameEnv.Instance.Current.Obj.GetGameObject().name);
            buff.Execute(TriggerType.OnAttached);
            Debuger.Log("buff {0} attached execute:{1}", buff.Name,buff.GetCount(TriggerType.OnAttached));
            GameEnv.Instance.PopEnv();
        }

        //buff改变时的回调
        private void OnBuffUpdate()
        {            
            //调用外部传入的事件（用于更新显示部分）
            if (onBuffChanged != null)
            {
                onBuffChanged.Invoke(this, Buffs);
            }
        }
    }
}
