using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FairyGUI;
using QGF.Common;
using QGF.Time;
using UnityEngine;
using UnityEngine.SceneManagement;
using static FairyGUI.UIContentScaler;

namespace QGF.Unity.FGUI
{
    public class FGUIManager:Singleton<FGUIManager>
    {
        class UIPageTrack
        {
            public string name;
            public object arg;
            public Type type;
        }

        class OpenTrack
        {
            public string name;
            public object arg;

            public override bool Equals(object obj)
            {
                if (obj.GetType() != GetType()||obj==null)
                {
                    return false;
                }
                OpenTrack r = obj as OpenTrack;
                return name.Equals(r.name);
            }

            public OpenTrack(string n,object a)
            {
                this.name = n;
                this.arg = a;
            }
        }

        private FGUISceneManager mSceneManager;//场景管理

        public static string DefaultLoadingPackageName = "";
        public static string DefaultLoadingComName = "";

        private static int refreshInterval=10;//每10ms检查一次显示序列
        private static DateTime mLastCheckTime;

        private Stack<UIPageTrack> mPageTrackStack;//page打开记录
        private UIPageTrack mCurPage;//当前page

        private DictionarySafe<string,FGUIPanel> mListLoadedPanel;//所有已经加载的UI
        private List<string> mLoadedPkgFileName;//加载了的包的名字
        

        private List<OpenTrack> mCacheOpenPanel;//打开panel的命令的缓存
        
        public FGUIManager()
        {
            mPageTrackStack = new Stack<UIPageTrack>();
            mListLoadedPanel = new DictionarySafe<string, FGUIPanel>();
            mLoadedPkgFileName = new List<string>();
            mCacheOpenPanel = new List<OpenTrack>();
        }

        public void Init(string uiResRoot,FGUISceneManager sceneMng)
        {
            FGUIPackageManager.Instance.Init(uiResRoot);
            GRoot.inst.SetContentScaleFactor(1920, 1080, ScreenMatchMode.MatchWidthOrHeight);
            mPageTrackStack.Clear();
            mListLoadedPanel.Clear();
            mSceneManager = sceneMng;
            
        }

        public void Clear()
        {
            ClearView();
            mLoadedPkgFileName.Clear();

            //销毁所有加载了的窗口
            foreach(var pair in mListLoadedPanel)
            {
                pair.Value.Destroy();
            }
            mListLoadedPanel.Clear();
        }
        public void ClearView()
        {
            //先清除未打开的窗口
            mCacheOpenPanel.Clear();

            CloseAllLoadedPanel();

            mPageTrackStack.Clear();
        }

        

        //隐藏所有的panel
        private void CloseAllLoadedPanel()
        {
            foreach(var pair in mListLoadedPanel)
            {
                var panel = pair.Value;
                if (panel == null)
                {
                    mListLoadedPanel.Remove(pair.Key);
                }
                else if (panel.IsOpened)
                {
                    panel.Close();
                }

            }
        }

        public void Destroy(string packageFileName,string name)
        {
            string realName = packageFileName + "." + name;
            if (mListLoadedPanel.ContainsKey(realName))
            {
                var panel = mListLoadedPanel[realName];
                panel.Destroy();
                mListLoadedPanel.Remove(realName);
            }
        }

        //==============
        //场景
        //===============
        public void LoadScene<T>(string scene, Action onLoadComplete,string loadingPkg=null,string loadingCom=null) where T:FGUILoading,new()
        {
            string pkgName=loadingPkg??DefaultLoadingPackageName;
            string comName = loadingCom ?? DefaultLoadingComName;

            string realName = pkgName + "." + comName;
            onLoadComplete += () =>
            {
                CloseLoading(realName);
            };
            OpenLoading<T>(comName,pkgName);

            mSceneManager.EnterScene(scene, onLoadComplete);
        }


