/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;
using Toves.Layout.Comp;
using Toves.Layout.Data;
using Toves.Layout.Model;
using Toves.Sim.Inst;
using Toves.Sim.Model;
using Toves.Util.Transaction;

namespace Toves.Layout.Sim
{
    public class LayoutSimulation
    {
        private static readonly bool Debug = false;

        private SimulationModel simModel = new SimulationModel();
        private Dictionary<LayoutNode, Node> nodeMap = new Dictionary<LayoutNode, Node>();
        private Dictionary<Component, ComponentInstance> instanceMap = new Dictionary<Component, ComponentInstance>();

        public LayoutSimulation(LayoutModel model) {
            model.LayoutModifiedEvent += onUpdate;
        }

        public SimulationModel SimulationModel { get { return simModel; } }

        public Value GetValueAt(ILayoutAccess layout, ISimulationAccess sim, Location loc) {
            LayoutNode sup = layout.FindNode(loc);
            if (nodeMap.ContainsKey(sup)) {
                return sim.Get(nodeMap[sup]);
            } else {
                return Value.X;
            }
        }

        public ComponentInstance GetInstance(ILayoutAccess access, Component component) {
            return instanceMap[component];
        }

        private void onUpdate(object sender, LayoutModifiedArgs args) {
            ILayoutAccess layout = args.Layout;
            foreach (LayoutNode node in layout.Nodes) {
                if (!nodeMap.ContainsKey(node)) {
                    nodeMap[node] = new Node();
                }
            }
            Transaction xn = new Transaction();
            ISimulationAccess sim = xn.RequestWriteAccess(simModel);
            using (xn.Start()) {
                foreach (Component comp in layout.Components) {
                    if (!instanceMap.ContainsKey(comp)) {
                        ComponentInstance instance = comp.CreateInstance();
                        instanceMap[comp] = instance;
                        sim.AddInstance(instance);
                    }
                }
                if (Debug) {
                    Console.WriteLine("Circuit edited");
                }
                foreach (Component comp in layout.Components) {
                    Location cloc = comp.Location;
                    String cstr = null;
                    ComponentInstance instance = instanceMap[comp];
                    Toves.Layout.Comp.Port[] compPorts = comp.Ports;
                    int i = -1;
                    foreach (Toves.Sim.Model.Port ip in instance.Ports) {
                        i++;
                        Toves.Layout.Comp.Port cp = compPorts[i];
                        Location ploc = cloc.Translate(cp.Dx, cp.Dy);
                        if (Debug) {
                            if (i == 0) {
                                cstr = String.Format("{0}[{1}]", comp.GetType().Name, cloc.ToString());
                            } else {
                                cstr = String.Format("{0," + cstr.Length + "}", "");
                            }
                            Console.WriteLine("  {0}.{1}: {2} [{3}]", cstr, i, layout.FindNode(ploc), ploc);
                        }
                        LinkPortTo(sim, ip, nodeMap[layout.FindNode(ploc)]);
                    }
                }
            }
        }

        private void LinkPortTo(ISimulationAccess sim, Toves.Sim.Model.Port ip, Node nDesired) {
            Node nCurrent = null;
            List<Node> nRemove = null;
            foreach (Node n in ip.Neighbors) {
                if (nCurrent == null) {
                    nCurrent = n;
                } else {
                    if (nRemove == null) {
                        nRemove = new List<Node>();
                    }
                    nRemove.Add(n);
                }
            }
            if (nRemove != null) {
                foreach (Node n in nRemove) {
                    if (Debug) {
                        Console.WriteLine("    remove link {0}-{0}", ip, n);
                    }
                    sim.RemoveLink(new Link(ip, n));
                }
            }
            if (nDesired != nCurrent) {
                if (nCurrent != null) {
                    if (Debug) {
                        Console.WriteLine("    remove link {0}-{1}", ip, nCurrent);
                    }
                    sim.RemoveLink(new Link(ip, nCurrent));
                }
                if (nDesired != null) {
                    if (Debug) {
                        Console.WriteLine("    add link {0}-{1}", ip, nDesired);
                    }
                    sim.AddLink(new Link(ip, nDesired));
                }
            }
        }
    }
}

