namespace KafkaConsumer;

public class KafkaOptions
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string AutoOffsetReset { get; set; } = "Earliest";
}
