/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using Toves.Layout.Model;
using Toves.Proj.Model;

namespace Toves.Proj.Module {
    public class ProjectModule {
        private Project.Key key;
        private String name;

        internal ProjectModule(Project.Key key, String name) {
            if (key == null) {
                throw new InvalidOperationException("key required");
            }
            this.key = key;
            this.name = name;
            this.Layout = new LayoutModel();
        }

        public Presentation Presentation { get; private set; }

        public LayoutModel Layout { get; private set; }
        
        internal string GetName(Project.Key key) {
            if (this.key != key) {
                throw new InvalidOperationException("unrecognized key");
            }
            return this.name;
        }
        
        internal void SetName(Project.Key key, string value) {
            if (this.key != key) {
                throw new InvalidOperationException("unrecognized key");
            }
            this.name = value;
        }
    }
}

