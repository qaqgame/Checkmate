using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Game.Role;
using Checkmate.Game.Utils;
using Checkmate.Global.Data;
using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace Checkmate.Game.Effect
{
    //该类用于管理所有的effect
    public class EffectManager:Singleton<EffectManager>
    {
        List<Effect> mLoadedEffects;//所有加载过后的效果
        List<string> mLoadedEffectFile;//加载的效果文件

        Dictionary<int,Effect> mAllEffects;//所有在使用的效果

        private List<int> mTimelyEffect;//所有需要定时执行的效果

        static int InstanceCnt = 0;
#if UNITY_EDITOR
        private string RootPath=Application.dataPath+"/Test/TestEffect";
#else
        private string RootPath = Application.streamingAssetsPath + "/Effects";
#endif
        //使用一个所有effect的列表进行初始化
        public bool Init(List<EffectData> effects)
        {
            mTimelyEffect = new List<int>();

            mLoadedEffects = new List<Effect>();
            mLoadedEffectFile = new List<string>();
            mAllEffects = new Dictionary<int, Effect>();
            if (effects != null)
            {
                foreach (var e in effects)
                {
                    //加载效果
                    //未包含则加载
                    if (!mLoadedEffectFile.Contains(e.effect))
                    {
                        Effect effect = LoadEffect(e.effect);
                        mLoadedEffectFile.Add(e.effect);
                        mLoadedEffects.Add(effect);
                    }
                }
            }
            return true;
        }

        public void Clear()
        {
            mLoadedEffectFile.Clear();
            mLoadedEffects.Clear();
            mAllEffects.Clear();
            mTimelyEffect.Clear();
        }

        //获取效果实例
        public int InstanceEffect(int idx,out Effect result,Position pos)
        {
            Effect origin = mLoadedEffects[idx];

            //添加至实例列表
            int instanceId = InstanceCnt++;
            Effect target = origin.Clone() as Effect;
            if (target != null) 
            {
                target.EffectId = instanceId;
                mAllEffects.Add(instanceId, target);
                target.Position = pos;
                if (target.Timely)
                {
                    AddTimelyEffect(instanceId);
                }
            }

            result = target;
            return instanceId;
        }

        public int AddEffect(Position pos,string effect)
        {
            if (!mLoadedEffectFile.Contains(effect))
            {
                Effect temp=LoadEffect(effect);
                mLoadedEffectFile.Add(effect);
                mLoadedEffects.Add(temp);
            }
            int idx = mLoadedEffectFile.IndexOf(effect);
            CellController cell = MapManager.Instance.GetCell(pos);
            if(cell!= null)
            {
                Effect result = null;
                int id = InstanceEffect(idx, out result,pos);
                if (result != null)
                {
                    if (cell.effects == null)
                    {
                        cell.effects = new List<int>();
                    }
                    cell.effects.Add(id);
                    ExecuteEffect(id, EffectTrigger.OnAttached);
                    return id;
                }
            }
            return -1;
        }

        public void RemoveEffect(int id)
        {
            if (!mAllEffects.ContainsKey(id))
            {
                return;
            }
            ExecuteEffect(id, EffectTrigger.OnRemoved);
            Effect temp = mAllEffects[id];
            if (temp.Timely)
            {
                RemoveTimelyEffect(id);
            }
            CellController cell = MapManager.Instance.GetCell(temp.Position);
            if (cell != null)
            {
                cell.effects.Remove(id);
            }
            mAllEffects.Remove(id);
            
        }


        //添加定时效果
        public void AddTimelyEffect(int id)
        {
            mTimelyEffect.Add(id);
        }

        //移除定时效果
        public void RemoveTimelyEffect(int id)
        {
            mTimelyEffect.Remove(id);
        }

        public void ExecuteEffect(int id,EffectTrigger trigger=EffectTrigger.Enter,RoleController role=null)
        {
            Effect target = mAllEffects[id];
            //可用则执行
            if (target!=null&&target.Available)
            {
                EnvVariable env = new EnvVariable();
                CellController cell = MapManager.Instance.GetCell(target.Position);
                env.Src = env.Obj = cell;
                env.Center = target.Position;

                if (role == null)
                {
                    RoleController temp = cell != null && cell.HasRole ? RoleManager.Instance.GetRole(cell.Role) : null;
                    env.Dst = temp;
                }
                else
                {
                    env.Dst = role;
                }

                env.Main = target;
                GameEnv.Instance.PushEnv(env);
                target.Execute(trigger);
                GameEnv.Instance.PopEnv();
            }
        }

        public Effect GetEffect(int id)
        {
            return mAllEffects[id];
        }

        //回合开始执行地面效果
        public void NextTurn()
        {
            for(int i = 0; i < mTimelyEffect.Count; ++i)
            {
                Effect e = mAllEffects[mTimelyEffect[i]];
                e.CurTurn++;
                if (e.Available)
                {
                    ExecuteEffect(mTimelyEffect[i],EffectTrigger.Timely);
                }
            }

            //遍历所有effect，减少其life，如果为0，则移除
            for(int i = mAllEffects.Values.Count-1; i >=0 ;i--)
            {
                Effect e = mAllEffects.Values.ElementAt(i);
                if (e.Life != -1)
                {
                    e.Life--;
                    if (e.Life == 0)
                    {
                        RemoveEffect(e.EffectId);
                    }
                }
            }
        }


        private Effect LoadEffect(string path)
        {
            string fullPath = RootPath +"/"+ path+".xml";

            XmlDocument document = new XmlDocument();
            document.Load(fullPath);

            XmlNode root = document.DocumentElement;

            Effect result = EffectParser.ParseEffect(root);
            return result;
        }
    }

}
