using MediatR;

namespace UknfPlatform.Application.Communication.Messages.Queries;

/// <summary>
/// Query to download a message attachment
/// Story 5.2: Download message attachments
/// </summary>
public record DownloadAttachmentQuery(Guid MessageId, Guid AttachmentId) : IRequest<DownloadAttachmentResponse?>;

/// <summary>
/// Response with attachment file data
/// </summary>
public record DownloadAttachmentResponse(
    string FileName,
    string ContentType,
    Stream FileStream
);

