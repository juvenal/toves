/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;
using Toves.Components.Wiring;
using Toves.Layout.Comp;
using Toves.Layout.Data;
using Toves.Layout.Model;
using Toves.Util.Transaction;

namespace Toves.Proj.Module {
    public class LayoutPresentation : Presentation {
        private LayoutModel layoutModel;
        private Bounds bounds;
        private List<Pin> inputs;
        private List<PinOut> outputs;
        private ConnectionPoint[] connections;

        public LayoutPresentation(LayoutModel layoutModel) {
            this.layoutModel = layoutModel;

            Transaction xn = new Transaction();
            ILayoutAccess layout = xn.RequestReadAccess(layoutModel);
            using (xn.Start()) {
                Update(null, new LayoutModifiedArgs(layout));
            }
            layoutModel.LayoutModifiedEvent += Update;
        }

        public Bounds OffsetBounds {
            get { return bounds; }
        }

        public ConnectionPoint[] Connections {
            get { return connections; }
        }

        private void Update(object sender, LayoutModifiedArgs args) {
            ILayoutAccess layout = args.Layout;
            List<Pin> newInputs = new List<Pin>();
            List<PinOut> newOutputs = new List<PinOut>();
            foreach (Component comp in layout.Components) {
                if (comp is Pin) {
                    newInputs.Add((Pin) comp);
                } else if (comp is PinOut) {
                    newOutputs.Add((PinOut) comp);
                }
            }
            int inCount = newInputs.Count;
            int outCount = newOutputs.Count;
            int gridHeight = Math.Max(inCount, outCount);
            int height = 32 * (gridHeight + 1);
            int yOffs = -32 * ((gridHeight + 1) / 2);
            Bounds newBounds = new Bounds(-30, yOffs, 30, height);
            ConnectionPoint[] newConns = new ConnectionPoint[inCount + outCount];
            newInputs.Sort((p0, p1) => Location.YComparer.Compare(p0.GetLocation(layout), p1.GetLocation(layout)));
            newOutputs.Sort((p0, p1) => Location.YComparer.Compare(p0.GetLocation(layout), p1.GetLocation(layout)));
            int inOffs = -32 * ((inCount + 1) / 2);
            int outOffs = -32 * ((outCount + 1) / 2);
            for (int i = 0; i < newConns.Length; i++) {
                if (i < inCount) {
                    newConns[i] = ConnectionPoint.newInput(-30, inOffs + 32 * i);
                } else {
                    newConns[i] = ConnectionPoint.newOutput(0, outOffs + 32 * (i - inCount));
                }
            }

            inputs = newInputs;
            outputs = newOutputs;
            bounds = newBounds;
            connections = newConns;
        }

        public void Paint(IComponentPainter painter) {
            Bounds bds = bounds;
            painter.StrokeWidth = 10;
            painter.Color = 0x888888;
            painter.StrokeArc(bds.X + bds.Width / 2, bds.Y, 16, -180, 180);
            painter.Color = 0;
            painter.StrokeRectangle(bds.X, bds.Y, bds.Width, bds.Height);
            painter.PaintPorts();
        }
    }
}

