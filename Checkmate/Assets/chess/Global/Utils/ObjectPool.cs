using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace Checkmate.Game.Utils
{
    internal class PrefabMap
    {
        public string name;
        public string prefab;
    }
    public class ObjectPool:Singleton<ObjectPool>
    {
        private Dictionary<string, List<GameObject>> mGameObjectPool;//游戏对象池

        private Dictionary<string, GameObject> mPrefabs;//所有预制体的缓存


#if UNITY_EDITOR
        private static readonly string ConfigPath = Application.dataPath+"/Config/Prefab.json";
#else
        private static readonly string ConfigPath = Application.dataPath + "/Config/Prefab.json";
#endif
        /// <summary>
        /// 初始化对象池
        /// </summary>
        /// <param name="size">初始化每个类别对应的对象数</param>
        /// <param name="types">初始化的对象类别</param>
        public void Init(int size,List<string> types)
        {
            //初始化预制体映射
            InitPrefabs(ConfigPath);

            //初始化对象池
            mGameObjectPool = new Dictionary<string, List<GameObject>>();
            foreach(var type in types)
            {
                List<GameObject> objList;
                //无则生成
                if (!mGameObjectPool.ContainsKey(type))
                {
                    objList= new List<GameObject>(size);
                    mGameObjectPool.Add(type, objList);
                }
                objList = mGameObjectPool[type];

                //获取预制体
                GameObject prefab = mPrefabs[type];

                //生成实例并添加至池
                for(int i = 0; i < size; ++i)
                {
                    GameObject obj = GameObject.Instantiate(prefab);
                    obj.SetActive(false);
                    obj.name = type + i.ToString();
                    objList.Add(obj);
                }
                
            }
        }

        //获取对象
        public GameObject GetGameObject(string type)
        {
            GameObject result;
            //如果包含该类的池
            if (mGameObjectPool.ContainsKey(type))
            {
                //如果存在剩余对象
                if (mGameObjectPool[type].Count > 0)
                {
                    result= mGameObjectPool[type][0];
                    result.SetActive(true);
                    mGameObjectPool[type].Remove(result);
                    return result;
                }
            }

            //不然的话直接生成
            GameObject prefab = null;
            //如果已经加载过该预设体
            if (mPrefabs.ContainsKey(type))
            {
                prefab = mPrefabs[type];
            }
            else     //如果没有加载过该预设体
            {
                //加载预设体
                prefab = Resources.Load<GameObject>(type+"/Prefab");
                //更新字典
                mPrefabs.Add(type, prefab);
            }


            result = GameObject.Instantiate(prefab);
            result.name = type;

            return result;
        }

        //回收对象
        public void Recycle(GameObject obj,string type)
        {
            obj.SetActive(false);
            if (mGameObjectPool.ContainsKey(type))
            {
                //包含该类的池则直接加入
                mGameObjectPool[type].Add(obj);
            }
            else
            {
                //否则新建，并加入
                mGameObjectPool.Add(type, new List<GameObject>() { obj });
            }
        }


        //初始化预制体映射
        private void InitPrefabs(string configPath)
        {           
            mPrefabs = new Dictionary<string, GameObject>();
            string content = File.ReadAllText(configPath);
            List<PrefabMap> prefabs = JsonConvert.DeserializeObject<List<PrefabMap>>(content);
            //读取配置文件
            foreach(var prefab in prefabs)
            {
                //生成预制体，添加至映射
                GameObject prefabObj = Resources.Load<GameObject>(prefab.prefab);
                mPrefabs.Add(prefab.name, prefabObj);
            }
        }

    }
}
