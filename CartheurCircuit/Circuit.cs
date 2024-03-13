using System;
using System.Collections.Generic;
using CartheurCircuit.Elements;
using CartheurCircuit.Elements.Sources;
using CartheurCircuit.Handling;

namespace CartheurCircuit
{
    public class Circuit
    {
        public readonly static string OhmString = "ohm";
        const double Epsilon = 1E-6;
        /// <summary>
        /// Gets the time.
        /// </summary>
        public double Time
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets or sets the time step.
        /// </summary>
        public double TimeStep
        {
            get
            {
                return _timeStep;
            }
            set
            {
                _timeStep = value;
                _analyze = true;
            }
        }

        public int Speed { get; set; } // 172 // Math.Log(10 * 14.3) * 24 + 61.5;
        public List<ICircuitElement> Elements { get; set; }
        public List<long[]> NodeMesh { get; set; }
        public int NodeCount { get { return _nodeList.Count; } }

        public bool Converged;
        public int SubIterations;
        readonly IdWorker _handler;
        bool _analyze = true;
        double _timeStep = 5E-6;

        public readonly List<long> _nodeList = new List<long>();
        ICircuitElement[] _voltageSources;

        double[][] _circuitMatrix; // contains circuit state
        double[] _circuitRightSide;
        double[][] _origMatrix;
        double[] _origRightSide;
        RowInfo[] _circuitRowInfo;
        int[] _circuitPermute;
        int _circuitMatrixSize, _circuitMatrixFullSize;
        bool _circuitNonLinear, _circuitNeedsMap;

        readonly Dictionary<ICircuitElement, List<ScopeFrame>> _scopeMap;

        public Circuit()
        {
            Speed = 172;
            _handler = new IdWorker(1, 1);
            Elements = new List<ICircuitElement>();
            NodeMesh = new List<long[]>();
            _scopeMap = new Dictionary<ICircuitElement, List<ScopeFrame>>();
        }
        /// <summary>
        /// Creates the specified arguments in the circuit.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public T Create<T>(params object[] args) where T : class, ICircuitElement
        {
            var circuit = Activator.CreateInstance(typeof(T), args) as T;
            AddElement(circuit);
            return circuit;
        }

        public void AddElement(ICircuitElement elm)
        {
            if (!Elements.Contains(elm))
            {
                Elements.Add(elm);

                NodeMesh.Add(new long[elm.GetLeadCount()]);
                for (var x = 0; x < elm.GetLeadCount(); x++)
                    NodeMesh[NodeMesh.Count - 1][x] = -1;

                var eNdx = Elements.Count - 1;
                var mNdx = NodeMesh.Count - 1;
                if (eNdx != mNdx)
                    throw new System.Exception("AddElement array length mismatch");
            }
        }

        /// <summary>
        /// Connects leads to an element.
        /// </summary>
        /// <param name="left">The left lead.</param>
        /// <param name="right">The right lead.</param>
        public void Connect(Lead left, Lead right)
        {
            Connect(left.Element, left.Nodedx, right.Element, right.Nodedx);
        }

        public void Connect(ICircuitElement left, int leftLeadNdx, ICircuitElement right, int rightLeadNdx)
        {
            var leftNdx = Elements.IndexOf(left);
            var rightNdx = Elements.IndexOf(right);
            Connect(leftNdx, leftLeadNdx, rightNdx, rightLeadNdx);
            NeedAnalyze();
        }

        public void Connect(int leftNdx, int leftLeadNdx, int rightNdx, int rightLeadNdx)
        {
            var leftLeads = NodeMesh[leftNdx];
            var rightLeads = NodeMesh[rightNdx];

            if (leftLeadNdx >= leftLeads.Length)
                Array.Resize(ref leftLeads, leftLeadNdx + 1);

            if (rightLeadNdx >= rightLeads.Length)
                Array.Resize(ref rightLeads, rightLeadNdx + 1);

            var leftConn = leftLeads[leftLeadNdx];
            var rightConn = rightLeads[rightLeadNdx];

            // If both leads are unconnected, we need a new node
            var empty = leftConn == -1 && rightConn == -1;
            if (empty)
            {
                var id = _handler.NextId();
                leftLeads[leftLeadNdx] = id;
                rightLeads[rightLeadNdx] = id;
                return;
            }

            // If the left lead is unconnected, attach to right
            if (leftConn == -1)
                leftLeads[leftLeadNdx] = rightLeads[rightLeadNdx];

            // If the right lead is unconnected, attach to left node
            // If the right lead is _connected_, replace with left node
            rightLeads[rightLeadNdx] = leftLeads[leftLeadNdx];
        }

