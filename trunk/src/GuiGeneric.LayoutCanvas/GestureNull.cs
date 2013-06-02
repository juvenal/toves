/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;

using Toves.GuiGeneric.CanvasAbstract;
using Toves.Layout.Comp;
using Toves.Layout.Data;
using Toves.Layout.Model;
using Toves.Sim.Inst;
using Toves.Util.Transaction;

namespace Toves.GuiGeneric.LayoutCanvas
{
    public class GestureNull : IGesture
    {
        private LayoutCanvasModel layoutModel;
        private Location? current;

        public GestureNull(LayoutCanvasModel layoutModel)
        {
            this.layoutModel = layoutModel;
            this.current = null;
        }

        public void GestureStart(IPointerEvent evnt)
        {
            IGesture newGesture = GetGesture(evnt);
            if (newGesture != null) {
                if (current != null) {
                    current = null;
                    evnt.RepaintCanvas();
                }
                layoutModel.Gesture = newGesture;
                newGesture.GestureStart(evnt);
            }
        }

        private IGesture GetGesture(IPointerEvent evnt)
        {
            Location eLoc = new Location(evnt.X, evnt.Y);
            Location snapLoc = Constants.SnapToGrid(eLoc);
            if (layoutModel.WiringPoints.Contains(snapLoc)) {
                int d2 = eLoc.GetDistance2(snapLoc);
                if (d2 < 16 * 16) {
                    return new GestureWire(layoutModel, snapLoc);
                }
            }

            ComponentInstance found = null;
            Transaction xn = new Transaction();
            ILayoutAccess lo = xn.RequestReadAccess(layoutModel.Layout);
            using (xn.Start()) {
                foreach (Component component in lo.Components) {
                    Location iloc = component.Location;
                    if (component.Contains(eLoc.X - iloc.X, eLoc.Y - iloc.Y)) {
                        found = layoutModel.LayoutSim.GetInstance(lo, component);
                    }
                }
            }
            if (found != null) {
                if (found.Component is Pokeable) {
                    return new GesturePoke(layoutModel, found);
                } else {
                    return new GestureTranslate(layoutModel, found);
                }
            } else {
                return new GesturePan(layoutModel);
            }
        }


        public void GestureMove(IPointerEvent evnt)
        {
            Location eLoc = new Location(evnt.X, evnt.Y);
            Location snapLoc = Constants.SnapToGrid(eLoc);
            if (layoutModel.WiringPoints.Contains(snapLoc)) {
                if (current != snapLoc) {
                    current = snapLoc;
                    evnt.RepaintCanvas();
                }
            } else {
                if (current != null) {
                    current = null;
                    evnt.RepaintCanvas();
                }
            }
        }

        public void GestureComplete(IPointerEvent evnt)
        {
            GestureMove(evnt);
        }

        public void GestureCancel(IPointerEvent evnt)
        {
            this.current = null;
        }

        public void Paint(IPaintbrush pb)
        {
            if (current != null) {
                pb.Color = Constants.GetColorFor(Value.One);
                pb.StrokeCircle(current.Value.X, current.Value.Y, Constants.WIRING_READY_RADIUS);
            }
        }
    }
}

