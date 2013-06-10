/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using Toves.Sim.Inst;

namespace Toves.Layout.Comp {
    public class ComponentInstance : Instance {
        public ComponentInstance(Component component) : base(component.PortArgs) {
            this.Component = component;
        }

        public Component Component { get; private set; }

        public override void HandleEvent(InstanceEvent evnt, IInstanceState state) {
            if (evnt.Type == InstanceEvent.Types.InstanceDirty) {
                Component.Propagate(this, state);
            }
        }

        public override string ToString() {
            string typeName;
            if (this.GetType() == typeof(ComponentInstance)) {
                typeName = "";
            } else {
                typeName = string.Format("[{0}]", this.GetType().Name);
            }
            return string.Format("inst:{0}{1}", Component.GetType().Name, typeName);
        }
    }
}