        public List<ScopeFrame> Watch(ICircuitElement component)
        {
            if (!_scopeMap.ContainsKey(component))
            {
                var scope = new List<ScopeFrame>();
                _scopeMap.Add(component, scope);
                return scope;
            }
            return _scopeMap[component];
        }

        public void ResetTime()
        {
            Time = 0;
        }

        public void NeedAnalyze()
        {
            _analyze = true;
        }

        public void UpdateVoltageSource(int n1, int n2, int vs, double v)
        {
            var vn = _nodeList.Count + vs;
            StampRightSide(vn, v);
        }

        public long GetNodeId(int ndx)
        {
            return (ndx < _nodeList.Count) ? _nodeList[ndx] : 0;
        }

        public ICircuitElement GetElement(int n)
        {
            return (n < Elements.Count) ? Elements[n] : null;
        }

        public void DoTick()
        {
            DoTicks(1);
        }

        public void DoTicks(int ticks)
        {
            if (Elements.Count == 0) return;
            if (_analyze) Analyze();
            if (_analyze) return;
            for (var x = 0; x < ticks; x++)
            {
                Tick();
                if (_circuitMatrix == null) return;
                foreach (var kvp in _scopeMap)
                    kvp.Value.Add(kvp.Key.GetScopeFrame(Time));
            }
        }

        void Tick()
        {

            // Execute BeginStep() on all elements
            for (var i = 0; i != Elements.Count; i++)
                Elements[i].BeginStep(this);

            int subiter;
            const int subiterCount = 5000;
            for (subiter = 0; subiter != subiterCount; subiter++)
            {
                Converged = true;
                SubIterations = subiter;

                // Copy `origRightSide` to `circuitRightSide`
                for (var i = 0; i != _circuitMatrixSize; i++)
                    _circuitRightSide[i] = _origRightSide[i];

                // If the circuit is non linear, copy
                // `origMatrix` to `circuitMatrix`
                if (_circuitNonLinear)
                    for (var i = 0; i != _circuitMatrixSize; i++)
                        for (var j = 0; j != _circuitMatrixSize; j++)
                            _circuitMatrix[i][j] = _origMatrix[i][j];

                // Execute step() on all elements
                for (var i = 0; i != Elements.Count; i++)
                    Elements[i].Step(this);

                // Can't have any values in the matrix be NaN or Inf
                for (var j = 0; j != _circuitMatrixSize; j++)
                {
                    for (var i = 0; i != _circuitMatrixSize; i++)
                    {
                        var x = _circuitMatrix[i][j];
                        if (Double.IsNaN(x) || Double.IsInfinity(x))
                            Panic("NaN/Infinite matrix!", null);
                    }
                }

                // If the circuit is non-Linear, factor it now,
                // if it's linear, it was factored in analyze()
                if (_circuitNonLinear)
                {
                    // Break if the circuit has converged.
                    if (Converged && subiter > 0) break;
                    if (!LuFactor(_circuitMatrix, _circuitMatrixSize, _circuitPermute))
                        Panic("Singular matrix!", null);
                }

                // Solve the factorized matrix
                LuSolve(_circuitMatrix, _circuitMatrixSize, _circuitPermute, _circuitRightSide);

                for (var j = 0; j != _circuitMatrixFullSize; j++)
                {
                    double res;

                    var ri = _circuitRowInfo[j];
                    res = (ri.type == RowInfo.ROW_CONST) ? ri.value : _circuitRightSide[ri.mapCol];

                    // If any resuit is NaN, break
                    if (Double.IsNaN(res))
                    {
                        Converged = false;
                        break;
                    }

                    if (j < _nodeList.Count - 1)
                    {
                        // For each node in the mesh
                        for (var k = 0; k != NodeMesh.Count; k++)
                        {
                            var leads = new List<long>(NodeMesh[k]); // Get the leads conected to the node
                            var ndx = leads.IndexOf(GetNodeId(j + 1));
                            if (ndx != -1)
                                Elements[k].SetLeadVoltage(ndx, res);
                        }
                    }
                    else
                    {
                        var ji = j - (_nodeList.Count - 1);
                        _voltageSources[ji].SetCurrent(ji, res);
                    }
                }

                // if the matrix is linear, we don't
                // need to do any more iterations
                if (!_circuitNonLinear)
                    break;
            }

            if (subiter > 5)
                Debug.LogF("Nonlinear curcuit converged after {0} iterations.", subiter);

            if (subiter == subiterCount)
                Panic("Convergence failed!", null);

            Time = Math.Round(Time + TimeStep, 12); // Round to 12 digits
        }

