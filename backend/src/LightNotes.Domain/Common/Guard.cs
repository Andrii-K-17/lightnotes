namespace LightNotes.Domain.Common;

internal static class Guard
{
    /// <summary>Client-generated ids must be UUIDv7.</summary>
    public static void ClientId(Guid id)
    {
        if (id.Version != 7)
        {
            throw new DomainException(DomainErrorCodes.InvalidId, "Id must be a UUIDv7.");
        }
    }

    public static void OwnerId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException(DomainErrorCodes.InvalidId, "User id is required.");
        }
    }

    /// <summary>Trims the name and checks that it is not empty and not longer than the limit.</summary>
    public static string Name(string? value, int maxLength, string requiredCode, string tooLongCode)
    {
        var name = value?.Trim() ?? "";

        if (name.Length == 0)
        {
            throw new DomainException(requiredCode, "Name is required.");
        }

        if (name.Length > maxLength)
        {
            throw new DomainException(tooLongCode, $"Name is longer than {maxLength} characters.");
        }

        return name;
    }
}
