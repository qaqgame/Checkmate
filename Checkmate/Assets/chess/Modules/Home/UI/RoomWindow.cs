using Checkmate.Global.Data;
using Checkmate.Module.UI.Component;
using FairyGUI;
using QGF;
using QGF.Module;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Module.UI
{
    public class RoomWindow:FGUIWindow
    {
        GButton mReadyBtn, mExitBtn;
        PlayerList mPlayerList;//玩家列表ui
        GComboBox mTeamSelector;//队伍选择
        GTextField mRoomTitle, mMapTitle, mModeInfo,mCountInfo;

        RoomManager mManager;
        List<string> teamItems;//队伍选择项

        protected override bool IsModal { get { return true; } }

        protected override void OnLoad()
        {
            base.OnLoad();
            mReadyBtn = mCtrlTarget.GetChild("BtnReady").asButton;
            mExitBtn = mCtrlTarget.GetChild("BtnExit").asButton;
            GList pl = mCtrlTarget.GetChildByPath("PlayerList.List").asList;
            mPlayerList = new PlayerList(pl, 0);
            mTeamSelector = mCtrlTarget.GetChild("TeamSelector").asComboBox;
            mRoomTitle = mCtrlTarget.GetChild("Title").asTextField;
            mMapTitle = mCtrlTarget.GetChild("Map").asTextField;
            mModeInfo = mCtrlTarget.GetChild("Mode").asTextField;
            mCountInfo = mCtrlTarget.GetChild("Count").asTextField;

            mCountInfo.visible = false;

            mReadyBtn.onClick.Add(OnBtnReadyClicked);
            mExitBtn.onClick.Add(OnBtnExitClicked);

            mTeamSelector.onChanged.Add(OnTeamChanged);
            teamItems = new List<string>(10);
        }

        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);
            mManager = arg as RoomManager;
            //添加更新事件
            mManager.onUpdateRoomInfo.AddListener(OnUpdateRoomInfo);
            Debuger.Log("add listener");
            mManager.UpdateRoomInfo();
            //使窗口居中
            mWindow.AddRelation(GRoot.inst, RelationType.Center_Center);
        }

        //在更新房间内信息时被调用
        void OnUpdateRoomInfo(RoomData data)
        {
            Debuger.Log("room info update called");
            mRoomTitle.text = data.name;
            mMapTitle.SetVar("map", data.map);
            mModeInfo.SetVar("mode", data.mode);
            

            //更新playerlist
            mPlayerList.Update(data);
            teamItems.Clear();
            for (int i = 0; i < data.teams.Count; ++i)
            {
                teamItems.Add(i.ToString());
            }
            mTeamSelector.items = teamItems.ToArray();

            mTeamSelector.selectedIndex = (int)mManager.TeamId;

            if (data.ready)
            {
                mCountInfo.visible = true;
                mCountInfo.text = data.time.ToString();
            }
            else
            {
                mCountInfo.visible = false;
            }
        }

        //准备按钮
        void OnBtnReadyClicked()
        {
            mManager.RoomReady(true);
        }
        //退出按钮
        void OnBtnExitClicked()
        {
            mManager.ExitRoom();
        }

        //下拉框
        void OnTeamChanged()
        {
            //改变后发送消息给服务器
            mManager.ChangeTeam((uint)mTeamSelector.selectedIndex);
        }

        protected override void OnPanelDestroy()
        {
            base.OnPanelDestroy();
            mManager.onUpdateRoomInfo.RemoveListener(OnUpdateRoomInfo);
        }
    }
}
