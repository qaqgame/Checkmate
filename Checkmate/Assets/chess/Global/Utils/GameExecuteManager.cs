using QGF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Game.Utils
{
    //执行部分
    public class GameExecuteManager : MonoBehaviour
    {
        public static GameExecuteManager Instance;

        private Queue<GameEnvTrack> mExeTracks;//所有要执行的

        private GameEnvTrack mCurrent;//当前执行的
        private bool wait = false;//是否处于等待状态

        public delegate bool WaitAction();
        public void Add(GameEnvTrack track)
        {
            mExeTracks.Enqueue(track);
        }
        public void Add(EnvVariable env, params List<SkillAction>[] actions)
        {
            GameEnvTrack track = new GameEnvTrack();
            track.env = env;
            List<List<SkillAction>> temp = new List<List<SkillAction>>();
            foreach (var list in actions)
            {
                temp.Add(list);
            }
            track.actions = temp;
            mExeTracks.Enqueue(track);
        }

        public void Add(params List<SkillAction>[] actions)
        {
            GameEnvTrack track = new GameEnvTrack();
            EnvVariable env = new EnvVariable();
            env.Copy(GameEnv.Instance.Current);
            track.env = env;
            List<List<SkillAction>> temp = new List<List<SkillAction>>();
            Debuger.Log("added {0} actions", actions.Length);
            foreach (var list in actions)
            {
                temp.Add(list);
            }
            track.actions = temp;
            mExeTracks.Enqueue(track);
        }

        private void Awake()
        {
            Instance = this;
            mExeTracks = new Queue<GameEnvTrack>();
        }

        private void Start()
        {
            StartCoroutine(Execute());
        }

        public void Wait(WaitAction action)
        {
            StartCoroutine(WaitFor(action));
        }

        public void Wait(float second)
        {
            StartCoroutine(WaitSecond(second));
        }


        IEnumerator WaitSecond(float s)
        {
            wait = true;
            yield return new WaitForSeconds(s);
            wait = false;
        }

        IEnumerator WaitFor(WaitAction action)
        {
            wait = true;
            yield return action();
            wait = false;
        }

        IEnumerable Wait()
        {
            while (wait)
            {
                yield return null;
            }
        }

        //主执行函数，在循环中不断执行当前的项
        IEnumerator Execute()
        {
            while (true)
            {
                //如果当前未执行
                if (mExeTracks.Count > 0) 
                {
                    mCurrent = mExeTracks.Dequeue();
                    GameEnv.Instance.PushExeEnv(mCurrent.env);
                    //执行所有的action
                    for (int i = 0; i < mCurrent.actions.Count; ++i)
                    {
                        var list = mCurrent.actions[i];
                        Debuger.Log("execute {0} times", i.ToString());
                        //执行一个列表的action
                        for (int j=0;j<list.Count;++j) 
                        {
                            var action = list[j];
                            GameEnv.Instance.CurrentExe.ExecuteAction(action);
                            Debuger.Log("execute action:{0} for {1} times", action.Executes[0].method,j.ToString());
                            //无等待项则继续
                            yield return Wait();
                        }
                        //清空temp属性变动
                        if (GameEnv.Instance.CurrentExe.Main != null)
                        {
                            DataMap temp = GameEnv.Instance.CurrentExe.Main.Temp;
                            //清除所有涉及对象的属性加成
                            foreach (var role in temp.mUsedRoles)
                            {
                                role.TempMap.RemoveTrack(temp);
                            }
                            temp.Clear();
                        }
                    }
                    //弹出环境
                    GameEnv.Instance.PopExeEnv();
                }
                yield return null;
            }
        }
    }
}
