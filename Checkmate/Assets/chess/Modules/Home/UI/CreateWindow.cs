using Checkmate.Module;
using FairyGUI;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Modules.Home.UI
{
    public class CreateWindow : FGUIWindow
    {
        GButton mReadyBtn,mExitBtn;
        GComboBox mMapSelector;//地图选择
        GTextInput mTitle;

        RoomManager mManager;


        private string mActiveMap;//当前地图
       
        protected override bool IsModal { get { return true; } }

        protected override void OnLoad()
        {
            base.OnLoad();
            mReadyBtn = mCtrlTarget.GetChild("ConfirmBtn").asButton;
            mExitBtn = mCtrlTarget.GetChild("CancelBtn").asButton;
           
            mTitle = mCtrlTarget.GetChildByPath("Title.Input").asTextInput;

            mMapSelector = mCtrlTarget.GetChild("MapSelector").asComboBox;

            

            mReadyBtn.onClick.Add(OnBtnReadyClicked);
            mExitBtn.onClick.Add(OnBtnExitClicked);

            mMapSelector.onChanged.Add(OnMapChanged);
           
        }

        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);
            mManager = arg as RoomManager;

            mMapSelector.items = mManager.mAllMaps.ToArray();
            mMapSelector.values= mManager.mAllMaps.ToArray();
            //使窗口居中
            mWindow.AddRelation(GRoot.inst, RelationType.Center_Center);
            
        }

  

        //准备按钮
        void OnBtnReadyClicked()
        {
            string name = mTitle.text;
            mManager.CreateRoom(name, mActiveMap);
        }
        //退出按钮
        void OnBtnExitClicked()
        {
            mManager.CancelCreate();
        }

        //下拉框
        void OnMapChanged()
        {
            mActiveMap = mMapSelector.value;
        }


        protected override void OnPanelDestroy()
        {
            base.OnPanelDestroy();
            mMapSelector.onChanged.Clear();
        }
    }
}
