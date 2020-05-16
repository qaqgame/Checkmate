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
using Checkmate.Game.Buff;
using Checkmate.Game.Effect;
using QGF.Utils;

namespace Checkmate.Modules.Game
{
    public class GameManager:MonoBehaviour
    {
        public static GameManager Instance;

        private static Action onInitFinished;

        bool wait = true;
        //#if UNITY_EDITOR
        static string mTestMapPath ;
        static string mSkillPath;
        static string mScriptPath;
        static string mTestRolePath;
        //#else
        //        static string mTestMapPath = Application.streamingAssetsPath + "/Test/testMap.map";
        //        static string mSkillPath = Application.streamingAssetsPath + "/Skills";
        //        static string mScriptPath = Application.streamingAssetsPath + "/Scripts";
        //        static string mTestRolePath = Application.streamingAssetsPath + "/Test/Alice.json";
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
            Debuger.Log("invoke initall");
            ObjectPool.Instance.Init(10, types);
            Debuger.Log("object pool init");
            HexGrid hexGrid = GameObject.Find("Map").GetComponentInChildren<HexGrid>();
            Debuger.Log("grid get suc");
            MapManager.Instance.Init(hexGrid, mTestMapPath);
            Debuger.Log("map init");
            SkillManager.Instance.Init(mSkillPath);
            Debuger.Log("skill init");
            DrawUtil.Init();
            Debuger.Log("draw init");
            RoleManager.Instance.Init();
            Debuger.Log("role init");
            MoveManager.Instance.Init();
            Debuger.Log("move init");
            GameEnv.Instance.Init();//初始化环境
            Debuger.Log("env init");

            ExecuteUtil.Instance.Init(mScriptPath);
            Debuger.Log("execute init");

            APManager.Instance.Init();
            APManager.Instance.AddListener(OnAPUpdate);
            Debuger.Log("ap init");

            BuffManager.Instance.Init();

            InitEvent();

            while (wait)
            {
                Debuger.Log("init waiting");
                yield return null;
            }

            OnInitFinished();
            GameNetManager.Instance.StartGame();
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
            
            //=============================
            //测试部分
            //InitTestPlayer();


            RoleData alice = JsonConvert.DeserializeObject<RoleData>(FileUtils.ReadString(mTestRolePath));
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

            alice.id = 2;
            alice.name = "Frank";
            alice.team = 2;
            alice.position.x = 3;
            alice.position.y = -6;
            alice.position.z = 3;
            AddRole(alice);

            RoleController role = RoleManager.Instance.GetRole(1);
            Debug.Log(role.Name);
            
            CellController cell = MapManager.Instance.GetCell(role.Position);
            Debug.Log(cell.Terrain);

            Debug.Log(role.CanStand(cell.Terrain));

            int sid = SkillManager.Instance.GetSkill("TestSkill");
            Debug.Log("load skill suc:" + sid);

            //-------load page
            GamingPageManager.Instance.OpenPage();
            GamingPageManager.Instance.onRoundEndClicked = OnRoundEndClick;



            IMode mode = ModeParser.ParseMode("KillMode");
            StartCoroutine(CheckGameCondition(mode));
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
            GameNetManager.Instance.onRoundEnd = OnRoundEnd;
            GameNetManager.Instance.onRoundBegin = OnRoundBegin;
            wait = false;
        }

        private void OnControlStart(uint pid)
        {
            Debuger.Log("control start:{0}", pid);
            if (pid == PlayerManager.Instance.PID)
            {
                PlayerManager.Instance.Operating = true;
                GamingPageManager.Instance.OnNextTurn(true);
            }
            else
            {
                PlayerManager.Instance.Operating = false;
                GamingPageManager.Instance.OnNextTurn(false);
            }
        }
        //===========================================
        //回合结束点击时处理
        private void OnRoundEndClick()
        {
            GameNetManager.Instance.EndRound();
        }
        //回合结束处理(此处为更新行动顺序)
        private void OnRoundEnd(byte[] content)
        {
            //更新操作


            //操作结束后进入下一回合
            GameNetManager.Instance.StartRound();
        }

        //============================================
        //回合开始处理
        private void OnRoundBegin(bool needTurn)
        {
            if (needTurn)
            {
                //下一回合
                //更新行动点
                APManager.Instance.Reset();
                //更新buff
                BuffManager.Instance.NextTurn();
                //更新地面效果
                EffectManager.Instance.NextTurn();

            }
            StartCoroutine(WaitForBeginControl());
        }
        IEnumerator WaitForBeginControl()
        {
            while (GameExecuteManager.Instance.WaitForExecute)
            {
                yield return null;
            }
            GameNetManager.Instance.StartControl();
        }

        //===============================
        //AP更新事件
        private void OnAPUpdate()
        {
            GamingPageManager.Instance.UpdateAP();
        }
        //===========================================

        // Update: Update is Called pear frame
        float last=0;
        void Update()
        {
            
            MoveManager.Instance.Update();

            InputManager.Instance.HandleInput();

            GameNetManager.Instance.Update();

            //===================================
            //测试部分
            if (Time.time - last > 3)
            {
                RoleController role = RoleManager.Instance.GetRole(1);
                if (role.Buffs.Count > 0)
                {
                    Buff buff = BuffManager.Instance.GetBuff(role.Buffs[0]);
                    Debuger.Log("{0}'s buff {1} has {2} tracks,{3} roles", role.Name, buff.Name, buff.Current.Tracks.Count, buff.Current.mUsedRoles.Count);
                }
                Debuger.Log("{0} physicRes:{1},current:{2}", role.Name, role.Temp.GetValue("PhysicalRes"), role.Current.GetValue("PhysicalRes"));
                Debuger.Log("{0} tempHP:{1},currentHP:{2}", role.Name, role.Temp.Hp, role.Current.Hp);

                last = Time.time;
            }
        }


        IEnumerator CheckGameCondition(IMode mode)
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                //帧结束时判断是否游戏结束
                int winner;
                //如果满足则结束
                if(mode.CheckEnd(out winner))
                {
                    GameNetManager.Instance.EndGame(winner);
                }
            }
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
        //==============================
        //UI事件部分
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
            GamingPageManager.Instance.ShowRolePanel(role);
        }

        private void OnResetState()
        {
            DrawUtil.ClearAll();
            GamingPageManager.Instance.HideRolePanel();
        }
    }
}
