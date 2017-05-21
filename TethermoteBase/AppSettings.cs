using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Azi.TethermoteBase
{
    public static class AppSettings
    {
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

        public static string RemoteDevice
        {
            get { return (string)Values[GetCallerName()]; }
            set { Values[GetCallerName()] = value; }
        }

        private static IPropertySet Values => ApplicationData.Current.LocalSettings.Values;

        private static string GetCallerName([CallerMemberName] string name = null) => name;
    }
}