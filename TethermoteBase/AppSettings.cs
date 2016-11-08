using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Azi.TethermoteBase
{
    public static class AppSettings
    {
        private static IPropertySet Values => ApplicationData.Current.LocalSettings.Values;

        private static string GetCallerName([CallerMemberName] string name = null) => name;

        public static string RemoteDevice
        {
            get { return (string)Values[GetCallerName()]; }
            set { Values[GetCallerName()] = value; }
        }
        public static bool DisableOnUserNotPresent
        {
            get { return (bool)(Values[GetCallerName()] ?? false); }
            set { Values[GetCallerName()] = value; }
        }
        public static bool EnableOnUserPresent
        {
            get { return (bool)(Values[GetCallerName()] ?? false); }
            set { Values[GetCallerName()] = value; }
        }
    }
}
