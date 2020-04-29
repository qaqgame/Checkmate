using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using QGF;

namespace Checkmate.Game.Controller
{

    [AttributeUsage(AttributeTargets.Property)]
    public class GetPropertyAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SetPropertyAttribute : Attribute
    {

    }

    internal class CustomDataParam
    {
        public string type;
        public string value;
        public List<CustomDataParam> param;
    }
    internal class CustomData
    {
        public string name;
        public CustomDataParam param;
    }

    internal class CustomDataUtil
    {
        private static readonly string[] filteList =
        {
            "int","float","bool","string"
        };
        public static Dictionary<string,object> ParseDatas(string content)
        {
            List<CustomData> datas = JsonConvert.DeserializeObject<List<CustomData>>(content);

            Dictionary<string, object> result = new Dictionary<string, object>();
            if (datas != null)
            {
                foreach (var data in datas)
                {
                    string name = data.name;
                    object value = ParseObject(data.param);
                    result.Add(name, value);
                }
            }
            return result;
        }

        public static object ParseObject(CustomDataParam p)
        {
            //如果是原生类型,直接返回值
            if (filteList.Contains(p.type))
            {
                return ParsePrimitiveValue(p.type, p.value);
            }
            //获取类型
            Type type = Type.GetType(p.type);
            Debug.Log("extra type:" + p.type);
            //判断是否是枚举
            if (type.IsEnum)
            {
                return Enum.Parse(type, p.value);
            }

            //否则视为自定义类
            //解析参数
            List<object> allParams = new List<object>();
            foreach(var param in p.param)
            {
                allParams.Add(ParseObject(param));
            }

            object[] realParam = allParams.ToArray();
            //构建对象
            object result = Activator.CreateInstance(type, realParam);
            return result;
        }

        private static object ParsePrimitiveValue(string type,string value)
        {
            switch (type)
            {
                case "int":return int.Parse(value);
                case "float":return float.Parse(value);
                case "bool":return bool.Parse(value);
                case "string":return value;
            }
            return null;
        }
    }
    //类属性管理器
    internal class PropertyController
    {
        private static Dictionary<string, List<PropertyInfo>> mProperty=new Dictionary<string, List<PropertyInfo>>();

        private void RegistClass(Type type)
        {
            //将该类的所有public或static属性加入
            if (!mProperty.ContainsKey(type.Name))
            {
                
                List<PropertyInfo> properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static).ToList();   
                mProperty.Add(type.Name, properties);
            }
        }

        public PropertyInfo FindProperty(object obj,string param)
        {
            string type = obj.GetType().Name;
            //如果没有缓存则注册该类
            if (!mProperty.ContainsKey(type))
            {
                RegistClass(obj.GetType());
            }
            var property = mProperty[type].Find(a => a.Name == param);
            return property;
        }

    }
    public abstract class BaseController
    {
        private static PropertyController mPropertyController=new PropertyController();

        protected Dictionary<string, object> mExtraData;//额外数据的字典

        //标识控制器类别
        public abstract int Type { get; }

        //controller的实例id
        [GetProperty]
        public int Id
        {
            get
            {
                return this.GetHashCode();
            }
        }

       

        public BaseController(string extraData=null)
        {
            if (extraData != null)
            {
                mExtraData = CustomDataUtil.ParseDatas(extraData);
            }
            else
            {
                mExtraData = new Dictionary<string, object>();
            }
        }

        public void SetExtra(string extra)
        {
            if (extra!= null)
            {
                mExtraData = CustomDataUtil.ParseDatas(extra);
            }
            else
            {
                mExtraData = new Dictionary<string, object>();
            }
        }

        //获取属性值
        public object GetValue(string param)
        {
            //如果是试图获取子控制器的变量
            if (param.Contains('.'))
            {
                string newObj = param.Substring(0, param.IndexOf('.'));
                string newParam = param.Substring(param.IndexOf('.') + 1);
                //获取该变量
                object obj = GetValue(newObj);
                if (obj == null)
                {
                    Debug.LogError("obj null:" + newObj);
                    //为空代表无该变量
                    Debuger.LogError("error get property:{0} from {1}", newObj, this.GetType().Name);
                    return false;
                }
                Debug.Log("type:" + obj.GetType().Name);
                Debug.Log("available " + obj.GetType().IsSubclassOf(typeof(BaseController)));
                if (obj.GetType().IsSubclassOf(typeof(BaseController)))
                {
                    //如果为basecontroller的子类
                    return (obj as BaseController).GetValue(newParam);
                }
                //此处代表非子类
                Debuger.LogError("{0} is not a controller", obj.GetType().Name);
                return null;
            }
            else
            {
                int idx = 0;
                //表示为数组
                if (param.Contains('['))
                {
                    int idx1 = param.IndexOf('[');
                    int idx2 = idx1 + 1;
                    while (param[idx2] != ']')
                    {
                        idx2++;
                    }
                    //获取数组索引
                    idx= int.Parse(param.Substring(idx1 + 1, idx2 - idx1-1));

                    param = param.Substring(0, idx1);
                }
                var property = mPropertyController.FindProperty(this, param);
                //内部存在该属性
                if (property != null&&property.IsDefined(typeof(GetPropertyAttribute)))
                {
                    //如果是数组
                    if (typeof(IList).IsAssignableFrom(property.PropertyType))
                    {
                        IList list = property.GetValue(this) as IList;
                        return list[idx];
                    }
                    return property.GetValue(this);
                    
                }
                //否则从字典读取
                else
                {
                    return mExtraData[param];
                }
            }
        }

        //设置属性值
        public bool SetValue(string param,object value)
        {
            //如果包含.，代表是子控制器的变量
            if (param.Contains('.'))
            {
                string newObj = param.Substring(0, param.IndexOf('.'));
                string newParam = param.Substring(param.IndexOf('.') + 1);
                //获取该变量
                object obj = GetValue(newObj);
                if (obj == null)
                {
                    //为空代表无该变量
                    Debuger.LogError("error get property:{0} from {1}", newObj, this.GetType().Name);
                    return false;
                }
                if (obj.GetType().IsAssignableFrom(typeof(BaseController)))
                {
                    //如果为basecontroller的子类
                    return (obj as BaseController).SetValue(newParam, value);
                }
                //此处代表非子类
                Debuger.LogError("{0} is not a controller", obj.GetType().Name);
                return false;
            }
            else {
                var property = mPropertyController.FindProperty(this, param);
                //内部存在该属性
                if (property != null&&property.IsDefined(typeof(SetPropertyAttribute)))
                {
                    property.SetValue(this, value);
                    return true;
                }
                else if (mExtraData.ContainsKey(param))
                {
                    mExtraData[param] = value;
                    return true;
                }
                return false;
            }
        }

        

        
    }
}
