using Checkmate.Game.Buff;
using Checkmate.Game.Controller;
using Checkmate.Game.Global.Utils;
using Checkmate.Game.Map;
using Checkmate.Game.Role;
using Checkmate.Game.Utils;
using Checkmate.Global.Data;
using ProtoBuf;
using QGF;
using QGF.Codec;
using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

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
            mSkills.Clear();
        }

        public int GetSkill(string name)
        {
            XmlDocument document = new XmlDocument();

            string fullPath = RootPath + "/" + name + "/" + name + ".xml";

            document.Load(fullPath);
            XmlNode node = document.DocumentElement;

            BaseSkill skill = SkillParser.ParseSkill(node);
            if (skill.Icon != null)
            {
                IconManager.Instance.Load(skill.Icon);
            }
            int id = mSkills.Count;
            mSkills.Add(id, skill);
            return id;
        }

        public BaseSkill GetInstance(int id)
        {
            if (!mSkills.ContainsKey(id))
            {
                return null;
            }
            return mSkills[id];
        }

        public Texture2D GetIcon(int id)
        {
            BaseSkill skill = mSkills[id];
            if (skill == null || skill.Icon == null)
            {
                return null;
            }
            return IconManager.Instance.GetIcon(skill.Icon);
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
            GameExecuteManager.Instance.Add(() => { src.SetState(RoleState.Idle);mSkills[id].Current.Clear(); });
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

        //获取消耗
        public int GetCost(int id)
        {
            if (!mSkills.ContainsKey(id))
            {
                return 0;
            }
            return mSkills[id].Cost;
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
            BaseSkill skill = mSkills[message.skillId];
            Debuger.Log("{0} execute skill {1} in {2}", role.Name, skill.Name, center.ToString());
            ExecuteSkill(message.skillId, role, center);
        }

        //===============================
        [ProtoContract]
        internal class AttackMessage
        {
            [ProtoMember(1)]
            public int srcId;//源id
            [ProtoMember(2)]
            public v3i center;//位置
        }

        public void ExecuteAttack(byte[] msg)
        {
            AttackMessage message = PBSerializer.NDeserialize<AttackMessage>(msg);
            RoleController role = RoleManager.Instance.GetRole(message.srcId);
            Position center = new Position(message.center.x, message.center.y, message.center.z);
            int dstId = MapManager.Instance.GetCell(center).Role;
            if (dstId == -1)
            {
                Debuger.LogError("error get attack target role in {0}", center.ToString());
            }
            RoleController target = RoleManager.Instance.GetRole(dstId);
            Debuger.Log("execute attack:{0} to {1} ,target position:{2}", role.Name, target.Name, target.Position.ToString());
            //设置环境
            EnvVariable env = new EnvVariable();
            env.Src = role;
            env.Obj = role;
            env.Dst = target;
            env.Center = center;
            env.Main = null;
            env.Data = null;
            GameEnv.Instance.PushEnv(env);
            GameExecuteManager.Instance.Add(role.AtkAction);
            //添加结束时操作（重置该角色状态)
            GameExecuteManager.Instance.Add(() => { role.SetState(RoleState.Idle); });
            GameEnv.Instance.PopEnv();
        }
    }
}
