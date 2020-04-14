using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using Checkmate.Global.Data;
using QGF.Codec;
using QGF;

namespace Checkmate.Game
{
    public class HexMapEditor : MonoBehaviour
    {
        public HexGrid hexGrid;
        public Dropdown features, terrains;
        private int activeTerrain;//当前颜色
        private int activeElevation, activeWaterLevel, activeFeatureLevel;//当前高度,水平面高度
        private int brushSize = 0;
        private int activeFeature = -1;

        bool applyTerrain, applyElevation = false, applyWaterLevel = false, applyFeature = false;//是否使用颜色,高度，水面

        enum OptionalToggle
        {
            Ignore, Yes, No
        }

        OptionalToggle riverMode, roadMode;//河流编辑模式

        bool isDrag;//当前是否在拖动
        HexDirection dragDirection;//拖动方向
        HexCell previousCell;//先前的单元格

        private string rootPath;//
        private void Awake()
        {
#if UNITY_EDITOR
            rootPath = Application.dataPath + "/Test/";
#endif
        }

        // Start is called before the first frame update
        void Start()
        {
            hexGrid.CreateNewWorld(30, 30);
            InitDropDown();

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                HandleInput();
            }
            else
            {
                previousCell = null;
            }
        }

        private void HandleInput()
        {
            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(inputRay, out hit))
            {
                HexCell currentCell = hexGrid.GetCell(hit.point);
                //检查是否是拖动
                if (previousCell && previousCell != currentCell)
                {
                    ValidateDrag(currentCell);
                }
                else
                {
                    isDrag = false;
                }

                EditCells(currentCell);
                previousCell = currentCell;
            }
            else
            {
                previousCell = null;
            }
        }

        public void SelectTerrain(int idx)
        {
            activeTerrain = hexGrid.GetTerrainId(terrains.options[idx].text);
        }
        public void SetBrushSize(float size)
        {
            brushSize = (int)size;
        }
        public void SetElevation(float e)
        {
            activeElevation = (int)e;
        }

        private void EditCell(HexCell cell)
        {
            if (cell)
            {
                if (applyTerrain)
                {
                    cell.TerrainTypeIndex = activeTerrain;
                    Debug.Log("change terrain:" + activeTerrain);
                }
                if (applyElevation)
                {
                    cell.Elevation = activeElevation;
                }
                if (applyWaterLevel)
                {
                    cell.WaterLevel = activeWaterLevel;
                }
                if (applyFeature && cell.IsValidSmallFeature())
                {
                    cell.Feature = activeFeature;
                    cell.FeatureLevel = activeFeatureLevel;
                }
                if (riverMode == OptionalToggle.No)
                {
                    cell.RemoveRiver();
                }
                if (roadMode == OptionalToggle.No)
                {
                    cell.RemoveRoads();
                }
                if (isDrag)
                {
                    HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                    if (otherCell)
                    {
                        if (riverMode == OptionalToggle.Yes)
                        {
                            otherCell.SetOutgoingRiver(dragDirection);
                        }
                        if (roadMode == OptionalToggle.Yes)
                        {
                            otherCell.AddRoad(dragDirection);
                        }
                    }
                }
            }
            // hexGrid.Refresh();
        }
        //编辑以center为中心的范围
        private void EditCells(HexCell center)
        {
            Debug.Log("点击:" + center.coordinates.ToString());
            int centerX = center.coordinates.X;
            int centerY = center.coordinates.Y;
            for (int x = -brushSize; x <= brushSize; ++x)
            {
                int minOffset = Mathf.Max(-brushSize, -x - brushSize);
                int maxOffSet = Mathf.Min(brushSize, -x + brushSize);
                for (int y = minOffset; y <= maxOffSet; ++y)
                {
                    int rx = centerX + x;
                    int ry = centerY + y;
                    EditCell(hexGrid.GetCell(new Position(rx, ry, -rx - ry)));
                }
            }
        }

        //验证拖动并找到拖动方向
        private void ValidateDrag(HexCell currentCell)
        {
            //遍历所有方向
            for (dragDirection = HexDirection.NE; dragDirection <= HexDirection.N; ++dragDirection)
            {
                if (previousCell.GetNeighbor(dragDirection) == currentCell)
                {
                    isDrag = true;
                    return;
                }
            }
            isDrag = false;
        }

        public void SetApplyElevation(bool s)
        {
            applyElevation = s;
        }
        public void SetApplyWaterLevel(bool s)
        {
            applyWaterLevel = s;
        }
        public void SetApplyFeature(bool s)
        {
            applyFeature = s;
        }
        public void SetApplyTerrain(bool s)
        {
            applyTerrain = s;
        }

        public void SetWaterLevel(float l)
        {
            activeWaterLevel = (int)l;
        }
        public void SetFeatureLevel(float l)
        {
            activeFeatureLevel = (int)l;
        }

        public void SetRiverMode(int mode)
        {
            riverMode = (OptionalToggle)mode;
        }

        public void SetRoadMode(int mode)
        {
            roadMode = (OptionalToggle)mode;
        }

        public void ShowUI(bool v)
        {
            hexGrid.ShowUI(v);
        }

        private void InitDropDown()
        {
            features.AddOptions(hexGrid.GetFeatureNames());
            features.onValueChanged.AddListener(OnFeatureChanged);

            terrains.AddOptions(hexGrid.GetTerrainNames());
            terrains.value = 0;
        }

        private void OnFeatureChanged(int item)
        {
            Debug.Log("selected item:" + features.options[item].text);
            activeFeature = hexGrid.GetFeatureId(features.options[item].text);
        }

        //保存文件
        public void Save()
        {
            string path = rootPath + "testMap.map";
            MapData data = hexGrid.Save("test", "1.0");
            data.mode = "testmode";
            byte[] bytes = PBSerializer.NSerialize(data);
            if (bytes.Length <= 0)
            {
                Debuger.LogError("error generate byte:{0}", data.title);
            }
            using (BinaryWriter writer=new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(bytes);
            }
            Debug.Log("save finished");
        }

        //加载文件
        public void Load()
        {
            string path = rootPath + "testMap.map";
            byte[] bytes = File.ReadAllBytes(path);
            if (bytes.Length <= 0)
            {
                Debuger.LogError("read map file:{0} error!", "testMap");
            }
            MapData data = PBSerializer.NDeserialize<MapData>(bytes);
            if (data.version != "1.0")
            {
                Debuger.LogWarning("map version error!");
            }
            hexGrid.Load(data);
        }
    }
}