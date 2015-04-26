using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;

namespace DemoTopicClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string sbConnectionString =
                "YOURSERVICEBUSCONNECTIONSTRING";

            string topicName = "VideoUploaded";

            var namespaceManager = NamespaceManager.CreateFromConnectionString(sbConnectionString);

            TopicDescription topicDescription = null;
            if (!namespaceManager.TopicExists(topicName))
            {
                topicDescription = namespaceManager.CreateTopic(topicName);
            }
            else
            {
                topicDescription = namespaceManager.GetTopic(topicName);
            }

            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(sbConnectionString);
            TopicClient topicClient = factory.CreateTopicClient(topicDescription.Path);

            while(true)
            {
                Console.WriteLine("Message à envoyer:");
                string value = Console.ReadLine();
                topicClient.Send(new BrokeredMessage(value));
            }
        }
    }
}
