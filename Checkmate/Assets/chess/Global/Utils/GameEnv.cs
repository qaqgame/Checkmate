using Checkmate.Game.Controller;
using Checkmate.Game.Skill;
using QGF.Common;
using System;
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
        public BaseController Main;//主体(技能、buff等)
        public Position Center=null;//中心（仅技能时发挥作用）
        public object Data;

        public void ExecuteAction(SkillAction action)
        {
            ControllerPool.Clear();
            ExecuteTargets(action);
            foreach (var info in action.Executes)
            {
                ExecuteMain(info);
            }
        }

        #region 执行函数
        private void ExecuteTargets(SkillAction action)
        {
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

        private void ExecuteMain(ExecuteInfo info)
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
                param[i] = GetParam(info.parameters[i].type, info.parameters[i].value);

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
        /// <param name="value"></param>
        /// <param name="pos"></param>
        /// <returns>判断是否是内置变量</returns>
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
                        pos = null;
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


        private object GetParam(ParamType type, string value)
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
                string cn = value.Substring(1, value.IndexOf('.'));
                string v = value.Substring(value.IndexOf('.') + 1);
                BaseController controller = GetController(cn);
                return controller.GetValue(v);
            }
            //代表取列表的单个控制器
            else if (value.Contains('#') && type == ParamType.Controller)
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
                    idx = int.Parse(cname.Substring(idx + 1, idx2 - idx1 - 1));
                    cname = cname.Substring(0, idx1);
                }
                
                BaseController controller = ControllerPool[cname][idx];
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
                case "Dst": return Dst;
                case "Main": return Main;
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

    //管理游戏过程中的环境变量
    //即管理技能目标之类的变量
    public class GameEnv:Singleton<GameEnv>
    {
        private Stack<EnvVariable> mEnvStacks;//环境变量栈

        //private Dictionary<string, List<BaseController>> mTempTargets;//临时搜索/筛选得到的对象
        
        public EnvVariable Current
        {
            get
            {
                return mEnvStacks.Peek();
            }
        }
        
        public void Init()
        {
            mEnvStacks = new Stack<EnvVariable>();
        }

        public void Clear()
        {

        }

        public void PushEnv(EnvVariable value)
        {
            mEnvStacks.Push(value);
        }
        public void PushEnv(ModelController src,ModelController dst,BaseController main,object data = null)
        {
            EnvVariable variable = new EnvVariable();
            variable.Src = src;
            variable.Dst = dst;
            variable.Main = main;
            variable.Data = data;
            mEnvStacks.Push(variable);
        }

        public void Pop()
        {
            if (mEnvStacks.Count > 0)
            {
                mEnvStacks.Pop();
            }
        }

    }
}
