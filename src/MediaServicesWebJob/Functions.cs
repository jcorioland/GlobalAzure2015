using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;
using System;

namespace MediaServicesWebJob
{
    public class Functions
    {
        public static void ProcessTopicMessage([ServiceBusTrigger("VideoUploaded", "MediaServices")]BrokeredMessage message)
        {
            string outputMessage = string.Format("A new message has been received from the MediaServies subscription: {0}", message.GetBody<string>());
            Console.WriteLine(outputMessage);
        }
    }
}
