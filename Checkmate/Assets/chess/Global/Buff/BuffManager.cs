using Checkmate.Game.Controller;
using Checkmate.Game.Global.Utils;
using Checkmate.Game.Utils;
using QGF;
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
        private string IconRootPath= Application.dataPath + "/Test/Icons";
#else
        private string RootPath = Application.streamingAssetsPath + "/Buffs";
        private string IconRootPath= Application.streamingAssetsPath + "/Icons";
#endif

        private Dictionary<string, Buff> mLoadedBuff;//buff的加载缓存
        private Dictionary<int, Buff> mBuffInstances;//buff的实例缓存

        static int cnt = 0;
        public void Init()
        {
            mLoadedBuff = new Dictionary<string, Buff>();
            mBuffInstances = new Dictionary<int, Buff>();
        }

        public void Clear()
        {
            mLoadedBuff.Clear();
            mBuffInstances.Clear();
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
            int id = cnt++;
            result.BuffId = id;
            mBuffInstances.Add(id, result);
            return id;
        }

        public Buff RemoveBuff(int id)
        {
            if (!mBuffInstances.ContainsKey(id))
            {
                return null;
            }
            Buff buff = mBuffInstances[id];
            mBuffInstances.Remove(id);
            return buff;
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
            //如果存在该buff的icon
            if (result.Icon != null)
            {
                IconManager.Instance.Load(result.Icon);
            }
            return result;
        }

        /// <summary>
        /// 根据buffid获取其图标
        /// </summary>
        /// <param name="bid">buff id</param>
        /// <returns></returns>
        public Texture2D GetBuffIcon(int bid)
        {
            Buff buff = mBuffInstances[bid];
            if (buff == null || buff.Icon == null)
            {
                return null;
            }
            return IconManager.Instance.GetIcon(buff.Icon);
        }

        /// <summary>
        /// 到了新的回合的调用,更新buff回合数，并调用OnUpdate
        /// </summary>
        public void NextTurn()
        {
            foreach(var buff in mBuffInstances.Values)
            {
                if (buff.IsInfiniteTurn || buff.ReserveTurn > 0)
                {
                    EnvVariable env = new EnvVariable();
                    env.Center = buff.Obj.Position;
                    env.Main = buff;
                    env.Src = buff.Src;
                    env.Obj = buff.Obj;
                    env.Dst = null;

                    GameEnv.Instance.PushEnv(env);
                    buff.Execute(TriggerType.OnTurn);
                }
                if (!buff.IsInfiniteTurn)
                {
                    buff.ReserveTurn--;
                }
            }
        }
    }
}
