using System.Diagnostics.CodeAnalysis;
using MetadataReferenceService.BlazorWasm.Types;

namespace MetadataReferenceService.BlazorWasm.Models;

internal class DownloadFileResult
{
    public bool Success { get; }

    public FileType FileType { get; }

    [MemberNotNullWhen(true, nameof(Success))]
    public Stream? Stream { get; }

    public DownloadFileResult(FileType fileType) : this(fileType, default)
    {
    }

    public DownloadFileResult(FileType fileType, Stream? stream)
    {
        FileType = fileType;
        Success = stream != null;
        Stream = stream;
    }
}