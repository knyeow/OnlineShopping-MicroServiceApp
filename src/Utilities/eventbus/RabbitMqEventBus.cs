using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;

namespace PrivateUtilities.EventBus
{
    public class RabbitMQEventBus : IEventBus, IDisposable

    {
        private readonly IConnection _connection;
        private readonly IModel? _channel;

        public RabbitMQEventBus(string hostname = "localhost")
        {
            var factory = new ConnectionFactory() { HostName = hostname };
            int retryCount = 5;
                for (int i = 0; i < retryCount; i++)
                {
                    try
                    {
                        _connection = factory.CreateConnection();
                        _channel = _connection.CreateModel();
                        break;
                    }
                    catch (BrokerUnreachableException ex) when (i < retryCount - 1)
                    {
                        Console.WriteLine($"RabbitMQ bağlantı denemesi başarısız ({i + 1}/{retryCount}), tekrar deneniyor... Hata: {ex.Message}");
                        Thread.Sleep(3000); // 3 saniye bekle
                    }
                }

                if (_connection == null)
                    throw new Exception("RabbitMQ bağlantısı kurulamadı.");
            
        }

        public void Publish(EventBase @event)
        {
            var queueName = @event.GetType().Name;

            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false);

            var message = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            });
            var body = Encoding.UTF8.GetBytes(message);

            Console.WriteLine($"[Event Published] Event: {message}");
            Console.WriteLine($"[Event Published] Event Type: {@event.GetType().Name}");
            Console.WriteLine($"[Debug] JSON Message: {message}");
            Console.WriteLine($"[Debug] Byte Array Length: {body.Length}");

            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);

            
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : EventBase
        {
            var queueName = typeof(TEvent).Name;
            

            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (_, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                Console.WriteLine($"[Event Received] Event: {json}");

                var @event = JsonSerializer.Deserialize<TEvent>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (@event == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize event of type {typeof(TEvent).Name}.");
                }
                Console.WriteLine($"[Event Received] @event: {@event.ToString()}");

                handler(@event);
            };

            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            
        }
        public void Publish<T>(T @event) where T : class
        {
            var eventName = typeof(T).Name;
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
            _channel.BasicPublish(exchange: "", routingKey: eventName, body: body);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }

    }
}
