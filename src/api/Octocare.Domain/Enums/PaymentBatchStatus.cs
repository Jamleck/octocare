namespace Octocare.Domain.Enums;

public static class PaymentBatchStatus
{
    public const string Draft = "draft";
    public const string Generated = "generated";
    public const string Sent = "sent";
    public const string Confirmed = "confirmed";

    public static readonly string[] ValidStatuses =
        [Draft, Generated, Sent, Confirmed];
}
