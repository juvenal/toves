/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections;
using System.Collections.Generic;
using Toves.Layout.Comp;
using Toves.Layout.Data;
using Toves.Util.Transaction;

namespace Toves.Layout.Model
{
    public class LayoutKey {
        internal LayoutKey() { }
    }

    public class LayoutModel : Resource<ILayoutAccess>
    {
        internal List<Component> components = new List<Component>();
        internal List<WireSegment> wires = new List<WireSegment>();
        internal LayoutNode.Updater nodes = new LayoutNode.Updater();
        private ResourceHelper resourceHelper = new ResourceHelper();

        public LayoutModel()
        {
        }
        
        public ResourceHelper ResourceHelper { get { return resourceHelper; } }

        public event EventHandler<LayoutModifiedArgs> LayoutModifiedEvent;
        
        protected virtual void OnLayoutModified(ILayoutAccess access)
        {
            EventHandler<LayoutModifiedArgs> handler = LayoutModifiedEvent;
            if (handler != null) {
                handler(this, new LayoutModifiedArgs(access));
            }
        }
        
        public ILayoutAccess CreateAccess(bool canWrite) {
            return new MyAccess(this, canWrite);
        }

        public void Hook(ResourceHookType hookType, IResourceAccess rawAccess) {
            ILayoutAccess access = rawAccess as ILayoutAccess;
            if (hookType == ResourceHookType.AfterWrite) {
                nodes.LayoutChanged(access);
            } else if (hookType == ResourceHookType.AfterDowngrade) {
                OnLayoutModified(access);
            }
        }
    }

    internal class MyAccess : ResourceAccess, ILayoutAccess
    {
        private LayoutModel layout;
        private LayoutKey key = new LayoutKey();

        internal MyAccess(LayoutModel layout, bool canWrite) : base(layout, canWrite)
        {
            this.layout = layout;
        }

        /*
        internal void SetReadOnly() {
            canWrite = false;
        }
        */

        public IEnumerable<Component> Components {
            get {
                CheckReadAccess();
                return layout.components.AsReadOnly();
            }
        }

        public IEnumerable<WireSegment> Wires {
            get {
                CheckReadAccess();
                return layout.wires.AsReadOnly();
            }
        }
        
        public IEnumerable<LayoutNode> Nodes {
            get {
                CheckReadAccess();
                return layout.nodes.Nodes;
            }
        }

        public LayoutNode FindNode(Location loc) {
            CheckReadAccess();
            return layout.nodes.GetNode(loc);
        }

        public Component AddComponent(Component prototype, int x, int y)
        {
            CheckWriteAccess();
            Component clone = prototype.Clone();
            clone.SetLocation(key, new Location(x, y));
            layout.components.Add(clone);
            return clone;
        }

        public void RemoveComponent(Component component)
        {
            CheckWriteAccess();
            layout.components.Remove(component);
        }

        public void MoveComponent(Component component, int dx, int dy)
        {
            CheckWriteAccess();
            component.SetLocation(key, component.Location.Translate(dx, dy));
        }

        public void AddWire(Location end0, Location end1)
        {
            CheckWriteAccess();
            layout.wires.Add(new WireSegment(end0, end1));
        }

        public void RemoveWire(WireSegment wire)
        {
            CheckWriteAccess();
            layout.wires.Remove(wire);
        }
    }
}

