using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
namespace Checkmate.Game.Skill
{
    public class SkillManager:Singleton<SkillManager>
    {
        private Dictionary<int, BaseSkill> mSkills;//所有技能
        public string RootPath;//根路径
        public void Init(string path)
        {
            mSkills = new Dictionary<int, BaseSkill>();
            RootPath = path;
        }

        public void Clear()
        {

        }

        public int GetSkill(string name)
        {
            XmlDocument document = new XmlDocument();

            string fullPath = RootPath + "/" + name + "/" + name + ".xml";

            document.Load(fullPath);
            XmlNode node = document.DocumentElement;

            BaseSkill skill = SkillParser.ParseSkill(node);
            int id = mSkills.Count;
            mSkills.Add(id, skill);
            return id;
        }

        
    }
}
