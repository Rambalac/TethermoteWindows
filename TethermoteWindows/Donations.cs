namespace Azi.TethermoteWindows
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Windows.Services.Store;

    public class Donations : INotifyPropertyChanged
    {
        private static StoreContext context;
        private bool canDonate = true;
        private bool inProgress;

        public event PropertyChangedEventHandler PropertyChanged;

        public static StoreContext Context => context ?? (context = StoreContext.GetDefault());

        public static ObservableCollection<FakeStoreProduct> DonationItems { get; } = new ObservableCollection<FakeStoreProduct>();

        public bool CanDonate
        {
            get => canDonate;

            set
            {
                if (value == canDonate)
                {
                    return;
                }

                canDonate = value;
                OnPropertyChanged();
            }
        }

        public bool InProgress
        {
            get => inProgress;

            set
            {
                if (value == inProgress)
                {
                    return;
                }

                inProgress = value;
                OnPropertyChanged();
            }
        }

        public class FakeStoreProduct
        {
            public string Title { get; set; } = "fgsdgf";
            public string StoreId { get; set; } = "3452354353";
        }

        public static async Task Init(string donationTag)
        {
            try
            {
                var prods = await Context.GetAssociatedStoreProductsAsync(new[] { "UnmanagedConsumable" });
                if (prods.ExtendedError != null)
                {
                    Debug.WriteLine(prods.ExtendedError);
                    return;
                }

                DonationItems.Add(new FakeStoreProduct());

                //var productsValues = prods.Products.Values.ToList();
                //foreach (var item in productsValues.Where(p => p.Keywords.Contains(donationTag)))
                //{
                //    DonationItems.Add(item);
                //}
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public async Task<StorePurchaseStatus> PurchaseAddOn(string storeId)
        {
            InProgress = true;
            try
            {
                var errorTries = 3;
                do
                {
                    var result = await Context.RequestPurchaseAsync(storeId);
                    switch (result.Status)
                    {
                        case StorePurchaseStatus.AlreadyPurchased:
                            await Consume(storeId);
                            break;

                        case StorePurchaseStatus.Succeeded:
                            await Consume(storeId);
                            return StorePurchaseStatus.Succeeded;

                        case StorePurchaseStatus.NotPurchased:
                            return StorePurchaseStatus.NotPurchased;

                        default:
                            Debug.WriteLine(result.ExtendedError);
                            break;
                    }
                }
                while (errorTries-- > 0);

                CanDonate = false;

                return StorePurchaseStatus.ServerError;
            }
            finally
            {
                InProgress = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task<bool> Consume(string storeId)
        {
            var result = await Context.ReportConsumableFulfillmentAsync(storeId, 1, Guid.NewGuid());

            if (result.Status == StoreConsumableStatus.Succeeded)
            {
                return true;
            }

            Debug.WriteLine(result.ExtendedError);
            return false;
        }
    }
}