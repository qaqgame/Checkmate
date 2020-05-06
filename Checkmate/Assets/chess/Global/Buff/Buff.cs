using Checkmate.Game.Controller;
using Checkmate.Game.Utils;
using QGF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Checkmate.Game.Buff
{
    public enum TriggerType
    {
        OnTurn,
        OnAttack,
        OnBeAttacked,
        OnMove,
        OnMiss,
        OnBeMissed,
        OnSkill,
        OnBeSkilled,
        OnBuff,
        OnBeBuffed,
        OnDamage,
        OnBeDamaged,
        OnKill,
        OnBekilled,
        OnStatusChanged,
        

        OnUpdateAttribute,//长期的更新属性
        OnAttached,//被添加至人身上时执行
        OnRemoved//从角色身上移出时执行
    }


    public class Buff:EffectController
    {

        public override int Type { get { return -1; } }

        [GetProperty]
        public int BuffId
        {
            get;
            set;
        }

        //名称
        [GetProperty]
        public string Name
        {
            get;
            set;
        }
        //描述
        [GetProperty]
        public string Description
        {
            get;
            private set;
        }

        //剩余回合
        [GetProperty]
        [SetProperty]
        public int ReserveTurn { get; set; }

        //剩余次数
        [GetProperty]
        [SetProperty]
        public int ReserveTime { get; set; }

        //是否是debuff
        [GetProperty]
        public bool IsDebuff
        {
            get;
            private set;
        }

        //能否被外部移除
        [GetProperty]
        public bool CanRemove
        {
            get;
            private set;
        }

        //是否无限回合
        [GetProperty]
        public bool IsInfiniteTurn
        {
            get
            {
                return ReserveTurn == -1;
            }
        }
        //是否无限次数
        [GetProperty]
        public bool IsInfiniteTimes
        {
            get
            {
                return ReserveTime == -1;
            }
        }


        //该buff的来源对象
        [GetProperty]
        public ModelController Src
        {
            get;
            private set;
        }

        //该buff的所属
        [GetProperty]
        public RoleController Obj
        {
            get;
            private set;
        }

        //该buff的来源
        [GetProperty]
        public BaseController SrcEffect {
            get;
            private set;
        }

        public List<TriggerType> Triggers;//所能触发的类型

        private Dictionary<TriggerType, List<SkillAction>> mActions;//所有的动作


        //添加至某个物体时调用
        public void AttachTo(RoleController dst)
        {
            Obj = dst;
            Src = GameEnv.Instance.Current.Src;
            SrcEffect = GameEnv.Instance.Current.Main;
        }

        //执行
        public void Execute(TriggerType trigger)
        {
            //不包含直接返回
            if (!mActions.ContainsKey(trigger))
            {
                return;
            }

            foreach(var action in mActions[trigger])
            {
                GameEnv.Instance.Current.ExecuteAction(action);
            }
            //清除所有涉及对象的属性加成
            foreach(var role in Temp.mUsedRoles)
            {
                role.TempMap.RemoveTrack(Temp);
            }
            Temp.Clear();
        }

        //========================================
        #region 解析
        public void Parse(XmlNode node)
        {
            Name = node.Attributes["name"].Value;

            XmlNode description = node.SelectSingleNode("Description");
            Description = description.InnerText;

            //解析特殊字典
            XmlNode specialData = node.SelectSingleNode("SpecialData");
            if (specialData != null)
            {
                SetExtra(ObjectParser.GetExtraDataContent(Name, specialData.Attributes["file"].Value));
            }

            //解析字典
            XmlNode dataNode = node.SelectSingleNode("Data");
            if (dataNode.HasChildNodes)
            {
                var list = dataNode.ChildNodes;
                string tempKey;
                foreach (XmlNode l in list)
                {
                    object value = ObjectParser.ParseObject(l, out tempKey);
                    mExtraData.Add(tempKey, value);
                }
            }

            //解析内容
            XmlNode content = node.SelectSingleNode("Content");
            ParseContent(content);
        }

        private void ParseContent(XmlNode node)
        {
            Triggers = new List<TriggerType>();
            mActions = new Dictionary<TriggerType, List<SkillAction>>();
            //解析所有的action
            XmlNodeList cl = node.ChildNodes;
            foreach (XmlNode l in cl)
            {
                string triggers = l.Attributes["trigger"].Value;
                List<TriggerType> triggerTypes = ObjectParser.ParseEnums<TriggerType>(triggers);
                SkillAction action = new SkillAction(l);
                foreach(var type in triggerTypes)
                {
                    //未包含则添加该trigger
                    if (!Triggers.Contains(type))
                    {
                        Triggers.Add(type);
                    }
                    if (!mActions.ContainsKey(type))
                    {
                        List<SkillAction> actions = new List<SkillAction>();
                        mActions.Add(type, actions);
                    }
                    mActions[type].Add(action);
                }
            }
        }
        #endregion

        //拷贝
        public override BaseController Clone()
        {
            Buff result = new Buff();
            result.Name = this.Name;
            result.Description = this.Description;
            result.CanRemove = this.CanRemove;
            result.IsDebuff = this.IsDebuff;
            result.ReserveTurn = this.ReserveTurn;
            result.ReserveTime = this.ReserveTime;
            result.mActions = this.mActions;
            result.Triggers = this.Triggers;
            result.mExtraData = new Dictionary<string, object>(this.mExtraData);

            return result;
        }

        //获取action数目
        public int GetCount(TriggerType type)
        {
            return mActions.ContainsKey(type) ? mActions[type].Count : 0;
        }
    }


    //Buff解析类
    public static class BuffParser
    {
        static string BuffNameSpace = "Checkmate.Game.Buff.";
        public static Buff ParseBuff(XmlNode root)
        {
            //获取类名
            System.Type tp = System.Type.GetType(BuffNameSpace + root.Name);
            if (tp == null)
            {
                Debuger.LogError("error get class:" + BuffNameSpace + root.Name);
            }
            ConstructorInfo constructor = tp.GetConstructor(System.Type.EmptyTypes);

            Buff result = (Buff)constructor.Invoke(null);
            result.Parse(root);

            return result;
        }
    }
}
