using Checkmate.Game.Controller;
using Checkmate.Modules.Game.UI;
using QGF.Common;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Modules.Game
{
    public class GamingPageManager:Singleton<GamingPageManager>
    {
        static GamingPage mPage;
        static RoundPage mRoundPage;//回合切换页
        public Action onRoundEndClicked;//回合结束点击事件
        public void OpenPage()
        {
            mPage= FGUIManager.Instance.Open<GamingPage>("MainPage", "GamingPage", null);
            mPage.onRoundEnd = OnRoundEndClicked;
            mRoundPage = FGUIManager.Instance.LoadToMemory<RoundPage>("RoundChangePage", "GamingPage");
        }

        public void OnRoundEndClicked()
        {
            if (onRoundEndClicked != null)
            {
                onRoundEndClicked.Invoke();
            }
        }

        public void UpdateAP()
        {
            int curAP = APManager.Instance.GetCurAP();
            int maxAP = APManager.Instance.GetMaxAp();
            if (mPage.IsOpened)
            {
                mPage.UpdateAP(curAP, maxAP);
            }
        }
        //显示角色面板
        public void ShowRolePanel(RoleController role)
        {
            mPage.ShowRole();
        }

        //隐藏角色面板
        public void HideRolePanel()
        {
            mPage.HideRole();
        }


        /// <summary>
        /// 在更新回合时调用
        /// </summary>
        /// <param name="canOperate">当前用户能否操作</param>
        public void OnNextTurn(bool canOperate)
        {
            if (!canOperate)
            {
                //不能，则隐藏回合结束与ap
                mPage.HideAll();
            }
            else
            {
                mPage.ShowAll();
            }
        }
        #region 回合切换显示
        //开始回合开始显示
        public void StartRoundBegin()
        {
            if (!mRoundPage.IsOpened)
            {
                FGUIManager.Instance.Open<RoundPage>("RoundChangePage", "GamingPage",RoundAnim.Begin);
            }
            else
            {
                mRoundPage.Show();
                mRoundPage.PlayBegin();
            }
        }
        //开始回合结束显示
        public void StartRoundEnd()
        {
            if (!mRoundPage.IsOpened)
            {
                FGUIManager.Instance.Open<RoundPage>("RoundChangePage", "GamingPage", RoundAnim.End);
            }
            else
            {
                mRoundPage.Show();
                mRoundPage.PlayEnd();
            }
        }

        public void EndRoundChanging()
        {
            mRoundPage.Hide();
        }

        //是否正在播放回合动效
        public bool RoundChanging()
        {
            return mRoundPage.IsPlaying;
        }
        #endregion
    }
}
