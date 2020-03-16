﻿using Assets.chess;
using System;
using UnityEngine;
using QGF.Time;
using QGF.Module;
using QGF;
using QGF.Unity.FGUI;
namespace Assets.Chess
{
    public class AppMain:MonoBehaviour
    {

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            //初始化时间
            QGFTime.TimeAppStart = DateTime.Now;

            //初始化debugger
            InitDebuger();

            //初始化AppConfig
            AppConfig.Init();

            //初始化版本管理
            InitVersion();

#if UNITY_EDITOR
            //防止重复添加
            UnityEditor.EditorApplication.playModeStateChanged -= OnEditorPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnEditorPlayModeChanged;
#endif

        }

#if UNITY_EDITOR
        private void OnEditorPlayModeChanged(UnityEditor.PlayModeStateChange state)
        {
            //暂停或退出时
            if (Application.isPlaying == false)
            {
                UnityEditor.EditorApplication.playModeStateChanged -= OnEditorPlayModeChanged;
                //退出游戏逻辑
                Exit("模式变化");
            }
        }
#endif

        private void Exit(string msg)
        {
            //清理模块管理器
            ModuleManager.Instance.Clear();
            //清理UI管理
            FGUIManager.Instance.Clear();
            //清理在线管理
            //清理ILR
            //清理版本管理
        }

        private void InitDebuger()
        {
            //设置debugger的开关
            Debuger.Init(Application.persistentDataPath + "/DebuggerLog/", new UnityDebugerConsole());
            Debuger.EnableLog = true;
            Debuger.EnableSave = true;
            Debuger.EnableStack = true;
            Debuger.Log("init over");

        }

        private void InitVersion()
        {
            //进行版本更新

            //版本更新或检查完成后，初始化服务模块
            InitServices();
        }


        private void InitServices()
        {
            //初始化ILR
            //初始化模块管理器
            ModuleManager.Instance.Init();
            ModuleManager.Instance.RegistModuleActivator(new NativeModuleActivator(ModuleDef.Namespace, ModuleDef.NativeAssemblyName));

            //初始化UI管理
            FGUIManager.Instance.Init("ui/");
            //初始化在线管理


            //显示登陆界面

            //如果登录成功，初始化普通业务模块
            GlobalEvent.onLogin += OnLogin;

            //example
            ModuleManager.Instance.CreateModule("ExampleAModule");
            ModuleManager.Instance.ShowModule("ExampleAModule");
        }

        private void OnLogin(bool success)
        {
            GlobalEvent.onLogin -= OnLogin;

            if (success)
            {
                //隐藏登录界面
                //通过ILR启动业务模块

                //bool result=ILRManager.Instance.Invoke("Checkmate.ScriptMain", "Init");
                //if (!result)
                //{
                //    //初始化业务模块失败
                //}
            }
            else
            {
                //登陆失败
            }
        }


        private void Update()
        {
            GlobalEvent.onUpdate.Invoke();
            FGUIManager.Instance.Tick();
        }

        private void FixedUpdate()
        {
            GlobalEvent.onFixedUpdate.Invoke();
        }

    }
}
