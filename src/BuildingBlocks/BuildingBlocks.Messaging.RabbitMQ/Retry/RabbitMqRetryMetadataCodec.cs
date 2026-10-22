using System.Text;

namespace BuildingBlocks.Messaging.RabbitMQ.Retry;

public static class RabbitMqRetryMetadataCodec
{
    public static void Write(
        IDictionary<string, object?> headers,
        RabbitMqRetryMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(metadata);

        headers[RabbitMqRetryHeaders.RetryAttempt] =
            metadata.RetryAttempt;

        headers[RabbitMqRetryHeaders.OriginalExchange] =
            metadata.OriginalExchange;

        headers[RabbitMqRetryHeaders.OriginalRoutingKey] =
            metadata.OriginalRoutingKey;

        headers[RabbitMqRetryHeaders.FirstFailedAtUnixTime] =
            new DateTimeOffset(metadata.FirstFailedOnUtc)
                .ToUnixTimeSeconds();
    }
    public static bool TryRead(
        IDictionary<string, object?>? headers,
        out RabbitMqRetryMetadata? metadata)
    {
        if (headers is null ||
            !ContainsAnyRetryHeader(headers))
        {
            metadata = null;
            return false;
        }

        metadata = Read(headers);
        return true;
    }

    public static RabbitMqRetryMetadata Read(
        IDictionary<string, object?> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        int retryAttempt = ReadInt32(
            headers,
            RabbitMqRetryHeaders.RetryAttempt);

        string originalExchange = ReadString(
            headers,
            RabbitMqRetryHeaders.OriginalExchange);

        string originalRoutingKey = ReadString(
            headers,
            RabbitMqRetryHeaders.OriginalRoutingKey);

        long firstFailedAtUnixTime = ReadInt64(
            headers,
            RabbitMqRetryHeaders.FirstFailedAtUnixTime);

        if (retryAttempt < 0)
        {
            throw new InvalidDataException(
                $"RabbitMQ header '{RabbitMqRetryHeaders.RetryAttempt}' " +
                "cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(originalRoutingKey))
        {
            throw new InvalidDataException(
                $"RabbitMQ header '{RabbitMqRetryHeaders.OriginalRoutingKey}' " +
                "cannot be empty.");
        }

        DateTime firstFailedOnUtc;

        try
        {
            firstFailedOnUtc = DateTimeOffset
                .FromUnixTimeSeconds(firstFailedAtUnixTime)
                .UtcDateTime;
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new InvalidDataException(
                $"RabbitMQ header '{RabbitMqRetryHeaders.FirstFailedAtUnixTime}' " +
                "contains an invalid Unix timestamp.",
                exception);
        }

        return new RabbitMqRetryMetadata(
            retryAttempt,
            originalExchange,
            originalRoutingKey,
            firstFailedOnUtc);
    }


    private static bool ContainsAnyRetryHeader(
        IDictionary<string, object?> headers)
    {
        return
            headers.ContainsKey(
                RabbitMqRetryHeaders.RetryAttempt) ||

            headers.ContainsKey(
                RabbitMqRetryHeaders.OriginalExchange) ||

            headers.ContainsKey(
                RabbitMqRetryHeaders.OriginalRoutingKey) ||

            headers.ContainsKey(
                RabbitMqRetryHeaders.FirstFailedAtUnixTime);
    }
    private static string ReadString(
        IDictionary<string, object?> headers,
        string key)
    {
        object value = GetRequiredValue(headers, key);

        return value switch
        {
            string text => text,

            byte[] bytes =>
                Encoding.UTF8.GetString(bytes),

            ReadOnlyMemory<byte> memory =>
                Encoding.UTF8.GetString(memory.Span),

            _ => throw CreateInvalidTypeException(key, value)
        };
    }

    private static int ReadInt32(
        IDictionary<string, object?> headers,
        string key)
    {
        object value = GetRequiredValue(headers, key);

        return value switch
        {
            byte number => number,
            short number => number,
            int number => number,

            long number
                when number is >= int.MinValue and <= int.MaxValue =>
                (int)number,

            _ => throw CreateInvalidTypeException(key, value)
        };
    }

    private static long ReadInt64(
        IDictionary<string, object?> headers,
        string key)
    {
        object value = GetRequiredValue(headers, key);

        return value switch
        {
            byte number => number,
            short number => number,
            int number => number,
            long number => number,

            _ => throw CreateInvalidTypeException(key, value)
        };
    }

    private static object GetRequiredValue(
        IDictionary<string, object?> headers,
        string key)
    {
        if (!headers.TryGetValue(key, out object? value) ||
            value is null)
        {
            throw new InvalidDataException(
                $"Required RabbitMQ header '{key}' is missing.");
        }

        return value;
    }

    private static InvalidDataException CreateInvalidTypeException(
        string key,
        object value)
    {
        return new InvalidDataException(
            $"RabbitMQ header '{key}' has unsupported type " +
            $"'{value.GetType().Name}'.");
    }
}
