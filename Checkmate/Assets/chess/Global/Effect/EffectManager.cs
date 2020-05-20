using Checkmate.Game.Controller;
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

        List<Effect> mAllEffects;//所有在使用的效果

        private List<int> mTimelyEffect;//所有需要定时执行的效果
        private List<CellController> mTimelyCell;//需要定时执行效果的方格

#if UNITY_EDITOR
        private string RootPath=Application.dataPath+"/Test/TestEffect";
#else
        private string RootPath = Application.streamingAssetsPath + "/Effects";
#endif
        //使用一个所有effect的列表进行初始化
        public bool Init(List<EffectData> effects)
        {
            mTimelyCell = new List<CellController>();
            mTimelyEffect = new List<int>();

            mLoadedEffects = new List<Effect>();
            mLoadedEffectFile = new List<string>();
            mAllEffects = new List<Effect>();
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
            mTimelyCell.Clear();
            mTimelyEffect.Clear();
        }

        //获取效果实例
        public int InstanceEffect(int idx,out Effect result)
        {
            Effect origin = mLoadedEffects[idx];

            //添加至实例列表
            int instanceId = mAllEffects.Count;
            Effect target = origin.Clone() as Effect;
            mAllEffects.Add(target);

            result = target;
            return instanceId;
        }

        //添加定时效果
        public void AddTimelyEffect(int id, CellController cell)
        {
            mTimelyEffect.Add(id);
            mTimelyCell.Add(cell);
        }

        public void ExecuteEffect(int id,CellController src,RoleController role=null,EffectTrigger trigger=EffectTrigger.Enter)
        {
            EnvVariable env = new EnvVariable();
            env.Src = env.Obj = src;
            env.Center = src.Position;
            if (role != null)
            {
                env.Dst = role;
            }
            else
            {
                if (src.HasRole)
                {
                    env.Dst = RoleManager.Instance.GetRole(src.Role);
                }
            }
            
            Effect target = mAllEffects[id];
            //可用则执行
            if (target.Available)
            {
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
                    CellController cell = mTimelyCell[i];
                    ExecuteEffect(mTimelyEffect[i],cell,null,EffectTrigger.Timely);
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
