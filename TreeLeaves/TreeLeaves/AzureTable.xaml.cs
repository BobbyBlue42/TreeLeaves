using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeLeaves.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TreeLeaves
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AzureTable : ContentPage
    {

        MobileServiceClient client = AzureManager.AzureManagerInstance.AzureClient;

        public AzureTable()
        {
            InitializeComponent();
        }

        private async void Handle_ClickedAsync(object sender, EventArgs e)
        {
            List<TreeLeavesModel> treeLeavesInformation = await AzureManager.AzureManagerInstance.GetTreeLeavesInformation();

            TreeLeavesList.ItemsSource = treeLeavesInformation;
        }
    }
}