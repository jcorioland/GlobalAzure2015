using BackOffice.Models;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace BackOffice.Controllers
{
    public class UploadController : Controller
    {
        private readonly string _azureStorageConnectionString =
            WebConfigurationManager.ConnectionStrings["AzureStorage"].ConnectionString;

        private readonly string _azureServiceBusConnectionString =
            WebConfigurationManager.ConnectionStrings["AzureServiceBus"].ConnectionString;

        // GET: Upload
        public ActionResult Index()
        {
            var model = new VideoModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(VideoModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cloudStorageAccount = CloudStorageAccount.Parse(_azureStorageConnectionString);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var container = cloudBlobClient.GetContainerReference("uploads");
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlockBlobReference(System.IO.Path.GetFileName(model.File.FileName));
            await blob.UploadFromStreamAsync(model.File.InputStream);

            var newVideo = new BackOffice.Common.Video
            {
                Description = model.Description,
                Id = model.Id,
                OriginalUrl = blob.Uri.ToString(),
                Title = model.Name,
                UploadedDate = DateTime.UtcNow
            };

            NamespaceManager nsMgr = NamespaceManager.CreateFromConnectionString(_azureServiceBusConnectionString);
            TopicDescription topic = null;
            if(!(await nsMgr.TopicExistsAsync("NewMediaUploaded")))
            {
                topic = await nsMgr.CreateTopicAsync("NewMediaUploaded");
            }
            else
            {
                topic = await nsMgr.GetTopicAsync("NewMediaUploaded");
            }

            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(_azureServiceBusConnectionString);
            TopicClient topicClient = factory.CreateTopicClient(topic.Path);

            await topicClient.SendAsync(new BrokeredMessage(newVideo));

            return RedirectToAction("Index", "Home");
        }
    }
}