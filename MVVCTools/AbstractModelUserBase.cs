using Windows.UI.Xaml;

namespace Azi.Mvvc
{
    public abstract class AbstractModelUserBase : DependencyObject
    {
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            "Model", typeof(object), typeof(AbstractModelUserBase), new PropertyMetadata(null, OnModelChanged));

        public static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AbstractModelUserBase)d).ModelChanged(e.OldValue, e.NewValue);
        }

        protected abstract void ModelChanged(object eOldValue, object eNewValue);
    }
}