using ExceptionSoftware.TreeViewTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ExceptionSoftware.Variables
{

    public class VariablesWindow : EditorWindow
    {
        [NonSerialized] bool m_Initialized;
        [NonSerialized] Rect[] _rectLayout = null;
        [SerializeField] TreeViewState m_TreeViewState;

        // Serialized in the window layout file so it survives assembly reloading
        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        UnityEditor.IMGUI.Controls.SearchField m_SearchField;
        VariablesTable m_TreeView;
        VariablesSettingsAsset _db;

        public void SetTreeAsset(VariablesSettingsAsset Inputdb)
        {
            _db = Inputdb;
            m_Initialized = false;
        }

        Rect toolbarRect { get { return _rectLayout[0]; } }
        Rect searchbarRect { get { return _rectLayout[1]; } }
        Rect EntryTableViewRect { get { return _rectLayout[2]; } }
        public VariablesTable treeView { get { return m_TreeView; } }

        Vector2 _windowSize;
        bool _resized = false;
        void CheckWindowResize()
        {
            if (_windowSize != base.position.size)
            {
                _windowSize = base.position.size;
                _resized = true;
            }
        }
        void InitIfNeeded()
        {
            if (_db == null)
            {
                _db = VariablesUtility.Settings;
            }

            if (_rectLayout == null || _resized)
            {
                _resized = false;
                Rect rpos = base.position.CopyToZero();
                _rectLayout = rpos.SplitSuperFixedFlexible(true, 20, 21, -1);
                //Rect r;
                Vector2 center;
                for (int x = 1; x < _rectLayout.Length; x++)
                {
                    center = _rectLayout[x].center;
                    _rectLayout[x].size = new Vector2(_rectLayout[x].width - 40, _rectLayout[x].height - 5);
                    _rectLayout[x].center = center;
                }
            }


            VariablesUtilityEditor.onNewVariablesCreated -= ReupdateTable;
            VariablesUtilityEditor.onNewVariablesCreated += ReupdateTable;

            if (!m_Initialized)
            {
                // Check if it already exists (deserialized from window layout file or scriptable object)
                if (m_TreeViewState == null)
                    m_TreeViewState = new TreeViewState();

                bool firstInit = m_MultiColumnHeaderState == null;
                var headerState = VariablesTable.CreateDefaultMultiColumnHeaderState(EntryTableViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                m_MultiColumnHeaderState = headerState;

                var multiColumnHeader = new MyMultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                var treeModel = new TreeModel<VariablesElement>(GetData());

                m_TreeView = new VariablesTable(m_TreeViewState, multiColumnHeader, treeModel);

                m_SearchField = new UnityEditor.IMGUI.Controls.SearchField();
                m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

                m_TreeView.searchString = "";
                m_Initialized = true;


            }
        }

        IList<VariablesElement> GetData()
        {
            // generate some test data
            return VariablesTableGenerator.GenerateTree();
        }
        public void ReupdateTable()
        {
            if (m_TreeView == null) return;
            m_TreeView.SetSelection(new List<int>());
            m_TreeView.treeModel.SetData(GetData());
            m_TreeView.Reload();
        }
        private void OnFocus() => ReupdateTable();
        void OnGUI()
        {
            CheckWindowResize();
            InitIfNeeded();

            Toolbar(toolbarRect);
            SearchBar(searchbarRect);
            DoTreeView(EntryTableViewRect);
        }

        void Toolbar(Rect rect)
        {
            GUILayout.BeginArea(rect, EditorStyles.toolbar);
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Reload", EditorStyles.toolbarButton))
                    {
                        ReupdateTable();
                    }

                    GUILayout.FlexibleSpace();

                    //Editting
                    {
                        EditorGUI.BeginChangeCheck();
                        treeView.editing = EditorGUILayout.ToggleLeft("Editing", treeView.editing, GUILayout.Width(100));
                        if (EditorGUI.EndChangeCheck())
                        {
                            ReupdateTable();
                        }
                    }

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Test Xml", EditorStyles.toolbarButton))
                    {
                        Debug.Log(XMLUtils.SerializeObjectToString(VariablesUtility.GetVariablesSerialized()));
                    }
                    if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                    {
                        EditorUtility.SetDirty(_db);
                        AssetDatabase.SaveAssets();
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }

        void SearchBar(Rect rect)
        {
            treeView.Filter.textFilter = treeView.searchString = m_SearchField.OnGUI(rect, treeView.searchString).Trim();
        }

        void DoTreeView(Rect rect)
        {
            m_TreeView.OnGUI(rect);
        }


        #region Creation
        void CreateItem()
        {
            //switch (currentContent)
            //{
            //    case MyContent.Devices:
            //        ShellEditor.Inputs.CreateNewDevice();
            //        break;
            //    case MyContent.Layers:
            //        ShellEditor.Inputs.CreateNewLayer();
            //        break;
            //    case MyContent.Actions:
            //        if (m_TreeView.selection.ilayer != null)
            //        {
            //            ShellEditor.Inputs.CreateNewAction(m_TreeView.selection.ilayer.id);
            //        }
            //        else
            //        {
            //            ShellEditor.Inputs.CreateNewAction(m_TreeView.selection.iaction.idParent);
            //        }
            //        break;
            //}

            ReupdateTable();
        }

        void RemoveItems()
        {
            //if (currentContent == MyContent.Keymap) return;
            //List<int> toDelete = GetToDeleteList();
            //switch (currentContent)
            //{
            //    case MyContent.Devices:
            //        RemoveDevice(toDelete);
            //        break;
            //    case MyContent.Layers:
            //        RemoveLayers(toDelete);
            //        break;
            //    case MyContent.Actions:
            //        break;
            //}

            ReupdateTable();
        }

        List<int> GetToDeleteList()
        {
            List<int> toDeleteList = m_TreeView.GetSelection().ToList();
            for (int i = 0; i < toDeleteList.Count; i++)
            {
                //toDeleteList[i] = m_TreeView.treeModel.Find(toDeleteList[i]).variable.id;
            }
            return toDeleteList;
        }

        #endregion
    }


    internal class MyMultiColumnHeader : MultiColumnHeader
    {
        Mode m_Mode;

        public enum Mode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        public MyMultiColumnHeader(MultiColumnHeaderState state) : base(state)
        {
            mode = Mode.DefaultHeader;
        }

        public Mode mode
        {
            get { return m_Mode; }
            set
            {
                m_Mode = value;
                switch (m_Mode)
                {
                    case Mode.LargeHeader:
                        canSort = true;
                        height = 37f;
                        break;
                    case Mode.DefaultHeader:
                        canSort = true;
                        height = DefaultGUI.defaultHeight;
                        break;
                    case Mode.MinimumHeaderWithoutSorting:
                        canSort = false;
                        height = DefaultGUI.minimumHeight;
                        break;
                }
            }
        }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            // Default column header gui
            base.ColumnHeaderGUI(column, headerRect, columnIndex);

            // Add additional info for large header
            if (mode == Mode.LargeHeader)
            {
                // Show example overlay stuff on some of the columns
                if (columnIndex > 2)
                {
                    headerRect.xMax -= 3f;
                    var oldAlignment = EditorStyles.largeLabel.alignment;
                    EditorStyles.largeLabel.alignment = TextAnchor.UpperRight;
                    GUI.Label(headerRect, 36 + columnIndex + "%", EditorStyles.largeLabel);
                    EditorStyles.largeLabel.alignment = oldAlignment;
                }
            }
        }
    }

}
