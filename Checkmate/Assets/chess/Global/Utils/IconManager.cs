using Checkmate.Game.Utils;
using QGF;
using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Game.Global.Utils
{
    public class IconManager:Singleton<IconManager>
    {
#if UNITY_EDITOR
        private string IconRootPath = Application.dataPath + "/Test/Icons";
#else
        private string IconRootPath = Application.streamingAssetsPath + "/Icons";
#endif


        private Dictionary<string, Texture2D> mIconRes;//图标缓存


        public void Init()
        {
            mIconRes = new Dictionary<string, Texture2D>();
        }


        public Texture2D GetIcon(string name)
        {
            if (!mIconRes.ContainsKey(name))
            {
                Load(name);
            }
            return mIconRes[name];
        }

        public void Load(string name)
        {
            if (!mIconRes.ContainsKey(name))
            {
                Debuger.Log("loaded icon:{0}", name);
                Texture2D sprite = AssetUtil.LoadPicture(IconRootPath + "/" + name, 512);
                if (sprite == null)
                {
                    Debuger.LogError("error load icon:{0}", name);
                }
                mIconRes.Add(name, sprite);
            }
        }
    }
}
