using MediatR;
using UknfPlatform.Application.Auth.Authentication.DTOs;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Command to update the authenticated user's profile
/// PESEL cannot be updated (immutable after registration)
/// Email changes require confirmation
/// </summary>
public record UpdateUserProfileCommand : IRequest<UpdateUserProfileResponse>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

