using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeLeaves.Model;

namespace TreeLeaves
{
    class AzureManager
    {
        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<TreeLeavesModel> treeLeavesTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("https://treeleaves.azurewebsites.net");
            this.treeLeavesTable = this.client.GetTable<TreeLeavesModel>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task<List<TreeLeavesModel>> GetTreeLeavesInformation()
        {
            return await this.treeLeavesTable.ToListAsync();
        }

        public async Task PostTreeLeavesInformation(TreeLeavesModel treeLeavesModel)
        {
            await this.treeLeavesTable.InsertAsync(treeLeavesModel);
        }

        public async Task UpdateTreeLeavesInformation(TreeLeavesModel treeLeavesModel)
        {
            await this.treeLeavesTable.UpdateAsync(treeLeavesModel);
        }

        public async Task DeleteTreeLeavesInformation(TreeLeavesModel treeLeavesModel)
        {
            await this.treeLeavesTable.DeleteAsync(treeLeavesModel);
        }
    }
}
