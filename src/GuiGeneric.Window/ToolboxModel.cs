/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;
using Toves.Layout.Comp;
using Toves.Components.Gates;
using Toves.Components.Io;
using Toves.Proj;
using Toves.Util.Transaction;

namespace Toves.GuiGeneric.Window {
    public class ToolboxModel {
        private class ToolboxModule : ToolboxItem {
            private string name;

            internal ToolboxModule(ProjectModule module, string name) {
                this.Module = module;
                this.name = name;
            }

            public override String Name { get { return name; } }

            internal ProjectModule Module { get; private set; }
        }

        private class ToolboxComponent : ToolboxItem {
            internal ToolboxComponent(Component comp) {
                this.Component = comp;
            }

            public override String Name { get { return this.Component.Name; } }

            internal Component Component { get; private set; }
        }

        private WindowModel window;
        private IEnumerable<ToolboxDrawer> drawers;
        private List<ToolboxModule> modules;

        public ToolboxModel(WindowModel window) {
            this.window = window;
            List<ToolboxItem> builtins = new List<ToolboxItem>();
            Component[] masters = {
                new ToggleSwitch(),
                new AndGate(),
                new OrGate(),
                new NotGate(),
                new Led()
            };
            foreach (Component master in masters) {
                builtins.Add(new ToolboxComponent(master));
            }

            modules = new List<ToolboxModule>();
            Transaction xn = new Transaction();
            IProjectAccess proj = xn.RequestReadAccess(window.Project);
            using (xn.Start()) {
                foreach (ProjectModule mod in proj.GetModules()) {
                    modules.Add(new ToolboxModule(mod, proj.GetModuleName(mod)));
                }
            }

            List<ToolboxDrawer> drawers = new List<ToolboxDrawer>();
            drawers.Add(new ToolboxDrawer("Project", modules.AsReadOnly()));
            drawers.Add(new ToolboxDrawer("Built-Ins", builtins.AsReadOnly()));
            this.drawers = drawers.AsReadOnly();
        }

        public IEnumerable<ToolboxDrawer> Drawers {
            get { return drawers; }
        }

        public void SelectItem(ToolboxItem item) {
            if (item is ToolboxComponent) {
                window.BeginAdd(((ToolboxComponent) item).Component, null);
            }
        }

        public void ActivateItem(ToolboxItem item) {
            if (item is ToolboxModule) {
                window.SetView(((ToolboxModule) item).Module);
            }
        }
    }
}