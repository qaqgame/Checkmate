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
    public class EffectTrack
    {
        public EffectTrigger Trigger;//触发方式
        public int Interval;//间隔回合
        public int Cur;//当前回合
        public int Id;//id
    }
    //该类用于管理所有的effect
    public class EffectManager:Singleton<EffectManager>
    {
        List<Effect> mLoadedEffects;//所有加载过后的效果
        List<string> mLoadedEffectFile;//加载的效果文件
        List<EffectTrack> mLoadedTrack;//加载的效果参数

        List<Effect> mAllEffects;//所有在使用的效果

        private List<EffectTrack> mTimelyEffect;//所有需要定时执行的效果
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
            mTimelyEffect = new List<EffectTrack>();

            mLoadedEffects = new List<Effect>();
            mLoadedEffectFile = new List<string>();
            mLoadedTrack = new List<EffectTrack>();
            mAllEffects = new List<Effect>();
            if (effects != null)
            {
                foreach (var e in effects)
                {
                    EffectTrack track = new EffectTrack();
                    track.Cur = track.Interval = e.cd;
                    track.Trigger = (EffectTrigger)Enum.Parse(typeof(EffectTrigger),e.trigger);

                    //加载效果
                    //未包含则加载
                    if (!mLoadedEffectFile.Contains(e.effect))
                    {
                        Effect effect = LoadEffect(e.effect);
                        mLoadedEffectFile.Add(e.effect);
                        mLoadedEffects.Add(effect);
                    }
                    //找到对应索引
                    int idx = mLoadedEffectFile.IndexOf(e.effect);
                    track.Id = idx;

                    mLoadedTrack.Add(track);
                }
            }
            return true;
        }

        //获取效果实例
        public EffectTrack InstanceEffect(int idx)
        {
            EffectTrack track = mLoadedTrack[idx];
            Effect origin = mLoadedEffects[track.Id];

            EffectTrack result = new EffectTrack();
            result.Cur = track.Cur;
            result.Interval = track.Interval;
            result.Trigger = track.Trigger;

            //添加至实例列表
            int instanceId = mAllEffects.Count;
            Effect target = origin.Clone() as Effect;
            mAllEffects.Add(target);

            result.Id = instanceId;

            

            return result;
        }

        //添加定时效果
        public void AddTimelyEffect(EffectTrack track, CellController cell)
        {
            mTimelyEffect.Add(track);
            mTimelyCell.Add(cell);
        }

        public void ExecuteEffect(int id,CellController src,RoleController role=null)
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
            env.Main = target;
            GameEnv.Instance.PushEnv(env);
            target.Execute();
            GameEnv.Instance.PopEnv();
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
                mTimelyEffect[i].Cur++;
                if (mTimelyEffect[i].Cur >= mTimelyEffect[i].Interval)
                {
                    int id = mTimelyEffect[i].Id;
                    CellController cell = mTimelyCell[i];
                    ExecuteEffect(id,cell);
                    mTimelyEffect[i].Cur = 0;
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
