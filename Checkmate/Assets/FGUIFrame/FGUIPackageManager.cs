using FairyGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Common;

namespace QGF.Unity.FGUI
{
    public class FGUIPackageManager:Singleton<FGUIPackageManager>
    {
        string mRootPath = "ui/";

        public void Init(string root)
        {
            mRootPath = root;
        }

        List<string> mLoadedPackages = new List<string>();
        public string LoadPackage(string name)
        {
            UIPackage pkg= UIPackage.AddPackage(mRootPath+name);
            mLoadedPackages.Add(pkg.name);
            return pkg.name;
        }

        //异步创建UI
        public void CreateUIAsync(string package,string name,UIPackage.CreateObjectCallback onCreateFinished)
        {
            //如果未加载包，报错
            if (!mLoadedPackages.Contains(package))
            {
                Debuger.LogError("cannot find package:{0}", package);
            }
            UIPackage.CreateObjectAsync(package, name, onCreateFinished);
        }
    }
}
