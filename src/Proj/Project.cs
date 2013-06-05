/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;
using Toves.Util.Transaction;

namespace Toves.Proj {
    public class Project : Resource<IProjectAccess> {
        public class Key {
            internal Key(Project proj) {
                if (proj.key != null) {
                    throw new InvalidOperationException("key already created for project");
                }
                this.Project = proj;
            }

            public Project Project { get; private set; }
        }

        private class ProjectAccess : ResourceAccess, IProjectAccess {
            private Key key;

            internal ProjectAccess(Key key, bool writing) : base(key.Project, writing) {
                this.key = key;
            }

            public IEnumerable<ProjectModule> GetModules() {
                CheckReadAccess();
                return key.Project.modules.AsReadOnly();
            }
            
            public ProjectModule GetModule(String name) {
                CheckReadAccess();
                foreach (ProjectModule mod in key.Project.modules) {
                    if (mod.GetName(key).Equals(name)) {
                        return mod;
                    }
                }
                return null;
            }
            
            public string GetModuleName(ProjectModule module) {
                CheckReadAccess();
                return module.GetName(key);
            }

            public void AddModule(String name) {
                CheckWriteAccess();
                ProjectModule found = GetModule(name);
                if (found != null) {
                    throw new InvalidOperationException("module already exists by that name");
                }
                key.Project.modules.Add(new ProjectModule(key, name));
            }

            public void SetModuleName(ProjectModule module, String value) {
                CheckReadAccess();
                ProjectModule found = GetModule(value);
                if (found != null && found != module) {
                    throw new InvalidOperationException("module already exists by that name");
                }
                module.SetName(key, value);
            }

        }

        private Key key = null;
        private List<ProjectModule> modules = new List<ProjectModule>();

        public Project() {
            this.key = new Key(this);
            this.ResourceHelper = new ResourceHelper();
            modules.Add(new ProjectModule(this.key, "main"));
            modules.Add(new ProjectModule(this.key, "dummy"));
        }

        public event EventHandler<ProjectModifiedArgs> ProjectModifiedEvent;

        public IProjectAccess CreateAccess(bool writing) {
            return new ProjectAccess(key, writing);
        }

        public ResourceHelper ResourceHelper { get; private set; }

        public void Hook(ResourceHookType type, IResourceAccess access) {
            if (type == ResourceHookType.AfterDowngrade) {
                EventHandler<ProjectModifiedArgs> handler = ProjectModifiedEvent;
                if (handler != null) {
                    handler(this, new ProjectModifiedArgs(access as IProjectAccess));
                }
            }
        }

    }
}

