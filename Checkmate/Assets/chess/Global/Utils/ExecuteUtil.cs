﻿using QGF.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Xml;
using Checkmate.Game.Controller;
using Checkmate.Game.Utils;
using QGF;

namespace Checkmate.Game.Utils
{
    //一个文件夹对应的dll信息
    internal class DllInfo
    {
        public string name;//名称
        public string main;//主dll
        public string mainClassName;//主class的命名空间+类名
    }

    internal class DllLoader
    {
        public static DllInfo GetInfo(string path)
        {
            XmlDocument document = new XmlDocument();
            try
            {
                document.Load(path + "/config.xml");
            }
            catch(Exception exception)
            {
                //报错
                Debug.LogError("load error:" + path);
                return null;
            }
            DllInfo result = new DllInfo();
            XmlNode config = document.SelectSingleNode("Config");
            //获取名称
            result.name = config.Attributes["name"].Value;
            XmlNode main = config.SelectSingleNode("Main");
            //获取main
            result.main = main.InnerText;
            result.mainClassName = main.Attributes["namespace"].Value +"."+ main.Attributes["class"].Value;

            return result;
        }


    }


    //该类用于通过反射执行具体的函数
    public class ExecuteUtil : Singleton<ExecuteUtil>
    {
        private Dictionary<string, object> mClassCache;//方法所属类的缓存
        private Dictionary<string, MethodInfo> mMethodLoaded;//加载完成的方法

#if UNITY_EDITOR
        private static string rootPath=Application.dataPath+"/Test";
#else
        private static string rootPath = Application.streamingAssetsPath + "/Scripts";
#endif
        public void Init(string path)
        {
            mClassCache = new Dictionary<string, object>();
            mMethodLoaded = new Dictionary<string, MethodInfo>();

            DirectoryInfo folder = new DirectoryInfo(path);

            foreach(DirectoryInfo nextFolder in folder.GetDirectories())
            {
                string folderPath = nextFolder.FullName;
                LoadDllInDic(folderPath);
            }
        }

        public void Clear()
        {
            //清除所有加载的类型与方法
            mClassCache.Clear();
            mMethodLoaded.Clear();
            mClassCache = null;
            mMethodLoaded = null;
        }

        //执行
        public object Execute(string method, object[] param)
        {
            string className = method.Substring(0, method.LastIndexOf('.'));
            object result;
            //如果没有加载该类，报错
            if (!mClassCache.ContainsKey(className))
            {
                result = null;
                Debug.LogError("error get class:" + className);
                return null;
            }
            //如果包含该方法，则直接执行
            if (mMethodLoaded.ContainsKey(method))
            {
                MethodInfo instance = mMethodLoaded[method];
                if (instance == null)
                {
                    Debuger.LogError("error get method:{0}", method);
                }
                if (!mClassCache.ContainsKey(className))
                {
                    Debuger.LogError("error get class of {0}", className);

                }
                result = instance.Invoke(mClassCache[className], param);
                return result;
            }
            string methodName = method.Substring(method.LastIndexOf('.')+1);
            //否则，加载该方法，并执行
            Type type = mClassCache[className].GetType();
            MethodInfo info = type.GetMethod(methodName);
            mMethodLoaded.Add(method, info);
            result = info.Invoke(mClassCache[className], param);
            return result;
        }

        private void LoadDllInDic(string path)
        {

            DllInfo info = DllLoader.GetInfo(path);
            if (info != null)
            {
                string name = info.name;

                //加载主dll
                var assembly = Assembly.LoadFrom(path + "/" + info.main);
                object instance = assembly.CreateInstance(info.mainClassName);
                //执行初始化
                Type type = instance.GetType();
                MethodInfo init = type.GetMethod("Init");
                init.Invoke(instance, null);
                //加入缓存
                mClassCache.Add(name, instance);
                Debug.Log("Load dll:" + name);
            }

        }
        ////加载某个方法
        //private void LoadMethod(string method)
        //{
        //    string className = method.Substring(0, method.LastIndexOf('.'));
        //    //如果没有加载该类，报错
        //    if (!mClassCache.ContainsKey(className))
        //    {
        //        Debug.LogError("error get class:" + className);
        //        return;
        //    }

