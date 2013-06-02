/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using Toves.Sim.Inst;

namespace Toves.Layout.Comp
{
    public class Port : PortArgs
    {
        public static Port newInput(int dx, int dy) {
            return new Port(dx, dy, PortType.Input);
        }

        public static Port newOutput(int dx, int dy) {
            return new Port(dx, dy, PortType.Output);
        }

        private Port(int dx, int dy, PortType type) : base(type, 1)
        {
            this.Dx = dx;
            this.Dy = dy;
        }

        public int Dx { get; private set; }

        public int Dy { get; private set; }
    }
}

