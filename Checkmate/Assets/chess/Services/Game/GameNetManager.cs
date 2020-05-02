using QGF.Common;
using QGF.Network.FSPLite;
using QGF.Network.FSPLite.Client;
using QGF.Network.General.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Services.Game
{
    //网络管理器
    public class GameNetManager:Singleton<GameNetManager>
    {
        FSPManager mFSP;
        uint mPid;

        Action<byte[]> onActionRecv=null;//接收到操作后的处理(发送至外部)

        const int ActionCmd = 7;

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

        //

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
