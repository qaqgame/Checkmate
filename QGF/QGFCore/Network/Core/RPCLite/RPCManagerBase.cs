using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Network.Core.RPCLite
{

    public class RPCAttribute : Attribute
    {

    }

    public class RPCRequestAttribute : Attribute
    {

    }

    public class RPCResponseAttribute : Attribute
    {

    }

    public class RPCNotifyAttribute : Attribute
    {

    }

    //指明方法来自谁
    public class RPCMethodHelper
    {
        public object listener;
        public MethodInfo method;
    }

    public class RPCManagerBase
    {
        protected List<object> mListListener;
        protected DictionarySafe<string, RPCMethodHelper> mMethodHelper;
        public void Init()
        {
            mListListener = new List<object>();
            mMethodHelper = new DictionarySafe<string, RPCMethodHelper>();
        }

        public void Clear()
        {
            mListListener.Clear();
            foreach (var pair in mMethodHelper)
            {
                pair.Value.listener = null;
                pair.Value.method = null;
            }
            mMethodHelper.Clear();
        }

        //输出已经存在的RPC映射
        public void Dump()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var pair in mMethodHelper)
            {
                RPCMethodHelper helper = pair.Value;
                sb.AppendFormat("\t<name:{0}, \tmethod:{1}.{2}>\n", pair.Key,
                    helper.method.DeclaringType.Name, helper.method.Name);
            }

            Debuger.LogWarning("\nRPC Cached Methods ({0}):\n{1}", mMethodHelper.Count, sb);
        }




        public void RegistListner(object listener)
        {
            if (!mListListener.Contains(listener))
            {
                Debuger.Log("{0}", listener.GetType().Name);
                mListListener.Add(listener);
            }
        }

        public void UnRegistListner(object listener)
        {
            if (mListListener.Contains(listener))
            {
                Debuger.Log("{0}", listener.GetType().Name);
                mListListener.Remove(listener);
            }
        }

        //根据函数名获取方法
        public RPCMethodHelper GetMethodHelper(string name)
        {
            var helper = mMethodHelper[name];
            if (helper == null)
            {
                //不存在则遍历所有的listener,尝试找到其中的方法
                MethodInfo mi = null;
                object listener = null;
                for (int i = 0; i < mListListener.Count; i++)
                {
                    listener = mListListener[i];
                    mi = listener.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    if (mi != null)
                    {
                        break;
                    }
                }

                if (mi != null)
                {
                    helper = new RPCMethodHelper();
                    helper.listener = listener;
                    helper.method = mi;
                    mMethodHelper.Add(name, helper);
                }
            }

            return helper;
        }
    }
}
