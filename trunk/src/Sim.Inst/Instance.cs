/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;
using Toves.Sim.Model;

namespace Toves.Sim.Inst {
    public abstract class Instance {
        private Port[] portArray;

        public Instance(PortArgs[] ports) {
            InitPorts(ports);
        }

        public IEnumerable<Port> Ports { get; private set; }

        private void InitPorts(PortArgs[] ports) {
            Port[] copy = new Port[ports.Length];
            for (int i = 0; i < copy.Length; i++) {
                copy[i] = new Port(this, ports[i]);
            }
            this.portArray = copy;
            this.Ports = Array.AsReadOnly(copy);
        }

        public Port GetPort(int index) {
            return portArray[index];
        }

        public void SetPorts(SimulationModel.Key key, PortArgs[] ports) {
            if (key == null) {
                throw new InvalidOperationException("need key");
            } else {
                InitPorts(ports);
            }
        }

        public abstract void HandleEvent(InstanceEvent evnt, IInstanceState state);
    }
}

