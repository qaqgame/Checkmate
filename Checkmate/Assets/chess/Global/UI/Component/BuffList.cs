using Checkmate.Game.Controller;
using FairyGUI;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Checkmate.Game.Buff;
using UnityEngine;

namespace Checkmate.Game.UI.Component
{
    public class BuffList:VirtualList
    {
        List<int> mBuffs;//所有的buff

        public BuffList(GList list):base(list,0)
        {
            onRenderItem = RenderItem;
        }

        void RenderItem(int itemIdx, int childIdx, GObject obj)
        {
            //获取组件
            GComponent com = obj.asCom;
            GTextField TurnText = com.GetChild("Turns").asTextField;
            GTextField TimeText = com.GetChild("Times").asTextField;
            GImage mask = com.GetChild("Mask").asImage;
            GLoader icon = com.GetChild("Img").asLoader;

            int bid = mBuffs[itemIdx];
            Checkmate.Game.Buff.Buff buff = BuffManager.Instance.GetBuff(bid);
            if (buff != null)
            {
                Texture2D tex = BuffManager.Instance.GetBuffIcon(bid);
                //赋予主图片
                icon.texture = new NTexture(tex);

                //设置回合数
                TurnText.text = buff.IsInfiniteTurn ? "inf" : buff.ReserveTurn.ToString();
                //设置次数
                TimeText.text = buff.IsInfiniteTimes ? "inf" : buff.ReserveTime.ToString();
                mask.fillAmount = 0;
            }
            
        }

        public void Update(List<int> buffs)
        {
            mBuffs = buffs;
            //刷新显示
            Refresh(mBuffs.Count);
        }
    }
}
