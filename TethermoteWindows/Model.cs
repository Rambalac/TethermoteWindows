using Azi.TethermoteBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azi.TethermoteWindows
{
    public sealed class Model
    {
        public bool EnableOnPresent
        {
            get
            {
                return AppSettings.EnableOnUserPresent;
            }
            set
            {
                AppSettings.EnableOnUserPresent = value;
            }
        }
        public bool DisableOnAway
        {
            get
            {
                return AppSettings.DisableOnUserNotPresent;
            }
            set
            {
                AppSettings.DisableOnUserNotPresent = value;
            }
        }
    }
}
