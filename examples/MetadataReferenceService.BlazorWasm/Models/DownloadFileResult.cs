using System.Diagnostics.CodeAnalysis;
using MetadataReferenceService.BlazorWasm.Types;

namespace MetadataReferenceService.BlazorWasm.Models;

internal class DownloadFileResult
{
    public bool Success { get; }

    public FileType FileType { get; }

    [MemberNotNullWhen(true, nameof(Bytes))]
    public byte[]? Bytes { get; }

    public DownloadFileResult(FileType fileType) : this(fileType, default)
    {
    }

    public DownloadFileResult(FileType fileType, byte[]? bytes)
    {
        FileType = fileType;
        Success = bytes != null;
        Bytes = bytes;
    }
}