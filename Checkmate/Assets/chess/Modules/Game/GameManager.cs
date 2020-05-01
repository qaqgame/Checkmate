using Checkmate.Game;
using Checkmate.Global.Data;
using Checkmate.Game.Map;
using Checkmate.Game.Role;
using Checkmate.Modules.Game.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Checkmate.Game.Controller;
using QGF.Network.FSPLite.Client;
using Checkmate.Game.Utils;
using Checkmate.Game.Skill;
using Checkmate.Modules.Game.Control;
using Checkmate.Game.Player;
using QGF.Network.General.Client;
using Checkmate.Services.Online;
using Checkmate.Services.Game;
using QGF.Codec;
using QGF.Network.FSPLite;
using QGF;

namespace Checkmate.Modules.Game
{
    public class GameManager:MonoBehaviour
    {
        public static GameManager Instance;

        private static Action onInitFinished;

        

        static readonly List<string> types = new List<string>()
        {
            RoleManager.prefabType
        };

        //初始化函数
        public void Init(Action onInitComplete)
        {
            onInitFinished = onInitComplete;
            StartCoroutine(InitAll());
        }
        private void OnInitFinished()
        {
            if (onInitFinished != null)
            {
                onInitFinished.Invoke();
            }
        }
        //初始化所有的协程
        public IEnumerator InitAll()
        {
            ObjectPool.Instance.Init(10, types);
            HexGrid hexGrid = GameObject.Find("Map").GetComponentInChildren<HexGrid>();
            MapManager.Instance.Init(hexGrid, Application.dataPath + "/Test/testMap.map");

            SkillManager.Instance.Init(Application.dataPath + "/Test");
            DrawUtil.Init();

            RoleManager.Instance.Init();

            MoveManager.Instance.Init();

            GameEnv.Instance.Init();//初始化环境

            ExecuteUtil.Instance.Init(Application.dataPath + "/Test");


            APManager.Instance.Init();

            InitEvent();

            OnInitFinished();
            yield return true;
        }


        private void Awake()
        {
            Instance = this;
            
        }

        private void Start()
        {
            
            //=============================
            //测试部分
            //InitTestPlayer();
            

            RoleData alice = JsonConvert.DeserializeObject<RoleData>(File.ReadAllText(Application.dataPath + "/Test/Alice.json"));
            AddRole(alice);
            alice.id = 1;
            alice.model = "Bob";
            alice.name = "Bob";
            alice.position.x = 2;
            alice.position.y = -4;
            alice.position.z = 2;
            AddRole(alice);
            Debug.Log("extra:" + RoleManager.Instance.GetRole(1).GetValue("Current.test"));
            RemoveRole(0);

            RoleController role = RoleManager.Instance.GetRole(1);
            Debug.Log(role.Name);
            CellController cell = MapManager.Instance.GetCell(role.Position);
            Debug.Log(cell.Terrain);

            Debug.Log(role.CanStand(cell.Terrain));

            int sid = SkillManager.Instance.GetSkill("TestSkill");
            Debug.Log("load skill suc:" + sid);
        }


        //============================
        public void InitTestPlayer()
        {
            //MaskData data = new MaskData();
            //data.pid = 1;
            //data.enemyMask = 0x00ff;
            //data.friendMask = 0xff00;
            //PlayerTeamData pd = new PlayerTeamData();
            //pd.masks = new List<MaskData>();
            //pd.masks.Add(data);

            //PlayerManager.Instance.Init(pd);
            //PlayerManager.Instance.PID = 1;
        }

        //初始化player数据
        public void InitPlayer(PlayerTeamData data,uint pid,FSPParam param)
        {
            PlayerManager.Instance.Init(data);
            PlayerManager.Instance.PID = pid;
            GameNetManager.Instance.Init(pid);//初始化网络管理器
            Debuger.Log("pid:{0},param:{1}", pid, param.ToString());
            GameNetManager.Instance.Start(param);
            GameNetManager.Instance.SetActionListener(HandleAction);
            GameNetManager.Instance.StartGame();
        }

        // Test move
        public void TestMoveObj()
        {
            RoleController rc = RoleManager.Instance.GetRole(1);
            string test = "{\"OperationType\":\"Move\",\"OperationCnt\":{\"StartPosition\":\"" + "(2,-1,-1)" + "\",\"MoveDirection\":[0,0,0,2,3],\"EndPosition\":\"" + "(2,2,2)" + "\"},\"OperationObjID\":1}";
            MoveManager.Instance.Move(test);
        }


        // Update: Update is Called pear frame
        float last=0;
        void Update()
        {
            
            MoveManager.Instance.Update();

            InputManager.Instance.HandleInput();

            GameNetManager.Instance.Update();

            //===================================
            //测试部分
            //if (Time.time - last > 5)
            //{
            //    RoleController role = RoleManager.Instance.GetRole(1);
            //    role.Current.Hp -= 10;
            //    last = Time.time;
            //}
        }
        //=================================

        //消息处理分发函数
        private void HandleAction(byte[] message)
        {
            GameActionData action= PBSerializer.NDeserialize<GameActionData>(message);
            switch (action.OperationType)
            {
                case GameAction.Move:
                    {
                        return;
                    }
            }
        }

        public void AddRole(RoleData data)
        {
            RoleController controller = RoleManager.Instance.AddRole(data);
            Debug.Log(data.name + ",data:" + controller.GetValue("Id"));

        }

        public void RemoveRole(int id)
        {
            RoleManager.Instance.RemoveRole(id);
        }

        private void InitEvent()
        {
            GameEvent.Init();
            GameEvent.onControllerClicked.AddListener(OnControllerClick);
            GameEvent.onRoleClicked.AddListener(OnRoleClicked);
            GameEvent.onResetAll.AddListener(OnResetState);
        }

        private void OnControllerClick(ModelController controller)
        {
            Debug.Log("click pos:" + controller.GetPosition().ToString());
            DrawUtil.ClearAll();
            DrawUtil.DrawSingle(controller.GetPosition(), 1);
        }

        private void OnRoleClicked(RoleController role)
        {
            Debug.Log("clicked role:" + role.Name);
        }

        private void OnResetState()
        {
            DrawUtil.ClearAll();
        }
    }
}
