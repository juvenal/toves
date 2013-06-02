/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;
using Toves.Layout.Comp;
using Toves.Layout.Data;
using Toves.Layout.Model;
using Toves.GuiGeneric.CanvasAbstract;
using Toves.Sim.Inst;

namespace Toves.GuiGeneric.LayoutCanvas
{
    public class GestureAdd : IGesture
    {
        private LayoutCanvasModel layoutModel;
        private Component master;
        private ComponentInstance masterInstance;
        private Location current;
        private bool currentValid;

        public event EventHandler<GestureCompleteArgs> GestureCompleteEvent;

        public GestureAdd(LayoutCanvasModel layoutModel, Component master)
        {
            this.layoutModel = layoutModel;
            this.current = new Location(0, 0);
            this.currentValid = false;
            this.master = master;
            this.masterInstance = master.CreateInstance();
        }

        private bool Update(IPointerEvent evnt)
        {
            Location cur = current;
            Location next = new Location(evnt.X, evnt.Y);
            if (master.ShouldSnap) {
                next = Constants.SnapToGrid(next);
            }
            if (cur == next) {
                return false;
            } else {
                current = next;
                currentValid = true;
                return true;
            }
        }

        public void GestureStart(IPointerEvent evnt)
        {
            if (Update(evnt)) {
                evnt.RepaintCanvas();
            }
        }

        public void GestureMove(IPointerEvent evnt)
        {
            if (Update(evnt)) {
                evnt.RepaintCanvas();
            }
        }

        protected virtual void OnGestureCompleteEvent(bool success)
        {
            EventHandler<GestureCompleteArgs> handler = GestureCompleteEvent;
            if (handler != null) {
                handler(this, new GestureCompleteArgs(success));
            }
        }

        public void GestureComplete(IPointerEvent evnt)
        {
            Location cur = current;
            if (currentValid) {
                // Console.WriteLine("**** Adding {0} ****", master.GetType().Name);
                layoutModel.Execute((ILayoutAccess lo) => {
                    Component clone = lo.AddComponent(master, cur.X, cur.Y);
                    WireTools.CheckForSplits(lo, layoutModel.WiringPoints, new Component[] { clone });
                });
            }
            this.current = new Location(0, 0);
            this.currentValid = false;
            OnGestureCompleteEvent(true);
            layoutModel.Gesture = null;
            evnt.RepaintCanvas();
        }

        public void GestureCancel(IPointerEvent evnt)
        {
            this.current = new Location(0, 0);
            this.currentValid = false;
            OnGestureCompleteEvent(false);
            evnt.RepaintCanvas();
        }

        public void Paint(IPaintbrush pb)
        {
            Location cur = current;
            IComponentPainter ip = new ComponentPainter(pb, new DummyInstanceState(masterInstance));
            pb.TranslateCoordinates(cur.X, cur.Y);
            if (currentValid) {
                pb.Color = Constants.COLOR_GHOST;
            } else {
                pb.Color = Constants.COLOR_DEAD;
            }
            master.Paint(ip);
        }
    }
}

