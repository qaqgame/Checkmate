using Checkmate.Global.Data;
using FairyGUI;
using QGF;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Module.UI.Component
{
    //目前仅支持6个队
    public class PlayerList : VirtualList
    {
        readonly static List<Color> bgColors = new List<Color>
        {
            Color.black,Color.red,Color.cyan,Color.yellow,Color.green,Color.blue
        };
        RoomData mCurRoom;//当前的房间数据
        List<int> mPlayerIndex;//player的index
        List<int> mSeparatorIndex;//分割线的index
        public PlayerList(GList list, int size) : base(list, size)
        {
            mList.itemProvider = GetListItemResource;
            onRenderItem = RenderItem;
            mPlayerIndex = new List<int>(10);
            mSeparatorIndex = new List<int>(10);
        }

        string GetListItemResource(int idx)
        {
            if (IsSeparator(idx))
            {
                //代表分割线
                return "ui://Home/Separator";
            }
            return "ui://Home/PlayerItem";
        }

        void RenderItem(int itemIdx,int childIdx,GObject obj)
        {
            //该项是玩家数据
            if (!IsSeparator(itemIdx))
            {
                PlayerRoomData data = GetPlayerData(itemIdx);
                Debuger.Log("player:<{0},{1}>", data.name, data.isReady);
                GComponent com = obj.asCom;
                GTextField title = com.GetChild("Title").asTextField;
                GTextField ready = com.GetChild("Ready").asTextField;
                GTextField team = com.GetChild("Team").asTextField;
                title.text = data.name;
                ready.text = data.isReady ? "Ready!" : "";
                team.text = data.team.ToString();

                GGraph bg = com.GetChild("Bg").asGraph;
                bg.color = bgColors[(int)data.team];
            }
        }

        //判断是否是分割线
        bool IsSeparator(int idx)
        {
            return mSeparatorIndex.Contains(idx);
        }

        //根据显示的itemidx获取player
        PlayerRoomData GetPlayerData(int idx)
        {
            int pidx = mPlayerIndex.IndexOf(idx);
            return mCurRoom.players[pidx];
        }

        public void Update(RoomData newData)
        {
            mCurRoom = newData;
            mPlayerIndex.Clear();
            mSeparatorIndex.Clear();

            int lastTeam = 0;
            for(int i = 0, index = 0; i < mCurRoom.players.Count; ++i)
            {
                PlayerRoomData pd = mCurRoom.players[i];
                //与上一个不同阵营，代表加一个分割线
                if (pd.team != lastTeam)
                {
                    mSeparatorIndex.Add(index++);
                }
                mPlayerIndex.Add(index++);
            }

            //刷新列表显示
            Refresh(mPlayerIndex.Count + mSeparatorIndex.Count);
        }

        public void Clear()
        {

        }
    }
}
