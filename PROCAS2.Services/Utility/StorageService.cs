﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

using PROCAS2.Data.AzureTable;

namespace PROCAS2.Services.Utility
{
    public class StorageService : IStorageService
    {
        private IConfigService _configService;

        public StorageService(IConfigService configService)
        {
            _configService = configService;
        }

        /// <summary>
        /// Create and store the consent PDF in Azure storage
        /// </summary>
        /// <param name="PDF">Base 64 encoded PDF</param>
        /// <param name="filename">File name to save the consent form as</param>
        /// <returns>true if successful, else false</returns>
        public bool ProcessConsentPDF(string PDF, string filename)
        {
            try
            {
                byte[] sPDFDecoded = Convert.FromBase64String(PDF);

                MemoryStream stream = new MemoryStream(sPDFDecoded);

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configService.GetConnectionString("CollatorPrimaryStorage"));
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_configService.GetAppSetting("StorageConsentContainer"));

                var blob = container.GetBlockBlobReference(filename);
                blob.UploadFromStreamAsync(stream).Wait();
                stream.Dispose();

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create and store the CRA message in Azure storage
        /// </summary>
        /// <param name="message">HL7 message</param>
        /// <param name="filename">File name to save the message as</param>
        /// <returns>true if successful, else false</returns>
        public bool StoreCRAMessage(string message, string filename)
        {
            try
            {
                byte[] sPDFDecoded = Encoding.ASCII.GetBytes(message);

                MemoryStream stream = new MemoryStream(sPDFDecoded);

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configService.GetConnectionString("CollatorPrimaryStorage"));
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_configService.GetAppSetting("StorageCRAMessageContainer"));

                var blob = container.GetBlockBlobReference(filename);
                blob.UploadFromStreamAsync(stream).Wait();
                stream.Dispose();

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create and store the Volpara message in Azure storage
        /// </summary>
        /// <param name="message">JSON message</param>
        /// <param name="filename">File name to save the message as</param>
        /// <returns>true if successful, else false</returns>
        public bool StoreVolparaMessage(string message, string filename)
        {
            try
            {
                byte[] sPDFDecoded = Encoding.ASCII.GetBytes(message);

                MemoryStream stream = new MemoryStream(sPDFDecoded);

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configService.GetConnectionString("CollatorPrimaryStorage"));
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_configService.GetAppSetting("StorageVolparaMessageContainer"));

                var blob = container.GetBlockBlobReference(filename);
                blob.UploadFromStreamAsync(stream).Wait();
                stream.Dispose();

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Store the passed invite message in the Volpara invited Azure table.
        /// </summary>
        /// <param name="message">A JSON message containing the hashed ID</param>
        /// <returns>true if successful at saving the message, else false</returns>
        public bool StoreInviteMessage(string studyNumber, string hashedNHSNumber)
        {
            try
            {
                // Parse the connection string and return a reference to the storage account.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    _configService.GetConnectionString("CollatorPrimaryStorage"));

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                // Retrieve a reference to the table.
                CloudTable table = tableClient.GetTableReference(_configService.GetAppSetting("VolparaInvitedTable"));

                // Create the table if it doesn't exist.
                table.CreateIfNotExists();

                // Create a retrieve operation that takes a customer entity.
                TableOperation retrieveOperation = TableOperation.Retrieve<Invited>("invited", studyNumber);

                // Execute the retrieve operation.
                TableResult retrievedResult = table.Execute(retrieveOperation);

                // Print the phone number of the result.
                if (retrievedResult.Result == null)
                {
                    Invited invited = new Invited(studyNumber, "invited");
                    invited.HashedPatientId = hashedNHSNumber;

                    // Create the TableOperation object that inserts the customer entity.
                    TableOperation insertOperation = TableOperation.Insert(invited);

                    // Execute the insert operation.
                    table.Execute(insertOperation);
                }

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }

}
