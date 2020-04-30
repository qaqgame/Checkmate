using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkmate.Game
{
    //边缘类型
    public enum HexEdgeType
    {
        Flat, Slope, Cliff
    }

    public struct HexHash
    {
        public float a, b, c;
        public static HexHash Create()
        {
            HexHash hash;
            hash.a = Random.value * 0.999f;
            hash.b = Random.value * 0.999f;
            hash.c = Random.value * 0.999f;
            return hash;
        }
    }

    public static class HexMetrics
    {
        public const float outerToInner = 0.866025404f;
        public const float innerToOuter = 1f / outerToInner;
        public const float outerRadius = 10.0f;
        public const float innerRadius = outerRadius * outerToInner;

        public const int chunkSizeX = 5, chunkSizeZ = 5;//每块的单元格数目



        public const float solidFactor = 0.8f;//固有颜色区域
        public const float beldFactor = 1f - solidFactor;//混合区域
        public const float elevationStep = 3f;//单位高度的值
        public const int terracesPerSlope = 2;//每个高度等级的阶梯数
        public const int terraceSteps = terracesPerSlope * 2 + 1;//阶梯的面数(即水平方向上的级数)
        public const float horizontalTerraceStepSize = 1f / terraceSteps;//水平方向的插值步长
        public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);//竖直方向上的插值步长

        public static Texture2D noiseSource;//噪声纹理
        public const float cellPerturbStrength = 4f;//噪声扰动程度
        public const float noiseScale = 0.003f;//噪声坐标缩放比例
        public const float elevationPerturbStrength = 1.5f;//高度的扰动强度

        //水面河流相关参数，都是相对于高度的调整
        public const float streamBedElevationOffset = -1.75f;//河床的高度偏移量
        public const float waterElevationOffset = -0.5f;//水面高度
        public const float waterFactor = 0.6f;//水面比例
        public const float waterBlendFactor = 1 - waterFactor;//泡沫比例

        //特征相关参数////////
        //特征等级阈值
        static float[] featureThresholds ={
            0.4f,0.7f,1.0f
        };
        //特征的集合
        public static List<Transform> featurePrefabs;//[第几个特征]
        public static float GetFeatureThresholds(int level)
        {
            return featureThresholds[level];
        }
        public const int hashGridSize = 256;//用于提供随机性的hash网格
        public const float hashGridScale = 0.25f;
        static HexHash[] hashGrid;
        public static void InitializeHashGrid(int seed)
        {
            hashGrid = new HexHash[hashGridSize * hashGridSize];
            Random.State currentState = Random.state;
            Random.InitState(seed);
            for (int i = 0; i < hashGrid.Length; ++i)
            {
                hashGrid[i] = HexHash.Create();
            }
            Random.state = currentState;
        }
        private static Vector3[] corners ={
        new Vector3(0.5f*outerRadius,0f,innerRadius),
        new Vector3(outerRadius,0f,0f),
        new Vector3(0.5f*outerRadius,0f,-innerRadius),
        new Vector3(-0.5f*outerRadius,0f,-innerRadius),
        new Vector3(-outerRadius,0f,0f),
        new Vector3(-0.5f*outerRadius,0f,innerRadius),
        new Vector3(0.5f*outerRadius,0f,innerRadius)
    };

        public static Vector3 GetFirstCorner(HexDirection d)
        {
            return corners[(int)d];
        }

        public static Vector3 GetSecondCorner(HexDirection d)
        {
            return corners[(int)d];
        }

        public static Vector3 GetFirstSolidCorner(HexDirection d)
        {
            return corners[(int)d] * solidFactor;
        }

        public static Vector3 GetSecondSolidCorner(HexDirection d)
        {
            return corners[(int)(d.Next())] * solidFactor;
        }
        public static Vector3 GetFirstWaterCorner(HexDirection direction)
        {
            return corners[(int)direction] * waterFactor;
        }

        public static Vector3 GetSecondWaterCorner(HexDirection direction)
        {
            return corners[(int)direction.Next()] * waterFactor;
        }

        public static Vector3 GetBridge(HexDirection d)
        {
            return (corners[(int)d] + corners[(int)d.Next()]) * beldFactor;
        }

        public static Vector3 GetWaterBridge(HexDirection direction)
        {
            return (corners[(int)direction] + corners[(int)direction.Next()]) * waterBlendFactor;
        }
        //获取solid边界的中心
        public static Vector3 GetSolidEdgeMiddle(HexDirection direction)
        {
            return (corners[(int)direction] + corners[(int)direction.Next()]) *
                (0.5f * solidFactor);
        }

        //根据高度差获取边缘类型
        public static HexEdgeType GetEdgeType(int e1, int e2)
        {
            if (e1 == e2)
            {
                return HexEdgeType.Flat;
            }
            int delta = e1 - e2;
            if (delta == 1 || delta == -1)
            {
                return HexEdgeType.Slope;
            }
            return HexEdgeType.Cliff;
        }

        //通过两点的坐标，进行插值返回第step面的坐标
        public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
        {
            float h = step * horizontalTerraceStepSize;//插值率
            a.x += (b.x - a.x) * h;
            a.z += (b.z - a.z) * h;
            //插值高度(只在第奇数个顶点调整高度)
            float v = ((step + 1) / 2) * verticalTerraceStepSize;
            a.y += (b.y - a.y) * v;

            return a;
        }

        public static Color TerraceLerp(Color a, Color b, int step)
        {
            float h = step * horizontalTerraceStepSize;
            return Color.Lerp(a, b, h);
        }
        //采样噪声
        public static Vector4 SampleNoise(Vector3 position)
        {
            return noiseSource.GetPixelBilinear(position.x * noiseScale, position.z * noiseScale);
        }

        //扰动
        public static Vector3 Perturb(Vector3 pos)
        {
            Vector4 sample = SampleNoise(pos);
            pos.x += (sample.x * 2f - 1f) * cellPerturbStrength;
            pos.z += (sample.z * 2f - 1f) * cellPerturbStrength;
            return pos;
        }

        //采样hashGrid
        public static HexHash SampleHashGrid(Vector3 position)
        {
            int x = (int)(position.x * hashGridScale) % hashGridSize;
            if (x < 0)
            {
                x += hashGridSize;
            }
            int z = (int)(position.z * hashGridScale) % hashGridSize;
            if (z < 0)
            {
                z += hashGridSize;
            }
            return hashGrid[x + z * hashGridSize];
        }

    }

    public struct EdgeVertices
    {
        public Vector3 v1, v2, v3, v4, v5;

        public EdgeVertices(Vector3 corner1, Vector3 corner2)
        {
            v1 = corner1;
            v2 = Vector3.Lerp(corner1, corner2, 0.25f);
            v3 = Vector3.Lerp(corner1, corner2, 0.5f);
            v4 = Vector3.Lerp(corner1, corner2, 0.75f);
            v5 = corner2;
        }
        public EdgeVertices(Vector3 corner1, Vector3 corner2, float outerStep)
        {
            v1 = corner1;
            v2 = Vector3.Lerp(corner1, corner2, outerStep);
            v3 = Vector3.Lerp(corner1, corner2, 0.5f);
            v4 = Vector3.Lerp(corner1, corner2, 1f - outerStep);
            v5 = corner2;
        }

        public static EdgeVertices TerraceLerp(EdgeVertices a, EdgeVertices b, int step)
        {
            EdgeVertices result;
            result.v1 = HexMetrics.TerraceLerp(a.v1, b.v1, step);
            result.v2 = HexMetrics.TerraceLerp(a.v2, b.v2, step);
            result.v3 = HexMetrics.TerraceLerp(a.v3, b.v3, step);
            result.v4 = HexMetrics.TerraceLerp(a.v4, b.v4, step);
            result.v5 = HexMetrics.TerraceLerp(a.v5, b.v5, step);
            return result;
        }


    }
}