        public void Analyze()
        {

            if (Elements.Count == 0)
                return;

            _nodeList.Clear();
            var internalList = new List<bool>();
            Action<long, bool> pushNode = (id, isInternal) =>
            {
                if (!_nodeList.Contains(id))
                {
                    _nodeList.Add(id);
                    internalList.Add(isInternal);
                }
            };

            #region //// Look for Voltage or Ground element ////
            // Search the circuit for a Ground, or Voltage sourcre
            ICircuitElement voltageElement = null;
            var gotGround = false;
            var gotRail = false;
            for (var i = 0; i != Elements.Count; i++)
            {
                var element = Elements[i];
                if (element is Ground)
                {
                    gotGround = true;
                    break;
                }

                if (element is VoltageInput)
                    gotRail = true;

                if (element is Voltage && voltageElement == null)
                    voltageElement = element;

            }

            // If no ground and no rails, then the voltage elm's first terminal is ground.
            if (!gotGround && !gotRail && voltageElement != null)
            {
                var elmNdx = Elements.IndexOf(voltageElement);
                var ndxs = NodeMesh[elmNdx];
                pushNode(ndxs[0], false);
            }
            else
            {
                // If the circuit contains a ground, rail, or voltage element, push a temporary node to the node list.
                pushNode(_handler.NextId(), false);
            }
            #endregion

            // At this point, there is 1 node in the list, the special `global ground` node.

            #region //// Nodes and Voltage Sources ////
            var vscount = 0; // Number of voltage sources
            for (var i = 0; i != Elements.Count; i++)
            {
                var elm = Elements[i];
                var leads = elm.GetLeadCount();

                // If the leadCount reported by the element does
                // not match the number of leads allocated, 
                // resize the array.
                if (leads != NodeMesh[i].Length)
                {
                    var leadMap = NodeMesh[i];
                    Array.Resize(ref leadMap, leads);
                    NodeMesh[i] = leadMap;
                }

                // For each lead in the element
                for (var leadX = 0; leadX != leads; leadX++)
                {
                    var leadNode = NodeMesh[i][leadX];        // Id of the node leadX is connected too
                    var nodeNdx = _nodeList.IndexOf(leadNode); // Index of the leadNode in the nodeList
                    if (nodeNdx == -1)
                    {
                        // If the nodeList doesn't contain the node, push it
                        // onto the list and assign it's new index to the lead.
                        elm.SetLeadNode(leadX, _nodeList.Count);
                        pushNode(leadNode, false);
                    }
                    else
                    {
                        // Otherwise, assign the lead the index of 
                        // the node in the nodeList.
                        elm.SetLeadNode(leadX, nodeNdx);

                        if (leadNode == 0)
                        {
                            // if it's the ground node, make sure the 
                            // node voltage is 0, cause it may not get set later
                            elm.SetLeadVoltage(leadX, 0); // TODO: ??
                        }
                    }
                }

                // Push an internal node onto the list for
                // each internal lead on the element.
                var internalLeads = elm.GetInternalLeadCount();
                for (var x = 0; x != internalLeads; x++)
                {
                    elm.SetLeadNode(leads + x, _nodeList.Count);
                    pushNode(_handler.NextId(), true);
                }

                vscount += elm.GetVoltageSourceCount();
            }
            #endregion

            // == Creeate the voltageSources array.
            // Also determine if circuit is nonlinear.

            // VoltageSourceId -> ICircuitElement map
            _voltageSources = new ICircuitElement[vscount];
            vscount = 0;

            _circuitNonLinear = false;
            //for(int i = 0; i != elements.Count; i++) {
            //	ICircuitElement elem = elements[i];
            foreach (var elem in Elements)
            {

                if (elem.NonLinear()) _circuitNonLinear = true;

                // Assign each votage source in the element a globally unique id,
                // (the index of the next open slot in voltageSources)
                for (var leadX = 0; leadX != elem.GetVoltageSourceCount(); leadX++)
                {
                    _voltageSources[vscount] = elem;
                    elem.SetVoltageSource(leadX, vscount++);
                }
            }

            #region //// Matrix setup ////
            var matrixSize = _nodeList.Count - 1 + vscount;

            // setup circuitMatrix
            _circuitMatrix = new double[matrixSize][];
            for (var z = 0; z < matrixSize; z++)
                _circuitMatrix[z] = new double[matrixSize];

            _circuitRightSide = new double[matrixSize];

            // setup origMatrix
            _origMatrix = new double[matrixSize][];
            for (var z = 0; z < matrixSize; z++)
                _origMatrix[z] = new double[matrixSize];

            _origRightSide = new double[matrixSize];

            // setup circuitRowInfo
            _circuitRowInfo = new RowInfo[matrixSize];
            for (var i = 0; i != matrixSize; i++)
                _circuitRowInfo[i] = new RowInfo();

            _circuitPermute = new int[matrixSize];
            _circuitMatrixSize = _circuitMatrixFullSize = matrixSize;
            _circuitNeedsMap = false;
            #endregion

            // Stamp linear circuit elements.
            for (var i = 0; i != Elements.Count; i++)
                Elements[i].Stamp(this);

            #region //// Determine nodes that are unconnected ////
            var closure = new bool[_nodeList.Count];
            var changed = true;
            closure[0] = true;
            while (changed)
            {
                changed = false;
                for (var i = 0; i != Elements.Count; i++)
                {
                    var ce = Elements[i];
                    // loop through all ce's nodes to see if they are connected
                    // to other nodes not in closure
                    for (var leadX = 0; leadX < ce.GetLeadCount(); leadX++)
                    {
                        if (!closure[ce.GetLeadNode(leadX)])
                        {
                            if (ce.LeadIsGround(leadX))
                                closure[ce.GetLeadNode(leadX)] = changed = true;
                            continue;
                        }
                        for (var k = 0; k != ce.GetLeadCount(); k++)
                        {
                            if (leadX == k) continue;
                            var kn = ce.GetLeadNode(k);
                            if (ce.LeadsAreConnected(leadX, k) && !closure[kn])
                            {
                                closure[kn] = true;
                                changed = true;
                            }
                        }
                    }
                }

                if (changed)
                    continue;

                // connect unconnected nodes
                for (var i = 0; i != _nodeList.Count; i++)
                {
                    if (!closure[i] && !internalList[i])
                    {
                        //System.out.println("node " + i + " unconnected");
                        StampResistor(0, i, 1E8);
                        closure[i] = true;
                        changed = true;
                        break;
                    }
                }
            }
            #endregion

            #region //// Sanity checks ////
            for (var i = 0; i != Elements.Count; i++)
            {
                var ce = Elements[i];

                // look for inductors with no current path
                if (ce is Inductor)
                {
                    var fpi = new FindPathInfo(this, FindPathInfo.PathType.Inductance, ce, ce.GetLeadNode(1));
                    // first try findPath with maximum depth of 5, to avoid slowdowns
                    if (!fpi.FindPath(ce.GetLeadNode(0), 5) && !fpi.FindPath(ce.GetLeadNode(0)))
                    {
                        //System.out.println(ce + " no path");
                        ce.Reset();
                    }
                }

                // look for current sources with no current path
                if (ce is CurrentSource)
                {
                    var fpi = new FindPathInfo(this, FindPathInfo.PathType.Inductance, ce, ce.GetLeadNode(1));
                    if (!fpi.FindPath(ce.GetLeadNode(0)))
                        Panic("No path for current source!", ce);
                }

                // look for voltage source loops
                if ((ce is Voltage && ce.GetLeadCount() == 2) || ce is Wire)
                {
                    var fpi = new FindPathInfo(this, FindPathInfo.PathType.Voltage, ce, ce.GetLeadNode(1));
                    if (fpi.FindPath(ce.GetLeadNode(0)))
                        Panic("Voltage source/wire loop with no resistance!", ce);
                }

                // look for shorted caps, or caps w/ voltage but no R
                if (ce is Capacitor)
                {
                    var fpi = new FindPathInfo(this, FindPathInfo.PathType.Short, ce, ce.GetLeadNode(1));
                    if (fpi.FindPath(ce.GetLeadNode(0)))
                    {
                        //System.out.println(ce + " shorted");
                        ce.Reset();
                    }
                    else
                    {
                        fpi = new FindPathInfo(this, FindPathInfo.PathType.CapVoltage, ce, ce.GetLeadNode(1));
                        if (fpi.FindPath(ce.GetLeadNode(0)))
                            Panic("Capacitor loop with no resistance!", ce);
                    }
                }
            }
            #endregion

            #region //// Simplify the matrix //// =D
            for (var i = 0; i != matrixSize; i++)
            {
                int qm = -1, qp = -1;
                double qv = 0;
                var re = _circuitRowInfo[i];

                if (re.lsChanges || re.dropRow || re.rsChanges)
                    continue;

                double rsadd = 0;

                // look for rows that can be removed
                var leadX = 0;
                for (; leadX != matrixSize; leadX++)
                {

                    var q = _circuitMatrix[i][leadX];
                    if (_circuitRowInfo[leadX].type == RowInfo.ROW_CONST)
                    {
                        // keep a running total of const values that have been removed already
                        rsadd -= _circuitRowInfo[leadX].value * q;
                        continue;
                    }

                    if (Math.Abs(q) < Epsilon)
                        continue;

                    if (qp == -1)
                    {
                        qp = leadX;
                        qv = q;
                        continue;
                    }

                    if (qm == -1 && Math.Abs(q - (-qv)) < Epsilon)
                    {
                        qm = leadX;
                        continue;
                    }

                    break;
                }

                if (leadX == matrixSize)
                {

                    if (qp == -1)
                        Panic("Matrix error.", null);

                    var elt = _circuitRowInfo[qp];
                    if (qm == -1)
                    {
                        // we found a row with only one nonzero entry;
                        // that value is a constant
                        for (var k = 0; elt.type == RowInfo.ROW_EQUAL && k < 100; k++)
                        {
                            // follow the chain
                            // System.out.println("following equal chain from " + i + " " + qp + " to " + elt.nodeEq);
                            qp = elt.nodeEq;
                            elt = _circuitRowInfo[qp];
                        }

                        if (elt.type == RowInfo.ROW_EQUAL)
                        {
                            // break equal chains
                            // System.out.println("Break equal chain");
                            elt.type = RowInfo.ROW_NORMAL;
                            continue;
                        }

                        if (elt.type != RowInfo.ROW_NORMAL)
                        {
                            //System.out.println("type already " + elt.type + " for " + qp + "!");
                            continue;
                        }

                        elt.type = RowInfo.ROW_CONST;
                        elt.value = (_circuitRightSide[i] + rsadd) / qv;
                        _circuitRowInfo[i].dropRow = true;
                        // System.out.println(qp + " * " + qv + " = const " + elt.value);
                        i = -1; // start over from scratch
                    }
                    else if (Math.Abs(_circuitRightSide[i] + rsadd) < Epsilon)
                    {
                        // we found a row with only two nonzero entries, and one
                        // is the negative of the other; the values are equal
                        if (elt.type != RowInfo.ROW_NORMAL)
                        {
                            // System.out.println("swapping");
                            var qq = qm;
                            qm = qp;
                            qp = qq;
                            elt = _circuitRowInfo[qp];
                            if (elt.type != RowInfo.ROW_NORMAL)
                            {
                                // we should follow the chain here, but this hardly
                                // ever happens so it's not worth worrying about
                                //System.out.println("swap failed");
                                continue;
                            }
                        }
                        elt.type = RowInfo.ROW_EQUAL;
                        elt.nodeEq = qm;
                        _circuitRowInfo[i].dropRow = true;
                        // System.out.println(qp + " = " + qm);
                    }
                }
            }

            // == Find size of new matrix
            var nn = 0;
            for (var i = 0; i != matrixSize; i++)
            {
                var elt = _circuitRowInfo[i];
                if (elt.type == RowInfo.ROW_NORMAL)
                {
                    elt.mapCol = nn++;
                    // System.out.println("col " + i + " maps to " + elt.mapCol);
                    continue;
                }
                if (elt.type == RowInfo.ROW_EQUAL)
                {
                    RowInfo e2;
                    // resolve chains of equality; 100 max steps to avoid loops
                    for (var leadX = 0; leadX != 100; leadX++)
                    {
                        e2 = _circuitRowInfo[elt.nodeEq];
                        if (e2.type != RowInfo.ROW_EQUAL)
                            break;

                        if (i == e2.nodeEq)
                            break;

                        elt.nodeEq = e2.nodeEq;
                    }
                }
                if (elt.type == RowInfo.ROW_CONST)
                    elt.mapCol = -1;
            }

            for (var i = 0; i != matrixSize; i++)
            {
                var elt = _circuitRowInfo[i];
                if (elt.type == RowInfo.ROW_EQUAL)
                {
                    var e2 = _circuitRowInfo[elt.nodeEq];
                    if (e2.type == RowInfo.ROW_CONST)
                    {
                        // if something is equal to a const, it's a const
                        elt.type = e2.type;
                        elt.value = e2.value;
                        elt.mapCol = -1;
                    }
                    else
                    {
                        elt.mapCol = e2.mapCol;
                    }
                }
            }

            // == Make the new, simplified matrix.
            var newsize = nn;
            var newmatx = new double[newsize][];
            for (var z = 0; z < newsize; z++)
                newmatx[z] = new double[newsize];

            var newrs = new double[newsize];
            var ii = 0;
            for (var i = 0; i != matrixSize; i++)
            {
                var rri = _circuitRowInfo[i];
                if (rri.dropRow)
                {
                    rri.mapRow = -1;
                    continue;
                }
                newrs[ii] = _circuitRightSide[i];
                rri.mapRow = ii;
                // System.out.println("Row " + i + " maps to " + ii);
                for (var leadX = 0; leadX != matrixSize; leadX++)
                {
                    var ri = _circuitRowInfo[leadX];
                    if (ri.type == RowInfo.ROW_CONST)
                    {
                        newrs[ii] -= ri.value * _circuitMatrix[i][leadX];
                    }
                    else
                    {
                        newmatx[ii][ri.mapCol] += _circuitMatrix[i][leadX];
                    }
                }
                ii++;
            }
            #endregion

            #region //// Copy matrix to orig ////
            _circuitMatrix = newmatx;
            _circuitRightSide = newrs;
            matrixSize = _circuitMatrixSize = newsize;

            // copy `rightSide` to `origRightSide`
            for (var i = 0; i != matrixSize; i++)
                _origRightSide[i] = _circuitRightSide[i];

            // copy `matrix` to `origMatrix`
            for (var i = 0; i != matrixSize; i++)
                for (var leadX = 0; leadX != matrixSize; leadX++)
                    _origMatrix[i][leadX] = _circuitMatrix[i][leadX];
            #endregion

            _circuitNeedsMap = true;
            _analyze = false;

            // If the matrix is linear, we can do the lu_factor 
            // here instead of needing to do it every frame.
            if (!_circuitNonLinear)
                if (!LuFactor(_circuitMatrix, _circuitMatrixSize, _circuitPermute))
                    Panic("Singular matrix!", null);
        }

