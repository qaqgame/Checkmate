using Checkmate.Game.Controller;
using Checkmate.Game.Utils;
using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace Checkmate.Game.Buff
{
    public class BuffManager:Singleton<BuffManager>
    {

#if UNITY_EDITOR
        private string RootPath=Application.dataPath+"/Test/TestBuff";
#else
        private string RootPath = Application.streamingAssetsPath + "/Buffs";
#endif

        private Dictionary<string, Buff> mLoadedBuff;//buff的加载缓存
        private Dictionary<int, Buff> mBuffInstances;//buff的实例缓存
        
        public void Init()
        {
            mLoadedBuff = new Dictionary<string, Buff>();
            mBuffInstances = new Dictionary<int, Buff>();
        }

        public Buff GetBuff(int id)
        {
            if (!mBuffInstances.ContainsKey(id))
            {
                return null;
            }
            return mBuffInstances[id];
        }

        /// <summary>
        /// 实例化buff
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public int InstanceBuff(string file)
        {
            Buff result;
            //如果未加载，则加载
            if (!mLoadedBuff.ContainsKey(file)) 
            { 
                Buff temp = LoadBuff(file);
                mLoadedBuff.Add(file, temp);

            }
            result = mLoadedBuff[file].Clone() as Buff;
            int id = mBuffInstances.Count;
            result.BuffId = id;
            mBuffInstances.Add(id, result);
            return id;
        }
        /// <summary>
        /// 执行buff，并自动设置环境
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="role"></param>
        public void ExecuteWithEnv(TriggerType trigger,RoleController role,object data=null)
        {
            foreach (var buffId in role.Buffs)
            {
                Buff buff = mBuffInstances[buffId];
                EnvVariable env = new EnvVariable();
                env.Copy(GameEnv.Instance.Current);
                env.Center = role.Position;
                env.Main = buff;
                env.Data = data;

                GameEnv.Instance.PushEnv(env);
                buff.Execute(trigger);
                GameEnv.Instance.PopEnv();
            }
        }

        //执行controller的buff(不设置环境)
        public void Execute(TriggerType trigger,RoleController controller)
        {
            foreach(var buffId in controller.Buffs)
            {
                Buff buff = mBuffInstances[buffId];
                buff.Execute(trigger);
            }
        }

        private Buff LoadBuff(string file)
        {
            string fullPath = RootPath + "/" + file + ".xml";

            XmlDocument document = new XmlDocument();
            document.Load(fullPath);

            XmlNode root = document.DocumentElement;

            Buff result = BuffParser.ParseBuff(root);
            return result;
        }
    }
}
