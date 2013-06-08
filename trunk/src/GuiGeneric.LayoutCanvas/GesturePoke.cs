/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using Toves.GuiGeneric.CanvasAbstract;
using Toves.Layout.Comp;
using Toves.Layout.Data;
using Toves.Layout.Model;
using Toves.Sim.Inst;
using Toves.Sim.Model;
using Toves.Util.Transaction;

namespace Toves.GuiGeneric.LayoutCanvas {
    public class GesturePoke : IGesture {
        private class MyPokeEvent : PokeEventArgs {
            private IPointerEvent evnt;
            internal bool repropagateRequested = false;

            internal MyPokeEvent(PokeEventType type, int x, int y, IPointerEvent evnt) {
                this.Type = type;
                this.X = x;
                this.Y = y;
                this.StateUpdate = null;
                this.evnt = evnt;
            }

            public PokeEventType Type { get; private set; }

            public int X { get; private set; }

            public int Y { get; private set; }

            public Action<IInstanceState> StateUpdate { get; set; }

            public void Repaint() {
                evnt.RepaintCanvas();
            }

            public void Repropagate() {
                repropagateRequested = true;
            }
        }

        private LayoutCanvasModel layoutModel;
        private ComponentInstance poking;
        private Location pokingLocation;

        public GesturePoke(LayoutCanvasModel layoutModel, ComponentInstance poking) {
            this.layoutModel = layoutModel;
            this.poking = poking;

            Transaction xn = new Transaction();
            ILayoutAccess layout = xn.RequestReadAccess(layoutModel.Layout);
            using (xn.Start()) {
                pokingLocation = poking.Component.GetLocation(layout);
            }
        }

        private void Send(PokeEventType type, IPointerEvent evnt) {
            if (poking != null) {
                Pokeable dest = poking.Component as Pokeable;
                Location loc = pokingLocation;
                MyPokeEvent pokeEvnt = new MyPokeEvent(type, evnt.X - loc.X, evnt.Y - loc.Y, evnt);
                dest.ProcessPokeEvent(pokeEvnt);
                if (pokeEvnt.StateUpdate != null || pokeEvnt.repropagateRequested) {
                    Transaction xn = new Transaction();
                    ISimulationAccess sim = xn.RequestWriteAccess(layoutModel.LayoutSim.SimulationModel);
                    using (xn.Start()) {
                        if (pokeEvnt.StateUpdate != null) {
                            pokeEvnt.StateUpdate(new InstanceState(sim, poking));
                        }
                        if (pokeEvnt.repropagateRequested) {
                            sim.MarkInstanceDirty(poking);
                        }
                    }
                }
            }
        }

        public void GestureStart(IPointerEvent evnt) {
            Send(PokeEventType.PokeStart, evnt);
        }

        public void GestureMove(IPointerEvent evnt) {
            Send(PokeEventType.PokeMove, evnt);
        }

        public void GestureComplete(IPointerEvent evnt) {
            Send(PokeEventType.PokeEnd, evnt);
            layoutModel.Gesture = null;
        }

        public void GestureCancel(IPointerEvent evnt)
        {
            Send(PokeEventType.PokeCancel, evnt);
            layoutModel.Gesture = null;
            poking = null;
        }

        public void Paint(IPaintbrush pb)
        {
            ComponentInstance cur = poking;
            if (cur != null) {
                Location loc = pokingLocation;
                pb.TranslateCoordinates(loc.X, loc.Y);
                Pokeable dest = cur.Component as Pokeable;
                dest.PaintPokeProgress(new ComponentPainter(pb, new DummyInstanceState(cur)));
            }
        }
    }
}

