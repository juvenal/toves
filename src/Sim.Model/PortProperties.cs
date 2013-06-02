/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;

namespace Toves.Sim.Model
{
    public class IPortProperties
    {
        public IPortProperties(PortType type, int width) {
            this.GetType = type;
            this.Width = width;
        }
        
        PortType Type { get; private set; }
        
        int Width { get; private set; }
    }
}

