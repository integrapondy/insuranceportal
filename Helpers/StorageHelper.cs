using InsuranceClientPortal.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceClientPortal.Helpers
{
    /// <summary>
    /// Contains the methods for adding customer data into Table and image to blob storage.
    /// </summary>
    public class StorageHelper
    {
        private CloudStorageAccount storageAccount;
        private CloudBlobClient blobClient;
        private CloudTableClient tableClient;
        private CloudQueueClient queueClient;

        public String StorageConnectionString
        {
            set
            {
                this.storageAccount = CloudStorageAccount.Parse(value);
                this.blobClient = storageAccount.CreateCloudBlobClient();
                //this.tableClient = storageAccount.CreateCloudTableClient();
                this.queueClient = storageAccount.CreateCloudQueueClient();
            }
        }

        public String TableConnectionString
        {
            set
            {
                var sa = CloudStorageAccount.Parse(value);
                this.tableClient = storageAccount.CreateCloudTableClient();
            }
        }

        public async Task<CloudBlobContainer> CreateContainerIfNotExistsAsync(String containerName)
        {
            var container = blobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
            return container;
        }

        public async Task<string> UploadFileAsync(string imagePath, string containerName)
        {
            var fileName = Path.GetFileName(imagePath);
            var container = await CreateContainerIfNotExistsAsync(containerName);
            var blob = container.GetBlockBlobReference(fileName);
            await blob.UploadFromFileAsync(imagePath);
            return blob.Uri.AbsolutePath;
        }

        private async Task<CloudTable> CreateTableIfNotExistsAsync(string tableName)
        {
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        public async Task<Customer> SaveInsuranceDetailsAsync(Customer customer, string tableName)
        {
            TableOperation tableOperation = TableOperation.InsertOrMerge(customer);
            var table = await CreateTableIfNotExistsAsync(tableName);
            TableResult entity = await table.ExecuteAsync(tableOperation);
            return entity.Result as Customer;
        }

        public async Task<CloudQueue> CreateQueueIfNotExistsAsync(String queueName)
        {
            var queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            return queue;
        }

        public async Task<bool> SendMessageAsync(string messageText, string queueName)
        {
            var queue = await CreateQueueIfNotExistsAsync(queueName);

            CloudQueueMessage message = new CloudQueueMessage(messageText);
            await queue.AddMessageAsync(message, TimeSpan.FromMinutes(30), TimeSpan.Zero, null, null);
            return true;
        }
    }
}
