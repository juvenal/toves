/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Reflection;
using Gtk;
using Toves.GuiGeneric.Toolbox;

namespace Toves.GuiImpl.GtkMain
{
    internal class ToolbarItem : ToggleToolButton
    {
        internal ToolbarItem(GtkToolbox toolbox, IToolboxModel model, int i)
        {
            this.IconWidget = GtkToolbox.GetImage(model.GetImageName(i));
            this.Index = i;
            this.Toggled += (sender, e) => {
                if (!toolbox.Updating) {
                    model.Selected = this.Active ? i : -1;
                }
            };
        }

        internal int Index { get; private set; }
    }

    public class GtkToolbox : Toolbar
    {
        private ToolbarItem[] buttons;

        internal static Image GetImage(string name) {
            Assembly assem = Assembly.GetExecutingAssembly();
            string rsrc = string.Format("{0}.images.{1}", assem.GetName().Name,
                                        name);
            return new Image(assem, rsrc);
        }

        public GtkToolbox(Window parent, IToolboxModel model)
        {
            this.Updating = false;
            this.ToolbarStyle = ToolbarStyle.Icons;
            this.Tooltips = false;

            buttons = new ToolbarItem[model.Count];
            for (int i = 0; i < buttons.Length; i++) {
                buttons[i] = new ToolbarItem(this, model, i);
                this.Insert(buttons[i], -1);
            }

            ToolItem space = new ToolItem();
            space.Expand = true;
            ToolButton info = new ToolButton(Stock.Info);
            info.Clicked += (sender, args) => {
                GtkAbout.ShowAbout(parent);
            };
            this.Insert(space, -1);
            this.Insert(info, -1);

            model.ToolboxChangeEvent += (sender, e) => SetSelected(e.Value);
        }

        internal bool Updating { get; set; }

        private void SetSelected(int value)
        {
            this.Updating = true;
            try {
                foreach (ToolbarItem item in buttons) {
                    bool act = value == item.Index;
                    if (item.Active != act) {
                        item.Active = act;
                    }
                }
            } finally {
                this.Updating = false;
            }
        }
    }
}

