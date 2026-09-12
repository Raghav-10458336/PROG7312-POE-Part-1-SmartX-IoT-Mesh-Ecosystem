namespace SmartX.Shared.Models
{
    public class PowerReading
    {
        public double Watts { get; set; }

        public PowerReading(double watts)
        {
            Watts = watts;
        }

        public static PowerReading operator +(PowerReading first, PowerReading second)
        {
            return new PowerReading(first.Watts + second.Watts);
        }

        public static PowerReading operator -(PowerReading first, PowerReading second)
        {
            return new PowerReading(first.Watts - second.Watts);
        }

        public static bool operator >(PowerReading first, PowerReading second)
        {
            return first.Watts > second.Watts;
        }

        public static bool operator <(PowerReading first, PowerReading second)
        {
            return first.Watts < second.Watts;
        }

        public override string ToString()
        {
            return $"{Watts:F2} W";
        }
    }
}
