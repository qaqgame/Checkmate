using Checkmate.Game.Controller;
using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Modules.Game
{

    //环境变量
    internal class EnvVariable
    {
        public BaseController Src;//来源
        public BaseController Dst;//目标
        public BaseController Main;//主体
        public object Data;
    }

    //管理游戏过程中的环境变量
    //即管理技能目标之类的变量
    public class GameEnv:Singleton<GameEnv>
    {
        private Stack<EnvVariable> mEnvStacks;//环境变量栈

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
