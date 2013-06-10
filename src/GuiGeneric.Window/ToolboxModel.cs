/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;
using Toves.Layout.Comp;
using Toves.Components.Gates;
using Toves.Components.Io;
using Toves.Components.Wiring;
using Toves.Proj.Model;
using Toves.Proj.Module;
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

            internal void SetName(string value) {
                this.name = value;
            }
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
        private List<ToolboxModule> modules = new List<ToolboxModule>();
        private ToolboxModule currentModule = null;

        public ToolboxModel(WindowModel window) {
            this.window = window;

            Transaction xn = new Transaction();
            IProjectAccess proj = xn.RequestReadAccess(window.Project);
            using (xn.Start()) {
                foreach (ProjectModule mod in proj.GetModules()) {
                    string modName = proj.GetModuleName(mod);
                    ToolboxModule toAdd = new ToolboxModule(mod, modName);
                    if (mod == window.CurrentModule) {
                        toAdd.SetName(toAdd.Name + "*");
                        currentModule = toAdd;
                    }
                    modules.Add(toAdd);
                }
            }
            window.Project.ProjectModifiedEvent += RefreshModules;

            List<ToolboxItem> builtins = new List<ToolboxItem>();
            Component[] masters = {
                new ToggleSwitch(),
                new AndGate(),
                new OrGate(),
                new NotGate(),
                new Led(),
                new Pin(),
                new PinOut()
            };
            foreach (Component master in masters) {
                builtins.Add(new ToolboxComponent(master));
            }

            List<ToolboxDrawer> drawers = new List<ToolboxDrawer>();
            drawers.Add(new ToolboxDrawer("Project", modules.AsReadOnly()));
            drawers.Add(new ToolboxDrawer("Built-Ins", builtins.AsReadOnly()));
            this.drawers = drawers.AsReadOnly();
        }

        public event EventHandler<ToolboxChangedArgs> ToolboxChangedEvent;

        private void RefreshModules(object src, ProjectModifiedArgs args) {
            ProjectModule changedModule = args.ChangedModule;
            ToolboxItem changedItem = null;

            Transaction xn = new Transaction();
            IProjectAccess proj = xn.RequestReadAccess(window.Project);
            using (xn.Start()) {
                Dictionary<ProjectModule, ToolboxModule> oldItems = new Dictionary<ProjectModule, ToolboxModule>();
                List<ToolboxModule> newModules = new List<ToolboxModule>();
                foreach (ToolboxModule item in modules) {
                    oldItems[item.Module] = item;
                    if (item.Module == changedModule) {
                        changedItem = item;
                    }
                }
                foreach (ProjectModule mod in proj.GetModules()) {
                    string curName = proj.GetModuleName(mod);
                    ToolboxModule nextItem;
                    if (oldItems.TryGetValue(mod, out nextItem)) {
                        if (nextItem.Name != curName) {
                            nextItem.SetName(curName);
                        }
                    } else {
                        nextItem = new ToolboxModule(mod, curName);
                    }
                    newModules.Add(nextItem);
                    if (nextItem.Module == changedModule) {
                        changedItem = nextItem;
                    }
                }
                this.modules.Clear();
                foreach (ToolboxModule item in newModules) {
                    this.modules.Add(item);
                }
            }

            EventHandler<ToolboxChangedArgs> handler = ToolboxChangedEvent;
            if (handler != null && changedItem != null) {
                ToolboxChangedArgs.ChangeTypes changeType;
                bool found = true;
                switch (args.ChangeType) {
                case ProjectModifiedArgs.ChangeTypes.ModuleAdded:
                    changeType = ToolboxChangedArgs.ChangeTypes.ItemAdded;
                    break;
                case ProjectModifiedArgs.ChangeTypes.ModuleRemoved:
                    changeType = ToolboxChangedArgs.ChangeTypes.ItemRemoved;
                    break;
                case ProjectModifiedArgs.ChangeTypes.ModuleRenamed:
                    changeType = ToolboxChangedArgs.ChangeTypes.ItemRenamed;
                    break;
                default:
                    changeType = ToolboxChangedArgs.ChangeTypes.ItemAdded; // unused value to trick compiler
                    found = false;
                    break;
                }
                if (found) {
                    handler(this, new ToolboxChangedArgs(changeType, changedItem));
                }
            }
        }

        internal void UpdateCurrent(ProjectModule newValue) {
            ToolboxModule newCurrent = null;
            foreach (ToolboxDrawer drawer in drawers) {
                foreach (ToolboxItem rawItem in drawer.GetContents()) {
                    ToolboxModule item = rawItem as ToolboxModule;
                    if (item != null && item.Module == newValue) {
                        newCurrent = item;
                    }
                }
            }

            ToolboxModule oldCurrent = currentModule;
            if (oldCurrent != newCurrent) {
                EventHandler<ToolboxChangedArgs> handler = ToolboxChangedEvent;
                if (oldCurrent != null && oldCurrent.Name.EndsWith("*")) {
                    oldCurrent.SetName(oldCurrent.Name.Substring(0, oldCurrent.Name.Length - 1));
                    if (handler != null) {
                        handler(this, new ToolboxChangedArgs(ToolboxChangedArgs.ChangeTypes.ItemRenamed, oldCurrent));
                    }
                }
                if (newCurrent != null) {
                    newCurrent.SetName(newCurrent.Name + "*");
                    if (handler != null) {
                        handler(this, new ToolboxChangedArgs(ToolboxChangedArgs.ChangeTypes.ItemRenamed, newCurrent));
                    }
                }
                currentModule = newCurrent;
            }
        }

        public IEnumerable<ToolboxDrawer> Drawers {
            get { return drawers; }
        }

        public void SelectItem(ToolboxItem item) {
            Component master;
            if (item is ToolboxComponent) {
                master = ((ToolboxComponent) item).Component;
            } else if (item is ToolboxModule) {
                ProjectModule module = ((ToolboxModule) item).Module;
                master = new LayoutComponent(window.Project, module);
            } else {
                master = null;
            }
            if (master != null) {
                window.BeginAdd(master, null);
            }
        }

        public void ActivateItem(ToolboxItem item) {
            if (item is ToolboxModule) {
                window.SetView(((ToolboxModule) item).Module);
            }
        }
    }
}