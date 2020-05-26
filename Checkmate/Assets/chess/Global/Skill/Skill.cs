using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Game.Player;
using Checkmate.Game.Role;
using Checkmate.Game.Utils;
using QGF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace Checkmate.Game.Skill
{
    internal enum TeamType { 
        Self,
        Friend,
        Enemy,
        Neutual
    }
    //技能的施法范围
    public class SkillRange
    {
        private int mMax, mMin;//范围限制
        private List<TeamType> mTeamFilter;//队伍过滤
        private List<ControllerType> mTypeFilter;//类型过滤
        private List<IRange> mRanges;//具体范围


        List<Position> temps = new List<Position>();
        List<Position> result = new List<Position>();
        /// <summary>
        /// 获取所有范围内所有可用的中心坐标
        /// </summary>
        /// <param name="Start">起始中心位置</param>
        /// <returns></returns>
        public List<Position> GetPositions(Position Start)
        {
            temps.Clear();

            foreach(var range in mRanges)
            {
                List<Position> t = range.GetResult(Start, Start);
                foreach(var position in t)
                {
                    int dis = HexMapUtil.GetDistance(Start, position);
                    //如果该position在范围内
                    if (dis >= mMin && dis <= mMax)
                    {
                        //且未包含则加入
                        if (!temps.Contains(position))
                        {
                            temps.Add(position);
                        }
                    }
                }
            }

            result.Clear();
            //对方格进行筛选
            foreach(var pos in temps)
            {
                if (CheckPosition(pos, mTypeFilter, mTeamFilter))
                {
                    result.Add(pos);
                }
            }

            return result;

        }

        private bool CheckPosition(Position pos,List<ControllerType> typeFilter,List<TeamType> teamFilter=null)
        {
            CellController cell = MapManager.Instance.GetCell(pos);
            //该位置不存在或在视野外
            if (cell == null)
            {
                return false;
            }

            //如果包含cell
            if (typeFilter.Contains(ControllerType.Cell))
            {
                return true;
            }

            foreach (var type in typeFilter)
            {
                switch(type)
                {
                    case ControllerType.Role:
                        {
                            RoleController role = cell.HasRole ? RoleManager.Instance.GetRole(cell.Role) : null;
                            if (role != null)
                            {
                                return FilteRole(role, teamFilter);
                            }
                            return false;
                        }
                }
            }
            return false;
        }


        private bool FilteRole(RoleController role,List<TeamType> teams)
        {
            int src = (int)PlayerManager.Instance.PID;
            int dst = role.Team;
            CellController cell = MapManager.Instance.GetCell(role.Position);
            if (!cell.Visible)
            {
                return false;
            }
            foreach(var team in teams) {
                if (CheckTeam(src, dst, team))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckTeam(int src,int dst,TeamType team)
        {
            switch (team)
            {
                case TeamType.Self:
                    return src == dst;
                case TeamType.Friend:
                    return PlayerManager.Instance.IsFriend(src, dst);
                case TeamType.Enemy:
                    return PlayerManager.Instance.IsEnemy(src, dst);
                case TeamType.Neutual:
                    return PlayerManager.Instance.IsNeutual(src, dst);
            }
            return false;
        }


        public SkillRange(XmlNode node)
        {
            mMax = int.Parse(node.Attributes["max"].Value);
            mMin = int.Parse(node.Attributes["min"].Value);

            string types = node.Attributes["type"].Value;
            mTypeFilter = ObjectParser.ParseEnums<ControllerType>(types);

            string teams = node.Attributes["team"].Value;
            mTeamFilter = ObjectParser.ParseEnums<TeamType>(teams);

            mRanges = new List<IRange>();
            XmlNodeList list = node.ChildNodes;
            foreach(XmlNode l in list)
            {
                IRange temp = RangeParser.ParseRange(l);
                mRanges.Add(temp);
            }
        }

        
    }



    public abstract class BaseSkill:EffectController
    {
        public override int Type { get { return (int)ControllerType.Skill; } }

        private List<IRange> mEffectRanges;//用于标识效果的显示范围

        private SkillRange mMouseRange;//鼠标的可选范围

        private List<Position> tempResult = new List<Position>();//临时存储结果


        //消耗行动点
        public int Cost
        {
            get;
            private set;
        }

        //获取鼠标范围
        public List<Position> GetMousePositions(Position Start)
        {
            return mMouseRange.GetPositions(Start);
        }

        //获取效果范围
        public List<Position> GetEffectPositions(Position Start,Position Center)
        {
            tempResult.Clear();
            foreach(var range in mEffectRanges)
            {
                List<Position> temp = range.GetResult(Start, Center);
                tempResult.AddRange(temp);
            }
            return tempResult;
        }


        //冷却时间
        protected int _coolTurn;
        [GetProperty]
        [SetProperty]
        public int CoolTurns
        {
            get
            {
                return _coolTurn;
            }
            set
            {
                if (_coolTurn < MaxCool && value == MaxCool)
                {
                    _coolTurn = value;
                    OnCoolOver();
                }
                if (_coolTurn == MaxCool && value == 0)
                {
                    _coolTurn = 0;
                    OnCoolBegin(MaxCool);

                }
                else
                {
                    _coolTurn = value;
                }
                //事件回调
                EnvVariable v = GameEnv.Instance.CurrentExe;
                if (v != null && v.Obj != null && v.Obj.Type == (int)ControllerType.Role)
                {
                    (v.Obj as RoleController).UpdateSkill();
                }
            }
        }

        //冷却
        public int MaxCool
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        //技能描述
        public string Description
        {
            get;
            private set;
        }
        
        //图标
        public string Icon
        {
            get;
            private set;
        }

        public void Parse(XmlNode node)
        {
            try
            {
                Name = node.Attributes["name"].Value;
                CoolTurns = MaxCool = int.Parse(node.Attributes["cd"].Value);
                Cost = int.Parse(node.Attributes["cost"].Value);
                Icon = node.Attributes["icon"].Value;


                OnParseRoot(node);

                XmlNode description = node.SelectSingleNode("Description");
                Description = description.InnerText;

                //解析鼠标范围
                XmlNode skillRange = node.SelectSingleNode("SkillRange");
                mMouseRange = new SkillRange(skillRange);

                //解析影响范围
                XmlNode effectRange = node.SelectSingleNode("EffectRange");
                mEffectRanges = new List<IRange>();
                XmlNodeList list = effectRange.ChildNodes;
                foreach (XmlNode l in list)
                {
                    IRange range = RangeParser.ParseRange(l);
                    mEffectRanges.Add(range);
                }

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
                    list = dataNode.ChildNodes;
                    string tempKey;
                    foreach (XmlNode l in list)
                    {
                        object value = ObjectParser.ParseObject(l, out tempKey);
                        mExtraData.Add(tempKey, value);
                    }
                }


                //解析内容
                XmlNode content = node.SelectSingleNode("Content");
                OnParseContent(content);
            }
            catch(Exception e)
            {
                Debuger.LogError(e.StackTrace);
            }
        }
        protected virtual void OnParseRoot(XmlNode node) { }
        //解析函数
        protected abstract void OnParseContent(XmlNode node);

        /// <summary>
        /// 开始冷却的事件
        /// </summary>
        public virtual void OnCoolBegin(int turns)
        {
            
        }

        /// <summary>
        /// 冷却结束的事件
        /// </summary>
        public virtual void OnCoolOver()
        {

        }

        /// <summary>
        /// 加载的事件(绑定至角色时调用)
        /// </summary>
        public virtual void OnLoad()
        {

        }

        //执行的事件
        public virtual void OnExecute()
        {

        }
    }


    public class Skill : BaseSkill
    {
        #region 内部类
        internal enum ActionTrigger
        {
            Load,//加载时
            CoolOver,//结束冷却时
            CoolBegin,//进入冷却时
            Execute//执行时
        }
        
        
        #endregion

        //=======================================
        private Dictionary<ActionTrigger, List<SkillAction>> mActions;//所有的活动


        //是否是被动技能
        public bool IsPassive
        {
            get;
            private set;
        }

        

        protected override void OnParseRoot(XmlNode node)
        {
            base.OnParseRoot(node);

            IsPassive = false;
            if (node.Attributes["active"] != null)
            {
                IsPassive = !(bool.Parse(node.Attributes["active"].Value));
            }

            
        }

        protected override void OnParseContent(XmlNode node)
        {
            mActions = new Dictionary<ActionTrigger, List<SkillAction>>();
            //解析所有的action
            XmlNodeList cl = node.ChildNodes;
            foreach(XmlNode l in cl)
            {
                SkillAction action = new SkillAction(l);
                string triggers = l.Attributes["trigger"].Value;
                List<ActionTrigger> tempTrigger = ObjectParser.ParseEnums < ActionTrigger>(triggers);

                foreach(var trigger in tempTrigger)
                {
                    //不包含则创建
                    if (!mActions.ContainsKey(trigger))
                    {
                        List<SkillAction> actionList = new List<SkillAction>();
                        mActions.Add(trigger, actionList);
                    }
                    mActions[trigger].Add(action);
                }
            }
        }

        public override void OnLoad()
        {
            if (!mActions.ContainsKey(ActionTrigger.Load) || mActions[ActionTrigger.Load].Count == 0)
            {
                return;
            }
            GameExecuteManager.Instance.Add(mActions[ActionTrigger.Load]);
            //foreach (var action in mActions[ActionTrigger.Load])
            //{
            //    GameEnv.Instance.Current.ExecuteAction(action);
            //}
            //foreach (var r in Temp.mUsedRoles)
            //{
            //    r.TempMap.RemoveTrack(Temp);
            //}
            //Temp.Clear();
        }

        public override void OnExecute()
        {
            if (!mActions.ContainsKey(ActionTrigger.Execute) || mActions[ActionTrigger.Execute].Count == 0)
            {
                return;
            }
            CoolTurns = 0;
            GameExecuteManager.Instance.Add(mActions[ActionTrigger.Execute]);
            //foreach (var action in mActions[ActionTrigger.Execute])
            //{
            //    GameEnv.Instance.Current.ExecuteAction(action);
            //}
            //foreach (var r in Temp.mUsedRoles)
            //{
            //    r.TempMap.RemoveTrack(Temp);
            //}
            //Temp.Clear();
        }
    }

    //技能解析类
    public static class SkillParser
    {
        static string RangeNameSpace = "Checkmate.Game.Skill.";
        public static BaseSkill ParseSkill(XmlNode root)
        {
            //获取类名
            System.Type tp = System.Type.GetType(RangeNameSpace + root.Name);
            if (tp == null)
            {
                Debuger.LogError("error get class:" + RangeNameSpace + root.Name);
            }
            try
            {
                ConstructorInfo constructor = tp.GetConstructor(System.Type.EmptyTypes);

                BaseSkill skill = (BaseSkill)constructor.Invoke(null);
                skill.Parse(root);
                return skill;
            }
            catch(Exception e)
            {
                Debuger.LogError(e.StackTrace);
                return null;
            }
            
        }
    }
}
