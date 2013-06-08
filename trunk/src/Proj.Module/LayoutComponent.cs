/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using Toves.Layout.Comp;
using Toves.Layout.Data;
using Toves.Layout.Model;
using Toves.Proj.Model;
using Toves.Sim.Inst;
using Toves.Util.Transaction;

namespace Toves.Proj.Module {
    public class LayoutComponent : Component {
        private Project project;
        private ProjectModule module;
        private LayoutPresentation presentation;

        public LayoutComponent(Project project, ProjectModule module) {
            this.project = project;
            this.module = module;
            this.presentation = new LayoutPresentation(module.Layout);
        }

        public override string Name {
            get {
                Transaction xn = new Transaction();
                IProjectAccess proj = xn.RequestReadAccess(project);
                using (xn.Start()) {
                    return proj.GetModuleName(module);
                }
            }
        }

        public override PortArgs[] PortArgs {
            get { return presentation.Connections; }
        }

        public override ConnectionPoint[] Connections {
            get { return presentation.Connections; }
        }

        public override Bounds OffsetBounds {
            get { return presentation.OffsetBounds; }
        }

        public override bool ShouldSnap {
            get { return true; }
        }

        public override bool Contains(int offsetX, int offsetY) {
            return presentation.OffsetBounds.Contains(offsetX, offsetY);
        }
        
        public override void Paint(IComponentPainter painter) {
            presentation.Paint(painter);
        }

        /*
        public virtual ComponentInstance CreateInstance() {
            return new ComponentInstance(this);
        }
        */

        public override void Propagate(ComponentInstance instance, IInstanceState state) {
        }
    }
}

