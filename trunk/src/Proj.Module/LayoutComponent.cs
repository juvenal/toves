/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;
using Toves.Layout.Comp;
using Toves.Layout.Data;
using Toves.Layout.Model;
using Toves.Layout.Sim;
using Toves.Proj.Model;
using Toves.Sim.Inst;
using Toves.Sim.Model;
using Toves.Util.Transaction;

namespace Toves.Proj.Module {
    public class LayoutComponent : Component {
        private class LayoutInstance : ComponentInstance {
            private LayoutSimulation subsim = null;

            public LayoutInstance(LayoutComponent component) : base(component) {
            }

            public override void HandleEvent(InstanceEvent evnt, IInstanceState state) {
                if (evnt.Type == InstanceEvent.Types.InstanceAdded) {
                    LayoutComponent comp = this.Component as LayoutComponent;

                    Transaction xn = new Transaction();
                    ILayoutAccess layout = xn.RequestReadAccess(comp.module.Layout);
                    ISimulationAccess sim = xn.RequestWriteAccess(state.Simulation);
                    using (xn.Start()) {
                        subsim = new LayoutSimulation(state.Simulation, comp.module.Layout);
                        subsim.SetActivated(true);
                        IEnumerator<Port> ports = this.Ports.GetEnumerator();
                        IEnumerator<Component> pins = comp.presentation.Pins;
                        while (ports.MoveNext() && pins.MoveNext()) {
                            Port compPort = ports.Current;
                            Component pin = pins.Current;
                            Instance pinInstance = subsim.GetInstance(layout, pin);
                            if (pinInstance != null) {
                                foreach (Port pinPort in pinInstance.Ports) {
                                    sim.AddLink(new Link(compPort, pinPort));
                                    break;
                                }
                            }
                        }
                    }
                } else if (evnt.Type == InstanceEvent.Types.InstanceRemoved) {
                    Console.Error.WriteLine("removed");
                    subsim.SetActivated(false);
                } else {
                    base.HandleEvent(evnt, state);
                }
            }
        }

        private Project project;
        private ProjectModule module;
        private LayoutPresentation presentation;

        public LayoutComponent(Project project, ProjectModule module) {
            this.project = project;
            this.module = module;
            this.presentation = new LayoutPresentation(module.Layout);
        }

        public override string Name {
            get {
                Transaction xn = new Transaction();
                IProjectAccess proj = xn.RequestReadAccess(project);
                using (xn.Start()) {
                    return proj.GetModuleName(module);
                }
            }
        }

        public override PortArgs[] PortArgs {
            get { return presentation.Connections; }
        }

        public override ConnectionPoint[] Connections {
            get { return presentation.Connections; }
        }

        public override Bounds OffsetBounds {
            get { return presentation.OffsetBounds; }
        }

        public override bool ShouldSnap {
            get { return true; }
        }

        public override bool Contains(int offsetX, int offsetY) {
            return presentation.OffsetBounds.Contains(offsetX, offsetY);
        }
        
        public override void Paint(IComponentPainter painter) {
            presentation.Paint(painter);
        }

        public override ComponentInstance CreateInstance() {
            return new LayoutInstance(this);
        }

        public override void Propagate(ComponentInstance instance, IInstanceState state) {
        }
    }
}

