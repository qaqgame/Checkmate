using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Services.Downloader
{
    interface IDownloadItem
    {
        IEnumerator Download(string srcUrl, string dstPath,string fileName,Action<string,float> onDownload=null,Action<string> onFinished=null,Action<string,float> onCanceled=null);
        void Stop();
    }
}
