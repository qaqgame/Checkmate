using Checkmate.Game.Controller;
using Checkmate.Global.Data;
using Checkmate.Modules.Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Modules.Game.Role
{
    //角色管理器(管理所有角色）
    public class RoleManager
    {
        public const string prefabType = "Role";//类别名


        private List<string> mActiveRoles;//处于激活状态的role
        private Dictionary<string, RoleController> mRolePool;//角色池

        private Dictionary<string, GameObject> mModelPrefabs;//模型预制体缓存

        public RoleManager()
        {
            mActiveRoles = new List<string>();
            mRolePool = new Dictionary<string, RoleController>(20);
            mModelPrefabs = new Dictionary<string, GameObject>(10);
        }

        //添加角色
        public RoleController AddRole(RoleData role)
        {
            string nameKey = role.name + role.team.ToString();

            GameObject obj = ObjectPool.Instance.GetGameObject(prefabType);
            //生成模型
            GameObject modelPrefab;
            if (!mModelPrefabs.ContainsKey(role.model))
            {
                modelPrefab= Resources.Load<GameObject>("Role/Model/" + role.model);
                mModelPrefabs.Add(role.model, modelPrefab);
            }
            else
            {
                modelPrefab = mModelPrefabs[role.model];
            }
            //将模型挂在节点上
            GameObject model = GameObject.Instantiate(modelPrefab,obj.transform.Find("Model").transform);
            

            RoleController controller = new RoleController(role,obj);
            mRolePool.Add(nameKey, controller);
            //添加到激活列表
            mActiveRoles.Add(nameKey);
            return controller;
        }

        //移除角色
        public void RemoveRole(RoleController controller)
        {
            //将其隐藏
            controller.GetGameObject().SetActive(false);

            string key = controller.Name + controller.Team.ToString();
            //从激活列表中移除
            mActiveRoles.Remove(key);
        }

        //复活角色
        public void Revive(string name,int team,RoleProperty property)
        {
            string key = name + team.ToString();
            if (mRolePool.ContainsKey(key))
            {
                RoleController controller = mRolePool[key];
                //设置角色的当前属性值
                controller.SetCurProperty(property);

                //设置实例可见
                controller.GetGameObject().SetActive(true);

                //添加至激活列表
                mActiveRoles.Add(key);
            }
        }


        //获取角色

    }
}
