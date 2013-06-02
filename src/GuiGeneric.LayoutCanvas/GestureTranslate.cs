/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using Toves.GuiGeneric.CanvasAbstract;
using Toves.Layout.Comp;
using Toves.Layout.Data;
using Toves.Layout.Model;
using Toves.Sim.Inst;

namespace Toves.GuiGeneric.LayoutCanvas
{
    public class GestureTranslate : IGesture
    {
        private LayoutCanvasModel layoutModel;
        private ComponentInstance moving;
        private int initX = 0;
        private int initY = 0;
        private int curDx = 0;
        private int curDy = 0;
        private bool curValid = true;

        public GestureTranslate(LayoutCanvasModel layoutModel,
                                ComponentInstance moving)
        {
            this.layoutModel = layoutModel;
            this.moving = moving;
        }

        private bool Update(IPointerEvent evnt)
        {
            if (moving == null) return false;
            int dx = evnt.X - initX;
            int dy = evnt.Y - initY;
            Location loc = moving.Component.Location;
            if (moving.Component.ShouldSnap) {
                Location newLoc = loc.Translate(dx, dy);
                newLoc = Constants.SnapToGrid(newLoc);
                dx = newLoc.X - loc.X;
                dy = newLoc.Y - loc.Y;
            }

            if (dx == curDx && dy == curDy) {
                return false;
            } else {
                curDx = dx;
                curDy = dy;
                curValid = true;
                return true;
            }
        }

        public void GestureStart(IPointerEvent evnt)
        {
            initX = evnt.X;
            initY = evnt.Y;
            layoutModel.Hidden = new Component[] { moving.Component };
            Update(evnt);
            evnt.RepaintCanvas();
        }

        public void GestureMove(IPointerEvent evnt)
        {
            if (Update(evnt)) {
                evnt.RepaintCanvas();
            }
        }

        public void GestureComplete(IPointerEvent evnt)
        {
            ComponentInstance toMove = moving;
            if (toMove != null) {
                moving = null;
                layoutModel.Execute((ILayoutAccess lo) => {
                    Component comp = toMove.Component;
                    Location[] toCheck = new Location[comp.Ports.Length];
                    for (int i = 0; i < toCheck.Length; i++) {
                        toCheck[i] = comp.Location.Translate(comp.Ports[i].Dx, comp.Ports[i].Dy);
                    }

                    if (curValid) {
                        lo.MoveComponent(toMove.Component, curDx, curDy);
                        layoutModel.WiringPoints.Update();
                        WireTools.CheckForMerges(lo, layoutModel.WiringPoints, toCheck);
                        layoutModel.WiringPoints.Update();
                        WireTools.CheckForSplits(lo, layoutModel.WiringPoints, new Component[] { comp });
                    } else {
                        lo.RemoveComponent(toMove.Component);
                        layoutModel.WiringPoints.Update();
                        WireTools.CheckForMerges(lo, layoutModel.WiringPoints, toCheck);
                    }
                });
                layoutModel.Hidden = null;
                layoutModel.Gesture = null;
                evnt.RepaintCanvas();
            }
        }

        public void GestureCancel(IPointerEvent evnt)
        {
            moving = null;
            curValid = false;
            layoutModel.Hidden = null;
            evnt.RepaintCanvas();
        }

        public void Paint(IPaintbrush pb)
        {
            ComponentInstance cur = moving;
            if (cur != null) {
                if (curValid) {
                    pb.Color = Constants.COLOR_GHOST;
                } else {
                    pb.Color = Constants.COLOR_DEAD;
                }
                Location loc = cur.Component.Location;
                pb.TranslateCoordinates(loc.X + curDx, loc.Y + curDy);
                cur.Component.Paint(new ComponentPainter(pb, new DummyInstanceState(cur)));
            }
        }
    }
}

