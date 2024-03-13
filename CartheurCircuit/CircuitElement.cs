using System;

namespace CartheurCircuit
{
    public abstract class CircuitElement : ICircuitElement
    {
        protected int VoltageSource;
        protected double Current;
        protected int[] LeadNode;
        protected double[] VoltageLead;

        public readonly static double Pi = 3.14159265358979323846;
        /// <summary>
        /// Gets the zeroth lead.
        /// </summary>
        protected Lead LeadZero { get { return new Lead(this, 0); } }
        /// <summary>
        /// Gets the first lead.
        /// </summary>
        protected Lead LeadOne { get { return new Lead(this, 1); } }

        protected CircuitElement()
        {
            AllocateLeads();
        }

        public int GetBasicInfo(string[] arr)
        {
            arr[1] = "I = " + CircuitUtilities.GetCurrentText(Current).ToString();
            arr[2] = "Vd = " + CircuitUtilities.GetVoltageText(CircuitUtilities.GetVoltageDifference().ToString());
            return 3;
        }

        protected void AllocateLeads()
        {
            LeadNode = new int[GetLeadCount() + GetInternalLeadCount()];
            VoltageLead = new double[GetLeadCount() + GetInternalLeadCount()];
        }

        public virtual void CalculateCurrent() { }
        public virtual double GetPower() { return CircuitUtilities.GetVoltageDifference() * Current; }
        public virtual void GetInfo(string[] arr) { }

        #region //// Interface ////

        public int GetLeadNode(int leadNdx)
        {
            if (LeadNode == null) AllocateLeads();
            return LeadNode[leadNdx];
        }

        public void SetLeadNode(int leadNdx, int nodeNdx)
        {
            if (LeadNode == null) AllocateLeads();
            LeadNode[leadNdx] = nodeNdx;
        }

        public virtual void BeginStep(Circuit simulation) { }
        public virtual void Step(Circuit simulation) { }
        public virtual void Stamp(Circuit simulation) { }

        public virtual void Reset()
        {
            for (int i = 0; i != GetLeadCount() + GetInternalLeadCount(); i++)
                VoltageLead[i] = 0;
        }

        #region //// Lead count ////
        public virtual int GetLeadCount() { return 2; }
        public virtual int GetInternalLeadCount() { return 0; }
        #endregion

        #region //// Lead voltage ////
        public virtual double GetLeadVoltage(int leadX)
        {
            return VoltageLead[leadX];
        }

        public virtual void SetLeadVoltage(int leadX, double vValue)
        {
            VoltageLead[leadX] = vValue;
            CalculateCurrent();
        }
        #endregion

        #region //// Current ////
        public virtual double GetCurrent() { return Current; }

        public virtual void SetCurrent(int voltageSourceNdx, double c) { Current = c; }
        #endregion

        #region //// Voltage ////
        public virtual double GetVoltageDelta() { return VoltageLead[0] - VoltageLead[1]; }

        public virtual int GetVoltageSourceCount() { return 0; }

        public virtual void SetVoltageSource(int leadX, int voltageSourceNdx) { VoltageSource = voltageSourceNdx; }
        #endregion

        #region //// Connection ////
        public virtual bool LeadsAreConnected(int leadX, int leadY) { return true; }

        public virtual bool LeadIsGround(int leadX) { return false; }
        #endregion

        #region //// State ////
        public virtual bool IsWire() { return false; }
        public virtual bool NonLinear() { return false; }
        #endregion

        #endregion

        protected static bool ComparePair(int x1, int x2, int y1, int y2)
        {
            return ((x1 == y1 && x2 == y2) || (x1 == y2 && x2 == y1));
        }
    }
}
