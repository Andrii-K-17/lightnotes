namespace LightNotes.Domain.Common;

/// <summary>A violated domain invariant.</summary>
public sealed class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
