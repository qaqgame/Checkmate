using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System.IO;
using Checkmate.Global.Data;
using Checkmate.Game.Utils;
using QGF.Codec;
using Newtonsoft.Json;
using Checkmate.Game.Feature;
using Checkmate.Game.Effect;
using QGF;

namespace Checkmate.Game
{
    public class HexGrid : MonoBehaviour
    {
        public Color defaultColor = Color.white;//默认方块颜色
        public Color touchedColor = Color.magenta;//点击颜色
        public HexCell cellPrefab;//单元格预制体
        public RectTransform cellPanelPrefab;//坐标显示的预制体
        public Texture2D noiseSource;//噪声纹理
        public HexGridChunk chunkPrefab;//块的预制体
                                        // Canvas gridCanvas;//画布
                                        // HexMesh hexMesh;//六边形的网格

        public int chunkCountX = 4, chunkCountZ = 4;//块的数目

        private int cellCountX, cellCountZ;//单元格数目
        private HexCell[] cells;//单元格
        private HexGridChunk[] chunks;//块
        private HexCellShaderData cellShaderData;//块的纹理数据
        public int seed;//随机数种子

        private static List<FeatureData> mFeatureData;//特征的名称
        private static List<Checkmate.Global.Data.TerrainData> mTerrainData;//地形数据
        private static Texture2DArray mTerrainTextures;//地形的贴图

        private static FeatureManager mFeatureMng;//特征管理（列表管理）
        private static EffectManager mEffectMng;//效果管理（列表管理)

        public static FeatureManager Features
        {
            get
            {
                return mFeatureMng;
            }
        }

        public static EffectManager Effects
        {
            get
            {
                return mEffectMng;
            }
        }

        //获取目标索引地形的id
        public static int GetTerrainId(int idx)
        {
            return mTerrainData[idx].id;
        }

        private void OnEnable()
        {
            if (!HexMetrics.noiseSource)
            {
                HexMetrics.noiseSource = noiseSource;
                HexMetrics.InitializeHashGrid(seed);
            }
        }

        private void OnDestroy()
        {
            //清除特征与效果管理
        }

        private void Awake()
        {

            // gridCanvas=GetComponentInChildren<Canvas>();
            // hexMesh=GetComponentInChildren<HexMesh>();
            HexMetrics.noiseSource = noiseSource;
            mFeatureData = new List<FeatureData>();
            mTerrainData = new List<Global.Data.TerrainData>();
            HexMetrics.featurePrefabs = new List<Transform>();
            HexMetrics.featureEffects = new Dictionary<int, string>();
            cellShaderData = gameObject.AddComponent<HexCellShaderData>();
        }

        public void CreateNewWorld(int cellX, int cellZ)
        {
            //检测地图大小
            if (cellX <= 0 || cellZ <= 0 || cellX % HexMetrics.chunkSizeX != 0 || cellZ % HexMetrics.chunkSizeZ != 0)
            {
                Debug.LogError("error create world:" + cellX + "," + cellZ);
                return;
            }
            DestroyAll();
            seed = (int)Random.value;
            HexMetrics.InitializeHashGrid(seed);
            CreateFeatures();
            CreateTerrains();
            if (HexMetrics.featurePrefabs.Count > 0)
            {
                Debug.Log("create features success");
            }
            chunkCountX = cellX / HexMetrics.chunkSizeX;
            chunkCountZ = cellZ / HexMetrics.chunkSizeZ;
            cellCountX = cellX;
            cellCountZ = cellZ;
            cellShaderData.Init(cellCountX, cellCountZ);
            CreateChunks();
            CreateCell();
        }
        // Start is called before the first frame update
        // void Start()
        // {
        //     hexMesh.Triangulate(cells);
        // }

        // Update is called once per frame
        void Update()
        {
        }

        // public void Refresh(){
        //     hexMesh.Triangulate(cells);
        // }
        //此函数由Load调用
        private void CreateWorld(int chunkX, int chunkZ, int randomSeed, List<FeatureData> features,List<Checkmate.Global.Data.TerrainData> terrains,List<EffectData> effects)
        {
            DestroyAll();
            LoadFeatures(features);
            LoadTerrains(terrains);
            HexMetrics.InitializeHashGrid(randomSeed);
            chunkCountX = chunkX;
            chunkCountZ = chunkZ;
            cellCountX = chunkCountX * HexMetrics.chunkSizeX;
            cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
            cellShaderData.Init(cellCountX, cellCountZ);
            CreateChunks();
            CreateCell();
        }