        public void Panic(String why, ICircuitElement element)
        {
            _circuitMatrix = null;
            _analyze = true;
            throw new CircuitException(why, element);
        }

        #region //// Stamp ////
        // http://en.wikipedia.org/wiki/Electrical_element

        public void StampCurrentSource(int n1, int n2, double i)
        {
            StampRightSide(n1, -i);
            StampRightSide(n2, i);
        }
        /// <summary>
        /// Stamp independent voltage source #vs, from n1 to n2, amount v
        /// </summary>
        /// <param name="n1">The n1.</param>
        /// <param name="n2">The n2.</param>
        /// <param name="vs">The vs.</param>
        /// <param name="v">The v.</param>
        public void StampVoltageSource(int n1, int n2, int vs, double v)
        {
            var vn = _nodeList.Count + vs;
            StampMatrix(vn, n1, -1);
            StampMatrix(vn, n2, 1);
            StampRightSide(vn, v);
            StampMatrix(n1, vn, 1);
            StampMatrix(n2, vn, -1);
        }

        // use this if the amount of voltage is going to be updated in doStep()
        public void StampVoltageSource(int n1, int n2, int vs)
        {
            var vn = _nodeList.Count + vs;
            StampMatrix(vn, n1, -1);
            StampMatrix(vn, n2, 1);
            StampRightSide(vn);
            StampMatrix(n1, vn, 1);
            StampMatrix(n2, vn, -1);
        }

