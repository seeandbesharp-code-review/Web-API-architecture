using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace KafkaConsumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly KafkaOptions _kafkaOptions;

    public Worker(ILogger<Worker> logger, IOptions<KafkaOptions> kafkaOptions)
    {
        _logger = logger;
        _kafkaOptions = kafkaOptions.Value;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_kafkaOptions.BootstrapServers) ||
            string.IsNullOrWhiteSpace(_kafkaOptions.Topic) ||
            string.IsNullOrWhiteSpace(_kafkaOptions.GroupId))
        {
            throw new InvalidOperationException("Kafka consumer settings are missing in appsettings.json.");
        }

        return Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
    }

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        ConsumerConfig config = new()
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            GroupId = _kafkaOptions.GroupId,
            AutoOffsetReset = ParseOffsetReset(_kafkaOptions.AutoOffsetReset),
            EnableAutoCommit = true
        };

        using IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_kafkaOptions.Topic);

        _logger.LogInformation(
            "Kafka consumer started. Topic: {Topic}, GroupId: {GroupId}, BootstrapServers: {BootstrapServers}",
            _kafkaOptions.Topic,
            _kafkaOptions.GroupId,
            _kafkaOptions.BootstrapServers);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    ConsumeResult<Ignore, string> result = consumer.Consume(stoppingToken);
                    _logger.LogInformation(
                        "Received Kafka message. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, Value: {Value}",
                        result.Topic,
                        result.Partition.Value,
                        result.Offset.Value,
                        result.Message.Value);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer cancellation requested.");
        }
        finally
        {
            consumer.Close();
            _logger.LogInformation("Kafka consumer stopped.");
        }
    }

    private static AutoOffsetReset ParseOffsetReset(string autoOffsetReset)
    {
        return autoOffsetReset.Equals("Latest", StringComparison.OrdinalIgnoreCase)
            ? AutoOffsetReset.Latest
            : AutoOffsetReset.Earliest;
    }
}