        private void CreateChunks()
        {
            chunks = new HexGridChunk[chunkCountX * chunkCountZ];
            for (int z = 0, i = 0; z < chunkCountZ; ++z)
            {
                for (int x = 0; x < chunkCountX; ++x)
                {
                    HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                    chunk.transform.SetParent(transform);
                }
            }
        }
        private void CreateCell()
        {
            cells = new HexCell[cellCountZ * cellCountX];
            for (int z = 0, i = 0; z < cellCountZ; ++z)
            {
                for (int x = 0; x < cellCountX; ++x)
                {
                    CreateCell(x, z, i++);
                }
            }
        }

        private void CreateCell(int x, int z, int i)
        {
            Vector3 position;
            position.x = x * HexMetrics.outerRadius * 1.5f;
            position.y = 0f;
            //向上取,在我们规定的坐标系中，y轴向下为正
            position.z = (z - 0.5f * (x & 1)) * HexMetrics.innerRadius * 2f;

            HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
            //cell.transform.SetParent(transform,false);
            cell.transform.localPosition = position;
            cell.coordinates = HexPosition.FromOffsetToHex(x, z);
            cell.ShaderData = cellShaderData;
            cell.TerrainTypeIndex = 0;
            cell.Index = i;

            //设置邻接
            if (z > 0)
            {
                cell.SetNeighbor(HexDirection.S, cells[i - cellCountX]);
            }
            if (x > 0)
            {
                if ((x & 1) == 1)
                {
                    cell.SetNeighbor(HexDirection.NW, cells[i - 1]);
                    if (z > 0)
                    {
                        cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                        if (x < cellCountX - 1)
                        {
                            cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                        }
                    }
                }
                else
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - 1]);
                }
            }

            RectTransform cellPanel = Instantiate<RectTransform>(cellPanelPrefab);
            //label.rectTransform.SetParent(gridCanvas.transform,false);
            cellPanel.anchoredPosition = new Vector2(position.x, position.z);
            cellPanel.GetChild(0).GetComponent<Text>().text = cell.coordinates.ToStringOnSeparateLines();
            cell.uiRect = cellPanel;
            cell.Elevation = 0;

            AddCellToChunk(x, z, cell);
        }
        //添加单元格到块
        private void AddCellToChunk(int x, int z, HexCell cell)
        {
            int chunkX = x / HexMetrics.chunkSizeX;
            int chunkZ = z / HexMetrics.chunkSizeZ;
            HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];
            //计算单元格在块中的坐标
            int localX = x - chunkX * HexMetrics.chunkSizeX;
            int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
            chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
        }


        public int GetCellIdx(HexPosition hex)
        {
            int row = hex.Z + (hex.X + (hex.X & 1)) / 2;
            if (row < 0 || row >= cellCountZ)
            {
                return -1;
            }
            if (hex.X < 0 || hex.X >= cellCountX)
            {
                return -1;
            }
            return hex.X + row * cellCountX;
        }

        public HexCell GetCell(HexPosition hex)
        {
            int idx = GetCellIdx(hex);
            if (idx == -1)
            {
                return null;
            }
            return cells[idx];
        }
        public HexCell GetCell(Position pos)
        {
            return GetCell(new HexPosition(pos.x, pos.z));
        }
        //通过世界坐标（与平面的交点）来获取cell
        public HexCell GetCell(Vector3 worldPosition)
        {
            worldPosition = transform.InverseTransformPoint(worldPosition);
            HexPosition hex = HexPosition.FromWorldPosition(worldPosition);
            return cells[GetCellIdx(hex)];
        }

        public void ShowUI(bool visible)
        {
            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].ShowUI(visible);
            }
        }

        private void DestroyAll()
        {
            if (chunks != null)
            {
                for (int i = 0; i < chunks.Length; ++i)
                {
                    Destroy(chunks[i].gameObject);
                }
            }
        }
        //创建特征
        private void CreateFeatures()
        {
            string path = Application.dataPath + "/Config/Features.json";
            string ePath = Application.dataPath + "/Config/FeatureEffect.json";
            HexMetrics.featurePrefabs.Clear();
            HexMetrics.featureEffects.Clear();
            mFeatureData.Clear();
            //创建时从配置文件中读取
            List<FeatureData> features = MapUtils.ParseFeatures(path);
            InitializeFeatures(features);
        }
        private void LoadFeatures(List<FeatureData> features)
        {
            mFeatureData= features;
            HexMetrics.featurePrefabs.Clear();
            InitializeFeatures(mFeatureData);
        }

        private void InitializeFeatures(List<FeatureData> features)
        {
            mFeatureData = features;
            string path = "Map/Features/";
            foreach (var f in features)
            {
                Debug.Log("load features:" + f.name + f.file);

                Transform prefab= (Resources.Load(path + f.file) as GameObject).transform;
                
                HexMetrics.featurePrefabs.Add(prefab);
            }
        }
        //创建地形
        private void CreateTerrains()
        {
            string path = Application.dataPath + "/Config/Terrains.json";
            mTerrainData.Clear();
            List<Checkmate.Global.Data.TerrainData> terrains = MapUtils.ParseTerrains(path);
            InitializeTerrains(terrains);
        }

        //加载地形
        private void LoadTerrains(List<Checkmate.Global.Data.TerrainData> terrains)
        {
            mTerrainData.Clear(); 
            InitializeTerrains(terrains);
        }
        private void InitializeTerrains(List<Global.Data.TerrainData> terrains)
        {
            mTerrainData = terrains;
            List<Texture2D> textures = new List<Texture2D>();
            string path = Application.dataPath + "/Resources/Map/Terrains/";
            for (int i = 0; i < terrains.Count; ++i)
            {
                textures.Add(AssetUtil.LoadPicture(path + terrains[i].file, 1024));
            }
            mTerrainTextures = AssetUtil.CreateTextureArray(textures);
            Debug.Log("texs size:" + mTerrainTextures.depth);
        }

        public int GetFeaturesCount()
        {
            return mFeatureData.Count;
        }

        public int GetFeatureId(string name)
        {
            return mFeatureData.FindIndex(a=>a.name==name);
        }

        public List<string> GetFeatureNames()
        {
            List<string> result = new List<string>();
            foreach(var item in mFeatureData)
            {
                result.Add(item.name);
            }
            return result;
        }

        public int GetTerrainId(string name)
        {
            return mTerrainData.FindIndex(a => a.name == name);
        }
        public List<string> GetTerrainNames()
        {
            List<string> result = new List<string>();
            foreach (var item in mTerrainData)
            {
                result.Add(item.name);
            }
            return result;
        }

        public static Texture2DArray GetTerrainTextures()
        {
            return mTerrainTextures;
        }





        // public void ColorCell(Position position,Color color){
        //     HexPosition hex=new HexPosition(position.x,position.z);
        //     cells[GetCellIdx(hex)].color=color;
        //     hexMesh.Triangulate(cells);
        // }

        // public void ColorCell(HexPosition position,Color color){
        //     cells[GetCellIdx(position)].color=color;
        //     hexMesh.Triangulate(cells);
        // }
        private List<FeatureData> SaveFeatures()
        {
            return mFeatureData;
        }

        private List<Checkmate.Global.Data.TerrainData> SaveTerrain()
        {
            return mTerrainData;
        }

        //private void SaveTerrain(BinaryWriter writer)
        //{
        //    List<TerrainFormat> formats = new List<TerrainFormat>();
        //    foreach (var pair in terrainNames)
        //    {
        //        string file = terrainFiles[pair.Value];
        //        formats.Add(new TerrainFormat(pair.Key, file));
        //    }
        //    string content = JsonConvert.SerializeObject(formats);
        //    Debug.Log("save terrains:" + content);
        //    writer.Write(content);
        //}

        //private void SaveFeatureEffect(BinaryWriter writer)
        //{
        //    List<FeatureEffectFormat> formats = new List<FeatureEffectFormat>();
        //    foreach (var pair in HexMetrics.featureEffects)
        //    {
        //        formats.Add(new FeatureEffectFormat(pair.Key, pair.Value));
        //    }
        //    string content = JsonConvert.SerializeObject(formats);
        //    writer.Write(content);
        //}

        //保存
        public MapData Save(string title,string version)
        {
            MapData data = new MapData();
            data.sizeX = chunkCountX;
            data.sizeZ = chunkCountZ;
            data.seed = seed;
            data.title = title;
            data.version = version;


            data.terrains = SaveTerrain();
            data.features = SaveFeatures();
            Debug.Log("saved features:"+data.features.Count);
            data.effects = new List<EffectData>();

            List<MapCellData> tempCellData = new List<MapCellData>();
            for (int i = 0; i < cells.Length; ++i)
            {
                MapCellData cellData= cells[i].Save();
                tempCellData.Add(cellData);
            }
            data.cells = tempCellData;

            return data;
        }

        //加载
        public void Load(MapData data)
        {
            int chunkX = data.sizeX;
            int chunkZ = data.sizeZ;
            int seed = data.seed;
            List<FeatureData> features = data.features;
            Debug.Log("load features:"+features.Count);
            List<Checkmate.Global.Data.TerrainData> terrains = data.terrains;
            List<EffectData> effects = data.effects;
            //开始加载效果和地形列表
            LoadData(features, effects);

            CreateWorld(chunkX, chunkZ, seed,features,terrains,null );
            for (int i = 0; i < cells.Length; ++i)
            {
                cells[i].Load(data.cells[i]);
            }
            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].Refresh();
            }
        }


        //加载非渲染数据
        private void LoadData(List<FeatureData> features,List<EffectData> effects)
        {
            mEffectMng = new EffectManager();
            mFeatureMng = new FeatureManager();
            //加载效果
            mEffectMng.Init(effects);
            //加载特征
            mFeatureMng.Init(features);
            
        }
    }
}