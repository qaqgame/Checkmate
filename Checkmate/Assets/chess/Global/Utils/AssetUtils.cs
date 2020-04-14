using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Checkmate.Game.Utils
{
    public class AssetUtil : MonoBehaviour
    {
#if UNITY_ANDROID
    public static string abPath="jar:file://" + Application.dataPath + "!/assets/AssetBundles/";
#elif UNITY_IPHONE
    public static string abPath = Application.dataPath + "/Raw/AssetBundles/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
        public static string abPath = Application.dataPath + "/AssetBundles/";
#else
    public static string abPath = string.Empty;
#endif

        private static Dictionary<string, AssetBundle> abs;
        private static AssetBundle ab = null;

        private void Awake()
        {
            
        }

        public void Load(string keyName, string path)
        {
            StartCoroutine(LoadAssetBundle(keyName, path));
        }


        public static T GetAsset<T>(string file, string bundleName) where T : class
        {
            while (!abs.ContainsKey(bundleName)) { };
            return abs[bundleName].LoadAsset(file) as T;
        }

        public static Sprite LoadPictureAsSprite(string fullPath, int size)
        {
            Texture2D tex = new Texture2D(size, size);
            tex.LoadImage(ReadImage(fullPath));
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            return sprite;
        }

        public static Texture2D LoadPicture(string fullPath, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.LoadImage(ReadImage(fullPath));
            return tex;
        }

        public static Texture2DArray CreateTextureArray(List<Texture2D> textures)
        {
            Texture2DArray texture2DArray = new Texture2DArray(textures[0].width, textures[0].height, textures.Count, textures[0].format, textures[0].mipmapCount > 1);
            texture2DArray.anisoLevel = textures[0].anisoLevel;
            texture2DArray.filterMode = textures[0].filterMode;
            texture2DArray.wrapMode = textures[0].wrapMode;

            for (int i = 0; i < textures.Count; ++i)
            {
                for (int m = 0; m < textures[0].mipmapCount; ++m)
                {
                    Graphics.CopyTexture(textures[i], 0, m, texture2DArray, i, m);
                }
            }
            return texture2DArray;
        }


        static byte[] ReadImage(string fullPath)
        {
            //读取到文件
            FileStream files = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            //新建比特流对象
            byte[] imgByte = new byte[files.Length];
            //将文件写入对应比特流对象
            files.Read(imgByte, 0, imgByte.Length);
            //关闭文件
            files.Close();
            //返回比特流的值
            return imgByte;
        }

        static IEnumerator LoadAssetBundle(string keyName, string path)
        {
            yield return LoadByUnityWebRequest(path);
            abs[keyName] = ab;
            ab = null;
        }

        static IEnumerator LoadFromMemoryAsyn(byte[] bytes)
        {
            if (ab != null)
            {
                yield return null;
            }
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(bytes);
            yield return request;
            ab = request.assetBundle;
        }

        static IEnumerator LoadFromLocalFileAsyn(string fileName)
        {
            if (ab != null)
            {
                yield return null;
            }
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(abPath + fileName);
            yield return request;
            ab = request.assetBundle;
        }

        static IEnumerator LoadByUnityWebRequest(string filePath)
        {
            if (ab != null)
            {
                yield return null;
            }
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(filePath);
            yield return request.Send();
            ab = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
        }

        static IEnumerator Release(bool releaseLoadedSource)
        {
            ab.Unload(releaseLoadedSource);
            yield return true;
        }
    }

}
