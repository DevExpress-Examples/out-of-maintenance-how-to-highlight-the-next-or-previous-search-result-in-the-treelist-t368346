using DevExpress.Utils.Paint;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public class FindHelper : Component
    {
        public FindHelper()
        {
            BackGroundColor = Color.Green;
            HighLightColor = Color.Gold;
        }

        ButtonEdit edit;
        TreeList treeList;
        string filterCellText = string.Empty;
        Dictionary<TreeCell, bool> findList = new Dictionary<TreeCell, bool>();
        bool FindListIsEmpty { get { return findList.Keys.Count == 0; } }
        public Color BackGroundColor { get; set; }
        public Color HighLightColor { get; set; }
        EditorButton showResult;

        public TreeList TargetControl
        {
            get
            {
                return treeList;
            }
            set
            {
                SubscibeTreeListEvent(false);
                treeList = value;
                SubscibeTreeListEvent(true);
            }
        }

        public ButtonEdit SearchControl
        {
            get
            {
                return edit;
            }
            set
            {
                SubscribeRIEvent(false);
                edit = value;
                ResetActiveRI();
            }
        }
  
        private void ResetActiveRI()
        {
            if (DesignMode) return;
            edit.Properties.Buttons.Clear();
            showResult = new EditorButton(ButtonPredefines.Glyph, "0 of 0", 0, false, true, false, ImageLocation.MiddleCenter, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), null, "", null, null, true);
            edit.Properties.Buttons.AddRange(new EditorButton[] {
            new EditorButton(ButtonPredefines.Search),
            new EditorButton(ButtonPredefines.Clear),
            new EditorButton(ButtonPredefines.SpinLeft),
            new EditorButton(ButtonPredefines.SpinRight),
            showResult
            });

            SubscribeRIEvent(true);
        }

        private void SubscribeRIEvent(bool subscribe)
        {
            if (edit == null) return;
            edit.Properties.ButtonClick -= ButtonClick;
            if (subscribe)
                edit.Properties.ButtonClick += ButtonClick;
        }

        private void SubscibeTreeListEvent(bool subscribe)
        {
            if (treeList == null) return;
            treeList.CustomDrawNodeCell -= treeList_CustomDrawNodeCell;
            if(subscribe)
                treeList.CustomDrawNodeCell += treeList_CustomDrawNodeCell;
        }

        void treeList_CustomDrawNodeCell(object sender, CustomDrawNodeCellEventArgs e)
        {
            if (FindListIsEmpty) return;

            int filterTextIndex = e.CellText.IndexOf(filterCellText, StringComparison.CurrentCultureIgnoreCase);
            if (filterTextIndex == -1)
                return;
            TreeCell temp = new TreeCell(e.Node.Id, e.Column);

            if (NeedHighLight(temp))                
                using(SolidBrush brush = new SolidBrush(BackGroundColor)) {
                    e.Cache.FillRectangle(brush, e.Bounds);
                }
                

            TextEditViewInfo tevi = e.EditViewInfo as TextEditViewInfo;
            if (tevi == null)
                return;
            e.Appearance.BackColor = Color.Empty;                        
            e.Cache.Paint.DrawMultiColorString(e.Cache, tevi.MaskBoxRect, e.CellText, filterCellText, e.Appearance, e.Appearance.ForeColor, HighLightColor, false, filterTextIndex);
            e.Handled = true;
        }

        protected override void Dispose(bool disposing)
        {
            SubscibeTreeListEvent(false);
            SubscribeRIEvent(false);
            base.Dispose(disposing);
        }


        void ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            ButtonEdit edit = sender as ButtonEdit;
            switch (e.Button.Kind)
            {
                case ButtonPredefines.Search:
                    PerformSearch(edit.EditValue);
                    break;
                case ButtonPredefines.Clear:
                    ClearEditValue(edit);
                    PerformSearch(null);
                    break;
                case ButtonPredefines.SpinLeft:
                    HighLightPrevious();
                    break;
                case ButtonPredefines.SpinRight:
                    HighLightNext();
                    break;
            }
        }

        private void UpdateShowResult()
        {
            int index;
            for (index = 0; index < findList.Keys.Count; index++)
                if (findList[findList.Keys.ElementAt(index)])
                {
                    index++;
                    break;
                }
            showResult.Caption = string.Format("{0} of {1}", index, findList.Keys.Count);
          
        }

        private void HighLightPrevious()
        {

            if (FindListIsEmpty) return;

            TreeCell currItem = findList.Keys.ElementAt(0);
            TreeCell targetItem = findList.Keys.ElementAt(findList.Keys.Count - 1);
            TreeCell temp;
            for (int i = 1; i < findList.Keys.Count; i++)
            {
                temp = findList.Keys.ElementAt(i);
                if (findList[temp])
                {
                    targetItem = findList.Keys.ElementAt(i - 1);
                    currItem = temp;
                    break;
                }
            }

            findList[currItem] = false;
            findList[targetItem] = true;
            EnsureNodeVisible(targetItem);
            RefreshTreeList();
            UpdateShowResult();
        }

        private void HighLightNext()
        {
            if (FindListIsEmpty) return;

            bool needBreak = false;
            TreeCell currItem = null;
            TreeCell targetItem = null;
            foreach (TreeCell item in findList.Keys)
            {
                if (needBreak)
                {
                    targetItem = item;
                    break;
                }
                if (findList[item])
                {
                    currItem = item;
                    needBreak = true;
                }
            }

            if (targetItem == null)
                targetItem = findList.Keys.ElementAt(0);

            findList[currItem] = false;
            findList[targetItem] = true;
            EnsureNodeVisible(targetItem);
            RefreshTreeList();
            UpdateShowResult();
        }

        private void EnsureNodeVisible(TreeCell cell)
        {
            treeList.MakeNodeVisible(treeList.FindNodeByID(cell.NodeID));
            treeList.FocusedColumn = cell.Column;
        }

        private void ClearEditValue(ButtonEdit edit)
        {
            edit.EditValue = null;
        }

        private void PerformSearch(object val)
        {
            findList.Clear();
            if (val == null) val = string.Empty;
            filterCellText = val.ToString();
            InitFindList();
            if (!FindListIsEmpty)
                EnsureNodeVisible(findList.Keys.ElementAt(0));
            RefreshTreeList();
            UpdateShowResult();
        }

        private void InitFindList()
        {
            if (String.IsNullOrEmpty(filterCellText))
                return;
            treeList.NodesIterator.DoOperation((node) =>
            {
                foreach (TreeListColumn col in node.TreeList.Columns)
                {
                    if (!col.Visible || (col.RealColumnEdit as RepositoryItemTextEdit) == null)
                        continue;
                    if (node.GetDisplayText(col).IndexOf(filterCellText, StringComparison.CurrentCultureIgnoreCase) == -1)
                        continue;
                    findList.Add(new TreeCell(node.Id, col), false);
                }
            });
            if (FindListIsEmpty) return;
            findList[findList.Keys.ElementAt(0)] = true;
        }

        private void RefreshTreeList()
        {
            treeList.LayoutChanged();
        }

        private bool NeedHighLight(TreeCell cell)
        {
            foreach (TreeCell item in findList.Keys)
                if (item.NodeID == cell.NodeID && item.Column == cell.Column)
                    return findList[item];
            return false;
        }

        class TreeCell
        {
            public int NodeID { get; set; }
            public TreeListColumn Column { get; set; }
            public TreeCell(int id, TreeListColumn c)
            {
                NodeID = id;
                Column = c;
            }
        }
    }
}
