using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Checkmate.Services.Downloader
{
    public class WebDownloader : IDownloadItem
    {
        UnityWebRequest mWebRequest;

        bool mIsStop=false;//是否停止
        public IEnumerator Download(string srcUrl, string dstPath,string fileName, Action<string, float> onDownload=null, Action<string> onFinished=null,Action<string,float> onCanceled=null)
        {
            mIsStop = false;
            using (mWebRequest = UnityWebRequest.Get(srcUrl))
            {
                mWebRequest.downloadHandler = new DownloadHandlerFile(dstPath);

                mWebRequest.SendWebRequest();
                while (!mWebRequest.isDone)
                {
                    if (mIsStop) break;
                    if (onDownload != null)
                    {
                        onDownload(fileName, mWebRequest.downloadProgress);
                    }
                    yield return null;
                }

                if (mWebRequest.error != null)
                {
                    Debug.LogError(mWebRequest.error);
                }
                else
                {
                    if (!mWebRequest.isDone && mIsStop)
                    {
                        //如果是中途中断
                        mWebRequest.Abort();
                        if (onCanceled != null)
                        {
                            onCanceled(fileName, mWebRequest.downloadProgress);
                        }
                    }
                    //下载结束
                    else
                    {
                        if (onFinished != null)
                        {
                            onFinished(fileName);
                        }
                    }
                }
            }
        }

        public void Stop()
        {
            mIsStop = true;
        }
    }
}