        //打开某个UI
        public T Open<T>(string name,string package,object arg = null) where T : FGUIPanel,new()
        {
            string realPkgName = package;
            //检查有没有加载过该包
            if (!mLoadedPkgFileName.Contains(package))
            {
                //未加载则加入
                realPkgName=FGUIPackageManager.Instance.LoadPackage(package);
                mLoadedPkgFileName.Add(package);
            }
            string idxName = package + "." + name;
            T ui = mListLoadedPanel[idxName] as T;
            //没有则尝试加载
            if (ui == null)
            {
                Debuger.LogWarning("not found, start to load");
                ui = Load<T>(package,realPkgName,name);
            }
            //有则调出
            if (ui != null)
            {
                OpenTrack track = new OpenTrack(idxName, arg);
                //加入显示队列,如果已存在则移除原本的
                if (mCacheOpenPanel.Contains(track))
                {
                    mCacheOpenPanel.Remove(track);
                }
                mCacheOpenPanel.Add(track);
            }
            else
            {
                Debuger.LogError("cannot find panel:{0}", name);
            }
            return ui;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="pkgFile">包的文件名</param>
        /// <param name="packageName">包名</param>
        /// <param name="name">组件名</param>
        /// <returns></returns>
        private T Load<T>(string pkgFile,string packageName,string name) where T : FGUIPanel,new()
        {

            T ui = new T();

            ui.Load(packageName, name);
            if (ui != null)
            {
                //加入缓存
                mListLoadedPanel.Add(pkgFile + "." + name, ui);
            }
            else
            {
                Debuger.LogError("Res Not Found:" + name);
            }
            return ui;
        }


        public void Tick()
        {
            DateTime current;
            if (mCacheOpenPanel.Count > 0 && ((current=DateTime.UtcNow) - mLastCheckTime).TotalMilliseconds >= refreshInterval)
            {
                for(int i = mCacheOpenPanel.Count - 1; i >= 0; --i)
                {
                    var panel = mListLoadedPanel[mCacheOpenPanel[i].name];
                    if (panel.IsLoaded && !panel.IsOpened)
                    {
                        //打开后将其从显示序列中移除
                        panel.Open(GRoot.inst,mCacheOpenPanel[i].arg);
                        mCacheOpenPanel.RemoveAt(i);
                    }
                }
                mLastCheckTime = current;
            }
        }


        public FGUIPanel GetUI(string name)
        {
            if (mListLoadedPanel.ContainsKey(name))
            {
                return mListLoadedPanel[name];
            }
            return null;
        }

        public FGUIPanel GetUI(string packageFileName,string name)
        {
            string realName = packageFileName + "." + name;
            if (mListLoadedPanel.ContainsKey(realName))
            {
                return mListLoadedPanel[name];
            }
            return null;
        }

        //=============
        #region page
        public T OpenPage<T>(string name,string pkg, object arg = null) where T : FGUIPage,new()
        {
            Debuger.Log(name);

            if (mCurPage != null)
            {
                mPageTrackStack.Push(mCurPage);
            }

            return OpenPageWorker<T>(name,pkg, arg);
        }

        ////返回上一页
        //public void GoBackPage()
        //{
        //    //有上页则打开上页
        //    if (mPageTrackStack.Count > 0)
        //    {
        //        var track = mPageTrackStack.Pop();
        //        OpenPageWorker(track.name, track.arg, track.type);
        //    }
        //}
        //打开page
        private T OpenPageWorker<T>(string pageName,string pkg,object arg) where T:FGUIPage,new()
        {
            mCurPage = new UIPageTrack();
            mCurPage.name = pkg+"."+pageName;
            mCurPage.arg = arg;

            CloseAllLoadedPanel();
            return Open<T>(pageName, pkg,arg);
        }
        #endregion


        #region window

        public void CloseWindow(string name, object arg = null)
        {
            Debuger.Log(name);
            FGUIWindow ui = GetUI(name) as FGUIWindow;
            if (ui != null)
            {
                ui.Close(arg);
            }
        }

        public FGUIWindow OpenWindow<T>(string name,string pkg, object arg = null) where T : FGUIWindow,new()
        {
            Debuger.Log(name);
            FGUIWindow ui = Open<T>(name,pkg, arg);
            return ui;
        }

        #endregion

        #region Loading


        public void CloseLoading(string name, object arg = null)
        {
            Debuger.Log(name);
            FGUILoading ui = GetUI(name) as FGUILoading;
            if (ui != null)
            {
                ui.Close(arg);
            }
        }

        public FGUILoading OpenLoading<T>(string name, string pkg,object arg = null) where T : FGUILoading,new()
        {
            Debuger.Log(name);
            FGUILoading ui = Open<T>(name,pkg, arg);
            return ui;
        }
        #endregion
    }
}
