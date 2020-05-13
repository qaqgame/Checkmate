using Checkmate.Game.Buff;
using Checkmate.Game.Controller;
using Checkmate.Game.Role;
using Checkmate.Game.Utils;
using Checkmate.Global.Data;
using ProtoBuf;
using QGF.Codec;
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
            //执行该src的onSkill
            BuffManager.Instance.ExecuteWithEnv(TriggerType.OnSkill, src);
            //执行
            mSkills[id].OnExecute();
            //添加结束时操作（重置该角色状态)
            GameExecuteManager.Instance.Add(() => { src.SetState(RoleState.Idle); });
            //清除环境
            GameEnv.Instance.PopEnv();
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


        //=========================
        [ProtoContract]
        internal class SkillMessage
        {
            [ProtoMember(1)]
            public int skillId;//技能id

            [ProtoMember(2)]
            public int roleId;//角色id

            [ProtoMember(3)]
            public v3i center;//施放位置
        }

        public void Execute(byte[] msg)
        {
            SkillMessage message = PBSerializer.NDeserialize<SkillMessage>(msg);
            RoleController role = RoleManager.Instance.GetRole(message.roleId);
            Position center = new Position(message.center.x, message.center.y, message.center.z);

            ExecuteSkill(message.skillId, role, center);
        }

    }
}
