using Checkmate.Game.Player;
using FairyGUI;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Modules.Game.UI.Component
{
    public class PlayerList : VirtualList
    {
        private List<int> players;
        private bool IsCurrent;
        public PlayerList(GList list,bool current) : base(list, 0)
        {
            players = new List<int>();
            onRenderItem = OnRenderItem;
            IsCurrent = current;
        }

        private void OnRenderItem(int itemId,int childIdx,GObject obj)
        {
            // 获取组件
            GComponent com = obj.asCom;
            GTextField TurnText = com.GetChild("Text").asTextField;
            GTextField TimeText = com.GetChild("APPoint").asTextField;

            int pid = players[itemId];
            int ap = APManager.Instance.GetCurAP(pid);
            string name = PlayerManager.Instance.GetName(pid);

            if (itemId == 0&&IsCurrent)
            {
                GImage bg = com.GetChild("Bg").asImage;
                bg.color = new UnityEngine.Color(0, 0.5f, 0);
            }
        }

        //public void Update(List<int> ps)
        //{
        //    players = ps;
        //    if (players == null)
        //    {
        //        Refresh(0);
        //    }
        //    else
        //    {
        //        Refresh(players.Count);
        //    }
        //}

        public void Remove(int pid)
        {
            players.Remove(pid);
            Refresh(players.Count);
        }

        public void RemoveAll()
        {
            players.Clear();
            Refresh(players.Count);
        }

        public void Add(int pid)
        {
            int curAp = APManager.Instance.GetCurAP(pid);
            int idx=0;
            for(int i = 0; i < players.Count; ++i)
            {
                int tid = players[i];
                int ap = APManager.Instance.GetCurAP(tid);
                //按照从大到小排列
                if (curAp > ap)
                {
                    break;
                }
                idx++;
            }
            //结尾则直接add
            if (idx == players.Count)
            {
                players.Add(pid);
            }
            else
            {
                players.Insert(idx, pid);
            }

            Refresh(players.Count);
        }
    }
}
