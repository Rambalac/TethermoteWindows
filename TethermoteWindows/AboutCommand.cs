﻿namespace Azi.TethermoteWindows
{
    using System;
    using Tools;

    public class AboutCommand : AbstractSimpleCommand
    {
        protected override async void InternalExecute()
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/Rambalac/TethermoteWindows"));
        }
    }
}