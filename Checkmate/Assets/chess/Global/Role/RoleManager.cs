﻿using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Global.Data;
using Checkmate.Game.Map;
using Checkmate.Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using QGF.Common;

namespace Checkmate.Game.Role
{
    //角色管理器(管理所有角色）
    public class RoleManager:Singleton<RoleManager>
    {
        public const string prefabType = "Role";//类别名


        private List<int> mActiveRoles;//处于激活状态的role
        private Dictionary<int, RoleController> mRolePool;//角色池

        private Dictionary<string, GameObject> mModelPrefabs;//模型预制体缓存


        public void Init()
        {
            mActiveRoles = new List<int>();
            mRolePool = new Dictionary<int, RoleController>(20);
            mModelPrefabs = new Dictionary<string, GameObject>(10);
        }

        //添加角色
        public RoleController AddRole(RoleData role)
        {
            int id = role.id;

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
            mRolePool.Add(id, controller);
            //添加到激活列表
            mActiveRoles.Add(id);

            //设置角色的实际位置
            Position pos = new Position(role.position.x, role.position.y, role.position.z);
            controller.GetGameObject().transform.position = MapManager.Instance.GetCellWorldPosition(pos);
            CellController cell = MapManager.Instance.GetCell(pos);
            cell.Role = id;
            return controller;
        }

        //移除角色
        public void RemoveRole(int id)
        {
            if (mActiveRoles.Contains(id))
            {
                RoleController controller = mRolePool[id];
                //从地图移除
                CellController cell = MapManager.Instance.GetCell(controller.Position);
                cell.Role = -1;
                //将其隐藏
                controller.GetGameObject().SetActive(false);
                //从激活列表中移除
                mActiveRoles.Remove(id);
            }
        }

        //复活角色
        public void Revive(int id,RoleProperty property,Position position)
        {
            int key = id;
            if (mRolePool.ContainsKey(key))
            {
                RoleController controller = mRolePool[key];
                //设置角色的当前属性值
                controller.SetCurProperty(property);

                //设置实例可见
                controller.GetGameObject().SetActive(true);

                //添加至激活列表
                mActiveRoles.Add(key);
                //设置位置
                controller.GetGameObject().transform.position =MapManager.Instance.GetCellWorldPosition(position);
                //加至地图
                CellController cell = MapManager.Instance.GetCell(position);
                cell.Role = id;
            }
        }


        //获取角色
        public RoleController GetRole(int id)
        {
            return mRolePool[id];
        }
    }
}