using ExceptionSoftware.TreeViewTemplate;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace ExceptionSoftware.Variables
{

    public class TreeViewItem<T> : TreeViewItem where T : VariablesElement
    {
        public T data { get; set; }

        public TreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
        {
            this.data = data;
        }
    }

    public class VariablesTableWithModel<T> : TreeView where T : VariablesElement
    {
        TreeModel<T> m_TreeModel;
        readonly List<TreeViewItem> m_Rows = new List<TreeViewItem>(100);

        public event Action treeChanged;

        public TreeModel<T> treeModel { get { return m_TreeModel; } }
        public event Action<IList<TreeViewItem>> beforeDroppingDraggedItems;

        VariablesTableFilter _filter = new VariablesTableFilter();
        public VariablesTableFilter Filter { get { return _filter; } }
        public void RefreshTable()
        {
            searchString = " " + _filter.textFilter;
            Reload();
        }
        public VariablesTableWithModel(TreeViewState state, TreeModel<T> model) : base(state)
        {
            Init(model);
        }

        public VariablesTableWithModel(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model) : base(state, multiColumnHeader)
        {
            Init(model);
        }

        void Init(TreeModel<T> model)
        {
            m_TreeModel = model;
            m_TreeModel.modelChanged += ModelChanged;
        }

        void ModelChanged()
        {
            if (treeChanged != null)
                treeChanged();

            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            int depthForHiddenRoot = -1;
            return new TreeViewItem<T>(m_TreeModel.root.id, depthForHiddenRoot, m_TreeModel.root.name, m_TreeModel.root);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            if (m_TreeModel.root == null)
            {
                Debug.LogError("tree model root is null. did you call SetData()?");
            }

            m_Rows.Clear();
            if (_filter.HasFilter)
            {
                Search(m_TreeModel.root, _filter, m_Rows);
            }
            else
            {
                if (m_TreeModel.root.hasChildren)
                {
                    AddChildrenRecursive(m_TreeModel.root, 0, m_Rows);
                }
            }

            // We still need to setup the child parent information for the rows since this 
            // information is used by the TreeView internal logic (navigation, dragging etc)
            SetupParentsAndChildrenFromDepths(root, m_Rows);

            return m_Rows;
        }

        void AddChildrenRecursive(T parent, int depth, IList<TreeViewItem> newRows)
        {
            foreach (T child in parent.children)
            {
                var item = new TreeViewItem<T>(child.id, depth, child.name, child);
                newRows.Add(item);

                if (child.hasChildren)
                {
                    if (IsExpanded(child.id))
                    {
                        AddChildrenRecursive(child, depth + 1, newRows);
                    }
                    else
                    {
                        item.children = CreateChildListForCollapsedParent();
                    }
                }
            }
        }

        void Search(T searchFromThis, VariablesTableFilter searchFilter, List<TreeViewItem> result)
        {
            const int kItemDepth = 0; // tree is flattened when searching

            Stack<T> stack = new Stack<T>();
            foreach (var element in searchFromThis.children)
                stack.Push((T)element);

            while (stack.Count > 0)
            {
                T current = stack.Pop();

                // Matches search?
                if (current.children != null && current.children.Count > 0)
                {
                    foreach (var element in current.children)
                    {
                        stack.Push((T)element);
                    }
                }


                foreach (string search in searchFilter.textFilter.Split(' '))
                {

                    if (current.variable?.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        result.Add(new TreeViewItem<T>(current.id, kItemDepth, current.name, current));
                        break;
                    }
                    if (current.variable?.description.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        result.Add(new TreeViewItem<T>(current.id, kItemDepth, current.name, current));
                        break;
                    }
                }

            }
            SortSearchResult(result);
        }

        protected virtual void SortSearchResult(List<TreeViewItem> rows)
        {
            rows.Sort((x, y) => EditorUtility.NaturalCompare(x.displayName, y.displayName)); // sort by displayName by default, can be overriden for multicolumn solutions
        }

        protected override IList<int> GetAncestors(int id)
        {
            return m_TreeModel.GetAncestors(id);
        }

        protected override IList<int> GetDescendantsThatHaveChildren(int id)
        {
            return m_TreeModel.GetDescendantsThatHaveChildren(id);
        }

        // Selection
        //-----------
        //protected override void SelectionClick(TreeViewItem item, bool keepMultiSelection)
        //{
        //}

        // Dragging
        //-----------

        protected override bool CanStartDrag(CanStartDragArgs args) => false;

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            base.SetupDragAndDrop(args);
        }
    }

}
