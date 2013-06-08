/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using Toves.Layout.Comp;
using Toves.Layout.Data;

namespace Toves.Proj.Module {
    public interface Presentation {
        Bounds OffsetBounds { get; }
        ConnectionPoint[] Connections { get; }
        void Paint(IComponentPainter painter);
    }
}