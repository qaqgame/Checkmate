using Checkmate.Game.Skill;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Checkmate.Game.Utils
{
    public static class ObjectParser
    {
        public static string GetExtraDataContent(string name,string path)
        {
            string fullPath = SkillManager.Instance.RootPath + "/" + name + "/" + path;
            return File.ReadAllText(fullPath);
        }

        public static object ParseObject(XmlNode node, out string name)
        {
            name = node.Attributes["name"].Value;

            string type = node.Attributes["type"].Value;
            string value = node.InnerText;

            switch (type)
            {
                case "Int": return int.Parse(value);
                case "Float": return float.Parse(value);
                case "String": return value;
            }
            return null;
        }


        public static List<T> ParseEnums<T>(string content) where T : Enum
        {
            List<T> result = new List<T>();
            if (content == null)
            {
                return result;
            }
            if (content.Contains('|'))
            {
                string[] temp = content.Split('|');
                foreach (var type in temp)
                {
                    T ct = (T)Enum.Parse(typeof(T), type);
                    result.Add(ct);
                }
            }
            else
            {
                T ct = (T)Enum.Parse(typeof(T), content);
                result.Add(ct);
            }
            return result;
        }
    }
}
