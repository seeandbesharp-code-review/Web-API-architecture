namespace DTO_s
{
    public record TransactionMessage(
        string TransactionId,
        int UserId,
        int OrderId,
        decimal Amount,
        string Currency,
        string Status,
        DateTime CreatedAtUtc
    );
}
