using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using Checkmate.Global.Data;
using QGF.Codec;
using QGF;
using QGF.Utils;
using Newtonsoft.Json;

namespace Checkmate.Game
{
    public class HexMapEditor : MonoBehaviour
    {
        public HexGrid hexGrid;
        public Dropdown features, terrains,roles,teams,rules;
        private int activeTerrain;//当前颜色
        private int activeElevation, activeWaterLevel, activeFeatureLevel;//当前高度,水平面高度
        private int brushSize = 0;
        private int activeFeature = -1;
        private int maxTeam=1;//最大队伍数
        private string mActiveRule;//当前规则
        private string mActiveName;//当前地图名

        bool applyTerrain, applyElevation = false, applyWaterLevel = false, applyFeature = false;//是否使用颜色,高度，水面


        private List<string> mAllRoles = new List<string>();
        private string mActiveRole;//当前选择角色
        private int mActiveTeam;//当前选择队伍

        private List<string> mAllRules = new List<string> { "KillMode" };//所有规则


        private List<RoleTrack> mSelRoles = new List<RoleTrack>();

        enum OptionalToggle
        {
            Ignore, Yes, No
        }

        OptionalToggle riverMode, roadMode,roleMode;//河流编辑模式

        bool isDrag;//当前是否在拖动
        HexDirection dragDirection;//拖动方向
        HexCell previousCell;//先前的单元格

        private string rootPath;//
        private void Awake()
        {
            string rolePath;
#if UNITY_EDITOR
            rootPath = Application.dataPath + "/Test/";
            rolePath=Application.dataPath+"/Test/Roles";
#else
            rootPath = Application.streamingAssetsPath + "/Maps/";
            rolePath = Application.streamingAssetsPath + "/Roles";
#endif
            DirectoryInfo folder = new DirectoryInfo(rolePath);

            foreach(var fileInfo in folder.GetFiles())
            {
                Debug.Log("extension:" + fileInfo.Extension);
                if (fileInfo.Extension == ".json")
                {
                    string roleName = fileInfo.Name.Remove(fileInfo.Name.LastIndexOf('.'));
                    mAllRoles.Add(roleName);
                }
            }
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

        public void SelectRole(int idx)
        {
            mActiveRole = mAllRoles[idx];
            Debug.Log("select role:" + mActiveRole);
        }

        public void SelectTeam(int team)
        {
            mActiveTeam = team;
        }

        public void SetBrushSize(float size)
        {
            brushSize = (int)size;
        }
        public void SetElevation(float e)
        {
            activeElevation = (int)e;
        }

        public void SelectRule(int idx)
        {
            mActiveRule = mAllRules[idx];
        }

        public void OnMaxTeamChanged(string max)
        {
            Debug.Log("team changed:" + max);
            int m = int.Parse(max);
            maxTeam = m;
            UpdateTeams();
        }

        public void SetMapName(string name)
        {
            mActiveName = name;
            Debug.Log("set map name:" + name);
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
                if (roleMode == OptionalToggle.Yes)
                {
                    SetRole(cell);
                }
                else if (roleMode == OptionalToggle.No)
                {
                    RemoveRole(cell);
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
        //添加角色
        private void SetRole(HexCell cell)
        {
            Position pos = cell.coordinates.ToPosition();
            int idx = mSelRoles.FindIndex(temp => temp.position.Equals(pos.ToString()));
            //如果此处已有角色
            if (idx != -1)
            {
                //移除
                mSelRoles.RemoveAt(idx);
            }

            RoleTrack track = new RoleTrack();
            track.name = mActiveRole;
            track.team = mActiveTeam;
            track.position = pos.ToString();
            mSelRoles.Add(track);

            cell.EnableLable(track.team + ":" + track.name);
            Debug.Log("add role:" + track.name + " at " + track.position);
        }
        //移除角色
        private void RemoveRole(HexCell cell)
        {
            Position pos = cell.coordinates.ToPosition();
            int idx = mSelRoles.FindIndex(track => track.position.Equals(pos.ToString()));
            //如果此处已有角色
            if (idx != -1)
            {
                //移除
                mSelRoles.RemoveAt(idx);
                cell.EnableLable(pos.ToSeparateString());
                cell.DisableLabel();
                Debug.Log("remomve role at" + pos.ToString());
            }
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

        public void SetRoleMode(int mode)
        {
            roleMode = (OptionalToggle)mode;
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

            roles.AddOptions(mAllRoles);
            roles.value = 0;
            mActiveRole = mAllRoles[0];

            UpdateTeams();

            rules.AddOptions(mAllRules);
            rules.value =0;
            mActiveRule = mAllRules[0];
        }

        //更新队伍选择
        private void UpdateTeams()
        {
            teams.ClearOptions();
            for (int i = 0; i < maxTeam; ++i)
            {
                teams.options.Add(new Dropdown.OptionData(i.ToString()));
            }
        }



        private void OnFeatureChanged(int item)
        {
            Debug.Log("selected item:" + features.options[item].text);
            activeFeature = hexGrid.GetFeatureId(features.options[item].text);
        }

        

        //保存文件
        public void Save()
        {
            string path = rootPath + mActiveName+".map";
            string configPath = rootPath + mActiveName + "_config.json";
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

            //生成配置信息
            MapConfig config = new MapConfig();
            config.Rule = mActiveRule;
            config.MaxTeam = maxTeam;
            config.Roles = mSelRoles;

            string content = JsonConvert.SerializeObject(config);
            using (StreamWriter writer= new StreamWriter(File.Open(configPath, FileMode.Create),System.Text.Encoding.UTF8))
            {
                writer.Write(content);
            }


            Debug.Log("save finished");
        }

        //加载文件
        public void Load()
        {
            string path = rootPath + "testMap.map";
            byte[] bytes = FileUtils.ReadFile(path);
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