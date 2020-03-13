using UnityEngine;
using UnityEngine.UI;
namespace Assets.chess
{
    public class AppLoading:MonoBehaviour
    {
        public Text txtTitle;
        public Text txtTips;
        //public QGFProgressBar progressBar;
        private static AppLoading mInstance;

        private void OnDestroy()
        {
            mInstance = null;
        }

        //显示当前加载资源名以及进度
        public static void Show(string title,float progress)
        {
            //mInstance = UIRoot.Find<AppLoading>("AppLoading");
            if (mInstance != null)
            {
                //被隐藏了则让其显示
                if (!mInstance.gameObject.activeSelf)
                {
                    mInstance.gameObject.SetActive(true);
                }
                mInstance.ShowProgress(title, progress);
            }
        }
        //隐藏
        public static void Hide()
        {
            if (mInstance != null)
            {
                //被隐藏了则让其显示
                if (mInstance.gameObject.activeSelf)
                {
                    mInstance.gameObject.SetActive(false);
                }
            }
        }

        private void ShowProgress(string tile,float progress)
        {
            //显示数字百分比
            if (txtTitle != null)
            {
                txtTitle.text = txtTitle + "(" + (int)(progress * 100) + "%)";
            }

            //if (progressBar != null)
            //{
            //    progressBar.SetData(progress);
            //}
        }
    }
}
