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


//#if UNITY_EDITOR
        string mTestMapPath;
        string mSkillPath;
        string mScriptPath;
        string mTestRolePath;
//#else
//        string mTestMapPath = Application.streamingAssetsPath + "/Test/testMap.map";
//        string mSkillPath = Application.streamingAssetsPath+ "/Skills";
//        string mScriptPath = Application.streamingAssetsPath + "/Scripts";
//        string mTestRolePath = Application.streamingAssetsPath + "/Test/Alice.json";
//#endif
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
            MapManager.Instance.Init(hexGrid, mTestMapPath);

            SkillManager.Instance.Init(mSkillPath);
            DrawUtil.Init();

            RoleManager.Instance.Init();

            MoveManager.Instance.Init();

            GameEnv.Instance.Init();//初始化环境
            Debuger.Log("env init");

            ExecuteUtil.Instance.Init(mScriptPath);
            Debuger.Log("execute init");

            APManager.Instance.Init();
            Debuger.Log("ap init");

            InitEvent();

            OnInitFinished();
            yield return true;
        }


        private void Awake()
        {
            Instance = this;
#if UNITY_EDITOR
            mTestMapPath=Application.dataPath + "/Test/testMap.map";
            mSkillPath=Application.dataPath + "/Test";
            mScriptPath=Application.dataPath + "/Test";
            mTestRolePath= Application.dataPath + "/Test/Alice.json";
#else
            mTestMapPath = Application.streamingAssetsPath + "/Test/testMap.map";
            mSkillPath = Application.streamingAssetsPath + "/Skills";
            mScriptPath = Application.streamingAssetsPath + "/Scripts";
            mTestRolePath = Application.streamingAssetsPath + "/Test/Alice.json";
#endif

        }

        private void Start()
        {
            GameNetManager.Instance.StartGame();
            //=============================
            //测试部分
            //InitTestPlayer();


            RoleData alice = JsonConvert.DeserializeObject<RoleData>(File.ReadAllText(mTestRolePath));
            AddRole(alice);
            alice.id = 1;
            alice.model = "Bob";
            alice.name = "Bob";
            alice.team = 0;
            alice.position.x = 2;
            alice.position.y = -4;
            alice.position.z = 2;
            AddRole(alice);
            Debug.Log("extra:" + RoleManager.Instance.GetRole(1).GetValue("Current.test"));
 

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
            PlayerManager.Instance.Operating = false;
            GameNetManager.Instance.Init(pid);//初始化网络管理器
            Debuger.Log("pid:suib{0},param:{1}", pid, param.ToString());
            GameNetManager.Instance.Start(param);
            GameNetManager.Instance.SetActionListener(HandleAction);
            GameNetManager.Instance.onControlStart = OnControlStart;
        }

        private void OnControlStart(byte[] content)
        {
            PlayerManager.Instance.Operating = true;
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
            Debuger.Log("recv action");
            GameActionData action= PBSerializer.NDeserialize<GameActionData>(message);
            switch (action.OperationType)
            {
                case GameAction.Move:
                    {
                        MoveManager.Instance.Execute(action.OperationCnt);
                        return;
                    }
                case GameAction.Skill:
                    {
                        SkillManager.Instance.Execute(action.OperationCnt);
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
