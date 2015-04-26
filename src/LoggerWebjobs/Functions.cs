using System;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;

namespace LoggerWebjobs
{
    public class Functions
    {
        public static void ProcessTopicMessage([ServiceBusTrigger("VideoUploaded", "Logger")]BrokeredMessage message)
        {
            string outputMessage = string.Format("A new message has been received from the Logger subscription: {0}", message.GetBody<string>());
            Console.WriteLine(outputMessage);
        }
    }
}
