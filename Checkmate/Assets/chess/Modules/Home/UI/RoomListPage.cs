using Checkmate.Global.Data;
using FairyGUI;
using QGF;
using QGF.Unity.FGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Checkmate.Module.UI
{

    //房间列表UI
    public class RoomListPage : FGUIPage
    {
        public VirtualList mRooms;//房间列表的ui
        private RoomManager mManager;//房间列表的管理
        protected override void OnLoad()
        {
            base.OnLoad();
            //初始化房间列表UI
            GList rooms = mCtrlTarget.GetChildByPath("List.List").asList;
            mRooms = new VirtualList(rooms,0);
            mRooms.onRenderItem = OnRoomItemRender;
            //添加双击事件
            mRooms.onItemDoubleClicked = OnItemDoubleClicked;
        }

        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);
            mManager = arg as RoomManager;
            //添加更新事件
            mManager.onUpdateRoomList.AddListener(OnRoomUpdate);
            mManager.onJoin.AddListener(OnJoinRoom);
            mManager.onExit.AddListener(OnExitRoom);
            mManager.UpdateRoomList();

            mManager.CreateRoom("testRoom", "testMap");
        }

        protected override void OnPanelDestroy()
        {
            base.OnPanelDestroy();
            //销毁时先清除所有事件
            mManager.onUpdateRoomList.RemoveListener(OnRoomUpdate);
            mManager.onJoin.RemoveListener(OnJoinRoom);
            mManager.onExit.RemoveListener(OnExitRoom);
        }

        private void OnRoomUpdate(int size)
        {
            Debuger.Log("room update called:{0}",size);
            mRooms.Refresh(size);
        }

        //单个item的显示函数
        private void OnRoomItemRender(int itemIdx,int childIdx,GObject obj)
        {
            RoomData data = mManager.RoomList[itemIdx];
            int curCount = data.players.Count;
            int maxCount = data.maxPlayerCount;
            //获取组件
            GTextField title = obj.asCom.GetChild("Title").asTextField;
            GTextField mode= obj.asCom.GetChild("Mode").asTextField;
            GTextField count = obj.asCom.GetChild("Count").asTextField;

            //设定值
            title.text = data.name;
            mode.text = data.mode;
            count.SetVar("current", curCount.ToString()).SetVar("max", maxCount.ToString()).FlushVars();
        }

        //item的双击处理函数
        private void OnItemDoubleClicked(int itemIdx,int childIdx,GObject obj)
        {
            RoomData data = mManager.RoomList[itemIdx];
            mManager.JoinRoom(data.id);
        }

        //在加入房间成功后被调用
        private void OnJoinRoom()
        {
            FGUIManager.Instance.OpenWindow<RoomWindow>("RoomWindow", "Home", mManager);
        }

        //退出房间成功后被调用
        private void OnExitRoom()
        {
            FGUIManager.Instance.CloseWindow("Home.RoomWindow");
            mManager.UpdateRoomList();
        }
    }
}
