using System;
using CartheurCircuit.Elements;
using CartheurCircuit.Elements.Sources;

namespace CartheurCircuit
{
    public class FindPathInfo
    {
        public enum PathType
        {
            Inductance,
            Voltage,
            Short,
            CapVoltage,
        }

        readonly Circuit _simulation;
        bool[] used;
        int dest;
        ICircuitElement firstElm;
        PathType type;

        public FindPathInfo(Circuit r, PathType t, ICircuitElement e, int d)
        {
            _simulation = r;
            dest = d;
            type = t;
            firstElm = e;
            used = new bool[_simulation._nodeList.Count];
        }

        public bool FindPath(int n1)
        {
            return FindPath(n1, -1);
        }

        public bool FindPath(int n1, int depth)
        {
            if (n1 == dest)
                return true;

            if (depth-- == 0)
                return false;

            if (used[n1])
                return false;

            used[n1] = true;
            for (var i = 0; i != _simulation.Elements.Count; i++)
            {

                var ce = _simulation.GetElement(i);
                if (ce == firstElm)
                    continue;

                if (type == PathType.Inductance)
                    if (ce is CurrentSource)
                        continue;

                if (type == PathType.Voltage)
                    if (!(ce.IsWire() || ce is Voltage))
                        continue;

                if (type == PathType.Short && !ce.IsWire())
                    continue;

                if (type == PathType.CapVoltage)
                    if (!(ce.IsWire() || ce is Capacitor || ce is Voltage))
                        continue;

                if (n1 == 0)
                {
                    // look for posts which have a ground connection;
                    // our path can go through ground
                    for (var z = 0; z != ce.GetLeadCount(); z++)
                    {
                        if (ce.LeadIsGround(z) && FindPath(ce.GetLeadNode(z), depth))
                        {
                            used[n1] = false;
                            return true;
                        }
                    }
                }

                int j;
                for (j = 0; j != ce.GetLeadCount(); j++)
                    if (ce.GetLeadNode(j) == n1)
                        break;

                if (j == ce.GetLeadCount())
                    continue;

                if (ce.LeadIsGround(j) && FindPath(0, depth))
                {
                    // System.out.println(ce + " has ground");
                    used[n1] = false;
                    return true;
                }

                if (type == PathType.Inductance && ce is Inductor)
                {
                    var c = ce.GetCurrent();
                    if (j == 0)
                        c = -c;

                    // System.out.println("matching " + c + " to " + firstElm.getCurrent());
                    // System.out.println(ce + " " + firstElm);
                    if (Math.Abs(c - firstElm.GetCurrent()) > 1e-10)
                        continue;
                }

                for (var k = 0; k != ce.GetLeadCount(); k++)
                {
                    if (j == k)
                        continue;

                    // System.out.println(ce + " " + ce.getNode(j) + "-" + ce.getNode(k));
                    if (ce.LeadsAreConnected(j, k) && FindPath(ce.GetLeadNode(k), depth))
                    {
                        // System.out.println("got findpath " + n1);
                        used[n1] = false;
                        return true;
                    }
                    // System.out.println("back on findpath " + n1);
                }
            }

            used[n1] = false;
            // System.out.println(n1 + " failed");
            return false;
        }
    }
}
