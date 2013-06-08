/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using Toves.Layout.Comp;
using Toves.Layout.Data;
using Toves.Sim.Inst;

namespace Toves.Components.Wiring {
    public class Pin : ComponentSharedData {
        public Pin() {
            ShareOffsetBounds(new Bounds(-64, -32, 64, 64));
            ShareConnections(new ConnectionPoint[] { ConnectionPoint.newOutput(0, 0) });
        }

        public override string Name { get { return "Pin"; } }

        public override bool Contains(int offsetX, int offsetY) {
            return OffsetBounds.Contains(offsetX, offsetY, 5);
        }

        public override void Propagate(ComponentInstance instance, IInstanceState state) {
            state.Set(0, Value.Z, 1);
        }

        public override void Paint(IComponentPainter painter) {
            Value value = painter.GetPortValue(0);

            painter.StrokeWidth = 10;
            painter.Color = 0;
            painter.StrokeRectangle(-64, -32, 64, 64);
            painter.FontSize = 48;
            painter.FontStyle = Toves.GuiGeneric.CanvasAbstract.FontStyle.Bold;
            painter.Color = 0x0000ff;
            painter.DrawText(-32, -4, value.ToString(), TextAlign.Center | TextAlign.VCenter);
            painter.PaintPorts();
        }
    }
}