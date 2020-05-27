using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Game.Player;
using Checkmate.Global.Data;
using ProtoBuf;
using QGF;
using QGF.Codec;
using QGF.Common;
using QGF.Network.Core;
using QGF.Network.FSPLite;
using QGF.Network.FSPLite.Client;
using QGF.Network.General.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Checkmate.Game.Skill.SkillManager;

namespace Checkmate.Services.Game
{
    //网络管理器
    public class GameNetManager:Singleton<GameNetManager>
    {
        FSPManager mFSP;
        uint mPid;

        Action<byte[]> onActionRecv=null;//接收到操作后的处理(发送至外部)

        const int ActionCmd = 7;
        const int EndCmd = 8;//游戏结束的标识
        const int EmptyCmd = 404;//空包的标识

        public Action<bool> onRoundBegin;//回合开始事件
        public Action<uint> onControlStart=null;//操作开始
        public Action<byte[]> onRoundEnd = null;//回合结束
        public Action<bool> onGameEnd = null;//游戏结束事件

        GameActionData mTempAction = new GameActionData();
        v3i mTempPos = new v3i();

        private static NetBuffer DefaultReader = new NetBuffer(4096);
        public void SetActionListener(Action<byte[]> actionHandler)
        {
            onActionRecv = actionHandler;
        }

        public void Init(uint pid)
        {
            mFSP = new FSPManager();
            mPid = pid;
        }

        public void Clear()
        {
            mFSP.Stop();
            mFSP.SetFrameListener(null);
            mFSP.onGameBegin -= OnGameBegin;
            mFSP.onRoundBegin -= OnRoundBegin;
            mFSP.onControlStart -= OnControlBegin;
            mFSP.onRoundEnd -= OnRoundEnd;
            mFSP.onGameEnd -= OnGameEnd;
            onActionRecv = null;
        }

        public void Start(FSPParam param)
        {

            mFSP.Start(param, mPid);
            mFSP.SetFrameListener(OnRecv);
            mFSP.onGameBegin += OnGameBegin;
            mFSP.onRoundBegin += OnRoundBegin;
            mFSP.onControlStart += OnControlBegin;
            mFSP.onRoundEnd += OnRoundEnd;
            mFSP.onGameEnd += OnGameEnd;
        }

        //创建
        public void CreateGame()
        {
            
        }

        private void OnLogicRoundBegin() {
            mFSP.SendControlStart();
        }

        

        //开始游戏
        public void StartGame()
        {
            mFSP.SendGameBegin();
            Debuger.Log("send game beign");
        }

        //收到gamebegin的处理
        private void OnGameBegin(byte[] content)
        {
            StartRound();
        }


        public void StartRound()
        {
            mFSP.SendRoundBegin();
            Debuger.Log("send round begin");
        }

        private void OnRoundBegin(byte[] content)
        {
            if (onRoundBegin != null)
            {
                Debuger.Log("recv round begin:{0}", content.Length);
                Debuger.Log("data:{0}", content.ToString());
                lock (DefaultReader)
                {
                    DefaultReader.Attach(content,content.Length);
                    uint param = DefaultReader.ReadUInt();
                    bool condition = param != 0;
                    onRoundBegin.Invoke(condition);
                }
            }
        }

        //可以开始操作
        public void StartControl()
        {
            mFSP.SendControlStart();
            Debuger.Log("send control start");
        }
        //接收到可以操作的消息
        private void OnControlBegin(byte[] content)
        {
            Debuger.Log("recv control begin");
            if (onControlStart != null)
            {
                lock (DefaultReader)
                {
                    DefaultReader.Attach(content, content.Length);
                    uint id = DefaultReader.ReadUInt();
                    onControlStart.Invoke(id);
                }
            }
        }

        //结束回合
        public void EndRound()
        {
            int reserve = APManager.Instance.elementAP[(int)PlayerManager.Instance.PID].GetCurrentAP();
            //发送当前所有用户的剩余行动点

            mFSP.SendRoundEnd(reserve);
            Debuger.Log("send round end");
        }
        //结束时调用
        private void OnRoundEnd(byte[] content)
        {
            if (onRoundEnd != null)
            {
                onRoundEnd.Invoke(content);
            }
        }

        //结束游戏
        public void EndGame(int winner)
        {
     
            mFSP.SendGameEnd(winner);
            Debuger.LogWarning("end game with:{0}", winner);
        }

        [ProtoContract]
        private class EndMsg
        {
            [ProtoMember(1)]
            public List<uint> winners;
        }
        //游戏结束时调用
        private void OnGameEnd(byte[] content)
        {
            Debuger.Log("recv gameend");
            EndMsg msg=PBSerializer.NDeserialize<EndMsg>(content);
            bool result =msg.winners!=null&&msg.winners.Count>0&&msg.winners.Contains(PlayerManager.Instance.PID);
            if (onGameEnd != null)
            {
                onGameEnd.Invoke(result);
            }
        }

        public void GameExit()
        {
            mFSP.SendGameExit();
        }

        //移动
        public void Move(RoleController role,Position target)
        {
            byte[] content = MoveManager.Instance.CreateMoveMsg(role, role.Position, target);
            Debuger.Log("send move msg:{0} move to{1}", role.Name, target.ToString());
            SendAction(GameAction.Move, content);
        }
        //施放技能
        public void Skill(int skillId,RoleController role,Position center)
        {
            mTempPos.x = center.x;
            mTempPos.y = center.y;
            mTempPos.z = center.z;

            SkillMessage content = new SkillMessage();
            content.center = mTempPos;
            content.roleId = role.RoleId;
            content.skillId = skillId;

            byte[] cnt = PBSerializer.NSerialize(content);
            SendAction(GameAction.Skill, cnt);

        }

        public void Attack(RoleController src,Position center)
        {
            mTempPos.x = center.x;
            mTempPos.y = center.y;
            mTempPos.z = center.z;

            AttackMessage content = new AttackMessage();
            content.srcId = src.RoleId;
            content.center = mTempPos;

            byte[] cnt = PBSerializer.NSerialize(content);
            SendAction(GameAction.Attack, cnt);
        }

        private void SendAction(GameAction actionType,byte[] content)
        {
            Debuger.Log("send action:{0}", actionType.ToString());
            mTempAction.OperationType = actionType;
            mTempAction.OperationCnt = content;
            mFSP.SendFSP(ActionCmd, mTempAction);
        }

        public void Stop()
        {
            mFSP.Stop();
        }

        public void Update()
        {
            mFSP.Tick();
        }

        private void OnRecv(int frame,FSPMessage message)
        {
            switch (message.cmd)
            {
                case ActionCmd:
                    {
                        OnRecvAction(message.content);
                        return;
                    }
            }
        }


        
        private void OnRecvAction(byte[] content)
        {
            
            if (onActionRecv != null)
            {
                onActionRecv.Invoke(content);
            }
        }

    }
}
