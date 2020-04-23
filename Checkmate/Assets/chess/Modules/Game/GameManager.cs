using Checkmate.Game;
using Checkmate.Global.Data;
using Checkmate.Game.Map;
using Checkmate.Game.Role;
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
            MapManager.Instance.Init(hexGrid,Application.dataPath + "/Test/testMap.map");

            DrawUtil.Init(MapManager.Instance);

            RoleManager.Instance.Init();

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
            Debug.Log("extra:" + RoleManager.Instance.GetRole(1).GetValue("Current.test"));
            RemoveRole(0);

        }


        public void AddRole(RoleData data)
        {
            RoleController controller =RoleManager.Instance.AddRole(data);
            Debug.Log(data.name + ",data:"+controller.GetValue("Id"));
            
        }

        public void RemoveRole(int id)
        {
            RoleManager.Instance.RemoveRole(id);
        }
    }
}
