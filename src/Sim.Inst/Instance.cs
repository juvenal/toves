/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;
using Toves.Sim.Model;

namespace Toves.Sim.Inst
{
    public abstract class Instance
    {
        private Port[] portArray;

        public Instance(PortArgs[] ports)
        {
            InitPorts(ports);
        }

        public IEnumerable<Port> Ports { get; private set; }

        private void InitPorts(PortArgs[] ports)
        {
            Port[] copy = new Port[ports.Length];
            for (int i = 0; i < copy.Length; i++) {
                copy[i] = new Port(this, ports[i]);
            }
            this.portArray = copy;
            this.Ports = Array.AsReadOnly(copy);
        }

        public Port GetPort(int index)
        {
            return portArray[index];
        }

        public void SetPorts(SimulationModel.Key key, PortArgs[] ports)
        {
            if (key == null) {
                throw new InvalidOperationException("need key");
            } else {
                InitPorts(ports);
            }
        }
        
        public abstract void Propagate(IInstanceState state);

        /*
        public Value this[int index] {
            get {
                Port[] ps = portArray;
                if (index < 0 || index >= ps.Length) {
                    throw new IndexOutOfRangeException(string.Format("{0} given, length {1}", index, ps.Length));
                } else {
                    Subnet sub = ps[index].Subnet;
                    Value subValue = sub == null ? Value.Z : sub.GetValue(key);
                    if (ps[index].IsOutput) {
                        Value pVal = ps[index].GetDrivenValue(key);
                        return subValue.Resolve(pVal == null ? Value.X : pVal);
                    } else {
                        return subValue;
                    }
                }
            }
        }
        
        public Value Get(int index)
        {
            return this[index];
        }

        public void Set(int index, Value value, int delay)
        {
            Port[] ps = portArray;
            if (index < 0 || index >= ps.Length) {
                throw new IndexOutOfRangeException(string.Format("{0} given, length {1}", index, ps.Length));
            } else if (ps[index].IsOutput) {
                key.Set(ps[index], value, delay);
            } else {
                throw new InvalidOperationException(string.Format("port {0} is not an output", index.ToString()));
            }
        }

        public Value GetDriven(int index) {
            Port[] ps = portArray;
            if (index < 0 || index >= ps.Length) {
                throw new IndexOutOfRangeException(string.Format("{0} given, length {1}", index, ps.Length));
            } else if (ps[index].IsOutput) {
                Value pVal = ps[index].DrivenValue;
                return pVal == null ? Value.X : pVal;
            } else {
                throw new InvalidOperationException(string.Format("port {0} is not an output", index));
            }
        }
        */
    }
}

