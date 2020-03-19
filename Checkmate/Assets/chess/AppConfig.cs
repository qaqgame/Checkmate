using Checkmate.Global.Data;
using ProtoBuf;
using QGF;
using QGF.Codec;
using QGF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.chess
{
    [ProtoContract]
    public class AppConfig
    {
        [ProtoMember(1)]
        public UserData mainUserData = new UserData();
        [ProtoMember(2)]
        public bool enableBgMusic = true;
        [ProtoMember(3)]
        public bool enableSoundEffect = true;

        private static AppConfig mValue = new AppConfig();
        public static AppConfig Value { get { return mValue; } }


#if UNITY_EDITOR
        public readonly static string Path = Application.persistentDataPath + "/AppConfig_Editor.data";
#else
        public readonly static string Path = Application.persistentDataPath + "/AppConfig.data";
#endif
        public static void Init()
        {
            //加载配置
            Debuger.Log("Path = " + Path);
            //加载配置

            var data = FileUtils.ReadFile(Path);
            if (data != null && data.Length > 0)
            {
                var cfg = PBSerializer.NDeserialize(data, typeof(AppConfig));
                if (cfg != null)
                {
                    mValue = cfg as AppConfig;
                }
            }
        }

        public static void Save()
        {
            Debuger.Log("Value = " + mValue);

            if (mValue != null)
            {
                byte[] data = PBSerializer.NSerialize(mValue);
                FileUtils.SaveFile(Path, data);
            }
        }


    }
}
