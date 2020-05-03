using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QGF.Unity.FGUI
{
    public class FGUISceneManager:MonoBehaviour
    {
        class PreLoadArg
        {
            public string mPreLoadName;
            public AsyncOperation mAsync;

            public override bool Equals(object obj)
            {
                return obj != null && obj.GetType().Equals(GetType()) &&
                    ((obj as PreLoadArg).mPreLoadName.Equals(mPreLoadName));
            }
        }
        private string mCurScene;//当前场景名
        private AsyncOperation mAsyncOperation;//当前加载的结果
        private List<PreLoadArg> mPreLoaded;//所有已经预加载的场景
        private List<PreLoadArg> mPreLoadCache;//预加载场景的准备区

        private Action<string> onSceneLoaded;//场景加载完成的事件

        private bool IsSwithing = false;//是否正在进行场景切换

        public bool IsSwithingScene { get { return IsSwithing; } }

        //在切换场景时执行(转场效果)
        public delegate bool OnSceneSwitchingCallback();

        public void Init()
        {
            mPreLoaded = new List<PreLoadArg>();
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (onSceneLoaded != null) onSceneLoaded(scene.name);
            };
        }
        //===============
        //切换场景（移除当前其他场景)
        //===================
        public void EnterScene(string scene,Action onSceneLoadFinished,OnSceneSwitchingCallback callback=null)
        {
            if (IsSwithing)
            {
                Debuger.LogWarning("Now is switching scene! cannot enter scene:{0}", scene);
                return;
            }
            //如果是预加载场景
            if (IsLoaded(scene))
            {
                StartCoroutine(EnterPreLoad(scene, onSceneLoadFinished, callback));
            }
            else {
                this.onSceneLoaded = (sceneName) =>
                {
                    if (sceneName == scene)
                    {
                        IsSwithing = false;
                        mCurScene = scene;
                        onSceneLoaded = null;
                        if (onSceneLoadFinished != null) onSceneLoadFinished();
                    }
                };
                IsSwithing = true;
                SceneManager.LoadScene(scene);
            }
        }

        public void EnterSceneAsync(string scene,Action onSceneLoadFinished,Action<float> onSceneLoading)
        {
            if (IsSwithing)
            {
                Debuger.LogWarning("Now is switching scene! cannot enter scene:{0}", scene);
                return;
            }
            StartCoroutine(LoadSceneAsyncAutoShow(scene, onSceneLoadFinished, onSceneLoading));
        }

        //=======================
        //切换场景（不移除）(需预加载)
        //=======================
        public void SwitchScene(string scene, Action onSceneLoadFinished, OnSceneSwitchingCallback callback = null)
        {
            if (IsSwithing)
            {
                Debuger.LogWarning("Now is switching scene! cannot enter scene:{0}", scene);
                return;
            }
            //未加载则先加载
            if (!IsLoaded(scene))
            {
                AddPreLoad(scene);
            }
            StartCoroutine(SwitchPreLoad(scene, onSceneLoadFinished, callback));
        }
        //==============
        //预加载场景(多场景叠加)
        //===============

        //添加预加载scene
        public void AddPreLoad(string scene)
        {
            //要求与当前scene不相同且未加载
            if (scene != SceneManager.GetActiveScene().name&&!IsLoaded(scene))
            {
                StartCoroutine(PreLoadAsync(scene));
            }
        }
        //移除一个scene
        public void Unload(string scene)
        {
            if (IsLoaded(scene))
            {
                if (scene == mCurScene)
                {
                    Debuger.LogError("cannnot unload scene:{0}, it's cur scene!", scene);
                    return;
                }
                StartCoroutine(UnloadAsync(scene));
            }
            else
            {
                Debuger.LogError("scene:{0} not loaded! cannot unload!", scene);
            }
        }

        //移除除当前scene之外的所有的scene
        public void UnloadAllUnuseScenes()
        {
            foreach(var item in mPreLoaded)
            {
                Unload(item.mPreLoadName);
            }
            foreach(var item in mPreLoadCache)
            {
                Unload(item.mPreLoadName);
            }
        }


        IEnumerator UnloadAsync(string scene)
        {
            //对于未加载完的，等待加载完成
            yield return IsLoadedOver(scene);
            SceneManager.UnloadSceneAsync(scene);
            //移除
            RemoveFromLoadedOver(scene);
        }

        IEnumerator PreLoadAsync(string scene)
        {
            PreLoadArg arg = new PreLoadArg();
            arg.mPreLoadName = scene;
            arg.mAsync = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            arg.mAsync.allowSceneActivation = true;
            mPreLoadCache.Add(arg);
            while (!arg.mAsync.isDone)
            {
                yield return null;
            }
            //加载完之后加入到list
            mPreLoaded.Add(arg);
            //从正在加载的列表中移除
            mPreLoadCache.Remove(arg);
            //隐藏该scene
            HideLoadedScene(scene);

        }
        //进入预加载scene（删除其他）
        IEnumerator EnterPreLoad(string scene, Action onSwitchFinished, OnSceneSwitchingCallback callback)
        {
            yield return SwitchPreLoad(scene, onSwitchFinished, callback);
            //待显示完成后，移除所有其他的scene
            UnloadAllUnuseScenes();
        }
        //仅进行scene交换
        IEnumerator SwitchPreLoad(string scene,Action onSwitchFinished,OnSceneSwitchingCallback callback)
        {
            //如果当前preloaded中不存在，则将当前的scene保存
            string lastScene = mCurScene;
            if (!IsLoaded(mCurScene))
            {
                PreLoadArg arg = new PreLoadArg();
                arg.mPreLoadName = mCurScene;
                arg.mAsync = mAsyncOperation;
                mPreLoaded.Add(arg);
            }
            IsSwithing = true;
            //置换参数
            mCurScene = scene;
            foreach (var item in mPreLoaded)
            {
                if (item.mPreLoadName == mCurScene)
                {
                    mAsyncOperation = item.mAsync;
                }
            }
            yield return IsLoadedOver(scene);//等待加载完成
            yield return callback==null||callback();//播放转场效果
            //隐藏当前scene
            HideLoadedScene(lastScene);
            //显示目标scene
            ShowLoadedScene(scene);
            
            IsSwithing = false;
        }


        //=================
        IEnumerator LoadSceneAsyncAutoShow(string scene,Action onSceneLoadFinished,Action<float> onSceneLoading)
        {
            IsSwithing = true;
            this.onSceneLoaded = (sceneName) =>
            {
                if (sceneName == scene)
                {
                    this.onSceneLoaded = null;
                    if (onSceneLoadFinished != null) onSceneLoadFinished();
                }
            };
            mCurScene = scene;
            mAsyncOperation = SceneManager.LoadSceneAsync(scene);
            mAsyncOperation.allowSceneActivation = false;

            while (!mAsyncOperation.isDone)
            {
                onSceneLoading(mAsyncOperation.progress);
                Debuger.Log("current progress:{0}", mAsyncOperation.progress);
                if(mAsyncOperation.progress>=0.9f)
                {
                    onSceneLoading(1.0f);
                    Debuger.Log("change states called");
                    mAsyncOperation.allowSceneActivation = true;
                }
                yield return null;
            }
            IsSwithing = false;
            Debuger.Log("load scene finished");
        }

        //===================
        //是否在加载或加载中
        private bool IsLoaded(string scene)
        {
            if (IsLoadedOver(scene))
            {
                return true;
            }
            //检查正在加载的
            foreach(var item in mPreLoadCache)
            {
                if (item.mPreLoadName == scene)
                {
                    return true;
                }
            }
            return false;
        }
        //是否已经加载完成
        private bool IsLoadedOver(string scene)
        {
            //检查已加载的
            foreach (var item in mPreLoaded)
            {
                if (item.mPreLoadName == scene)
                {
                    return true;
                }
            }
            return false;
        }

        //从已加载完成中移除
        private void RemoveFromLoadedOver(string scene)
        {
            for(int i = mPreLoaded.Count - 1; i >= 0; --i)
            {
                if (mPreLoaded[i].mPreLoadName == scene)
                {
                    mPreLoaded.RemoveAt(i);
                }
            }
        }

        private void ShowLoadedScene(string scene)
        {
            Scene s = SceneManager.GetSceneByName(scene);
            if (s != null)
            {
                //将该scene设为active
                SceneManager.SetActiveScene(s);
                foreach (var obj in s.GetRootGameObjects())
                {
                    if (!obj.activeSelf)
                        obj.SetActive(true);
                }
            }
            else
            {
                Debuger.LogError("cannot show scene:{0}, not found", scene);
            }
        }

        private void HideLoadedScene(string scene)
        {
            Scene s = SceneManager.GetSceneByName(scene);
            if (s != null)
            {
                foreach (var obj in s.GetRootGameObjects())
                {
                    if (obj.activeSelf)
                        obj.SetActive(false);
                }
            }
            else
            {
                Debuger.LogError("cannot show scene:{0}, not found", scene);
            }
        }
    }
}
