/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;

namespace Toves.GuiGeneric.Toolbox
{
    
    public class ToolboxChangeArgs : EventArgs {
        public ToolboxChangeArgs(int oldValue, int newValue)
        {
            OldValue = oldValue;
            Value = newValue;
        }

        public int OldValue { get; private set; }
        public int Value { get; private set; }
    }

    public interface IToolboxModel
    {
        event EventHandler<ToolboxChangeArgs> ToolboxChangeEvent;

        int Count { get; }

        int Selected { get; set; }

        string GetImageName(int i);
    }
}

