using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Game.Player;
using Checkmate.Game.Role;
using Checkmate.Game.Utils;
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
            if (cell == null||!cell.Visible)
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


    public abstract class BaseSkill:BaseController
    {
        public override int Type { get { return (int)ControllerType.Skill; } }

        private List<IRange> mEffectRanges;//用于标识效果的显示范围

        private SkillRange mMouseRange;//鼠标的可选范围

        private List<Position> tempResult;//临时存储结果

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
                if (_coolTurn == 0 && value > 0)
                {
                    _coolTurn = value;
                    OnCoolBegin(value);
                }
                if (_coolTurn > 0 && value <= 0)
                {
                    _coolTurn = 0;
                    OnCoolOver();

                }
                else
                {
                    _coolTurn = value;
                }
            }
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
        
        public void Parse(XmlNode node)
        {
            Name = node.Attributes["name"].Value;
            CoolTurns = int.Parse(node.Attributes["cd"].Value);

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
            foreach(XmlNode l in list)
            {
                IRange range = RangeParser.ParseRange(l);
                mEffectRanges.Add(range);
            }

            //解析特殊字典
            XmlNode specialData = node.SelectSingleNode("SpecialData");
            if (specialData != null)
            {
                SetExtra(ObjectParser.GetExtraDataContent(Name,specialData.Attributes["file"].Value));
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
        protected virtual void OnParseRoot(XmlNode node) { }
        //解析函数
        protected abstract void OnParseContent(XmlNode node);

        /// <summary>
        /// 开始冷却的事件
        /// </summary>
        /// <returns>返回新的冷却时间</returns>
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
        private static Dictionary<string, List<ModelController>> ControllerPool=new Dictionary<string, List<ModelController>>();//所有skill共用的pool

        #region 内部类
        internal enum ActionTrigger
        {
            Load,//加载时
            CoolOver,//结束冷却时
            CoolBegin,//进入冷却时
            Execute//执行时
        }
        public const int SearchType = 0;
        public const int SelectType = 1;
        internal class SkillAction
        {
            
            internal class TargetTrack
            {
                public int type;//操作类型
                public int idx;//操作序列
                public TargetTrack(int t,int i)
                {
                    type = t;
                    idx = i;
                }
            }


            public List<ActionTrigger> Trigger;//触发方式
            public List<TargetTrack> TargetTracks;//执行目标搜索的顺序
            public List<BaseSearch> Searches;//所有搜索操作
            public List<Selects> Selectors;//所有筛选操作

            public List<ExecuteInfo> Executes;//所有的脚本操作

            public SkillAction(XmlNode node)
            {
                string trigger = node.Attributes["trigger"].Value;
                Trigger = ObjectParser.ParseEnums<ActionTrigger>(trigger);

                //解析目标部分
                XmlNode target = node.SelectSingleNode("Targets");
                XmlNodeList tl = target.ChildNodes;
                TargetTracks = new List<TargetTrack>();
                Searches = new List<BaseSearch>();
                Selectors = new List<Selects>();
                foreach(XmlNode l in tl)
                {
                    //解析Selects
                    if (l.Name == "Selects")
                    {
                        Selects s = new Selects(l);
                        TargetTracks.Add(new TargetTrack(SelectType, Selectors.Count));
                        Selectors.Add(s);
                    }
                    else
                    {
                        BaseSearch bs = SearchParser.Parse(l);
                        TargetTracks.Add(new TargetTrack(SearchType, Searches.Count));
                        Searches.Add(bs);
                    }
                }

                //解析脚本部分
                Executes = new List<ExecuteInfo>();
                XmlNode exRoot = node.SelectSingleNode("Executes");
                XmlNodeList el = exRoot.ChildNodes;
                foreach(XmlNode l in el)
                {
                    ExecuteInfo info = ExecuteUtil.ParseExecute(l);
                    Executes.Add(info);
                }
            }
        }
        #endregion

        //=======================================
        private Dictionary<ActionTrigger, List<SkillAction>> mActions;//所有的活动

        protected override void OnParseContent(XmlNode node)
        {
            mActions = new Dictionary<ActionTrigger, List<SkillAction>>();
            //解析所有的action
            XmlNodeList cl = node.ChildNodes;
            foreach(XmlNode l in cl)
            {
                SkillAction action = new SkillAction(l);
                foreach(var trigger in action.Trigger)
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
            foreach (var action in mActions[ActionTrigger.Load])
            {
                ExecuteAction(action);
            }

        }

        public override void OnExecute()
        {
            foreach (var action in mActions[ActionTrigger.Execute])
            {
                ExecuteAction(action);
            }
        }

        #region 功能函数
        private void ExecuteAction(SkillAction action)
        {
            ExecuteTargets(action);
            foreach(var info in action.Executes)
            {
                ExecuteMain(info);
            }
        }

        private void ExecuteTargets(SkillAction action)
        {
            foreach(var track in action.TargetTracks)
            {
                //执行搜索
                if (track.type == SearchType)
                {
                    BaseSearch search = action.Searches[track.idx];
                    List<ModelController> temp;
                    Position start, center;
                    if(TryGetSearchParam(search.start,out start)&&TryGetSearchParam(search.center,out center))
                    {
                        temp = search.GetSearchResult(start,center);
                    }
                    //如果存在列表
                    else if(search.start.Contains('#')||search.center.Contains('#'))
                    {
                        temp = new List<ModelController>();
                        List<ModelController> controllers;
                        //如果相同，则取等值
                        if (search.start == search.center)
                        {
                            
                            controllers= ControllerPool[search.start.Substring(1)];
                            foreach(var c in controllers)
                            {
                                temp.AddRange(search.GetSearchResult(c.GetPosition(), c.GetPosition()));
                            }
                        }
                        //起点为列表
                        else if (search.start.Contains('#'))
                        {
                            controllers = ControllerPool[search.start.Substring(1)];
                            TryGetSearchParam(search.center, out center);
                            foreach(var c in controllers)
                            {
                                temp.AddRange(search.GetSearchResult(c.GetPosition(), center));
                            }
                        }
                        //中心为列表
                        else
                        {
                            controllers = ControllerPool[search.center.Substring(1)];
                            TryGetSearchParam(search.start, out start);
                            foreach (var c in controllers)
                            {
                                temp.AddRange(search.GetSearchResult(start, c.GetPosition()));
                            }
                        }
                        temp = temp.Distinct().ToList();
                    }
                    else
                    {
                        Debug.LogError("error execute search:cannot build id:" + search.id);
                        return;
                    }
                    ControllerPool.Add(search.id, temp);
                }
                //执行筛选
                else
                {
                    Selects selects = action.Selectors[track.idx];
                    List<ModelController> src = ControllerPool[selects.src];
                    List<ModelController> temp = selects.ExecuteFilter(src);
                    ControllerPool.Add(selects.id, temp);
                }
            }
        }


        private void ExecuteMain(ExecuteInfo info)
        {
            object returnValue=null;
            object[] param = new object[info.parameters.Count];
            int flagList=-1;//标识ControllerList位置
            //解析非controllerList参数
            for(int i=0;i<info.parameters.Count;++i)
            {
                if (info.parameters[i].type == ParamType.ControllerList)
                {
                    flagList = i;
                    continue;
                }
                param[i] = GetParam(info.parameters[i].type, info.parameters[i].value);
            }

            //检查有没有包含controllerList
            //如果有包含
            if (flagList!=-1)
            {
                string cl = info.parameters[flagList].value;
                string tempVariable=null;
                //如果有.代表是变量
                if (cl.Contains('.'))
                {
                    tempVariable = cl.Substring(cl.IndexOf('.') + 1);
                    cl = cl.Substring(1, cl.IndexOf('.'));
                }
                List<ModelController> list = ControllerPool[cl.Substring(1)];
                foreach(ModelController l in list)
                {
                    object temp=l;
                    if (tempVariable != null)
                    {
                        temp = l.GetValue(tempVariable);
                    }
                    param[flagList] = temp;
                    returnValue=ExecuteUtil.Instance.Execute(info.method, param);
                }
            }
            else
            {
                returnValue = ExecuteUtil.Instance.Execute(info.method, param);
            }
            //处理返回值
            if (info.returnValue != null)
            {
                HandleReturn(info.returnValue, returnValue);
            }

        }

        #endregion

        #region 工具函数

        /// <summary>
        /// 获取搜索参数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pos"></param>
        /// <returns>判断是否是内置变量</returns>
        private bool TryGetSearchParam(string value,out Position pos)
        {
            switch (value)
            {
                case "Src":
                    {
                        ModelController c = GameEnv.Instance.Current.Src as ModelController;
                        if (c != null)
                        {
                            pos= c.GetPosition();
                        }
                        pos= null;
                        return true;
                    }
                case "Center":
                    {
                        pos= GameEnv.Instance.Current.Center;
                        return true;
                    }
            }
            pos=null;
            return false;
        }


        private object GetParam(ParamType type,string value)
        {
            //如果value是内部变量，直接获取
            if (value.Contains('$'))
            {
                return GetValue(value.Substring(1));
            }
            //外部变量
            else if (value.Contains('%'))
            {
                //如果是获取controller
                if (type == ParamType.Controller)
                {
                    return GetController(value.Substring(1));
                }
                //获取变量
                string cn = value.Substring(1, value.IndexOf('.'));
                string v = value.Substring(value.IndexOf('.') + 1);
                BaseController controller = GetController(cn);
                return controller.GetValue(v);
            }
            //代表取列表的单个控制器
            else if (value.Contains('#') && type == ParamType.Controller)
            {
                string cname = value.Substring(1);
                string tempValue = null;
                if (value.Contains('.'))
                {
                    cname = value.Substring(1, value.IndexOf('.'));
                    tempValue = value.Substring(value.IndexOf('.') + 1);
                }
                BaseController controller = ControllerPool[cname][0];
                return tempValue == null ? controller : controller.GetValue(tempValue);

            }
            //代表直接解析
            else
            {
                switch (type)
                {
                    case ParamType.Int:return int.Parse(value);
                    case ParamType.Float:return float.Parse(value);
                    case ParamType.String:return value;
                }
            }
            return null;
        }

        private BaseController GetController(string value)
        {
            switch (value)
            {
                case "Src": return GameEnv.Instance.Current.Src;
                case "Dst": return GameEnv.Instance.Current.Dst;
                case "Main": return GameEnv.Instance.Current.Main;
            }
            return null;
        }

        //
        private void HandleReturn(string target,object value)
        {
            //如果target是内部变量，直接设置
            if (target.Contains('$'))
            {
                SetValue(target.Substring(1), value);
            }
            //外部变量
            else if (target.Contains('%'))
            {
                
            }
            //代表取列表的变量
            else if (target.Contains('#'))
            {
                string cname = target.Substring(1, target.IndexOf('.'));
                string tempValue = target.Substring(target.IndexOf('.') + 1);
                List<ModelController> controllers = ControllerPool[cname];
                
                foreach(var model in controllers)
                {
                    model.SetValue(tempValue, value);
                }

            }
        }
        #endregion
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
                Debug.LogError("error get class:" + RangeNameSpace + root.Name);
            }
            ConstructorInfo constructor = tp.GetConstructor(System.Type.EmptyTypes);

            BaseSkill skill = (BaseSkill)constructor.Invoke(null);
            skill.Parse(root);

            return skill;
        }
    }
}
