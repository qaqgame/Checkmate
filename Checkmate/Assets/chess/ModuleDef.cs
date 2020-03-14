using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Chess
{
    public static class ModuleDef
    {
        public const string Namespace = "Chess.Module";
        public const string NativeAssemblyName = "Assembly-CSharp";
        public const string ScriptAssemblyName = "ILRScript";

        public const string HomeModule= "HomeModule";//主页模块名
        public const string PVEModule = "PVEModule";
        public const string PVPModule = "PVPModule";
        public const string RoomModule = "RoomModule";
    }
}
