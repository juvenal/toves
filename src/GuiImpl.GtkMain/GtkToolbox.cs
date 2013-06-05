/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using Gtk;
using Toves.GuiGeneric.Window;

namespace Toves.GuiImpl.GtkMain {
    public class GtkToolbox : TreeView {
        private ToolboxModel source;

        public GtkToolbox(ToolboxModel source) {
            this.source = source;
            CellRendererText nameRenderer = new CellRendererText();
            TreeViewColumn nameColumn = new TreeViewColumn();
            nameColumn.Title = "Component";
            nameColumn.PackStart(nameRenderer, true);
            nameColumn.SetCellDataFunc(nameRenderer, GetCellData);
            this.AppendColumn(nameColumn);

            this.Model = BuildStore(source);
            this.Selection.Changed += HandleSelect;
            this.RowActivated += HandleActivation;

            this.ExpandAll();
        }

        private TreeStore BuildStore(ToolboxModel source) {
            TreeStore store = new TreeStore(typeof(ToolboxItem));
            foreach (ToolboxDrawer drawer in source.Drawers) {
                TreeIter drawerIter = store.AppendValues(drawer);
                foreach (ToolboxItem item in drawer.GetContents()) {
                    store.AppendValues(drawerIter, item);
                }
            }
            return store;
        }

        private void GetCellData(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            CellRendererText cellText = cell as CellRendererText;
            TreeStore store = model as TreeStore;
            ToolboxItem val = store.GetValue(iter, 0) as ToolboxItem;
            if (val != null) {
                cellText.Text = val.Name;
            }
        }

        private void HandleSelect(object sender, EventArgs args) {
            TreeStore store = this.Model as TreeStore;
            if (store != null) {
                foreach (TreePath path in this.Selection.GetSelectedRows()) {
                    TreeIter iter;
                    if (store.GetIter(out iter, path)) {
                        ToolboxItem val = store.GetValue(iter, 0) as ToolboxItem;
                        if (val != null) {
                            source.SelectItem(val);
                        }
                    }
                }
            }
        }
        
        private void HandleActivation(object sender, RowActivatedArgs args) {
            TreeStore store = this.Model as TreeStore;
            if (store != null) {
                TreeIter iter;
                if (store.GetIter(out iter, args.Path)) {
                    ToolboxItem val = store.GetValue(iter, 0) as ToolboxItem;
                    if (val != null) {
                        source.ActivateItem(val);
                    }
                }
            }
        }
    }
}