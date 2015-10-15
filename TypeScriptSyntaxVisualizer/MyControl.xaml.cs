using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodeConnect.TypeScriptSyntaxVisualizer;
using System.Runtime.InteropServices;

namespace CodeConnect.TypeScriptSyntaxVisualizer
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class MyControl : UserControl
    {
        private readonly System.Windows.Forms.PropertyGrid _propertyGrid;

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

        internal void UpdateWithSyntaxRoot(TextViewCreationListener.CustomNode root, int position)
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
            _propertyGrid.SelectedObject = (TextViewCreationListener.CustomNode)correspondingTreeViewItem.DataContext;
        }

        private TreeViewItem findCorrespondingTreeViewItem(int position)
        {
            var currentItem = (TreeViewItem)TreeContainer.Items.GetItemAt(0);
            while (! currentItem.Items.IsEmpty)
            {
                foreach (TreeViewItem childItem in currentItem.Items)
                {
                    var childNode = (TextViewCreationListener.CustomNode)childItem.DataContext;
                    if (childNode.Pos == childNode.End && childNode.Pos == position)
                    {
                        currentItem.IsExpanded = true;
                        return childItem;
                    }
                    else if (childNode.Pos <= position && position < childNode.End)
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

        private void generateSyntaxTreeView(TextViewCreationListener.CustomNode node, TreeViewItem item)
        {
            item.Header = node.Kind + " [" + node.Pos + ".." + node.End + ")";
            if (node.IsToken) { item.Foreground = Brushes.Green; }
            else { item.Foreground = Brushes.Blue; }

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
            var node = (TextViewCreationListener.CustomNode)item.DataContext;
            //TODO
        }

        private void TreeContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeView)
            {
                var item = (sender as TreeView).SelectedItem;
                _propertyGrid.SelectedObject = (TextViewCreationListener.CustomNode)(item as TreeViewItem).DataContext;
                selectText(item as TreeViewItem);
            }
        }
    }
}