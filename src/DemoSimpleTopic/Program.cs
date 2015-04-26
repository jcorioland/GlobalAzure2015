using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading.Tasks;

namespace DemoSimpleTopic
{
    class Program
    {
        static void Main(string[] args)
        {
            string sbConnectionString =
                "YOURSERVICEBUSCONNECTIONSTRING";

            string topicName = "SimpleTopic";

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

            CreateSubscription(namespaceManager, factory, topicName, "Subscription1");
            CreateSubscription(namespaceManager, factory, topicName, "Subscription2");
            //CreateSubscription(namespaceManager, factory, topicName, "Subscription2");

            Console.WriteLine("Message à envoyer : ");
            string message = Console.ReadLine();
            while(message != "exit")
            {
                topicClient.Send(new BrokeredMessage(message));

                Console.WriteLine("Message à envoyer : ");
                message = Console.ReadLine();
            }

            topicClient.Close();
        }

        private static void CreateSubscription(NamespaceManager namespaceManager, MessagingFactory messagingFactory, string topicName, string subscriptionName)
        {
            Guid subscriberId = Guid.NewGuid();

            TopicDescription topicDescription = namespaceManager.GetTopic(topicName);

            SubscriptionDescription subscription = null;
            if(!namespaceManager.SubscriptionExists(topicDescription.Path, subscriptionName))
            {
                subscription = namespaceManager.CreateSubscription(topicDescription.Path, subscriptionName);
            }
            else
            {
                subscription = namespaceManager.GetSubscription(topicDescription.Path, subscriptionName);
            }

            Task.Factory.StartNew(() => {
                SubscriptionClient subscriptionClient = messagingFactory.CreateSubscriptionClient(topicDescription.Path, subscription.Name);

                BrokeredMessage message = null;
                while ((message = subscriptionClient.Receive(TimeSpan.FromSeconds(30))) != null)
                {
                    Console.WriteLine("Subscriber {0} - Receiving message from subscription '{1}': {2}", subscriberId, subscriptionName, message.GetBody<string>());
                    message.Complete();
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