        public void StampResistor(int n1, int n2, double r)
        {
            var r0 = 1 / r;
            StampMatrix(n1, n1, r0);
            StampMatrix(n2, n2, r0);
            StampMatrix(n1, n2, -r0);
            StampMatrix(n2, n1, -r0);
        }

        public void StampConductance(int n1, int n2, double r0)
        {
            StampMatrix(n1, n1, r0);
            StampMatrix(n2, n2, r0);
            StampMatrix(n1, n2, -r0);
            StampMatrix(n2, n1, -r0);
        }

        /// <summary>
        /// Voltage-controlled voltage source. Control voltage source vs with voltage from n1 to n2 (must also call stampVoltageSource())
        /// </summary>
        public void StampVcvs(int n1, int n2, double coef, int vs)
        {
            var vn = _nodeList.Count + vs;
            StampMatrix(vn, n1, coef);
            StampMatrix(vn, n2, -coef);
        }

        /// <summary>
        /// Voltage-controlled current source. Current from cn1 to cn2 is equal to voltage from vn1 to vn2, divided by g 
        /// </summary>
        public void StampVccs(int cn1, int cn2, int vn1, int vn2, double g)
        {
            StampMatrix(cn1, vn1, g);
            StampMatrix(cn2, vn2, g);
            StampMatrix(cn1, vn2, -g);
            StampMatrix(cn2, vn1, -g);
        }

