/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using Toves.Layout.Data;
using Toves.Layout.Model;
using Toves.Sim.Inst;

namespace Toves.Layout.Comp {
    public abstract class Component {
        private Location location;

        public abstract string Name { get; }

        public Location GetLocation(ILayoutAccess access) {
            access.CheckReadAccess();
            return this.location;
        }

        public void SetLocation(ILayoutAccess access, Location value) {
            access.CheckWriteAccess();
            this.location = value;
        }

        public abstract PortArgs[] PortArgs { get; }

        public abstract ConnectionPoint[] Connections { get; }

        public abstract Bounds OffsetBounds { get; }

        public abstract bool ShouldSnap { get; }

        public abstract bool Contains(int offsetX, int offsetY);

        public Component Clone() {
            return (this.MemberwiseClone() as Component);
        }

        public virtual ComponentInstance CreateInstance() {
            return new ComponentInstance(this);
        }

        public abstract void Propagate(ComponentInstance instance, IInstanceState state);

        public abstract void Paint(IComponentPainter painter);
    }
}