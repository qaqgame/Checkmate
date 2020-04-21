using Checkmate.Game;
using Checkmate.Global.Data;
using Checkmate.Modules.Game.Map;
using Checkmate.Modules.Game.Role;
using Checkmate.Modules.Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Checkmate.Game.Controller;
using QGF.Network.FSPLite.Client;

namespace Checkmate.Modules.Game
{
    public class GameManager:MonoBehaviour
    {
        public FSPManager fsp;
        public FSPClient client;
        public static GameManager Instance;

        public static MapManager MapManager
        {
            get { return mMapMng; }
        }



        private static MapManager mMapMng;//地图管理

        private static RoleManager mRoleMng;//角色管理

        static readonly List<string> types = new List<string>()
        {
            RoleManager.prefabType
        };

        private void Awake()
        {
            Instance = this;
            ObjectPool.Instance.Init(10, types);
        }

        private void Start()
        {
            HexGrid hexGrid = GameObject.Find("Map").GetComponentInChildren<HexGrid>();
            mMapMng = new MapManager();
            mMapMng.Init(hexGrid,Application.dataPath + "/Test/testMap.map");

            DrawUtil.Init(mMapMng);

            mRoleMng = new RoleManager(mMapMng);

            GameEnv.Instance.Init();//初始化环境
            //=============================
            //测试部分
            RoleData alice = JsonConvert.DeserializeObject<RoleData>(File.ReadAllText(Application.dataPath + "/Test/Alice.json"));
            AddRole(alice);
            alice.id = 1;
            alice.model = "Bob";
            alice.name = "Bob";
            alice.position.x = 2;
            alice.position.y = -1;
            alice.position.z = -1;
            AddRole(alice);
            Debug.Log("extra:" + mRoleMng.GetRole(1).GetValue("Current.test"));
            RemoveRole(0);

        }


        public void AddRole(RoleData data)
        {
            RoleController controller = mRoleMng.AddRole(data);
            Debug.Log(data.name + ",data:"+controller.GetValue("Id"));
            
        }

        public void RemoveRole(int id)
        {
            mRoleMng.RemoveRole(id);
        }
    }
}
