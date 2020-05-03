using QGF.Unity.FGUI;
using UnityEngine;
using UnityEngine.UI;
namespace Assets.chess
{
    public class AppLoading
    {
        public const string LoadingPkg = "Login";
        public const string LoadingCom = "AppLoadingPanel";

        public static DefaultLoading mLoadingPanel=null;

        //初始化界面
        public static void Init()
        {
            if (mLoadingPanel == null)
            {
                mLoadingPanel = FGUIManager.Instance.LoadToMemory<DefaultLoading>(LoadingCom, LoadingPkg);
            }
        }

        public static void Show(string name = null,float progress=0)
        {
            if (mLoadingPanel != null)
            {
                if (!mLoadingPanel.IsOpened)
                {
                    mLoadingPanel = FGUIManager.Instance.Open<DefaultLoading>(LoadingCom, LoadingPkg);
                }
                    if (name != null)
                {
                    mLoadingPanel.SetName(name);
                }
                double value = progress * (mLoadingPanel.MaxValue - mLoadingPanel.MinValue);
                mLoadingPanel.SetValue(value, 0);
            }
        }

        public static void Update(string name,float progress)
        {
            mLoadingPanel.SetName(name);
            double value = progress * (mLoadingPanel.MaxValue - mLoadingPanel.MinValue);
            mLoadingPanel.SetValue(value, 0.5f);
        }

        public static void Close()
        {
            FGUIManager.Instance.CloseLoading(LoadingPkg + "." + LoadingCom);
        }

    }
}
