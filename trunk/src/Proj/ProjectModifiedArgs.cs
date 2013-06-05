/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;

namespace Toves.Proj {
    public class ProjectModifiedArgs : EventArgs {
        public ProjectModifiedArgs(IProjectAccess access) {
            this.ProjectAccess = access;
        }

        public IProjectAccess ProjectAccess { get; private set; }
    }
}