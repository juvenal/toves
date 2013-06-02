/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using Toves.Layout.Comp;
using Toves.GuiGeneric.LayoutCanvas;
using Toves.Components.Gates;
using Toves.Components.Io;

namespace Toves.GuiGeneric.Toolbox
{
    internal struct ToolInfo {
        internal String imageName;
        internal Component toolMaster;

        internal ToolInfo(String imageName, Component toolMaster)
        {
            this.imageName = imageName;
            this.toolMaster = toolMaster;
        }
    }

    public class ToolboxModel : IToolboxModel
    {
        private LayoutCanvasModel canvasModel;
        private ToolInfo[] tools = new ToolInfo[] {
            new ToolInfo("gate-and.gif", new AndGate()),
            new ToolInfo("gate-or.gif", new OrGate()),
            new ToolInfo("gate-not.gif", new NotGate()),
            new ToolInfo("io-toggle.gif", new ToggleSwitch()),
            new ToolInfo("io-led.gif", new Led()),
        };
        private int selected = -1;

        public ToolboxModel(LayoutCanvasModel canvasModel) {
            this.canvasModel = canvasModel;
        }

        public event EventHandler<ToolboxChangeArgs> ToolboxChangeEvent;

        public int Count { get { return tools.Length; } }

        public int Selected {
            get { return selected; }
            set {
                if (selected != value) {
                    int old = selected;
                    selected = value;

                    if (value >= 0 && value < tools.Length) {
                        GestureAdd g = new GestureAdd(canvasModel, tools[value].toolMaster);
                        g.GestureCompleteEvent += (sender, e) => {
                            if (canvasModel.Gesture == g) {
                                this.Selected = -1;
                            }
                        };
                        canvasModel.Gesture = g;
                    } else {
                        canvasModel.Gesture = null;
                    }

                    OnToolboxChangeEvent(new ToolboxChangeArgs(old, value));
                }
            }
        }

        public string GetImageName(int i)
        {
            return tools[i].imageName;
        }

        protected virtual void OnToolboxChangeEvent(ToolboxChangeArgs args)
        {
            EventHandler<ToolboxChangeArgs> handler = ToolboxChangeEvent;
            if (handler != null) {
                handler(this, args);
            }
        }

    }
}

