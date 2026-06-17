using DTO_s;

namespace Service
{
    public interface IKafkaProducerService
    {
        Task SendTransactionAsync(TransactionMessage message, CancellationToken cancellationToken = default);
    }
}
