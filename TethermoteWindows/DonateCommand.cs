namespace Azi.TethermoteWindows
{
    using Mvvc;
    using System;
    using System.Threading.Tasks;
    using Windows.Services.Store;
    using Windows.UI.Popups;

    public class DonateCommand : AbstractParameterModelCommand<Model, string>
    {
        protected override bool InternalCanExecute(string param) => true;

        protected override async Task InternalExecute(string parameter)
        {
            switch (await Model.Donations.PurchaseAddOn(parameter))
            {
                case StorePurchaseStatus.Succeeded:
                    await new MessageDialog(App.GetString("Donate_Thankyou")).ShowAsync();
                    return;

                case StorePurchaseStatus.NotPurchased:
                    return;

                default:
                    await new MessageDialog(App.GetString("Donate_Error")).ShowAsync();
                    return;
            }
        }
    }
}