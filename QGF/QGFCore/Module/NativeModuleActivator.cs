using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Module
{
    public class NativeModuleActivator : IModuleActivator
    {
        private string mNamespace = "";
        private string mAssemblyName = "";

        public NativeModuleActivator(string ns,string assemblyName)
        {
            mNamespace = ns;
            mAssemblyName = assemblyName;
        }

        public GeneralModule CreateInstance(string moduleName)
        {
            var fullName = mNamespace + "." + moduleName;
            
            var type=Type.GetType(fullName + "," +mAssemblyName);
            if (type != null)
            {
                return Activator.CreateInstance(type) as GeneralModule;
            }

            return null;
        }
    }
}
