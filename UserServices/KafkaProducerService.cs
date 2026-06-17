using Confluent.Kafka;
using DTO_s;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Service
{
    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;
            string? bootstrapServers = configuration["Kafka:BootstrapServers"];
            _topic = configuration["Kafka:Topic"]
                ?? throw new InvalidOperationException("Kafka topic is not configured.");

            if (string.IsNullOrWhiteSpace(bootstrapServers))
            {
                throw new InvalidOperationException("Kafka bootstrap servers are not configured.");
            }

            ProducerConfig config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
            _logger.LogInformation("Kafka producer initialized for topic {Topic} on {BootstrapServers}", _topic, bootstrapServers);
        }

        public async Task SendTransactionAsync(TransactionMessage message, CancellationToken cancellationToken = default)
        {
            string payload = JsonSerializer.Serialize(message);

            try
            {
                DeliveryResult<Null, string> result = await _producer.ProduceAsync(
                    _topic,
                    new Message<Null, string> { Value = payload },
                    cancellationToken);

                _logger.LogInformation(
                    "Kafka message delivered. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, TransactionId: {TransactionId}",
                    result.Topic,
                    result.Partition.Value,
                    result.Offset.Value,
                    message.TransactionId);
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(ex, "Kafka delivery failed for transaction {TransactionId}", message.TransactionId);
                throw;
            }
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(5));
            _producer.Dispose();
            _logger.LogInformation("Kafka producer disposed for topic {Topic}", _topic);
        }
    }
}
