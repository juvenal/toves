/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;
using Toves.GuiGeneric.LayoutCanvas;
using Toves.GuiGeneric.Menu;
using Toves.Layout.Comp;
using Toves.Layout.Sim;
using Toves.Proj.Model;
using Toves.Proj.Module;

namespace Toves.GuiGeneric.Window {
    public class WindowModel : IDisposable {
        private SimulationThread simThread;
        private Dictionary<ProjectModule, LayoutSimulation> moduleSimulations
            = new Dictionary<ProjectModule, LayoutSimulation>();
        private List<GenericMenu> menus = new List<GenericMenu>();

        public WindowModel() {
            this.Project = new Project();
            this.LayoutCanvas = new LayoutCanvasModel();
            this.ToolboxModel = new ToolboxModel(this);
            this.ToolbarModel = new ToolbarModel(this);
            this.simThread = null;

            menus.Add(new ProjectMenu(this));
        }

        public Project Project { get; private set; }

        public ProjectModule CurrentModule { get; private set; }

        public IEnumerable<GenericMenu> Menus { get { return menus; } }

        public LayoutCanvasModel LayoutCanvas { get; private set; }

        public ToolboxModel ToolboxModel { get; private set; }

        public ToolbarModel ToolbarModel { get; private set; }

        public void Dispose() {
            simThread.RequestStop();
        }

        public void SetView(ProjectModule module) {
            LayoutSimulation layoutSim;
            if (!moduleSimulations.TryGetValue(module, out layoutSim)) {
                layoutSim = new LayoutSimulation(module.Layout);
                moduleSimulations[module] = layoutSim;
            }
            SimulationThread newThread = new SimulationThread(layoutSim.SimulationModel);
            newThread.Start();
            SimulationThread oldThread = simThread;
            simThread = newThread;
            if (oldThread != null) {
                oldThread.RequestStop();
            }
            this.CurrentModule = module;
            ToolboxModel.UpdateCurrent(module);
            LayoutCanvas.SetView(module.Layout, layoutSim);
        }

        public void BeginAdd(Component comp, Action onDone) {
            if (comp == null) {
                LayoutCanvas.Gesture = null;
            } else {
                GestureAdd g = new GestureAdd(LayoutCanvas, comp);
                if (onDone != null) {
                    g.GestureCompleteEvent += (sender, e) => {
                        if (LayoutCanvas.Gesture == g) {
                            onDone();
                        }
                    };
                }
                LayoutCanvas.Gesture = g;
            }
        }

        public void StopAdd(Component comp) {
            GestureAdd cur = LayoutCanvas.Gesture as GestureAdd;
            if (cur != null && cur.Master == comp) {
                LayoutCanvas.Gesture = null;
            }
        }
    }
}

