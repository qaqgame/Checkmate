using QGF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Checkmate.Game.Utils
{
    public interface ICheck
    {
        void Parse(XmlNode node);
        bool Check();
    }
    public class DefaultCheck : ICheck
    {
        public string src;
        public ParamType srcType;//源数据类型
        public string target;
        public ParamType targetType;//目标数据类型
        enum CompareType
        {
            More,
            Less,
            NotMore,
            NotLess,
            Equal,
            NotEqual
        }

        CompareType compare;
        public bool Check()
        {
            object a = GameEnv.Instance.CurrentExe.GetParam(srcType,src);
            object b = GameEnv.Instance.CurrentExe.GetParam(targetType, target);
            return Compare(a, b, compare);

        }

        public void Parse(XmlNode node)
        {
            src = node.Attributes["src"].Value;
            srcType = (ParamType)System.Enum.Parse(typeof(ParamType), node.Attributes["srcType"].Value);
            target = node.Attributes["target"].Value;
            targetType = (ParamType)System.Enum.Parse(typeof(ParamType), node.Attributes["targetType"].Value);

            compare = (CompareType)System.Enum.Parse(typeof(CompareType), node.Attributes["compare"].Value);
        }

        private bool Compare(dynamic a, dynamic b, CompareType type)
        {
            switch (type)
            {
                case CompareType.More:
                    return a > b;
                case CompareType.Less:
                    return a < b;
                case CompareType.NotMore:
                    return a <= b;
                case CompareType.NotLess:
                    return a >= b;
                case CompareType.Equal:
                    return a == b;
                case CompareType.NotEqual:
                    return a != b;
            }
            return false;
        }
    }


    public class Checks
    {
        List<ICheck> checks;

        public bool All = true;
        public Checks(XmlNode node)
        {
            if (node.Attributes["all"] != null)
            {
                All = bool.Parse(node.Attributes["all"].Value);
            }
            checks = new List<ICheck>();
            if (node.HasChildNodes)
            {
                XmlNodeList list = node.ChildNodes;
                foreach(XmlNode l in list)
                {
                    ICheck check = CheckParser.ParseCheck(l);
                    checks.Add(check);
                }
            }
        }

        public bool Execute()
        {
            if (All)
            {
                foreach(var c in checks)
                {
                    if (!c.Check())
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                foreach (var c in checks)
                {
                    if (c.Check())
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }

    public static class CheckParser
    {
        static string CheckNameSpace = "Checkmate.Game.Utils.";

        public static ICheck ParseCheck(XmlNode root)
        {
            //获取类名
            System.Type tp = System.Type.GetType(CheckNameSpace + root.Name);
            if (tp == null)
            {
                Debuger.LogError("error load selector:" + CheckNameSpace + root.Name);
            }
            ConstructorInfo constructor = tp.GetConstructor(System.Type.EmptyTypes);

            ICheck range = (ICheck)constructor.Invoke(null);
            range.Parse(root);

            return range;
        }
    }
}