        //    string methodName = method.Substring(method.LastIndexOf('.')+1);
        //    //否则，加载该方法，并执行
        //    Type type = mClassCache[className].GetType();
        //    MethodInfo info = type.GetMethod(methodName);
        //    mMethodLoaded.Add(method, info);
        //}

        //解析xml
        public static ExecuteInfo ParseExecute(XmlNode node)
        {
            ExecuteInfo info = new ExecuteInfo();
            info.method = node.Attributes["src"].Value;
            
            //解析参数
            info.parameters = new List<ParamInfo>();
            XmlNode param = node.SelectSingleNode("Params");
            XmlNodeList pl = param.ChildNodes;
            foreach(XmlNode l in pl)
            {
                ParamInfo pi = new ParamInfo();
                pi.type =(ParamType)Enum.Parse(typeof(ParamType),l.Attributes["type"].Value);
                pi.value = l.Attributes["value"].Value;
                info.parameters.Add(pi);
            }

            info.returnValue = null;
            XmlNode r = node.SelectSingleNode("Return");
            if (r != null)
            {
                info.returnValue = r.Attributes["value"].Value;
            }
            return info;
        }
    }

    //参数可选类别
    public enum ParamType
    {
        Controller,
        Int,
        Float,
        String,
        ControllerList,
        Bool,
        Position
    }


    //参数信息
    public class ParamInfo
    {
        public ParamType type;
        public string value;
    }

    //一个方法的执行信息
    public class ExecuteInfo
    {
        public string method;//方法名
        public List<ParamInfo> parameters;//参数
        public string returnValue;//返回值
    }

    //具体活动
    public class SkillAction
    {
        public const int SearchType = 0;
        public const int SelectType = 1;
        public class TargetTrack
        {
            public int type;//操作类型
            public int idx;//操作序列
            public TargetTrack(int t, int i)
            {
                type = t;
                idx = i;
            }
        }

        public Checks Checks=null;//所有的条件操作

        public List<TargetTrack> TargetTracks;//执行目标搜索的顺序
        public List<BaseSearch> Searches;//所有搜索操作
        public List<Selects> Selectors;//所有筛选操作

        public List<ExecuteInfo> Executes;//所有的脚本操作

        public SkillAction(XmlNode node)
        {
            Init(node);
        }

        public SkillAction(string content)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(content);
            XmlNode root = document.DocumentElement;
            Init(root);
        }
        
        private void Init(XmlNode node)
        {
            //解析条件部分
            XmlNode check = node.SelectSingleNode("Checks");
            if (check != null)
            {
                Checks = new Checks(check);
            }
            //解析目标部分
            XmlNode target = node.SelectSingleNode("Targets");
            XmlNodeList tl = target.ChildNodes;
            TargetTracks = new List<TargetTrack>();
            Searches = new List<BaseSearch>();
            Selectors = new List<Selects>();
            foreach (XmlNode l in tl)
            {
                //解析Selects
                if (l.Name == "Selects")
                {
                    Selects s = new Selects(l);
                    TargetTracks.Add(new TargetTrack(SelectType, Selectors.Count));
                    Selectors.Add(s);
                }
                else
                {
                    BaseSearch bs = SearchParser.Parse(l);
                    TargetTracks.Add(new TargetTrack(SearchType, Searches.Count));
                    Searches.Add(bs);
                }
            }

            //解析脚本部分
            Executes = new List<ExecuteInfo>();
            XmlNode exRoot = node.SelectSingleNode("Executes");
            XmlNodeList el = exRoot.ChildNodes;
            foreach (XmlNode l in el)
            {
                ExecuteInfo info = ExecuteUtil.ParseExecute(l);
                Executes.Add(info);
            }
        }

        public string GetMethods()
        {
            string result = "";
            foreach(var method in Executes)
            {
                result += method.method+",";
            }
            result += ";";
            return result;
        }
    }


}
