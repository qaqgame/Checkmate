using Assets.Chess;
using Checkmate.Global.Data;
using Checkmate.Services.Online;
using Newtonsoft.Json;
using QGF;
using QGF.Event;
using QGF.Network.Core.RPCLite;
using QGF.Network.FSPLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Module
{
    public class RoomManager
    {
        public readonly QGFEvent<int> onUpdateRoomList = new QGFEvent<int>();//更新房间列表事件
        public readonly QGFEvent onJoin = new QGFEvent();//加入房间事件
        public readonly QGFEvent onExit = new QGFEvent();//退出房间事件
        public readonly QGFEvent<RoomData> onUpdateRoomInfo = new QGFEvent<RoomData>();//更新房间内信息事件


        private List<RoomData> mListRooms = new List<RoomData>();//所有的房间数据

        private uint mMainUserId;//玩家的userId
        private bool mIsInRoom = false;
        private bool mIsReady = false;
        private bool mIsAllReady = false;

        private RoomData mCurRoom = new RoomData();//当前的房间
        public bool IsReady { get { return mIsReady; } }
        public bool IsInRoom { get { return mIsInRoom; } }
        public bool IsAllReady { get { return mIsAllReady; } }


        public List<string> mAllMaps=null;//所有的地图
#if UNITY_EDITOR
        string mapPath=Application.dataPath + "/Test/";
#else
        string mapPath = Application.streamingAssetsPath + "/Maps/";
#endif

        public uint TeamId
        {
            get
            {
                foreach(var player in mCurRoom.players)
                {
                    if (player.uid == mMainUserId)
                    {
                        return player.team;
                    }
                }
                return 0;
            }
        }

        public List<RoomData> RoomList
        {
            get
            {
                return mListRooms;
            }
        }
        public void Init()
        {
            OnlineManager.Net.RegistRPCListener(this);
            mAllMaps = new List<string>();

            DirectoryInfo folder = new DirectoryInfo(mapPath);
            foreach(var file in folder.GetFiles())
            {
                if (file.Extension == ".map")
                {
                    mAllMaps.Add(file.Name.Remove(file.Name.LastIndexOf('.')));
                }
            }
        }

        public void Clear()
        {
            OnlineManager.Net.UnRegistRPCListener(this);
            mAllMaps.Clear();
            mAllMaps = null;
        }

        public void Reset()
        {
            mIsReady = false;
            mIsInRoom = false;
            mMainUserId = OnlineManager.Instance.MainUserData != null ? OnlineManager.Instance.MainUserData.id : 0;

        }

        //=======================
        //RPC
        //======================
        [RPCRequest]
        public void UpdateRoomList()
        {
            OnlineManager.Net.Invoke("*ZoneServer.ZoneServer.UpdateRoomList");
        }

        //接收来自服务器的RPC以更新房间
        [RPCResponse]
        public void OnUpdateRoomList(RoomListData data)
        {
            if (data != null)
            {
                mListRooms = data.rooms;
                Debuger.Log("update room count:{0}", mListRooms.Count);
                for (int i = 0; i < mListRooms.Count; ++i)
                {
                    Debuger.Log(mListRooms[i].ToString());
                }
                onUpdateRoomList.Invoke(mListRooms.Count);
            }
            else
            {
                //数据为空
                Debuger.LogWarning("房间数据为空!");
            }
        }

        //==================
        //创建房间

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">房间名</param>
        /// <param name="mode">房间模式</param>
        /// <param name="teamLimits">队伍限制数</param>
        [RPCRequest]
        public void CreateRoom(string name, string map)
        {
            string content = File.ReadAllText(mapPath + map + "_config.json");
            MapConfig config = JsonConvert.DeserializeObject<MapConfig>(content);
            OnlineManager.Net.Invoke("*ZoneServer.ZoneServer.CreateRoom", name, map,config);
        }

        [RPCResponse]
        public void OnCreateRoom(RoomData data)
        {
            Debuger.Log("create room:{0}", data.ToString());

            mCurRoom = data;
            mIsInRoom = true;

            //调用加入房间事件
            onJoin.Invoke();

            //更新房间信息
            onUpdateRoomInfo.Invoke(mCurRoom);
        }


        //====================================
        //加入房间

        /// <summary>
        /// 加入房间
        /// </summary>
        /// <param name="roomId">房间id</param>
        [RPCRequest]
        public void JoinRoom(uint roomId)
        {
            OnlineManager.Net.Invoke("*ZoneServer.ZoneServer.JoinRoom", roomId);
        }

        [RPCResponse]
        private void OnJoinRoom(RoomData data)
        {
            Debuger.Log(data.ToString());
            mCurRoom = data;
            mIsInRoom = true;
            onJoin.Invoke();
        }

        //当加入房间错误时被调用
        private void OnJoinRoomError(string msg, uint roomId)
        {
            Debuger.LogError("加入房间失败:{0},msg:{1}",roomId,msg);
        }

        //========================
        //退出房间
        [RPCRequest]
        public void ExitRoom()
        {
            OnlineManager.Net.Invoke("*ZoneServer.ZoneServer.ExitRoom", mCurRoom.id);

            mIsReady = false;
            mIsInRoom = false;

            onExit.Invoke();
        }


        //====================================
        /// <summary>
        /// 房间准备、取消准备
        /// </summary>
        [RPCRequest]
        public void RoomReady(bool ready)
        {
            OnlineManager.Net.Invoke("*ZoneServer.ZoneServer.RoomReady", mCurRoom.id, ready);
        }

        private void OnRoomReadyError(string msg, uint roomId)
        {
            Debuger.LogError(msg);
        }

        //========================
        //更改阵营
        public void ChangeTeam(uint team)
        {
            OnlineManager.Net.Invoke("*ZoneServer.ZoneServer.ChangeTeam", mCurRoom.id, team);
        }

        //=======================
        public void UpdateRoomInfo()
        {
            OnlineManager.Net.Invoke("*ZoneServer.ZoneServer.UpdateRoomInfo", mCurRoom.id);
        }

        //================================
        //通知房间信息
        [RPCNotify]
        private void NotifyRoomUpdate(RoomData data)
        {
            Debuger.Log(data.ToString());
            mCurRoom = data;
            List<PlayerRoomData> players = mCurRoom.players;

            mIsInRoom = false;
            mIsReady = false;
            mIsAllReady = true;

            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i].uid == mMainUserId)
                {
                    mIsInRoom = true;
                    mIsReady = players[i].isReady;
                }

                if (!players[i].isReady)
                {
                    mIsAllReady = false;
                }
            }

            Debuger.Log("Player Count: {0}", players.Count);
            onUpdateRoomInfo.Invoke(mCurRoom);
        }

        //===========================
        //开始游戏
        [RPCRequest]
        public void StartGame()
        {
            OnlineManager.Net.Invoke("*ZoneServer.ZoneServer.StartGame", mCurRoom.id);
        }

        private void OnStartGameError(string msg, int roomId)
        {
            Debuger.LogError(msg);
        }


        [RPCNotify]
        private void NotifyGameStart(GameParam gp,FSPParam param)
        {
            Debuger.LogWarning("start game:{0}",gp.pid);
            GlobalEvent.onGameStart.Invoke(gp,param);
            

            //for (int i = 0; i < param.players.Count; i++)
            //{
            //    if (param.players[i].id == mMainUserId)
            //    {
            //        //param.fspParam.sid = param.players[i].sid;
            //        break;
            //    }
            //}

            //GameStartParam startParam = param;
            //onNotifyGameStart.Invoke(startParam);
        }
    }
}
