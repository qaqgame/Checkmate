using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Game.Player;
using Checkmate.Game.Role;
using Checkmate.Game.Utils;
using System;
using System.Collections.Generic;
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

    }


    public abstract class BaseSkill:BaseController
    {
        public override int Type { get { return (int)ControllerType.Skill; } }

        //冷却时间
        [GetProperty]
        [SetProperty]
        public int CoolTurns
        {
            get;
            set;
        }


        //解析函数
        public abstract void Parse(XmlNode node);

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



}
