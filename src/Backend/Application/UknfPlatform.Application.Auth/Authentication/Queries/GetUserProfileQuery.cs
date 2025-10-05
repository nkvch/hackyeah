using MediatR;
using UknfPlatform.Application.Auth.Authentication.DTOs;

namespace UknfPlatform.Application.Auth.Authentication.Queries;

/// <summary>
/// Query to get the current user's profile information
/// User ID is obtained from the authenticated user context
/// </summary>
public record GetUserProfileQuery : IRequest<UserProfileDto>;

