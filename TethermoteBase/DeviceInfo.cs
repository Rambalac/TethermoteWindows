using Windows.Devices.Enumeration;

namespace Azi.TethermoteBase
{
    sealed public class DeviceInfo
    {
        public string Name { get; set; }
        public DeviceInformation Device { get; set; }
        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Name == ((DeviceInfo)obj).Name; ;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}