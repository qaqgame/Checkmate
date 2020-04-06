using FairyGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Unity.FGUI
{
    public class VirtualList
    {
        protected GList mList;//列表ui

        /// <summary>
        /// 渲染时的调用事件
        /// itemIndex-childIndex-object
        /// </summary>
        public Action<int, int, GObject> onRenderItem;

        /// <summary>
        /// 点击时的调用事件
        /// itemIndex-childIndex-object
        /// </summary>
        public Action<int, int, GObject> onItemClicked;
        public Action<int, int, GObject> onItemDoubleClicked;

        //渲染单个项时的调用
        /// <summary>
        /// 在该函数内添加事件时，使用委托，传递引用，使用Set
        /// </summary>
        /// <param name="index"></param>
        /// <param name="obj"></param>
        protected void OnRenderItem(int index, GObject obj)
        {
            int childIndex = mList.ItemIndexToChildIndex(index);
            if(onRenderItem!=null)onRenderItem(index, childIndex, obj);
        }

        /// <summary>
        /// 点击事件
        /// </summary>
        /// <param name="context"></param>
        private void OnItemClicked(EventContext context)
        {
            GObject item = context.data as GObject;
            int index = mList.GetChildIndex(item);
            int selIdx = mList.ChildIndexToItemIndex(index);
            //如果是双击
            if (context.inputEvent.isDoubleClick)
            {
                if(onItemDoubleClicked!=null)onItemDoubleClicked(selIdx, index, item);
            }
            else
            {
                if(onItemClicked!=null)onItemClicked(selIdx, index, item);
            }
        }

        public VirtualList(GList list,int size)
        {
            mList = list;
            mList.itemRenderer = OnRenderItem;
            mList.onClickItem.Add(OnItemClicked);
            mList.numItems = size;
            //启动虚拟列表
            mList.SetVirtual();
        }

        public void Refresh(int newSize)
        {
            mList.numItems = newSize;
        }

    }
}
