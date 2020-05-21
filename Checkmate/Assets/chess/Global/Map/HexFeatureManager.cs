using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkmate.Game
{
    [System.Serializable]
    public struct HexFeatureCollection
    {
        public bool isLargeFeature;//是否是大型特征（只能放一个）
        public Transform[] prefabs;
        public List<string> files;//对应的文件名
        public Transform Pick(float choice)
        {
            Transform transform = prefabs[(int)(choice * prefabs.Length)];
            if (!transform)
            {
                Debug.LogError("get null prefab:" + (int)(choice * prefabs.Length));
            }
            else
            {
                Debug.Log("picked:" + (int)(choice * prefabs.Length));
            }
            return transform;
        }
    }

    public class HexFeatureManager : MonoBehaviour
    {
        public const string containerName = "Static Features Container";

        Transform container;


        //第一次必定选出一个
        Transform PickPrefab(int level, float hash, int idx, bool first)
        {
            if (level > 0 && HexMetrics.featurePrefabs.Count>idx)
            {
                float thresholds = HexMetrics.GetFeatureThresholds(level - 1);
                ////如果是单个特征，则直接返回该等级的样式
                //if (HexMetrics.featurePrefabs[idx].isLargeFeature)
                //{
                //    return HexMetrics.featurePrefabs[idx][level - 1].Pick(choice);
                //}


                //根据概率选择是显示哪一个等级的特征的物体
                if (hash < thresholds)
                {
                    return HexMetrics.featurePrefabs[idx];
                }
                if (first)
                {
                    return HexMetrics.featurePrefabs[idx];
                }
            }
            return null;
        }

        public void Clear()
        {
            if (container)
            {
                Destroy(container.gameObject);
            }
            container = new GameObject(containerName).transform;
            container.SetParent(transform, false);
        }
        public void Apply() { }

        public void AddFeature(HexCell cell, Vector3 position)
        {
            if (cell.Feature >= 0 && cell.FeatureLevel > 0)
            {
                if (HexMetrics.featurePrefabs == null)
                {
                    Debug.LogError("error add feature, no feature exist");
                }
                HexHash hash = HexMetrics.SampleHashGrid(position);
                Transform prefab = PickPrefab(cell.FeatureLevel, hash.a, cell.Feature, cell.firstAddFeature);
                if (!prefab)
                {
                    Debug.Log("prefab is null");
                    return;
                }
                Transform instance = Instantiate(prefab);
                instance.localPosition = HexMetrics.Perturb(position);
                if (cell.firstAddFeature)
                {
                    cell.firstAddFeature = false;
                }
                else
                {
                    instance.localRotation = Quaternion.Euler(0f, 360f * hash.c, 0f);
                }
                instance.SetParent(container, false);
            }
        }


    }
}