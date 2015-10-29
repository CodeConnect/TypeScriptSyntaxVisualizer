using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CodeConnect.TypeScriptSyntaxVisualizer
{
    public partial class MyControl : UserControl
    {
        private readonly System.Windows.Forms.PropertyGrid _propertyGrid;

        public bool IsWindowVisible { get; set; }
        DTE dte = Package.GetGlobalService(typeof(DTE)) as DTE;

        public MyControl()
        {
            InitializeComponent();

            _propertyGrid = new System.Windows.Forms.PropertyGrid();
            _propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            _propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            _propertyGrid.HelpVisible = false;
            _propertyGrid.ToolbarVisible = false;
            _propertyGrid.CommandsVisibleIfAvailable = true;
            windowsFormsHost.Child = _propertyGrid;
        }

        internal void UpdateWithSyntaxRoot(SyntaxNodeOrToken root, int position)
        {
            clear();
            TreeViewItem rootItem = null;
            rootItem = new TreeViewItem();
            TreeContainer.Items.Add(rootItem);

            rootItem.DataContext = root;
            generateSyntaxTreeView(root, rootItem);
            selectCorrespondingTreeViewItem(position);
        }

        private void clear()
        {
            TreeContainer.Items.Clear();
            _propertyGrid.SelectedObject = null;
        }

        private void selectCorrespondingTreeViewItem(int position)
        {
            var rootItem = (TreeViewItem)TreeContainer.Items.GetItemAt(0);
            collapseAllItems(rootItem);
            var correspondingTreeViewItem = findCorrespondingTreeViewItem(position);
            correspondingTreeViewItem.IsSelected = true;
            _propertyGrid.SelectedObject = (SyntaxNodeOrToken)correspondingTreeViewItem.DataContext;
        }

        private TreeViewItem findCorrespondingTreeViewItem(int position)
        {
            var currentItem = (TreeViewItem)TreeContainer.Items.GetItemAt(0);
            while (! currentItem.Items.IsEmpty)
            {
                foreach (TreeViewItem childItem in currentItem.Items)
                {
                    var childNode = (SyntaxNodeOrToken)childItem.DataContext;
                    if (childNode.StartPosition == childNode.End && childNode.StartPosition == position)
                    {
                        currentItem.IsExpanded = true;
                        return childItem;
                    }
                    else if (childNode.StartPosition <= position && position < childNode.End)
                    {
                        currentItem.IsExpanded = true;
                        currentItem = childItem;
                        break;
                    }
                }
            }
            return currentItem;
        }

        private void collapseAllItems(TreeViewItem item)
        {
            foreach (TreeViewItem childItem in item.Items)
            {
                collapseAllItems(childItem);
            }
            item.IsExpanded = false;
        }

        private void generateSyntaxTreeView(SyntaxNodeOrToken node, TreeViewItem item)
        {
            item.Header = node.Kind + " [" + node.StartPosition + ".." + node.End + ")";

            if (node.IsToken)
            {
                item.Foreground = Brushes.Green;
            }
            else
            {
                item.Foreground = Brushes.Blue;
            }

            foreach (var child in node.Children)
            {
                var childItem = new TreeViewItem();
                childItem.DataContext = child;
                item.Items.Add(childItem);
                generateSyntaxTreeView(child, childItem);
            }
        }

        private void selectText(TreeViewItem item)
        {
            try
            {
                var node = (SyntaxNodeOrToken)item.DataContext;
                var document = dte.ActiveDocument;
                var selected = (TextSelection)document.Selection;
                selected.StartOfDocument(false);
                selected.MoveToAbsoluteOffset(node.StartPosition + 1, false);
                selected.MoveToAbsoluteOffset(node.End + 1, true);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        private void treeContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeView)
            {
                var item = (sender as TreeView).SelectedItem;
                _propertyGrid.SelectedObject = (SyntaxNodeOrToken)(item as TreeViewItem).DataContext;
                selectText(item as TreeViewItem);
            }
        }
    }
}