using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Common;

namespace QGF.Module
{
    class ModuleManager:Singleton<ModuleManager>
    {
        class MessageObject
        {
            public string msg;//消息名
            public object[] args;//参数
        }

        private Dictionary<string, GeneralModule> mAllModules;

        private Dictionary<string, List<MessageObject>> mCacheMessage;

        private List<IModuleActivator> mListModuleActivator;

        public ModuleManager()
        {
            mAllModules = new Dictionary<string, GeneralModule>();
            mCacheMessage = new Dictionary<string, List<MessageObject>>();
        }

        //初始化
        public void Init()
        {
            Debuger.Log();
        }


        public void Clear()
        {
            mCacheMessage.Clear();

            foreach(var module in mAllModules.Values)
            {
                module.Release();
            }

            mAllModules.Clear();
            mListModuleActivator.Clear();
        }

        //注册模块创建器
        public void RegistModuleActivator(IModuleActivator activator)
        {
            if (!mListModuleActivator.Contains(activator))
            {
                mListModuleActivator.Add(activator);
            }

        }

        public GeneralModule CreateModule(string name,object args = null)
        {
            Debuger.Log("name="+name+", args="+args);

            if (HasModule(name))
            {
                Debuger.LogError("module {0} has existed",name);
                return null;
            }

            GeneralModule module = null;
            //尝试所有的activator以创建该模块
            for(int i = 0; i < mListModuleActivator.Count; ++i)
            {
                module = mListModuleActivator[i].CreateInstance(name);
                if (module != null)
                {
                    break;
                }
            }
            //如果模块创建不成功
            if (module == null)
            {
                Debuger.LogError("instantiate module failed");
                return null;
            }

            mAllModules.Add(name, module);
            module.Create(args);

            //处理缓存消息
            if (mCacheMessage.ContainsKey(name))
            {
                List<MessageObject> ml = mCacheMessage[name];
                for(int i = 0; i < ml.Count; ++i)
                {
                    module.HangleMessage(ml[i].msg, ml[i].args);
                }
            }

            return module;
        }

        //判断模块是否已存在
        public bool HasModule(string name)
        {
            return mAllModules.ContainsKey(name);
        }

        //获取现有模块
        public GeneralModule GetModule(string name)
        {
            GeneralModule module = null;
            mAllModules.TryGetValue(name, out module);
            return module;
        }





        //*///////////////////////////////////////////////
        //发送消息
        public void SendMessage(string target,string msg,params object[] args)
        {
            GeneralModule module = GetModule(target);
            //如果存在了该模块
            if (module != null)
            {
                module.HangleMessage(msg, args);
            }
            else
            {
                var list = GetCacheMessageList(target);
                MessageObject obj = new MessageObject();
                obj.msg = msg;
                obj.args = args;
                list.Add(obj);
            }
        }

        //获取缓存的message
        private List<MessageObject> GetCacheMessageList(string target)
        {
            List<MessageObject> list = null;
            //不存在则创建该list
            if(!mCacheMessage.TryGetValue(target,out list))
            {
                list = new List<MessageObject>();
                mCacheMessage.Add(target, list);
            }
            return list;
        }


        //显示模块
        public void ShowModule(string target,object arg = null)
        {
            SendMessage(target, "Show", arg);
        }

    }
}
