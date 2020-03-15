using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace QGF.Unity.UI
{
    public class UIManager:Singleton<UIManager>
    {
        public static string MainScene = "Main";
        public static string MainPage = "UIMainPage";
        public static string SceneLoading = "";//scene的loading资源
        class UIPageTrack
        {
            public string name;
            public object arg;
            public Type type;
        }

        private Stack<UIPageTrack> mPageTrackStack;//page打开记录
        private UIPageTrack mCurPage;//当前page

        private List<UIPanel> mListLoadedPanel;//所有已经加载的UI

        private Action<string> onSceneLoaded;
        public UIManager()
        {
            mPageTrackStack = new Stack<UIPageTrack>();
            mListLoadedPanel = new List<UIPanel>();
        }

        public void Init(string uiResRoot)
        {
            UIRes.UIResRoot = uiResRoot;
            mPageTrackStack.Clear();
            mListLoadedPanel.Clear();

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (onSceneLoaded != null) onSceneLoaded(scene.name);
            };
        }

        public void Clean()
        {
            CloseAllLoadedPanel();

            mPageTrackStack.Clear();
            mListLoadedPanel.Clear();
        }
        //关闭所有的panel
        private void CloseAllLoadedPanel()
        {
            for (int i = mListLoadedPanel.Count - 1; i >= 0; i--)
            {
                var panel = mListLoadedPanel[i];
                if (panel == null)
                {
                    mListLoadedPanel.RemoveAt(i);
                }
                else if (panel.IsOpen)
                {
                    panel.Close();
                }

            }
        }

        //打开某个UI
        public T Open<T>(string name,object arg=null) where T : UIPanel
        {
            T ui=UIRoot.Find<T>(name);
            //没有则尝试加载
            if (ui == null)
            {
                Debuger.LogWarning("not found, start to load");
                ui = Load<T>(name);
            }
            //有则调出
            if(ui!=null)
            {
                ui.Open(arg);
                UIRoot.Sort();
            }
            else
            {
                Debuger.LogError("cannot find panel:{0}", name);
            }
            return ui;
        }

        //加载name的prefab为T
        private T Load<T>(string name) where T : UIPanel
        {
            T ui = default(T);
            GameObject original=UIRes.LoadPrefab(name);
            if (original != null)
            {
                Debuger.Log("succeed load res:{0}", name);
                GameObject gobj = GameObject.Instantiate(original);
                ui = gobj.GetComponent<T>();
                //没有则尝试添加
                if (ui == null)
                {
                    try
                    {
                        ui = gobj.AddComponent<T>();
                    }
                    catch(Exception e)
                    {
                        Debuger.LogError("无法添加抽象panel!");
                    }
                }

                if (ui!=null)
                {
                    gobj.name = name;
                    UIRoot.AddChild(ui);
                    if (mListLoadedPanel.IndexOf(ui) < 0)
                    {
                        mListLoadedPanel.Add(ui);
                    }
                }
                else
                {
                    Debuger.LogError("prefab 没有对应的panel组件:" + name);
                }
            }
            else
            {
                Debuger.LogError("Res Not Found:" + name);
            }
            return ui;
        }
        //获取name对应的panel
        public UIPanel GetUI(string name)
        {
            for (int i = 0; i < mListLoadedPanel.Count; i++)
            {
                if (mListLoadedPanel[i].name == name)
                {
                    return mListLoadedPanel[i];
                }
            }
            return null;
        }


        //====================================================
        //scene管理

        public void LoadScene(string scene,Action onLoadComplete)
        {
            onSceneLoaded = (sceneName) =>
            {
                if (sceneName == scene)
                {
                    onSceneLoaded = null;
                    if (onLoadComplete != null) onLoadComplete();
                    CloseLoading(SceneLoading);
                }
            };
            OpenLoading(SceneLoading);
            SceneManager.LoadScene(scene);
        }

        //==================================
        #region Page

        public void OpenPage<T>(string name, object arg = null) where T : UIPage
        {
            Debuger.Log(name);

            if (mCurPage != null)
            {
                mPageTrackStack.Push(mCurPage);
            }

            OpenPageWorker(name, arg,typeof(T));
        }
        public void OpenPage(string pageName,object arg = null)
        {
            Debuger.Log("page:{0},arg:{1}", pageName, arg);
            //当前有page则将其推出堆栈
            if (mCurPage != null)
            {
                mPageTrackStack.Push(mCurPage);
            }
            OpenPageWorker(pageName, arg);
            
        }

        //返回上一页
        public void GoBackPage()
        {
            //有上页则打开上页
            if (mPageTrackStack.Count > 0)
            {
                var track = mPageTrackStack.Pop();
                OpenPageWorker(track.name, track.arg,track.type);
            }
            //否则打开主页
            else
            {
                EnterMainPage();
            }
        }
        //打开page
        private void OpenPageWorker(string pageName,object arg,Type type=null)
        {
            mCurPage = new UIPageTrack();
            mCurPage.name = pageName;
            mCurPage.arg = arg;
            mCurPage.type = type;

            CloseAllLoadedPanel();
            Open<UIPage>(pageName, arg);
        }

        public void EnterMainPage()
        {
            mPageTrackStack.Clear();
            OpenPageInScene(MainScene, MainPage, null, null);
        }

        public void EnterMainPage<T>()
        {
            mPageTrackStack.Clear();
            OpenPageInScene(MainScene, MainPage, null,typeof(T));
        }

        private void OpenPageInScene(string scene,string page,object arg,Type type)
        {
            Debuger.Log("scene:{0}, page:{1}, arg:{2}", scene, page, arg);
            string oldScene = SceneManager.GetActiveScene().name;
            //在该场景则直接显示
            if (oldScene == scene)
            {
                OpenPageWorker(page, arg,type);
            }
            //否则先回到该场景
            else
            {
                LoadScene(scene, () =>
                {
                    OpenPageWorker(page, arg,type);
                });
            }
        }
        #endregion 
        //=====================================
        #region UIWindow

        public UIWindow OpenWindow(string name, object arg = null)
        {
            Debuger.Log(name);
            UIWindow ui = Open<UIWindow>(name, arg);
            return ui;
        }


        public void CloseWindow(string name, object arg = null)
        {
            Debuger.Log(name);
            UIWindow ui = GetUI(name) as UIWindow;
            if (ui != null)
            {
                ui.Close(arg);
            }
        }

        public UIWindow OpenWindow<T>(string name, object arg = null) where T : UIWindow
        {
            Debuger.Log(name);
            UIWindow ui = Open<T>(name, arg);
            return ui;
        }


        #endregion
        //====================================
        #region UIWidget

        public UIWidget OpenWidget(string name, object arg = null)
        {
            Debuger.Log(name);
            UIWidget ui = Open<UIWidget>(name, arg);
            return ui;
        }

        public void CloseWidget(string name, object arg = null)
        {
            Debuger.Log(name);
            UIWidget ui = GetUI(name) as UIWidget;
            if (ui != null)
            {
                ui.Close(arg);
            }
        }

        public UIWidget OpenWidget<T>(string name, object arg = null) where T : UIWidget
        {
            Debuger.Log(name);
            UIWidget ui = Open<T>(name, arg);
            return ui;
        }

        #endregion
        //======================================
        #region UILoading

        public UILoading OpenLoading(string name, object arg = null)
        {
            Debuger.Log(name);
            UILoading ui = Open<UILoading>(name, arg);
            return ui;
        }

        public void CloseLoading(string name, object arg = null)
        {
            Debuger.Log(name);
            UILoading ui = GetUI(name) as UILoading;
            if (ui != null)
            {
                ui.Close(arg);
            }
        }

        public UILoading OpenLoading<T>(string name, object arg = null) where T : UILoading
        {
            Debuger.Log(name);
            UILoading ui = Open<T>(name, arg);
            return ui;
        }
        #endregion


        //==========================================

    }
}
