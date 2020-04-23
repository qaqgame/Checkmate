using Checkmate.Game.Controller;
using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Game.Utils
{

    //环境变量
    public class EnvVariable
    {
        public BaseController Obj;//当前对象
        public BaseController Src;//来源
        public BaseController Dst;//目标
        public BaseController Main;//主体(技能、buff等)
        public object Data;
    }

    //管理游戏过程中的环境变量
    //即管理技能目标之类的变量
    public class GameEnv:Singleton<GameEnv>
    {
        private Stack<EnvVariable> mEnvStacks;//环境变量栈

        private Dictionary<string, List<BaseController>> mTempTargets;//临时搜索/筛选得到的对象
        
        public EnvVariable Current
        {
            get
            {
                return mEnvStacks.Peek();
            }
        }
        
        public void Init()
        {

        }

        public void Clear()
        {

        }

        public void PushEnv(BaseController src,BaseController dst,BaseController main,object data = null)
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
