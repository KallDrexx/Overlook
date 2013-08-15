namespace Overlook.Common.Data
{
    public class Metric
    {
        /// <summary>
        /// Name of the device the metric is for
        /// </summary>
        public string Device { get; private set; }

        /// <summary>
        /// Category for the specific metric
        /// </summary>
        public string Category { get; private set; }

        /// <summary>
        /// Name of the metric
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Suffix displayed with the metric data (for display purposes only)
        /// </summary>
        public string SuffixLabel { get; private set; }

        public Metric(string device, string category, string name, string suffixLabel)
        {
            Device = device;
            Category = category;
            Name = name;
            SuffixLabel = suffixLabel;
        }

        public override string ToString()
        {
            return string.Format("Metric: {0} {1} {2}", Device, Category, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((Metric) obj);
        }

        protected bool Equals(Metric other)
        {
            return string.Equals(Device, other.Device) && string.Equals(Category, other.Category) && string.Equals(Name, other.Name) && string.Equals(SuffixLabel, other.SuffixLabel);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Device != null ? Device.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Category != null ? Category.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SuffixLabel != null ? SuffixLabel.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
