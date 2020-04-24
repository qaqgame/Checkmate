using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Game.Player;
using Checkmate.Game.Role;
using Checkmate.Game.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
            //该位置不存在
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
            int src = PlayerManager.Instance.PID;
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
        [GetProperty]
        [SetProperty]
        public int CoolTurns
        {
            get;
            set;
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
                SetExtra(ObjectParser.GetExtraDataContent(specialData.Attributes["file"].Value));
            }

            //解析字典
            XmlNode dataNode = node.SelectSingleNode("Data");
            list = dataNode.ChildNodes;
            string tempKey;
            foreach(XmlNode l in list)
            {
                object value = ObjectParser.ParseObject(l, out tempKey);
                mExtraData.Add(tempKey, value);
            }


            //解析内容
            XmlNode content = node.SelectSingleNode("Content");
            OnParseContent(content);

        }
        public virtual void OnParseRoot(XmlNode node) { }
        //解析函数
        public abstract void OnParseContent(XmlNode node);

        /// <summary>
        /// 开始冷却的事件
        /// </summary>
        /// <returns>返回新的冷却时间</returns>
        public virtual int OnCoolBegin(int turns)
        {
            return turns;
        }

        /// <summary>
        /// 冷却结束的事件
        /// </summary>
        public virtual void OnCoolOver()
        {

        }

        /// <summary>
        /// 加载的事件
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


        //=======================================
        private Dictionary<ActionTrigger, List<SkillAction>> mActions;//所有的活动

        public override void OnParseContent(XmlNode node)
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
                        //如果相同，则取等值
                        if (search.start == search.center)
                        {
                            
                        }
                    }
                    
                    //ControllerPool.Add(search.id, temp);
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
    }

    
}
