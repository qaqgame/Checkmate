using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Services.Downloader
{
    public class DownloadCallback
    {
        public Action<string, float> onProgress;
        public Action<string> onComplete;
        public Action onCancel;
    }
    public class DownloadManager:MonoBehaviour
    {
        public static DownloadManager Instance;

#if UNITY_EDITOR
        public static string MapDownloadPath = "";//地图下载路径
        public static string RoleDownloadPath = "";//角色下载路径
        public static string RuleDownloadPath = "";//规则下载路径
#else
        public static string MapDownloadPath = "";//地图下载路径
        public static string RoleDownloadPath = "";//角色下载路径
        public static string RuleDownloadPath = "";//规则下载路径
#endif
        private void Awake()
        {
            Instance = this;
        }

        public void DownloadMap(string mapName,string link,DownloadCallback callback)
        {
            List<DownloadTrack> list = new List<DownloadTrack>();

            //第一步，下载对应的xml文件
            //第二步，下载缺少的各个角色和规则
            //第三步，下载地图数据文件

            Downloader downloader = new Downloader(callback.onProgress,callback.onComplete,callback.onCancel);
            StartCoroutine(downloader.Download(null));
        }
    }
}
