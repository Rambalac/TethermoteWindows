namespace Azi.TethermoteWindows
{
    using System.ComponentModel;
    using TethermoteBase;

    public sealed class Model : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool DisableOnAway
        {
            get => AppSettings.DisableOnUserNotPresent;
            set => AppSettings.DisableOnUserNotPresent = value;
        }

        public Donations Donations { get; } = new Donations();

        public bool EnableOnPresent
        {
            get => AppSettings.EnableOnUserPresent;
            set => AppSettings.EnableOnUserPresent = value;
        }
    }
}