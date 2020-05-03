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

        public static string DefaultLoadingPackageName = "Login";
        public static string DefaultLoadingComName = "LoadPanel";

        public static string DefaultDialogPkgName = "Login";
        public static string DefaultDialogComName = "Dialog";
        public static string DefaultSimpleDialogComName = "ExDialog";

        private static int refreshInterval=10;//每10ms检查一次显示序列
        private static DateTime mLastCheckTime;

        private Stack<UIPageTrack> mPageTrackStack;//page打开记录
        private UIPageTrack mCurPage;//当前page

        private DictionarySafe<string,FGUIPanel> mListLoadedPanel;//所有已经加载的UI
        private Dictionary<string,string> mLoadedPkgFileName;//加载了的包的名字
        

        private List<OpenTrack> mCacheOpenPanel;//打开panel的命令的缓存

        private Action<float> onSceneLoading=null;
        private Action onSceneLoadFinished= null;
        private string mCurLoadingName;//当前的loading的全名
        
        public FGUIManager()
        {
            mPageTrackStack = new Stack<UIPageTrack>();
            mListLoadedPanel = new DictionarySafe<string, FGUIPanel>();
            mLoadedPkgFileName = new Dictionary<string, string>();
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
        
        public void LoadSceneWithClearAll(string scene,Action onStartLoad,Action onLoadComplete,Action<float> onLoading)
        {
            ClearView();
            onStartLoad.Invoke();

            onSceneLoadFinished = onLoadComplete;
            onSceneLoading = onLoading;

            mSceneManager.EnterSceneAsync(scene, OnSceneLoadFinished, OnSceneLoading);
        }

        public void LoadScene<T>(string scene, Action onLoadComplete,string loadingPkg=null,string loadingCom=null) where T:FGUILoading,new()
        {
            string pkgName=loadingPkg??DefaultLoadingPackageName;
            string comName = loadingCom ?? DefaultLoadingComName;

            string realName = pkgName + "." + comName;
            mCurLoadingName = realName;
            //加载结束时执行的事件
            onSceneLoadFinished = onLoadComplete;

            FGUILoading loadingBar= OpenLoading<T>(comName,pkgName);
            //如果存在该loading则在加载过程中加载loading界面，否则不显示
            if (loadingBar == null)
            {
                Debuger.LogError("error open loading:{0}!", realName);
            }
            else
            {
                //加载过程中执行的事件
                onSceneLoading = (progress) =>
                {
                    if (loadingBar.HasBar)
                    {
                        double value = (loadingBar.MaxValue - loadingBar.MinValue) * progress + loadingBar.MinValue;
                        loadingBar.SetValue(value, 0.5f);
                    }
                };
            }

            mSceneManager.EnterSceneAsync(scene, OnSceneLoadFinished,OnSceneLoading);
        }

        private void OnSceneLoadFinished()
        {
            //执行外部的结束事件
            if (onSceneLoadFinished != null)
            {
                onSceneLoadFinished();
                onSceneLoadFinished = null;
            }
            //取消加载中事件
            onSceneLoading = null;
            Debuger.Log("load scene complete called");
        }
        private void OnSceneLoading(float progress)
        {
            if (onSceneLoading != null)
            {
                onSceneLoading(progress);
            }
        }

        //加载某个UI
        public T LoadToMemory<T>(string name,string package) where T : FGUIPanel, new()
        {
            string realPkgName = package;
            //检查有没有加载过该包
            if (!mLoadedPkgFileName.ContainsKey(package))
            {
                //未加载则加入
                realPkgName = FGUIPackageManager.Instance.LoadPackage(package);
                mLoadedPkgFileName.Add(package, realPkgName);
            }
            else
            {
                //有则获取包名
                realPkgName = mLoadedPkgFileName[package];
            }
            string idxName = package + "." + name;

            //有则获取
            T ui = mListLoadedPanel[idxName] as T;
            //没有则尝试加载
            if (ui == null)
            {
                Debuger.LogWarning("not found, start to load");
                ui = Load<T>(package, realPkgName, name);
            }

            return ui;
        }

        //打开某个UI
        public T Open<T>(string name,string package,object arg = null) where T : FGUIPanel,new()
        {
            T ui = LoadToMemory<T>(name, package);

            string idxName = package + "." + name;

            //有则调出
            if (ui != null)
            {
                AddOpenCache(idxName, arg);
            }
            else
            {
                Debuger.LogError("cannot find panel:{0}", name);
            }
            return ui;
        }
        //立即打开UI
        public T OpenInstantly<T>(string name, string package, object arg = null) where T : FGUIPanel, new()
        {
            string idxName = package + "." + name;
            T ui = mListLoadedPanel[idxName] as T;

            //有则调出
            if (ui != null)
            {
                AddOpenCache(idxName, arg);
            }
            else
            {
                Debuger.LogError("cannot find panel:{0}", name);
            }
            return ui;
        }

        private void AddOpenCache(string name,object arg)
        {
            OpenTrack track = new OpenTrack(name, arg);
            //加入显示队列,如果已存在则移除原本的
            if (mCacheOpenPanel.Contains(track))
            {
                mCacheOpenPanel.Remove(track);
            }
            mCacheOpenPanel.Add(track);
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

        public void Close<T>(string name,object arg)where T:FGUIPanel
        {
            OpenTrack track = new OpenTrack(name, null);
            //从显示序列中清除
            if (mCacheOpenPanel.Contains(track))
            {
                mCacheOpenPanel.Remove(track);
            }

            T ui = GetUI(name) as T;
            if (ui != null)
            {
                ui.Close(arg);
            }

        }

        public void Tick()
        {
            DateTime current;
            //将等待打开的panel打开
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
                    //已打开则移除
                    else if (panel.IsOpened)
                    {
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
        public void GoBackPage()
        {
            //有上页则打开上页
            if (mPageTrackStack.Count > 0)
            {
                var track = mPageTrackStack.Pop();
                mCurPage.name = track.name;
                mCurPage.arg = track.arg;

                CloseAllLoadedPanel();
                //添加到打开缓冲区
                AddOpenCache(track.name, track.arg);
            }
        }
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
            Close<FGUIWindow>(name, arg);
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
            Close<FGUILoading>(name, arg);
        }

        public FGUILoading OpenLoading<T>(string name, string pkg,object arg = null) where T : FGUILoading,new()
        {
            Debuger.Log(name);
            FGUILoading ui = Open<T>(name,pkg, arg);
            return ui;
        }
        #endregion


        #region Dialog
        public Dialog ShowDialog(string content,Action onCancel=null)
        {
            DialogParam param = new DialogParam();
            param.content = content;
            param.onCancel = onCancel;
            Dialog ui=Open<Dialog>(DefaultDialogComName, DefaultDialogPkgName, param);
            return ui;
        }

        public ExDialog ShowExDialog(string content, Action onCancel=null,Action onConfirm=null)
        {
            DialogParam param = new DialogParam();
            param.content = content;
            param.onCancel = onCancel;
            param.onConfirm = onConfirm;
            ExDialog ui = Open<ExDialog>(DefaultSimpleDialogComName, DefaultDialogPkgName, param);
            return ui;
        }


        #endregion
    }
}
