using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Services.Downloader
{
    public class DownloadTrack
    {
        public string name;//描述名
        public string mSrcUrl;//资源路径
        public string mSavePath;//存储路径
        public string mFileName;//文件名
        public string mFileExt;//文件扩展名
        public string mTotalSavePath;//存储的完全路径
    }
    public class Downloader
    {
        IDownloadItem mDownloader;

        
        protected Action<string,float> onDownloadProgress;//下载过程中事件 文件名-进度
        protected Action<string> onDownloadComplete;//下载结束事件 -文件名
        protected Action onAllComplete;//所有下载都结束
        
        protected bool mIsDownloading;//是否正在下载

        Queue<DownloadTrack> mListWaitDownload;//待下载项
        DownloadTrack mCurDownload;//当前下载项

        public bool IsDownload
        {
            get { return mIsDownloading; }
        }

        public Downloader(Action<string,float> onDownload,Action<string> onItemFinished,Action onAllFinished)
        {
            onDownloadProgress = onDownload;
            onDownloadComplete = onItemFinished;
            onAllComplete = onAllFinished;
            mDownloader = new WebDownloader();
           
        }

        public IEnumerator Download(List<DownloadTrack> downloads)
        {
            mIsDownloading = true;
            mListWaitDownload =new Queue<DownloadTrack>(downloads.ToArray());
            while (mListWaitDownload.Count>0)
            {
                //取出项
                mCurDownload = mListWaitDownload.Dequeue();
                yield return mDownloader.Download(mCurDownload.mSrcUrl, mCurDownload.mTotalSavePath, mCurDownload.name, onDownloadProgress, onDownloadComplete);
                //等待下载完
            }
            mIsDownloading = false;
            onAllComplete();
        }

    }
}
