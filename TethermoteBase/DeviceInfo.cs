﻿using Windows.Devices.Enumeration;

namespace Azi.TethermoteBase
{
    public sealed class DeviceInfo
    {
        public DeviceInformation Device { get; set; }

        public string Name { get; set; }

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