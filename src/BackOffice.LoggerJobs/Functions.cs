using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace BackOffice.LoggerJobs
{
    public class Functions
    {
        private static readonly string StorageConnectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;

        public static async Task ProcessNewMediaUploadedMessageAsync([ServiceBusTrigger("NewMediaUploaded", "Logger")]BrokeredMessage message, TextWriter log)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();

            var logsTable = cloudTableClient.GetTableReference("Logs");
            await logsTable.CreateIfNotExistsAsync();

            var video = message.GetBody<Common.Video>();

            var logEntity = new LogEntity();
            logEntity.PartitionKey = "Medias";
            logEntity.RowKey = Guid.NewGuid().ToString();
            logEntity.Message = string.Format("Nouvelle vidéo uploadée : {0}", video.Title);

            TableOperation insertOperation = TableOperation.Insert(logEntity);
            await logsTable.ExecuteAsync(insertOperation);
        }

        public static async Task ProcessMediaJobStateChangedAsync([ServiceBusTrigger("MediaJobStateChanged", "Logger")]BrokeredMessage message, TextWriter log)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();

            var logsTable = cloudTableClient.GetTableReference("Logs");
            await logsTable.CreateIfNotExistsAsync();

            var jobStateChangedMessage = message.GetBody<Common.JobStateChangedMessage>();

            var logEntity = new LogEntity();
            logEntity.PartitionKey = "Medias";
            logEntity.RowKey = Guid.NewGuid().ToString();
            logEntity.Message = string.Format("Le job {0} est passé de l'état {1} à {2}", jobStateChangedMessage.JobId, jobStateChangedMessage.OldState, jobStateChangedMessage.NewState);

            TableOperation insertOperation = TableOperation.Insert(logEntity);
            await logsTable.ExecuteAsync(insertOperation);

            if(jobStateChangedMessage.StreamingInfo != null)
            { 
                var streamingInfoLogs = new LogEntity();
                streamingInfoLogs.PartitionKey = "Streaming";
                streamingInfoLogs.RowKey = Guid.NewGuid().ToString();
                streamingInfoLogs.Message = string.Format("La vidéo peut être visionnée en smooth streaming à l'URL : {0}", jobStateChangedMessage.StreamingInfo.SmoothStreamingUrl);

                TableOperation streamingLogInsertOperation = TableOperation.Insert(streamingInfoLogs);
                await logsTable.ExecuteAsync(streamingLogInsertOperation);
            }
        }
    }
}
