using QGF.Unity.FGUI;
using UnityEngine;
using UnityEngine.UI;
namespace Assets.chess
{
    public class AppLoading
    {
        public const string LoadingPkg = "Login";
        public const string LoadingCom = "AppLoadingPanel";

        public static DefaultLoading mLoadingPanel;

        public static void Show(string name = null)
        {
            if (mLoadingPanel == null || !mLoadingPanel.IsOpened)
            {
                mLoadingPanel = FGUIManager.Instance.Open<DefaultLoading>(LoadingCom, LoadingPkg);
                if (name != null)
                {
                    mLoadingPanel.SetName(name);
                }
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
