using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace QGF.Module
{
    public class GeneralModule : ModuleBase, ILogTag
    {
        private string mName = "";//模块名

        public string Name { get { return mName; } }

        public string Title;//显示名

        public GeneralModule()
        {
            mName = this.GetType().Name;
            LOG_TAG = mName;
        }

        //创建模块
        public virtual void Create(object args = null)
        {
            this.Log("args {0}", args);
        }

        public override void Release()
        {
            base.Release();
            this.Log();
        }

        //收到消息后，进行处理(通过反射来调用函数）
        internal void HangleMessage(string msg, object[] args)
        {
            this.Log("msg:{0}, args: {1}", msg, args);

            var mt=this.GetType().GetMethod(msg, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (mt != null)
            {
                mt.Invoke(this, BindingFlags.NonPublic, null, args, null);
            }
            //未找到该消息对应的方法时，由自定义的处理方法处理
            else
            {
                OnModuleMessage(msg, args);
            }
        }

        //由派生类实现以自定义处理消息
        protected virtual void OnModuleMessage(string msg,object[] args)
        {
            this.Log("msg:{0},args:{1} ", msg, args);
        }


        protected virtual void Show(object arg)
        {
            this.Log("args:{0}", arg);
        }

        public string LOG_TAG { get; protected set; }
       
    }
}
