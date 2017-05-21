using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;

namespace Azi.TethermoteBase
{
    public sealed class Tile
    {
        private const string DisableSwitchArgument = "disable";
        private const string EnableSwitchArgument = "enable";
        private const string SwitchTileId = "SwitchTile";

        private static readonly Uri logoOff = new Uri("ms-appx:///Assets/widget_off.png");
        private static readonly Uri logoOn = new Uri("ms-appx:///Assets/widget_on.png");
        public static bool Exists => SecondaryTile.Exists(SwitchTileId);

        public static IAsyncAction AddSwitchTile(FrameworkElement sender, bool enabled)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            var logo = enabled ? logoOn : logoOff;
            var deviceName = AppSettings.RemoteDevice;
            var tileText = string.Format(loader.GetString(enabled ? "Tile_Tooltip_ToDisable" : "Tile_Tooltip_ToEnable"), deviceName);
            var s = new SecondaryTile(SwitchTileId,
                            tileText,
                            !enabled ? EnableSwitchArgument : DisableSwitchArgument,
                            logo, TileSize.Square150x150);

            // Specify a foreground text value.
            return AsyncInfo.Run(async (cancel) => { await s.RequestCreateForSelectionAsync(GetElementRect(sender), Placement.Below); });
        }

        public static IAsyncAction UpdateTile(TetheringState state)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            var enabled = state == TetheringState.Enabled;
            var logo = enabled ? logoOn : logoOff;
            var deviceName = AppSettings.RemoteDevice;
            var tileText = string.Format(loader.GetString(enabled ? "Tile_Tooltip_ToDisable" : "Tile_Tooltip_ToEnable"), deviceName);
            var s = new SecondaryTile(SwitchTileId,
                                tileText,
                                !enabled ? EnableSwitchArgument : DisableSwitchArgument,
                                logo, TileSize.Square150x150);
            // Specify a foreground text value.
            return AsyncInfo.Run(async (cancel) =>
            {
                try
                {
                    await s.UpdateAsync();
                }
                catch (Exception)
                {
                    // Ignore
                }
            });
        }

        private static Rect GetElementRect(FrameworkElement element)
        {
            var rectangleBounds = new Rect();
            rectangleBounds = element.RenderTransform.TransformBounds(new Rect(0, 0, element.Width, element.Height));
            return rectangleBounds;
        }
    }
}