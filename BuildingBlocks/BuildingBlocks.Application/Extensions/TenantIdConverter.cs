namespace BuildingBlocks.Application.Extensions;

public static class TenantIdConverter
{
    /// <summary>
    /// Converts an integer TenantId to a deterministic Guid.
    /// Same integer will always produce the same Guid.
    /// </summary>
    public static Guid ToGuid(int tenantId)
    {
        // Create a deterministic Guid from integer
        // Uses a fixed namespace UUID to ensure consistency
        byte[] bytes = new byte[16];
        BitConverter.GetBytes(tenantId).CopyTo(bytes, 0);
        
        // Set version to 4 (random) and variant to RFC 4122
        bytes[7] = (byte)((bytes[7] & 0x0F) | 0x40);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
        
        return new Guid(bytes);
    }
    
    /// <summary>
    /// Extracts the original integer TenantId from a Guid (if it was created with ToGuid)
    /// </summary>
    public static int ToInt(Guid tenantIdGuid)
    {
        byte[] bytes = tenantIdGuid.ToByteArray();
        return BitConverter.ToInt32(bytes, 0);
    }
}