        // Current-controlled voltage source (CCVS)?

        /// <summary>
        /// Current-controlled current source. Stamp a current source from n1 to n2 depending on current through vs 
        /// </summary>
        public void StampCccs(int n1, int n2, int vs, double gain)
        {
            var vn = _nodeList.Count + vs;
            StampMatrix(n1, vn, gain);
            StampMatrix(n2, vn, -gain);
        }

        // stamp value x in row i, column j, meaning that a voltage change
        // of dv in node j will increase the current into node i by x dv
        // (Unless i or j is a voltage source node.)
        public void StampMatrix(int i, int j, double x)
        {
            if (i > 0 && j > 0)
            {
                if (_circuitNeedsMap)
                {
                    i = _circuitRowInfo[i - 1].mapRow;
                    var ri = _circuitRowInfo[j - 1];
                    if (ri.type == RowInfo.ROW_CONST)
                    {
                        _circuitRightSide[i] -= x * ri.value;
                        return;
                    }
                    j = ri.mapCol;
                }
                else
                {
                    i--;
                    j--;
                }
                _circuitMatrix[i][j] += x;
            }
        }

        // stamp value x on the right side of row i, representing an
        // independent current source flowing into node i
        public void StampRightSide(int i, double x)
        {
            if (i > 0)
            {
                i = (_circuitNeedsMap) ? _circuitRowInfo[i - 1].mapRow : i - 1;
                _circuitRightSide[i] += x;
            }
        }

