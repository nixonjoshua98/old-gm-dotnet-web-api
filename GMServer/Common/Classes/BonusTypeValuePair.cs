namespace GMServer.Common.Classes
{
    public class BonusTypeValuePair
    {
        public BonusType BonusType;
        public double Value;

        private BonusTypeValuePair() { }

        public BonusTypeValuePair(BonusType bonus, double val)
        {
            BonusType = bonus;
            Value = val;
        }
    }
}
