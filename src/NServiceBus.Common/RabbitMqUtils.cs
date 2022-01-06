using RabbitMQ.Client;

namespace NServiceBus.Common
{
    public static class QueueCreationUtils
    {
        private static void CreateQueue(string uri, string queueName, bool durableMessages, bool createExchange)
        {
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(uri)
            };

            using var connection = connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(
                queue: queueName,
                durable: durableMessages,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            if (createExchange)
            {
                CreateExchange(channel, queueName, durableMessages);
            }
        }

        static void CreateExchange(IModel channel, string queueName, bool durableMessages)
        {
            channel.ExchangeDeclare(queueName, ExchangeType.Fanout, durableMessages);
            channel.QueueBind(queueName, queueName, string.Empty);
        }

        public static void CreateQueuesForEndpoint(string uri, string endpointName, bool durableMessages, bool createExchanges)
        {
            // main queue
            QueueCreationUtils.CreateQueue(uri, endpointName, durableMessages, createExchanges);

            // callback queue
            // QueueCreationUtils.CreateQueue(uri, $"{endpointName}.{Environment.MachineName}", durableMessages, createExchanges);

            // timeout queue - not used if using native timeouts
            // QueueCreationUtils.CreateQueue(uri, $"{endpointName}.Timeouts", durableMessages, createExchanges);

            // timeout dispatcher queue - not used if using native timeouts
            // QueueCreationUtils.CreateQueue(uri, $"{endpointName}.TimeoutsDispatcher", durableMessages, createExchanges);

            // retries queue
            // TODO: Only required in Versions 3 and below
            //QueueCreationUtils.CreateQueue(uri, $"{endpointName}.Retries", durableMessages, createExchanges);

        }
    }
}
