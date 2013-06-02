/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;
using Toves.Sim.Inst;
using Toves.Util.Transaction;

namespace Toves.Sim.Model
{
    public class SimulationModel : Resource<ISimulationAccess>
    {
        public sealed class Key
        {
            private SimulationModel model;

            internal Key(SimulationModel model) {
                if (model.key != null) {
                    throw new ArgumentException("key already created");
                }
                this.model = model;
            }

            public Engine Engine { get { return model.engine; } }
            
            /*
            public void Set(Port port, Value value, int delay)
            {
            }
            */
        }

        private class Access : ResourceAccess, ISimulationAccess
        {
            private SimulationModel sim;
            internal List<Node> dirtyNodes = null;
            internal bool netsChanged = false;
            
            internal Access(SimulationModel sim, bool canWrite) : base(sim, canWrite)
            {
                this.sim = sim;
            }

            public IEnumerable<Node> Nodes {
                get {
                    CheckReadAccess();
                    return sim.nodes.AsReadOnly();
                }
            }
            
            public IEnumerable<Instance> Instances {
                get {
                    CheckReadAccess();
                    return sim.instances.AsReadOnly();
                }
            }
            
            public void AddLink(Link value) {
                CheckWriteAccess();
                Node n0 = value.Source;
                Node n1 = value.Destination;
                sim.nodes.Add(n0);
                sim.nodes.Add(n1);
                n0.AddLink(sim.key, value);
                n1.AddLink(sim.key, value);
                netsChanged = true;

                if (dirtyNodes == null) {
                    dirtyNodes = new List<Node>();
                }
                dirtyNodes.Add(n0);
                dirtyNodes.Add(n1);
            }
            
            public void RemoveLink(Link value) {
                CheckWriteAccess();
                Node n0 = value.Source;
                Node n1 = value.Destination;
                n0.RemoveLink(sim.key, value);
                n1.RemoveLink(sim.key, value);
                if (n0.UseCount == 0) {
                    sim.nodes.Remove(n0);
                }
                if (n1.UseCount == 0) {
                    sim.nodes.Remove(n1);
                }
                netsChanged = true;

                if (dirtyNodes == null) {
                    dirtyNodes = new List<Node>();
                }
                dirtyNodes.Add(n0);
                dirtyNodes.Add(n1);
            }
            
            public void AddInstance(Instance value) {
                CheckWriteAccess();
                sim.instances.Add(value);
                foreach (Port p in value.Ports) {
                    p.AddToUseCount(sim.key, 1);
                    sim.nodes.Add(p);
                }
                sim.engine.AddDirtyInstance(value);
                netsChanged = true;
            }
            
            public void RemoveInstance(Instance value) {
                CheckWriteAccess();
                sim.instances.Remove(value);
                foreach (Port p in value.Ports) {
                    p.AddToUseCount(sim.key, -1);
                    if (p.UseCount == 0) {
                        sim.nodes.Remove(p);
                    }
                }
                netsChanged = true;
            }

            public Value Get(Node n)
            {
                CheckReadAccess();
                Subnet sub = n.Subnet;
                Value val = sub == null ? null : sub.GetValue(sim.key);
                if (n is Port) {
                    Port port = (Port) n;
                    if (port.IsOutput) {
                        Value drive = port.GetDrivenValue(sim.key);
                        val = val == null ? drive : val.Resolve(drive ?? Value.X);
                    }
                }
                return val ?? Value.X;
            }
            
            public Value GetDriven(Port port)
            {
                CheckReadAccess();
                if (port.IsOutput) {
                    Value pVal = port.GetDrivenValue(sim.key);
                    return pVal ?? Value.X;
                } else {
                    throw new InvalidOperationException("port is not an output");
                }
            }

            public void Set(Port port, Value value, int delay)
            {
                CheckWriteAccess();
                if (port.IsOutput) {
                    sim.engine.QueueSet(port, value, delay);
                } else {
                    throw new InvalidOperationException("port is not an output");
                }
            }

            public void MarkInstanceDirty(Instance value)
            {
                CheckWriteAccess();
                sim.engine.AddDirtyInstance(value);
            }

            public bool IsStepPending()
            {
                CheckReadAccess();
                return sim.engine.IsStepPending();
            }

            public void StepSimulation()
            {
                CheckWriteAccess();
                sim.engine.Step(sim.key, this);
            }
        }

        private Key key = null;
        private Engine engine = new Engine();
        private List<Instance> instances = new List<Instance>();
        private List<Node> nodes = new List<Node>();
        private List<Subnet> subnets = new List<Subnet>();
        private ResourceHelper resourceHelper = new ResourceHelper();

        public SimulationModel()
        {
            this.key = new Key(this);
        }

        public ResourceHelper ResourceHelper { get { return resourceHelper; } }

        public event EventHandler<SimulationModelModifiedArgs> SimulationModelModifiedEvent;
        
        protected virtual void OnSimulationModelModified()
        {
            EventHandler<SimulationModelModifiedArgs> handler = SimulationModelModifiedEvent;
            if (handler != null) {
                handler(this, new SimulationModelModifiedArgs());
            }
        }

        public void Hook(ResourceHookType hookType, IResourceAccess rawAccess) {
            Access access = rawAccess as Access;
            if (hookType == ResourceHookType.AfterWrite) {
                if (access.netsChanged) {
                    this.subnets = Subnet.UpdateNets(key, access, subnets);
                }
                List<Node> dirtyNodes = access.dirtyNodes;
                if (dirtyNodes != null) {
                    foreach (Node n in dirtyNodes) {
                        engine.AddDirtyNet(n.Subnet);
                    }
                }
            } else if (hookType == ResourceHookType.AfterDowngrade) {
                OnSimulationModelModified();
            }
        }

        public ISimulationAccess CreateAccess(bool canWrite) {
            return new Access(this, canWrite);
        }
    }
}

