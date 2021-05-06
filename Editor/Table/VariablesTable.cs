
using ExceptionSoftware.TreeViewTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace ExceptionSoftware.Variables
{
    public class VariablesTable : VariablesTableWithModel<VariablesElement>
    {
        const float kRowHeights = 20f;
        const float kToggleWidth = 18f;
        public bool editing = false;
        GUIStyle guiStyle = new GUIStyle(EditorStyles.label);

        static Texture2D[] s_TestIcons = {
            EditorGUIUtility.FindTexture ("Favorite Icon"),
            EditorGUIUtility.FindTexture ("console.infoicon.sml"),
            EditorGUIUtility.FindTexture ("console.warnicon.sml")
        };



        // All columns
        enum MyColumns
        {
            Id = 0,
            Group,
            Type,
            Default,
            InGame,
            Description
        }

        public enum SortOption
        {
            Id = 0,
            Group,
            Type,
            Default,
            InGame,
            Description
        }

        // Sort options per column
        SortOption[] m_SortOptions = {
             SortOption.Id ,
            SortOption.Group,
            SortOption.Type,
            SortOption.Default,
            SortOption.InGame,
            SortOption.Description
        };

        public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
        {
            if (root == null)
                throw new NullReferenceException("root");
            if (result == null)
                throw new NullReferenceException("result");

            result.Clear();

            if (root.children == null)
                return;

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            for (int i = root.children.Count - 1; i >= 0; i--)
                stack.Push(root.children[i]);

            while (stack.Count > 0)
            {
                TreeViewItem current = stack.Pop();
                result.Add(current);

                if (current.hasChildren && current.children[0] != null)
                {
                    for (int i = current.children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(current.children[i]);
                    }
                }
            }
        }

        public VariablesTable(TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<VariablesElement> model) : base(state, multicolumnHeader, model)
        {
            Assert.AreEqual(m_SortOptions.Length, Enum.GetValues(typeof(MyColumns)).Length, "Ensure number of sort options are in sync with number of MyColumns enum values");

            // Custom setup
            rowHeight = kRowHeights;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            //customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            customFoldoutYOffset = 0; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = kToggleWidth;
            multicolumnHeader.sortingChanged += OnSortingChanged;

            Reload();
        }


        // Note we We only build the visible rows, only the backend has the full tree information.
        // The treeview only creates info for the row list.
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);
            SortIfNeeded(root, rows);
            return rows;
        }

        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
            {
                return; // No column to sort for (just use the order the data are in)
            }

            // Sort the roots of the existing tree items
            SortByMultipleColumns();
            TreeToList(root, rows);
            Repaint();
        }

        void SortByMultipleColumns()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            var myTypes = rootItem.children.Cast<TreeViewItem<VariablesElement>>();
            var orderedQuery = InitialOrder(myTypes, sortedColumns);
            for (int i = 1; i < sortedColumns.Length; i++)
            {
                SortOption sortOption = m_SortOptions[sortedColumns[i]];
                bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

                switch (sortOption)
                {
                    case SortOption.Id:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.variable.name, ascending);
                        break;
                    case SortOption.Group:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.variable.group, ascending);
                        break;
                }
            }

            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<TreeViewItem<VariablesElement>> InitialOrder(IEnumerable<TreeViewItem<VariablesElement>> myTypes, int[] history)
        {
            SortOption sortOption = m_SortOptions[history[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
            switch (sortOption)
            {
                case SortOption.Id:
                    return myTypes.Order(l => l.data.variable.name, ascending);
                case SortOption.Group:
                    return myTypes.Order(l => l.data.variable.group, ascending);
                default:
                    Assert.IsTrue(false, "Unhandled enum");
                    break;
            }

            return myTypes.Order(l => l.data.name, ascending);
        }


        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<VariablesElement>)args.item;
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
            }
        }

        string _description;
        string _variableID;
        string _variableGroup;
        Variable.VariableType _variableType;
        VariableValue _serializedVariable;
        void CellGUI(Rect cellRect, TreeViewItem<VariablesElement> item, MyColumns column, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case MyColumns.Id:
                    {
                        if (editing)
                        {
                            EditorGUI.BeginChangeCheck();
                            _variableID = EditorGUI.DelayedTextField(cellRect, item.data.variable.name).Trim().Replace(" ", "").Replace("_", "");
                            if (EditorGUI.EndChangeCheck())
                            {
                                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(item.data.variable), _variableID);
                            }
                        }
                        else
                        {
                            DefaultGUI.Label(cellRect, item.data.variable.name, args.selected, args.focused);
                        }
                    }
                    break;
                case MyColumns.Group:
                    {
                        if (editing)
                        {
                            EditorGUI.BeginChangeCheck();
                            _variableGroup = EditorGUI.DelayedTextField(cellRect, item.data.variable.group).Trim().Replace(" ", "").Replace("_", "");
                            if (EditorGUI.EndChangeCheck())
                            {
                                item.data.variable.group = _variableGroup;
                            }
                        }
                        else
                        {
                            DefaultGUI.Label(cellRect, item.data.variable.group, args.selected, args.focused);
                        }
                    }
                    break;

                case MyColumns.Type:
                    if (editing)
                    {
                        EditorGUI.BeginChangeCheck();
                        _variableType = (Variable.VariableType)EditorGUI.EnumPopup(cellRect, item.data.variable.Type);
                        if (EditorGUI.EndChangeCheck())
                        {
                            item.data.variable.Type = _variableType;
                            EditorUtility.SetDirty(item.data.variable);
                        }
                    }
                    else
                    {
                        DefaultGUI.Label(cellRect, item.data.variable.Type.ToString(), args.selected, args.focused);
                    }
                    break;

                case MyColumns.Default:
                    {
                        if (editing)
                        {
                            EditorGUI.BeginChangeCheck();
                            _serializedVariable = VariableEditor.DrawSerializedVariable(cellRect, item.data.variable.Type, item.data.variable.DefaultValue);
                            if (EditorGUI.EndChangeCheck())
                            {
                                EditorUtility.SetDirty(item.data.variable);
                            }
                        }
                        else
                        {
                            DefaultGUI.Label(cellRect, item.data.variable.DefaultValue.ToString(), args.selected, args.focused);
                        }
                    }
                    break;
                case MyColumns.InGame:
                    {
                        if (editing)
                        {
                            EditorGUI.BeginChangeCheck();
                            _serializedVariable = VariableEditor.DrawSerializedVariable(cellRect, item.data.variable.Type, item.data.variable.InGameValue);
                            if (EditorGUI.EndChangeCheck())
                            {
                                EditorUtility.SetDirty(item.data.variable);
                            }
                        }
                        else
                        {
                            DefaultGUI.Label(cellRect, item.data.variable.InGameValue.ToString(), args.selected, args.focused);
                        }
                    }
                    break;


                case MyColumns.Description:
                    {
                        if (editing)
                        {
                            EditorGUI.BeginChangeCheck();
                            _description = EditorGUI.DelayedTextField(cellRect, item.data.variable.description);
                            if (EditorGUI.EndChangeCheck())
                            {
                                item.data.variable.description = _description;
                            }
                        }
                        else
                        {
                            DefaultGUI.Label(cellRect, item.data.variable.description, args.selected, args.focused);
                        }
                    }
                    break;
            }


        }

        // Rename
        //--------

        protected override bool CanRename(TreeViewItem item)
        {
            Rect renameRect = GetRenameRect(treeViewRect, 0, item);
            return renameRect.width > 30;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (args.acceptedRename)
            {
                var element = treeModel.Find(args.itemID);
                VariablesUtilityEditor.RenameAsset(element.variable, args.newName);
                treeModel.SetData(VariablesTableGenerator.GenerateTree());
                Reload();
            }
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            Rect cellRect = GetCellRectForTreeFoldouts(rowRect);
            CenterRectUsingSingleLineHeight(ref cellRect);
            return base.GetRenameRect(cellRect, row, item);
        }

        // Misc
        //--------


        //static float 
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[] {
                 
                //NAME
                new MultiColumnHeaderState.Column {
                    headerContent = new GUIContent (MyColumns.Id.ToString()),
                    contextMenuText = "Name",
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    minWidth = 300,
                    width = 300,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column {
                    headerContent = new GUIContent (MyColumns.Group.ToString()),
                    contextMenuText = "Group",
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    minWidth = 150,
                    width = 150,
                    autoResize = true,
                    allowToggleVisibility = true
                },

                //Priority
                new MultiColumnHeaderState.Column {
                    headerContent = new GUIContent (MyColumns.Type.ToString()),
                    contextMenuText = "Type",
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    minWidth = 60,
                    width = 60,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                //keylist
                new MultiColumnHeaderState.Column {
                    headerContent = new GUIContent (MyColumns.Default.ToString()),
                    contextMenuText = "Default",
                    headerTextAlignment = TextAlignment.Left,
                     canSort=false,
                    minWidth = 120,
                    width = 120,
                    autoResize = true,
                    allowToggleVisibility = true
                },
              new MultiColumnHeaderState.Column {
                    headerContent = new GUIContent (MyColumns.InGame.ToString()),
                    contextMenuText = "InGame",
                    headerTextAlignment = TextAlignment.Left,
                     canSort=false,
                    minWidth = 120,
                    width = 120,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                //TYPEfILE
                new MultiColumnHeaderState.Column {
                    headerContent = new GUIContent (MyColumns.Description.ToString()),
                    contextMenuText = "Description",
                    headerTextAlignment = TextAlignment.Left,
                    canSort=false,
                    minWidth = 120,
                    width = 200,
                    autoResize = true,
                    allowToggleVisibility = false
                }
            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(MyColumns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state = new MultiColumnHeaderState(columns);
            return state;
        }



        #region Selected

        protected override bool CanMultiSelect(TreeViewItem item) => true;
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
            {
                selection = null;
            }
            else
            {
                selection.itemsSelected = treeModel.Find(selectedIds).Select(s => s.variable).ToList();
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            try
            {
                Selection.activeObject = treeModel.Find(id).variable;
            }
            catch { }


        }

        public VariablesSelected selection = new VariablesSelected();

        public class VariablesSelected
        {
            public List<Variable> itemsSelected = null;
        }

        protected override void KeyEvent()
        {
            base.KeyEvent();
        }

        #endregion



    }


}
