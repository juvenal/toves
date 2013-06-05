/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;
using Toves.Util.Transaction;

namespace Toves.Proj
{
    public interface IProjectAccess : IResourceAccess {
        IEnumerable<ProjectModule> GetModules();
        ProjectModule GetModule(String name);
        string GetModuleName(ProjectModule module);

        void AddModule(String name);
        void SetModuleName(ProjectModule module, String value);
    }
}

