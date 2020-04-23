using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace Checkmate.Game.Utils
{
    //范围接口（标识一个范围）
    public interface IRange
    {
        //解析方式
        void Parse(XmlNode node);
        bool IsSingle();
        /// <summary>
        /// 获取一个范围内的所有position
        /// </summary>
        /// <param name="start">选择的起点</param>
        /// <param name="center">选择的中心</param>
        /// <returns></returns>
        List<Position> GetResult(Position start, Position center);
    }

    //默认的范围类
    public class DefaultRange : IRange
    {
        enum RangeType
        {
            Line,
            Range,
            Single
        }

        private RangeType type;
        private int limit;

        public bool IsSingle()
        {
            return type == RangeType.Single;
        }

        public void Parse(XmlNode root)
        {
            //解析范围类型
            type = (RangeType)System.Enum.Parse(typeof(RangeType), root.Attributes["type"].Value);
            if (type != RangeType.Single)
            {
                string l = root.Attributes["limit"].Value;
                //解析选取的范围
                l = root.Attributes["limit"].Value;
                //解析range
                if (type == RangeType.Range)
                {
                    limit = int.Parse(l);
                }
                //解析line
                else if (type == RangeType.Line)
                {
                    //以center为结束点
                    if (l == "Center")
                    {
                        limit = -1;
                    }
                    //限制最长长度
                    else
                    {
                        limit = int.Parse(l);
                    }
                }
            }
        }

        //获取结果
        public List<Position> GetResult(Position start, Position center)
        {
            List<Position> result = new List<Position>();
            switch (type)
            {
                case RangeType.Line:
                    {
                        if (type != RangeType.Line)
                        {
                            throw new System.Exception("error get line positions, type:" + type);
                        }
                        return GetLineResult(start, center);
                    }
                case RangeType.Range:
                    {
                        if (type != RangeType.Range)
                        {
                            throw new System.Exception("error get range positions, type:" + type);
                        }
                        return GetRangeResult(center);
                    }
                case RangeType.Single:
                    {
                        if (type != RangeType.Single)
                        {
                            throw new System.Exception("error get single positions, type:" + type);
                        }
                        result.Add(center);
                        return result;
                    }
                default:
                    {
                        Debug.LogError("error get positions in range with unknown type:" + type);
                        return result;
                    }
            }
        }

        private List<Position> GetLineResult(Position start, Position center)
        {
            int range = limit;
            if (limit == -1)
            {
                range = HexMapUtil.GetDistance(start, center);
            }
            return HexMapUtil.GetLine(start, center, range);
        }

        private List<Position> GetRangeResult(Position center)
        {
            return HexMapUtil.GetRange(center, limit);
        }
    }

    //范围的解析类
    public static class RangeParser
    {
        static string RangeNameSpace = "Checkmate.Game.Utils.";
        public static IRange ParseRange(XmlNode root)
        {
            //获取类名
            System.Type tp = System.Type.GetType(RangeNameSpace+root.Name);
            ConstructorInfo constructor = tp.GetConstructor(System.Type.EmptyTypes);

            IRange range = (IRange)constructor.Invoke(null);
            range.Parse(root);

            return range;
        }
    }
}
