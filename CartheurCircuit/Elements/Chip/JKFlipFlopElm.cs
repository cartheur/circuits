namespace CartheurCircuit
{
    public class JKFlipFlopElm : Chip
    {
        private bool _hasReset;

        public Lead leadJ { get { return LeadZero; } }
        public Lead leadCLK { get { return LeadOne; } }
        public Lead leadK { get { return new Lead(this, 2); } }
        public Lead leadQ { get { return new Lead(this, 3); } }
        public Lead leadQL { get { return new Lead(this, 3); } }

        public bool HasResetPin
        {
            get
            {
                return _hasReset;
            }
            set
            {
                _hasReset = value;
                SetupPins();
                AllocateLeads();
            }
        }

        public override string GetChipName()
        {
            return "JK flip-flop";
        }

        public override void SetupPins()
        {
            pins = new Pin[GetLeadCount()];
            pins[0] = new Pin("J");

            pins[1] = new Pin("");
            pins[1].clock = true;

            pins[2] = new Pin("K");

            pins[3] = new Pin(" Q");
            pins[3].output = true;

            pins[4] = new Pin("|Q");
            pins[4].output = true;
            pins[4].lineOver = true;

            if (HasResetPin)
            {
                pins[5] = new Pin("R");
            }
        }

        public override int GetLeadCount()
        {
            return 5 + (HasResetPin ? 1 : 0);
        }

        public override int GetVoltageSourceCount()
        {
            return 2;
        }

        public override void Execute(Circuit sim)
        {
            if (!pins[1].value && lastClock)
            {
                bool q = pins[3].value;
                if (pins[0].value)
                {
                    if (pins[2].value)
                    {
                        q = !q;
                    }
                    else
                    {
                        q = true;
                    }
                }
                else if (pins[2].value)
                {
                    q = false;
                }
                pins[3].value = q;
                pins[4].value = !q;
            }
            lastClock = pins[1].value;

            if (HasResetPin)
            {
                if (pins[5].value)
                {
                    pins[3].value = false;
                    pins[4].value = true;
                }
            }
        }

    }
}
