using Checkmate.Game.Controller;
using Checkmate.Game.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Checkmate.Game.Utils
{
    //筛选的接口
    public abstract class BaseSelect
    {
        public abstract void Parse(XmlNode node);
        public abstract List<BaseController> GetFilterResult(List<BaseController> objs);
    }

    public class DefaultTeamSelector : BaseSelect
    {
        enum TeamType
        {
            Self,//己方
            Friend,//友方
            Enemy,//敌方
            Neutual//中立
        }

        List<TeamType> mTypes;

        public override void Parse(XmlNode node)
        {

            mTypes =ParseTypes(node.Attributes["team"].Value);
        }

        private List<TeamType> ParseTypes(string content)
        {
            List<TeamType> result = new List<TeamType>();
            if (content.Contains('|'))
            {
                string[] temps = content.Split('|');
                foreach(var c in temps)
                {
                    TeamType tempType= (TeamType)Enum.Parse(typeof(TeamType),c);
                    result.Add(tempType);
                }
            }
            else
            {
                TeamType tempType = (TeamType)Enum.Parse(typeof(TeamType), content);
                result.Add(tempType);
            }

            return result;
        }

        public override List<BaseController> GetFilterResult(List<BaseController> objs)
        {
            List<RoleController> result = new List<RoleController>();
            //先将objs筛选出其中的role
            foreach(var obj in objs)
            {
                if (obj.Type == (int)ControllerType.Role)
                {
                    result.Add(obj as RoleController);
                }
            }


            //获取当前的操作主体
            BaseController controller = GameEnv.Instance.Current.Obj;
            //如果为地面，则全触发
            if (controller.Type == (int)ControllerType.Cell)
            {
                return objs;
            }

            else
            {
                RoleController role = controller as RoleController;
                //如果当前为role
                if (role != null)
                {
                    return Filter(result, role, mTypes);
                }

                //否则返回空
                return null;
            }
        }


        //筛选types对应的role
        private List<BaseController> Filter(List<RoleController> roles,RoleController src,List<TeamType> types)
        {
            List<BaseController> result = new List<BaseController>();
            //遍历所有role
            foreach(var role in roles)
            {
                //遍历类型
                foreach(var type in types)
                {
                    //满足直接加入
                    if (CheckTeam(src, role, type))
                    {
                        result.Add(role);
                        break;
                    }
                }
            }

            return result;
        }

        //检查单个的role是否满足
        private bool CheckTeam(RoleController src,RoleController dst,TeamType type)
        {
            int srcTeam = src.Team;
            int dstTeam = dst.Team;

            switch (type)
            {
                case TeamType.Self:
                    return srcTeam == dstTeam;
                case TeamType.Friend:
                    return PlayerManager.Instance.IsFriend(srcTeam, dstTeam);
                case TeamType.Enemy:
                    return PlayerManager.Instance.IsEnemy(srcTeam, dstTeam);
                case TeamType.Neutual:
                    return PlayerManager.Instance.IsNeutual(srcTeam, dstTeam);
            }
            return false;
        }


    }

    //筛选器解析
    public static class SelectParser
    {
        static string SelectNameSpace = "Checkmate.Game.Utils";

        public static BaseSelect ParseRange(XmlNode root)
        {
            //获取类名
            System.Type tp = System.Type.GetType(SelectNameSpace + root.Name);
            ConstructorInfo constructor = tp.GetConstructor(System.Type.EmptyTypes);

            BaseSelect range = (BaseSelect)constructor.Invoke(null);
            range.Parse(root);

            return range;
        }
    }
}
