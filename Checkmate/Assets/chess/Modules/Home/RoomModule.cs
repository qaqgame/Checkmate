using Checkmate.Module.UI;
using QGF.Module;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Module
{
    public class RoomModule:GeneralModule
    {

        private RoomManager mRoomManager;
        public override void Create(object args = null)
        {
            base.Create(args);
            mRoomManager = new RoomManager();
            mRoomManager.Init();
        }


        public override void Release()
        {
            if (mRoomManager != null)
            {
                mRoomManager.Clear();
                mRoomManager = null;
            }
            base.Release();
        }

        protected override void Show(object arg)
        {
            base.Show(arg);
            mRoomManager.Reset();
            //打开房间的ui,将manager传入
            FGUIManager.Instance.OpenPage<RoomListPage>("Room", "Home", mRoomManager);
        }
    }
}
