/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;

using Toves.GuiGeneric.CanvasAbstract;
using Toves.Layout.Comp;
using Toves.Layout.Data;
using Toves.Layout.Model;
using Toves.Layout.Sim;
using Toves.Sim.Inst;
using Toves.Sim.Model;
using Toves.Util.Transaction;

namespace Toves.GuiGeneric.LayoutCanvas
{
    public class LayoutCanvasModel : AbstractCanvasModel
    {
        private ICollection<Component> hidden = new HashSet<Component>();
        private SimulationThread simThread;

        public LayoutCanvasModel() {
            Layout = new LayoutModel();
            LayoutSim = new LayoutSimulation(Layout);
            WiringPoints = new LayoutWiringPoints(Layout);
            this.NullGesture = new GestureNull(this);
            Layout.LayoutModifiedEvent += LayoutUpdated;
            simThread = new SimulationThread(LayoutSim.SimulationModel);
            simThread.Start();
        }

        public override void Disable() {
            simThread.RequestStop();
        }

        public LayoutModel Layout { get; private set; }

        public LayoutSimulation LayoutSim { get; private set; }

        public LayoutWiringPoints WiringPoints { get; private set; }

        public IEnumerable<Component> Hidden {
            get { return hidden; }
            set {
                hidden.Clear();
                if (value != null) {
                    foreach (Component component in value) {
                        hidden.Add(component);
                    }
                }
            }
        }

        public override void Dispose()
        {
        }

        public override void HandleKeyPressEvent(IKeyEvent evnt) {
        }

        private void LayoutUpdated(object sender, LayoutModifiedArgs evnt) {
            WiringPoints.Update();
            // solders.Update();
        }

        public void Execute(Action<ILayoutAccess> action)
        {
            Transaction xn = new Transaction();
            ILayoutAccess lo = xn.RequestWriteAccess(Layout);
            using (xn.Start()) {
                action(lo);
            }
        }

        protected override void PaintModel(IPaintbrush pb)
        {
            ComponentPainter ip = new ComponentPainter(pb, null);
            bool noHidden = hidden.Count == 0;
            Transaction xn = new Transaction();
            ISimulationAccess sim = xn.RequestReadAccess(LayoutSim.SimulationModel);
            ILayoutAccess lo = xn.RequestReadAccess(Layout);
            using (xn.Start()) {
                using (IPaintbrush pbSub = pb.Create()) {
                    pbSub.StrokeWidth = Constants.WIRE_WIDTH;
                    foreach (WireSegment wire in lo.Wires) {
                        Value val0 = LayoutSim.GetValueAt(lo, sim, wire.End0);
                        pbSub.Color = Constants.GetColorFor(val0);
                        pbSub.StrokeLine(wire.End0.X, wire.End0.Y, wire.End1.X, wire.End1.Y);
                    }
                }

                InstanceState state = new InstanceState(sim, null);
                ip.InstanceState = state;
                foreach (Component component in lo.Components) {
                    if (noHidden || !hidden.Contains(component)) {
                        Location loc = component.Location;
                        using (IPaintbrush pbSub = pb.Create()) {
                            pbSub.TranslateCoordinates(loc.X, loc.Y);
                            ip.Paintbrush = pbSub;
                            state.Instance = LayoutSim.GetInstance(lo, component);
                            component.Paint(ip);
                        }
                    }
                }

                using (IPaintbrush pbSub = pb.Create()) {
                    foreach (Location loc in WiringPoints.SolderPoints) {
                        Value val = LayoutSim.GetValueAt(lo, sim, loc);
                        pbSub.Color = Constants.GetColorFor(val);
                        if (noHidden || WiringPoints.IsSolderPoint(loc, hidden)) {
                            pbSub.FillCircle(loc.X, loc.Y, Constants.SOLDER_RADIUS);
                        }
                    }
                }
            }
        }
    }
}

