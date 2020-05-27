using Checkmate.Game.Controller;
using Checkmate.Game.Skill;
using QGF;
using QGF.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Checkmate.Game.Skill.Skill;

namespace Checkmate.Game.Utils
{

    //环境变量
    public class EnvVariable
    {
        public ModelController Obj;//当前对象
        public ModelController Src;//来源
        public ModelController Dst;//目标
        public EffectController Main;//主体(技能、buff等)
        public Position Center=null;//中心（仅技能时发挥作用）
        public object Data;
        public int Damage;//伤害

        public void Copy(EnvVariable target)
        {
            Obj = target.Obj;
            Src = target.Src;
            Dst = target.Dst;
            Main = target.Main;
            Center = target.Center;
            Data = target.Data;
        }

        //public void ExecuteAction(SkillAction action)
        //{
        //    ExecuteTargets(action);
        //    foreach (var info in action.Executes)
        //    {
        //        ExecuteMain(info);
        //    }
        //}

        #region 执行函数
        public bool ExecuteChecks(SkillAction action)
        {
            //无条件限制默认返回true
            if (action.Checks == null)
            {
                return true;
            }
            return action.Checks.Execute();
        }

        public void ExecuteTargets(SkillAction action)
        {
            ControllerPool.Clear();
            foreach (var track in action.TargetTracks)
            {
                //执行搜索
                if (track.type == SkillAction.SearchType)
                {
                    BaseSearch search = action.Searches[track.idx];
                    List<ModelController> temp;
                    Position start, center;
                    if (TryGetSearchParam(search.start, out start) && TryGetSearchParam(search.center, out center))
                    {
                        temp = search.GetSearchResult(start, center);
                    }
                    //如果存在列表
                    else if (search.start.Contains('#') || search.center.Contains('#'))
                    {
                        temp = new List<ModelController>();
                        List<ModelController> controllers;
                        //如果相同，则取等值
                        if (search.start == search.center)
                        {

                            controllers = ControllerPool[search.start.Substring(1)];
                            foreach (var c in controllers)
                            {
                                temp.AddRange(search.GetSearchResult(c.GetPosition(), c.GetPosition()));
                            }
                        }
                        //起点为列表
                        else if (search.start.Contains('#'))
                        {
                            controllers = ControllerPool[search.start.Substring(1)];
                            TryGetSearchParam(search.center, out center);
                            foreach (var c in controllers)
                            {
                                temp.AddRange(search.GetSearchResult(c.GetPosition(), center));
                            }
                        }
                        //中心为列表
                        else
                        {
                            controllers = ControllerPool[search.center.Substring(1)];
                            TryGetSearchParam(search.start, out start);
                            foreach (var c in controllers)
                            {
                                temp.AddRange(search.GetSearchResult(start, c.GetPosition()));
                            }
                        }
                        temp = temp.Distinct().ToList();
                    }
                    else
                    {
                        Debug.LogError("error execute search:cannot build id:" + search.id);
                        return;
                    }
                    ControllerPool.Add(search.id, temp);
                }
                //执行筛选
                else
                {
                    Selects selects = action.Selectors[track.idx];
                    List<ModelController> src = ControllerPool[selects.src];
                    List<ModelController> temp = selects.ExecuteFilter(src);
                    ControllerPool.Add(selects.id, temp);
                }
            }
        }

        public void ExecuteMain(ExecuteInfo info)
        {
            object returnValue = null;
            object[] param = new object[info.parameters.Count];
            int flagList = -1;//标识ControllerList位置
            //解析非controllerList参数
            for (int i = 0; i < info.parameters.Count; ++i)
            {
                if (info.parameters[i].type == ParamType.ControllerList)
                {
                    flagList = i;
                    continue;
                }
                object p= GetParam(info.parameters[i].type, info.parameters[i].value);
                if (p == null)
                {
                    Debuger.LogError("skip execute {0}, as {1}:{2} missed", info.method, info.parameters[i].value, info.parameters[i].type);
                    return;
                }
                param[i] = p;
            }

            //检查有没有包含controllerList
            //如果有包含
            if (flagList != -1)
            {
                string cl = info.parameters[flagList].value;
                string tempVariable = null;
                //如果有.代表是变量
                if (cl.Contains('.'))
                {
                    tempVariable = cl.Substring(cl.IndexOf('.') + 1);
                    cl = cl.Substring(1, cl.IndexOf('.'));
                }
                List<ModelController> list = ControllerPool[cl.Substring(1)];
                foreach (ModelController l in list)
                {
                    object temp = l;
                    if (tempVariable != null)
                    {
                        temp = l.GetValue(tempVariable);
                    }
                    param[flagList] = temp;
                    returnValue = ExecuteUtil.Instance.Execute(info.method, param);
                }
            }
            else
            {
                returnValue = ExecuteUtil.Instance.Execute(info.method, param);
            }
            //处理返回值
            if (info.returnValue != null)
            {
                HandleReturn(info.returnValue, returnValue);
            }

        }
#endregion

        #region 基本功能函数
        private static Dictionary<string, List<ModelController>> ControllerPool = new Dictionary<string, List<ModelController>>();//所有skill共用的pool
        /// <summary>
        /// 获取搜索参数
        /// </summary>
        /// <param name="value">搜索起始</param>
        /// <param name="pos"></param>
        /// <returns>判断是否成功获取</returns>
        private bool TryGetSearchParam(string value, out Position pos)
        {
            switch (value)
            {
                case "Src":
                    {
                        ModelController c = Src as ModelController;
                        if (c != null)
                        {
                            pos = c.GetPosition();
                        }
                        else
                        {
                            pos = null;
                        }
                        return true;
                    }
                case "Center":
                    {
                        pos = Center;
                        return true;
                    }
            }
            pos = null;
            return false;
        }


