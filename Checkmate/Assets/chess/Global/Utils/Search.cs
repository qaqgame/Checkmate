using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Game.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Checkmate.Game.Utils
{


    //搜索接口
    //外部调用:解析start-解析center-调用GetSearchResult-加入id为key的字典
    public abstract class BaseSearch {
        public string id;//生成id
        public string start;//起始
        public string center;//中心

        //解析方式,返回生成id
        public abstract void Parse(XmlNode node);

        public abstract List<ModelController> GetSearchResult(Position Start,Position Center);
    }

    //默认的搜索类
    public abstract class DefaultSearch : BaseSearch
    {
        public ControllerType type;//筛选类型
        public int limit;//限制数量
        public List<IRange> ranges;//解析出来的范围


        private List<CellController> mTempResult=new List<CellController>();//用于临时存储搜索结果
        public override void Parse(XmlNode node)
        {
            id = node.Attributes["id"].Value;
            type = (ControllerType)Enum.Parse(typeof(ControllerType), node.Attributes["type"].Value);
            limit = int.Parse(node.Attributes["limit"].Value);
            limit = limit == -1 ? int.MaxValue : limit;
            start = node.Attributes["start"].Value;
            center = node.Attributes["center"].Value;
            ranges = new List<IRange>();
            //解析范围
            XmlNodeList list = node.ChildNodes;
            foreach(XmlNode r in list)
            {
                ranges.Add(RangeParser.ParseRange(r));
            }

        }

        public override List<ModelController> GetSearchResult(Position Start, Position Center)
        {
            mTempResult.Clear();
            foreach(var range in ranges)
            {
                List<CellController> temp = MapManager.Instance.GetCells(range, Start, Center);
                mTempResult.AddRange(temp);
            }

            //遍历所有临时结果
            return FilterController(mTempResult, type,limit);
        }

        private List<ModelController> FilterController(List<CellController> cells,ControllerType filter,int limit=int.MaxValue)
        {
            List<ModelController> result = new List<ModelController>();

            int cnt = 0;
            foreach(var cell in cells)
            {
                ModelController temp;
                switch (type)
                {
                    case ControllerType.Cell:temp = cell;
                        break;
                    case ControllerType.Role:temp = cell.HasRole ? RoleManager.Instance.GetRole(cell.Role) : null;
                        break;
                    default:temp = null;
                        break;
                }
                //不为空且未包含,则加入
                if (temp != null&&!result.Contains(temp)&&cnt<limit)
                {
                    result.Add(temp);
                    ++cnt;
                }
            }

            return result;
        }
    }


    //搜索的解析类
    public static class SearchParser
    {
        static string SearchNameSpace = "Checkmate.Game.Utils.";

        public static BaseSearch Parse(XmlNode node)
        {
            //获取类名
            System.Type tp = System.Type.GetType(SearchNameSpace+node.Name);
            ConstructorInfo constructor = tp.GetConstructor(System.Type.EmptyTypes);

            BaseSearch search = (BaseSearch)constructor.Invoke(null);
            return search;
        }
    }
}
