namespace Azi.TethermoteWindows
{
    using System;
    using System.Collections;
    using Windows.ApplicationModel;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class BindableFlyout : DependencyObject
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.RegisterAttached(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(BindableFlyout),
                new PropertyMetadata(null, ItemsSourceChanged));

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.RegisterAttached(
                "ItemTemplate",
                typeof(DataTemplate),
                typeof(BindableFlyout),
                new PropertyMetadata(null, ItemsTemplateChanged));

        public static IEnumerable GetItemsSource(DependencyObject obj) => obj.GetValue(ItemsSourceProperty) as IEnumerable;

        public static DataTemplate GetItemTemplate(DependencyObject obj) => (DataTemplate)obj.GetValue(ItemTemplateProperty);

        public static void SetItemsSource(DependencyObject obj, IEnumerable value) => obj.SetValue(ItemsSourceProperty, value);

        public static void SetItemTemplate(DependencyObject obj, DataTemplate value) => obj.SetValue(ItemTemplateProperty, value);

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => Setup(d as Flyout);

        private static void ItemsTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => Setup(d as Flyout);

        private static async void Setup(Flyout m)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return;
            }

            var s = m.GetValue(ItemsSourceProperty);
            if (s == null)
            {
                return;
            }

            var t = GetItemTemplate(m);
            if (t == null)
            {
                return;
            }

            var c = new ItemsControl
            {
                ItemsSource = s,
                ItemTemplate = t
            };

            await m.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => m.Content = c);
        }
    }
}