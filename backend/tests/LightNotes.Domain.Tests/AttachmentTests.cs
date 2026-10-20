using LightNotes.Domain.Attachments;
using LightNotes.Domain.Common;
using Xunit;
using static LightNotes.Domain.Tests.TestData;

namespace LightNotes.Domain.Tests;

public class AttachmentTests
{
    private static readonly string Hash = new('a', 64);

    private static Attachment Create(
        string contentType = "image/png",
        long size = 1024,
        string storageKey = "key",
        string? sha256 = null
    ) =>
        Attachment.Create(
            NewId(),
            OwnerId,
            NewId(),
            contentType,
            size,
            storageKey,
            sha256 ?? Hash,
            Now
        );

    [Fact]
    public void Create_normalizes_type_and_hash()
    {
        var attachment = Create(contentType: " Image/PNG ", sha256: new string('A', 64));

        Assert.Equal("image/png", attachment.ContentType);
        Assert.Equal(Hash, attachment.Sha256);
    }

    [Theory]
    [InlineData("image/svg+xml")]
    [InlineData("application/pdf")]
    [InlineData("text/html")]
    [InlineData("")]
    public void Only_raster_image_types_are_allowed(string contentType)
    {
        AssertFails(DomainErrorCodes.AttachmentTypeNotAllowed, () => Create(contentType));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Empty_file_is_rejected(long size)
    {
        AssertFails(DomainErrorCodes.AttachmentSizeInvalid, () => Create(size: size));
    }

    [Fact]
    public void Size_is_limited_to_10_megabytes()
    {
        Assert.Equal(Attachment.MaxSizeBytes, Create(size: Attachment.MaxSizeBytes).SizeBytes);

        AssertFails(
            DomainErrorCodes.AttachmentTooLarge,
            () => Create(size: Attachment.MaxSizeBytes + 1)
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0000000000")]
    public void Hash_must_be_64_hex_characters(string sha256)
    {
        AssertFails(DomainErrorCodes.AttachmentHashInvalid, () => Create(sha256: sha256));
    }

    [Fact]
    public void Storage_key_is_required()
    {
        AssertFails(DomainErrorCodes.AttachmentStorageKeyInvalid, () => Create(storageKey: " "));
    }

    [Fact]
    public void Ids_must_be_uuid_v7()
    {
        AssertFails(
            DomainErrorCodes.InvalidId,
            () =>
                Attachment.Create(
                    Guid.NewGuid(),
                    OwnerId,
                    NewId(),
                    "image/png",
                    1,
                    "key",
                    Hash,
                    Now
                )
        );
        AssertFails(
            DomainErrorCodes.InvalidId,
            () =>
                Attachment.Create(
                    NewId(),
                    OwnerId,
                    Guid.NewGuid(),
                    "image/png",
                    1,
                    "key",
                    Hash,
                    Now
                )
        );
    }
}
