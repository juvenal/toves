/* Copyright (c) 2013, Carl Burch.  License information is in the GtkMain.cs
 * source file and at www.toves.org/. */
using System;
using System.Collections.Generic;

namespace Toves.Util.Collections
{
    public class UnionFind<T>
    {
        public class Node
        {
            private UnionFind<T> allNodes;
            private Node parent = null;
            private List<Node> subNodes = null;

            public Node(UnionFind<T> allNodes, T value) {
                this.allNodes = allNodes;
                this.Value = value;
            }

            public T Value { get; private set; }

            private Node Find() {
                Node ret = this.parent;
                if (ret == null) {
                    return this;
                } else {
                    Node next = ret.parent;
                    while (next != null) {
                        ret = next;
                        next = ret.parent;
                    }
                    this.parent = ret;
                    return ret;
                }
            }

            public bool IsInSameSet(Node other)
            {
                return this.Find() == other.Find();
            }

            public IEnumerable<Node> GetSetMembers()
            {
                Node a = this.Find();
                if (a.subNodes == null) {
                    List<Node> result = new List<Node>();
                    result.Add(a);
                    a.subNodes = result;
                    return result;
                } else {
                    return a.subNodes;
                }
            }

            public void Unite(Node other)
            {
                Node a = this.Find();
                Node b = other.Find();
                if (a != b) {
                    // need to unite a and b - a will be parent of b
                    int aSize = a.subNodes == null ? 1 : a.subNodes.Count;
                    int bSize = b.subNodes == null ? 1 : b.subNodes.Count;
                    if (aSize < bSize) { // a is smaller, so it should be child
                        Node t = a;
                        a = b;
                        b = t;
                    }
                    b.parent = a;
                    if (a.subNodes == null) {
                        a.subNodes = new List<Node>();
                        a.subNodes.Add(a);
                    }
                    if (b.subNodes == null) {
                        a.subNodes.Add(b);
                    } else {
                        foreach (Node bn in b.subNodes) {
                            a.subNodes.Add(bn);
                        }
                        b.subNodes = null;
                    }
                    allNodes.roots.Remove(b);
                }
            }
        }

        private List<Node> nodes = new List<Node>();
        private Dictionary<T, Node> nodeMap = new Dictionary<T, Node>();
        private HashSet<Node> roots = new HashSet<Node>();

        public UnionFind(IEnumerable<T> values)
        {
            foreach (T value in values) {
                Node n = new Node(this, value);
                nodes.Add(n);
                nodeMap[value] = n;
                roots.Add(n);
            }
        }

        public IEnumerable<Node> Nodes {
            get { return nodes; }
        }
        
        public IEnumerable<Node> Roots {
            get { return roots; }
        }

        public Node this[T value]
        {
            get { return nodeMap[value]; }
        }
    }
}

