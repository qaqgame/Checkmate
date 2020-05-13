using Checkmate.Game.Player;
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

        //有待执行项
        public bool WaitForExecute
        {
            get
            {
                return (mTracks != null && mTracks[0].Count > 0)||wait;
            }
        }

        private Queue<GameEnvTrack> mExeTracks;//所有要执行的

        private GameEnvTrack mCurrent;//当前执行的
        private bool wait = false;//是否处于等待状态

        private List<Queue<GameEnvTrack>> mTracks;//所有待执行
        private int mCurrentRec=-1;//当前递归层数
        public delegate bool WaitAction();
        public void Add(GameEnvTrack track)
        {
            Debuger.Log("mCurrentRec: {0}, mName: {1}: ", mCurrentRec, track.exe == null ? "myaction" : track.exe.Method.Name);
            //如果当前递归层数大于待执行栈
            if (mCurrentRec+1>=mTracks.Count)
            {
                //则新加一层
                Queue<GameEnvTrack> queue = new Queue<GameEnvTrack>();
                mTracks.Add(queue);
            }
            mTracks[mCurrentRec+1].Enqueue(track);
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
            Add(track);
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
            Add(track);
        }

        public void Add(Action action)
        {
            GameEnvTrack track = new GameEnvTrack();
            EnvVariable env = new EnvVariable();
            env.Copy(GameEnv.Instance.Current);
            track.env = env;
            track.exe = action;
            Add(track);
        }

        private void Awake()
        {
            Instance = this;
            mExeTracks = new Queue<GameEnvTrack>();
            mTracks = new List<Queue<GameEnvTrack>>();
            mTracks.Add(new Queue<GameEnvTrack>());
        }

        private void Start()
        {
            StartCoroutine(Execute());
        }

        public void Wait(WaitAction action,Action onFinish=null)
        {
            wait = true;
            StartCoroutine(WaitFor(action,onFinish));
        }

        public void Wait(float second,Action onFinish=null)
        {
            wait = true;
            StartCoroutine(WaitSecond(second,onFinish));
        }



        IEnumerator WaitSecond(float s,Action onFinish=null)
        {
            yield return new WaitForSeconds(s);
            if (onFinish != null)
            {
                onFinish();
            }
            wait = false;
        }

        IEnumerator WaitFor(WaitAction action,Action onFinish=null)
        {
            while (!action())
            {
                Debuger.Log("update wait action");
                yield return null;
            }

            if (onFinish != null)
            {
                onFinish();
            }
            wait = false;
            Debuger.Log("wait finished");
        }

        IEnumerator Wait()
        {
            while (wait)
            {
                yield return null;
            }
        }

        IEnumerator ExecuteTrack(int rec)
        {
            mCurrentRec = rec;
            Debuger.Log("mmCurrentRec: {0}, count: {1}", mCurrentRec, mTracks.Count > rec ? mTracks[rec].Count.ToString() : "null");
            //如果当前未到最高层且存在
            if (mTracks.Count > rec&&mTracks[rec].Count>0)
            {
                mCurrent = null;
                //如果当前层存在未执行未执行
                while (mTracks[rec].Count > 0)
                {
                    mCurrent = mTracks[rec].Dequeue();
                    PlayerManager.Instance.IsWaiting = true;
                    GameEnv.Instance.PushExeEnv(mCurrent.env);
                    if (mCurrent.exe != null)
                    {
                        Debuger.Log("executed: {0}", mCurrent.exe.Method.Name);
                        mCurrent.exe.Invoke();
                        //等待子项完成
                        yield return StartCoroutine(ExecuteTrack(rec + 1));
                        mCurrentRec = rec;
                        //无等待项则继续
                        yield return StartCoroutine(Wait());
                    }
                    else
                    {
                        //执行所有的action
                        List<List<SkillAction>> actions = mCurrent.actions;
                        for (int i = 0; i < actions.Count; ++i)
                        {
                            var list = actions[i];
                            Debuger.Log("execute {0} times", i.ToString());
                            //执行一个列表的action
                            for (int j = 0; j < list.Count; ++j)
                            {
                                var action = list[j];
                                //如果满足条件则执行
                                if (GameEnv.Instance.CurrentExe.ExecuteChecks(action))
                                {
                                    GameEnv.Instance.CurrentExe.ExecuteTargets(action);
                                    foreach (var exe in action.Executes)
                                    {
                                        GameEnv.Instance.CurrentExe.ExecuteMain(exe);
                                        Debuger.Log("execute action:{0}, rec: {1}", exe, rec);
                                        //等待子项完成
                                        yield return StartCoroutine(ExecuteTrack(rec + 1));
                                        mCurrentRec = rec;
                                        //无等待项则继续
                                        yield return StartCoroutine(Wait());
                                    }
                                }
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
                    }
                    //弹出环境
                    GameEnv.Instance.PopExeEnv();
                }
                
            }
            mCurrentRec = rec - 1;
            Debuger.Log("mmrec end: {0}", rec);
        }

        //主执行函数，在循环中不断执行当前的项
        IEnumerator Execute()
        {
            while (true)
            {
                //mCurrent = null;
                ////如果当前未执行
                //if(mExeTracks.Count > 0) 
                //{
                //    mCurrent = mExeTracks.Dequeue();
                //    PlayerManager.Instance.IsWaiting = true;
                //    GameEnv.Instance.PushExeEnv(mCurrent.env);
                //    //执行所有的action
                //    for (int i = 0; i < mCurrent.actions.Count; ++i)
                //    {
                //        var list = mCurrent.actions[i];
                //        Debuger.Log("execute {0} times", i.ToString());
                //        //执行一个列表的action
                //        for (int j=0;j<list.Count;++j) 
                //        {
                //            var action = list[j];
                //            //如果满足条件则执行
                //            if (GameEnv.Instance.CurrentExe.ExecuteChecks(action))
                //            {
                //                GameEnv.Instance.CurrentExe.ExecuteTargets(action);
                //                foreach (var exe in action.Executes)
                //                {
                //                    GameEnv.Instance.CurrentExe.ExecuteMain(exe);
                //                    Debuger.Log("execute action:{0}", exe);
                //                    //无等待项则继续
                //                    yield return StartCoroutine(Wait());
                //                }
                //            }
                //        }
                //        //清空temp属性变动
                //        if (GameEnv.Instance.CurrentExe.Main != null)
                //        {
                //            DataMap temp = GameEnv.Instance.CurrentExe.Main.Temp;
                //            //清除所有涉及对象的属性加成
                //            foreach (var role in temp.mUsedRoles)
                //            {
                //                role.TempMap.RemoveTrack(temp);
                //            }
                //            temp.Clear();
                //        }
                //    }
                //    //弹出环境
                //    GameEnv.Instance.PopExeEnv();
                //}
                if (mTracks.Count > 0 && mTracks[0].Count > 0)
                {
                    yield return StartCoroutine(ExecuteTrack(0));
                }
                mCurrentRec = -1;
                PlayerManager.Instance.IsWaiting = false;
                yield return null;
            }
        }
    }
}
