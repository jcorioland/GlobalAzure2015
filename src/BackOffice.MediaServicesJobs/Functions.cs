using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace BackOffice.MediaServicesJobs
{
    public class Functions
    {
        private static readonly string MediaServiceName =
            ConfigurationManager.AppSettings["MediaServiceName"];

        private static readonly string MediaServiceKey =
            ConfigurationManager.AppSettings["MediaServiceKey"];

        private static readonly string StorageConnectionString =
            ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;

        private static readonly string ServiceBusConnectionString =
            ConfigurationManager.ConnectionStrings["AzureWebJobsServiceBus"].ConnectionString;

        public static async Task ProcessNewMediaUploadedMessageAsync([ServiceBusTrigger("NewMediaUploaded", "MediaServices")]BrokeredMessage message, TextWriter log)
        {
            var video = message.GetBody<Common.Video>();
            var mediaServicesWrapper = new MediaServicesWrapper(MediaServiceName, MediaServiceKey, StorageConnectionString);

            var mediaServiceAsset = await mediaServicesWrapper.CreateMediaServiceAssetFromExistingBlobAsync(video.OriginalUrl);
            var job = await mediaServicesWrapper.CreateJobAsync(mediaServiceAsset);

            job.StateChanged += OnJobStateChanged;

            var jobTask = job.GetExecutionProgressTask(CancellationToken.None);
            await jobTask;
        }

        private static async void OnJobStateChanged(object sender, Microsoft.WindowsAzure.MediaServices.Client.JobStateChangedEventArgs e)
        {
            NamespaceManager nsMgr = NamespaceManager.CreateFromConnectionString(ServiceBusConnectionString);
            TopicDescription topic = null;
            if (!(await nsMgr.TopicExistsAsync("MediaJobStateChanged")))
            {
                topic = await nsMgr.CreateTopicAsync("MediaJobStateChanged");
            }
            else
            {
                topic = await nsMgr.GetTopicAsync("MediaJobStateChanged");
            }

            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(ServiceBusConnectionString);
            TopicClient topicClient = factory.CreateTopicClient(topic.Path);

            var job = ((IJob)sender);

            Common.AdaptiveStreamingInfo streamingInfo = null;
            if(job.State == JobState.Finished)
            {
                var mediaServicesWrapper = new MediaServicesWrapper(MediaServiceName, MediaServiceKey, StorageConnectionString);
                streamingInfo = await mediaServicesWrapper.PrepareAssetsForAdaptiveStreamingAsync(job.Id);
            }

            var jobStateChangedMessage = new Common.JobStateChangedMessage
            {
                JobId = job.Id,
                NewState = e.CurrentState.ToString(),
                OldState = e.PreviousState.ToString(),
                StreamingInfo = streamingInfo
            };

            await topicClient.SendAsync(new BrokeredMessage(jobStateChangedMessage));
        }
    }
}
