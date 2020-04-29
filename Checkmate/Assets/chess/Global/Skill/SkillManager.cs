using Checkmate.Game.Controller;
using Checkmate.Game.Utils;
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

        public void ExecuteSkill(int id,RoleController src,Position center)
        {
            if (!mSkills.ContainsKey(id))
            {
                return;
            }
            //设置环境
            EnvVariable env = new EnvVariable();
            env.Src = src;
            env.Obj = src;
            env.Dst = null;
            env.Center = center;
            env.Main = mSkills[id];
            env.Data = null;
            GameEnv.Instance.PushEnv(env);
            //执行
            mSkills[id].OnExecute();
            //清除环境
            GameEnv.Instance.Pop();
        }

        //获取效果的范围
        public List<Position> GetEffectRange(int id,Position start,Position center)
        {
            if (!mSkills.ContainsKey(id))
            {
                return null;
            }
            return mSkills[id].GetEffectPositions(start, center);
        }

        //获取边界
        public List<Position> GetBorderRange(int id,Position start)
        {
            if (!mSkills.ContainsKey(id))
            {
                return null;
            }
            return mSkills[id].GetMousePositions(start);
        }
    }
}
