using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

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
    }

}
