using QGF.Common;
using QGF.Network.FSPLite;
using QGF.Network.FSPLite.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Modules.Game
{
    //网络管理器
    public class GameNetManager:Singleton<GameNetManager>
    {
        FSPManager mFSP;
        uint mPid;
        public void Init(uint pid)
        {
            mFSP = new FSPManager();
            mPid = pid;
        }

        public void Clear()
        {

        }

        public void Start(bool local=false)
        {
            FSPParam param = new FSPParam();
            param.useLocal = local;

            mFSP.Start(param, mPid);
        }

        public void Stop()
        {
            mFSP.Stop();
        }

        public void Update()
        {
            mFSP.Tick();
        }
    }
}
