using MediatR;
using UknfPlatform.Application.Auth.AccessRequests.DTOs;

namespace UknfPlatform.Application.Auth.AccessRequests.Queries;

/// <summary>
/// Query to get the current user's access request.
/// Story 2.1: Automatic Access Request Creation
/// </summary>
public record GetAccessRequestQuery : IRequest<AccessRequestDto>
{
    // No parameters - gets current user's access request from context
}