        public object GetParam(ParamType type, string value)
        {
            //如果value是内部变量，直接获取
            if (value.Contains('$'))
            {
                return Main.GetValue(value.Substring(1));
            }
            //外部变量
            else if (value.Contains('%'))
            {
                //如果是获取controller
                if (type == ParamType.Controller)
                {
                    return GetController(value.Substring(1));
                }
                //获取变量
                if (value.Contains('.'))
                {
                    value = value.Substring(1);
                    string cn = value.Substring(0, value.IndexOf('.'));
                    string v = value.Substring(value.IndexOf('.') + 1);
                    BaseController controller = GetController(cn);
                    if (controller == null)
                    {
                        Debuger.LogError("error get controller:{0}, full:{1}", cn, value);
                    }
                    return controller.GetValue(v);
                }
                //否则直接转换
                else
                {

                }
            }
            //代表取列表的单个控制器
            else if (value.Contains('#') && type != ParamType.ControllerList)
            {
                string cname = value.Substring(1);
                string tempValue = null;

                if (value.Contains('.'))
                {
                    cname = value.Substring(1, value.IndexOf('.'));
                    tempValue = value.Substring(value.IndexOf('.') + 1);
                }

                int idx = 0;
                if (cname.Contains('['))
                {
                    int idx1 = cname.IndexOf('[');
                    int idx2 = idx1 + 1;
                    while (cname[idx2] != ']')
                    {
                        ++idx2;
                    }
                    string idxValue = cname.Substring(idx1 + 1, idx2 - idx1 - 1);
                    Debuger.Log("parse idx:{0}[{1}]", cname, idxValue);
                    idx = int.Parse(idxValue);
                    cname = cname.Substring(0, idx1);
                }
                
                List<ModelController> tempList = ControllerPool[cname];
                BaseController controller = null;
                if (tempList.Count > idx)
                {
                    controller = tempList[idx];
                }
                return tempValue == null ? controller : controller.GetValue(tempValue);

            }
            //代表直接解析
            else
            {
                switch (type)
                {
                    case ParamType.Int: return int.Parse(value);
                    case ParamType.Float: return float.Parse(value);
                    case ParamType.String: return value;
                    case ParamType.Bool:return bool.Parse(value);
                    case ParamType.Position:return Position.Parse(value);
                    default:Debuger.LogError("cannot get value:{0} of type:{1}", value, type);
                        break;
                }
            }
            Debug.LogError("error get param:" + value);
            return null;
        }

        private BaseController GetController(string value)
        {
            switch (value)
            {
                case "Src": return Src;
                case "Obj":
                    if (Obj == null)
                    {
                        Debuger.LogError("error get %Obj,null");
                    }
                    return Obj;
                case "Dst": return Dst;
                case "Main": return Main;
                case "Data":return Data as BaseController;
            }
            return null;
        }

        //
        private void HandleReturn(string target, object value)
        {
            //如果target是内部变量，直接设置
            if (target.Contains('$'))
            {
                Main.SetValue(target.Substring(1), value);
            }
            //外部变量
            else if (target.Contains('%'))
            {

            }
            //代表取列表的变量
            else if (target.Contains('#'))
            {
                string cname = target.Substring(1, target.IndexOf('.'));
                string tempValue = target.Substring(target.IndexOf('.') + 1);
                List<ModelController> controllers = ControllerPool[cname];

                foreach (var model in controllers)
                {
                    model.SetValue(tempValue, value);
                }

            }
        }

        #endregion
    }

    //执行的trck
    public class GameEnvTrack
    {
        public EnvVariable env;//脚本的环境变量
        public List<List<SkillAction>> actions;//所要执行的脚本
        public Action exe=null;

    }


    //管理游戏过程中的环境变量
    //即管理技能目标之类的变量
    public class GameEnv:Singleton<GameEnv>
    {
        public static int Damage;//当前伤害值
        private Stack<EnvVariable> mEnvStacks;//环境变量栈

        private Stack<EnvVariable> mExeEnvStacks;//执行时使用的环境栈
        public EnvVariable Current
        {
            get
            {
                return mEnvStacks.Peek();
            }
        }

        public EnvVariable CurrentExe
        {
            get
            {
                if (mExeEnvStacks.Count == 0)
                {
                    return null;
                }
                return mExeEnvStacks.Peek();
            }
        }
        
        public void Init()
        {
            mEnvStacks = new Stack<EnvVariable>();
            mExeEnvStacks = new Stack<EnvVariable>();
        }

        public void Clear()
        {
            Damage = 0;
            mEnvStacks.Clear();
            mExeEnvStacks.Clear();
        }

        public void PushEnv(EnvVariable value)
        {
            mEnvStacks.Push(value);
        }
        public void PushEnv(ModelController src,ModelController dst,EffectController main,object data = null)
        {
            EnvVariable variable = new EnvVariable();
            variable.Src = src;
            variable.Dst = dst;
            variable.Main = main;
            variable.Data = data;
            mEnvStacks.Push(variable);
        }

        public void PopEnv()
        {
            if (mEnvStacks.Count > 0)
            {
                mEnvStacks.Pop();
            }
        }


        public void PushExeEnv(EnvVariable value)
        {
            mExeEnvStacks.Push(value);
        }

        public void PopExeEnv()
        {
            if (mExeEnvStacks.Count > 0)
            {
                mExeEnvStacks.Pop();
            }
        }

    }

}
