/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using Toves.Layout.Comp;
using Toves.Layout.Data;
using Toves.Sim.Inst;

namespace Toves.Components.Gates
{
    public abstract class BasicGate : ComponentSharedData
    {
        public BasicGate()
        {
            this.ShareOffsetBounds(new Bounds(-96, -48, 96, 96));
            this.SharePorts(new Port[] {
                Port.newOutput(0, 0), Port.newInput(-96, -32),
                Port.newInput(-96, 32) });
        }

        public abstract override bool Contains(int offsetX, int offsetY);

        public abstract override void Propagate(ComponentInstance instance, IInstanceState state);

        public abstract override void Paint(IComponentPainter painter);
    }
}