        // indicate that the value on the right side of row i changes in doStep()
        public void StampRightSide(int i)
        {
            if (i > 0) _circuitRowInfo[i - 1].rsChanges = true;
        }

        // indicate that the values on the left side of row i change in doStep()
        public void StampNonLinear(int i)
        {
            if (i > 0) _circuitRowInfo[i - 1].lsChanges = true;
        }
        #endregion

        // Factors a matrix into upper and lower triangular matrices by
        // gaussian elimination. On entry, a[0..n-1][0..n-1] is the
        // matrix to be factored. ipvt[] returns an integer vector of pivot
        // indices, used in the lu_solve() routine.
        // http://en.wikipedia.org/wiki/Crout_matrix_decomposition
        public static bool LuFactor(double[][] a, int n, int[] ipvt)
        {
            int i, j;

            double[] scaleFactors = new double[n];

            // divide each row by its largest element, keeping track of the
            // scaling factors
            for (i = 0; i != n; i++)
            {
                double largest = 0;
                for (j = 0; j != n; j++)
                {
                    var x = Math.Abs(a[i][j]);
                    if (x > largest)
                        largest = x;
                }
                // if all zeros, it's a singular matrix
                if (Math.Abs(largest) < Epsilon)
                    return false;
                scaleFactors[i] = 1.0 / largest;
            }

            // use Crout's method; loop through the columns
            for (j = 0; j != n; j++)
            {

                // calculate upper triangular elements for this column
                int k;
                for (i = 0; i != j; i++)
                {
                    var q = a[i][j];
                    for (k = 0; k != i; k++)
                        q -= a[i][k] * a[k][j];
                    a[i][j] = q;
                }

                // calculate lower triangular elements for this column
                double largest = 0;
                var largestRow = -1;
                for (i = j; i != n; i++)
                {
                    var q = a[i][j];
                    for (k = 0; k != j; k++)
                        q -= a[i][k] * a[k][j];
                    a[i][j] = q;
                    var x = Math.Abs(q);
                    if (x >= largest)
                    {
                        largest = x;
                        largestRow = i;
                    }
                }

                // pivoting
                if (j != largestRow)
                {
                    for (k = 0; k != n; k++)
                    {
                        var x = a[largestRow][k];
                        a[largestRow][k] = a[j][k];
                        a[j][k] = x;
                    }
                    scaleFactors[largestRow] = scaleFactors[j];
                }

                // keep track of row interchanges
                ipvt[j] = largestRow;

                // avoid zeros
                if (Math.Abs(a[j][j]) < Epsilon)
                    a[j][j] = 1e-18;

                if (j != n - 1)
                {
                    var mult = 1.0 / a[j][j];
                    for (i = j + 1; i != n; i++)
                        a[i][j] *= mult;
                }
            }
            return true;
        }

        // Solves the set of n linear equations using a LU factorization
        // previously performed by lu_factor. On input, b[0..n-1] is the right
        // hand side of the equations, and on output, contains the solution.
        public static void LuSolve(double[][] a, int n, int[] ipvt, double[] b)
        {
            // find first nonzero b element
            int i;
            for (i = 0; i != n; i++)
            {
                var row = ipvt[i];
                var swap = b[row];
                b[row] = b[i];
                b[i] = swap;
                if (Math.Abs(swap) > Epsilon)
                    break;
            }

            var bi = i++;
            for (; i < n; i++)
            {
                var row = ipvt[i];
                var tot = b[row];

                b[row] = b[i];
                // forward substitution using the lower triangular matrix
                for (var j = bi; j < i; j++)
                    tot -= a[i][j] * b[j];

                b[i] = tot;
            }

            for (i = n - 1; i >= 0; i--)
            {
                var tot = b[i];

                // back-substitution using the upper triangular matrix
                for (var j = i + 1; j != n; j++)
                    tot -= a[i][j] * b[j];

                b[i] = tot / a[i][i];
            }
        }
    }
}
