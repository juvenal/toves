/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;

using Toves.GuiGeneric.CanvasAbstract;
using Toves.Layout.Data;
using Toves.Sim.Inst;

namespace Toves.Layout.Comp
{
    public class TextAlign
    {
        public static readonly Toves.GuiGeneric.CanvasAbstract.TextAlign BaselineLeft
            = Toves.GuiGeneric.CanvasAbstract.TextAlign.BaselineLeft;
        public static readonly Toves.GuiGeneric.CanvasAbstract.TextAlign Right
            = Toves.GuiGeneric.CanvasAbstract.TextAlign.Right;
        public static readonly Toves.GuiGeneric.CanvasAbstract.TextAlign Center
            = Toves.GuiGeneric.CanvasAbstract.TextAlign.Center;
        public static readonly Toves.GuiGeneric.CanvasAbstract.TextAlign Top
            = Toves.GuiGeneric.CanvasAbstract.TextAlign.Top;
        public static readonly Toves.GuiGeneric.CanvasAbstract.TextAlign Bottom
            = Toves.GuiGeneric.CanvasAbstract.TextAlign.Bottom;
        public static readonly Toves.GuiGeneric.CanvasAbstract.TextAlign VCenter
            = Toves.GuiGeneric.CanvasAbstract.TextAlign.VCenter;
    }

    public interface IComponentPainter : IPaintbrush
    {
        ComponentInstance Instance { get; }

        Value GetPortValue(int index);

        int GetColorFor(Value value);

        void SetColorFor(Value value);

        void PaintPorts();
    }
}

