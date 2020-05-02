using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Global.Data;
using QGF.Codec;
using QGF.Common;
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


        GameActionData mTempAction = new GameActionData();
        v3i mTempPos = new v3i();
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
            
        }

        public void Start(FSPParam param)
        {

            mFSP.Start(param, mPid);
            mFSP.SetFrameListener(OnRecv);
        }

        //开始游戏
        public void StartGame()
        {
            mFSP.SendGameBegin();
        }

        //移动
        public void Move(RoleController role,Position target)
        {
            byte[] content = MoveManager.Instance.CreateMoveMsg(role, role.Position, target);
            SendAction(GameAction.Move, content);
        }

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

        private void SendAction(GameAction actionType,byte[] content)
        {
            mTempAction.OperationType = actionType;
            mTempAction.OperationCnt = content;
            mFSP.SendFSP(1, mTempAction);
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
