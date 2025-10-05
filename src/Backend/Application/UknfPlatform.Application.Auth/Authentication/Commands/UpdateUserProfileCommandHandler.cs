using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Auth.Authentication.DTOs;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Shared.Exceptions;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Handler for UpdateUserProfileCommand - updates user profile and handles email changes
/// </summary>
public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UpdateUserProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailChangeTokenRepository _emailChangeTokenRepository;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

    public UpdateUserProfileCommandHandler(
        IUserRepository userRepository,
        IEmailChangeTokenRepository emailChangeTokenRepository,
        IEmailService emailService,
        ICurrentUserService currentUserService,
        IConfiguration configuration,
        ILogger<UpdateUserProfileCommandHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _emailChangeTokenRepository = emailChangeTokenRepository ?? throw new ArgumentNullException(nameof(emailChangeTokenRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UpdateUserProfileResponse> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken)
    {
        // Get authenticated user
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("User not authenticated");

        var userId = _currentUserService.UserId;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        var changedFields = new List<string>();
        var emailChangeRequiresConfirmation = false;

        // Check if email changed
        var emailChanged = !user.Email.Equals(command.Email, StringComparison.OrdinalIgnoreCase);
        
        if (emailChanged)
        {
            // Validate new email not already in use (by another user)
            var existingUser = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new ConflictException($"Email {command.Email} is already in use by another account");
            }

            // Generate email confirmation token
            var emailChangeToken = EmailChangeToken.Create(userId, command.Email);
            await _emailChangeTokenRepository.AddAsync(emailChangeToken, cancellationToken);

            // Set pending email (don't update email immediately)
            user.RequestEmailChange(command.Email);

            // Send confirmation email to NEW address
            var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:4200";
            var confirmationUrl = $"{frontendUrl}/auth/confirm-email-change?token={emailChangeToken.Token}";
            
            await _emailService.SendEmailChangeConfirmationAsync(
                command.Email,
                user.FirstName,
                confirmationUrl,
                cancellationToken);

            emailChangeRequiresConfirmation = true;
            changedFields.Add("Email (pending confirmation)");
            
            _logger.LogInformation(
                "Email change requested for user {UserId}. New email: {NewEmail}",
                userId,
                command.Email);
        }

        // Update basic profile fields (FirstName, LastName, Phone)
        if (user.FirstName != command.FirstName || user.LastName != command.LastName || user.Phone != command.PhoneNumber)
        {
            if (user.FirstName != command.FirstName) changedFields.Add("FirstName");
            if (user.LastName != command.LastName) changedFields.Add("LastName");
            if (user.Phone != command.PhoneNumber) changedFields.Add("PhoneNumber");
            
            user.UpdateProfile(command.FirstName, command.LastName, command.PhoneNumber);
        }

        // Save changes
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        
        if (emailChanged)
        {
            await _emailChangeTokenRepository.SaveChangesAsync(cancellationToken);
        }

        // Log profile update
        _logger.LogInformation(
            "Profile updated for user {UserId}. Changed fields: {ChangedFields}",
            userId,
            string.Join(", ", changedFields));

        // Build response message
        var message = emailChangeRequiresConfirmation
            ? "Profile updated successfully. Please check your new email to confirm the email change."
            : "Profile updated successfully.";

        // Return response with updated profile
        var updatedProfile = new UserProfileDto(
            UserId: user.Id,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Email: user.Email,
            PendingEmail: user.PendingEmail,
            PhoneNumber: user.Phone,
            PeselLast4: user.PeselLast4,
            UserType: user.UserType.ToString(),
            CreatedDate: user.CreatedDate,
            LastLoginDate: user.LastLoginDate
        );

        return new UpdateUserProfileResponse(
            Success: true,
            Message: message,
            EmailChangeRequiresConfirmation: emailChangeRequiresConfirmation,
            UpdatedProfile: updatedProfile
        );
    }
}

