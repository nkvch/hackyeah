using MediatR;
using Microsoft.Extensions.Logging;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Auth.Repositories;
using UknfPlatform.Domain.Shared.Exceptions;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Handler for ActivateAccountCommand
/// Story 2.1: Automatically creates access request after activation
/// </summary>
public class ActivateAccountCommandHandler : IRequestHandler<ActivateAccountCommand, ActivateAccountResponse>
{
    private readonly IActivationTokenRepository _activationTokenRepository;
    private readonly IAccessRequestRepository _accessRequestRepository;
    private readonly ILogger<ActivateAccountCommandHandler> _logger;

    public ActivateAccountCommandHandler(
        IActivationTokenRepository activationTokenRepository,
        IAccessRequestRepository accessRequestRepository,
        ILogger<ActivateAccountCommandHandler> logger)
    {
        _activationTokenRepository = activationTokenRepository ?? throw new ArgumentNullException(nameof(activationTokenRepository));
        _accessRequestRepository = accessRequestRepository ?? throw new ArgumentNullException(nameof(accessRequestRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ActivateAccountResponse> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing account activation for token");

        // Find token (includes User navigation property)
        var activationToken = await _activationTokenRepository.GetByTokenAsync(request.Token, cancellationToken);

        if (activationToken == null)
        {
            _logger.LogWarning("Activation attempt with invalid token");
            throw new InvalidTokenException("Activation link is invalid");
        }

        // Check if token is valid (not expired and not used)
        if (!activationToken.IsValid())
        {
            _logger.LogWarning("Activation attempt with expired token for user {UserId}", activationToken.UserId);
            throw new TokenExpiredException("Activation link has expired. Please request a new one.");
        }

        // Check if token is already used
        if (activationToken.IsUsed)
        {
            _logger.LogWarning("Activation attempt with already used token for user {UserId}", activationToken.UserId);
            
            // Check if user is already active (idempotent behavior)
            if (activationToken.User.IsActive)
            {
                _logger.LogInformation("User {UserId} is already activated, returning success", activationToken.User.Id);
                return new ActivateAccountResponse(
                    activationToken.User.Id,
                    "This account has already been activated. You can now log in.");
            }

            throw new TokenAlreadyUsedException("This activation link has already been used");
        }

        // If user is already active, return success (idempotent)
        if (activationToken.User.IsActive)
        {
            _logger.LogInformation("User {UserId} is already active, marking token as used and returning success", activationToken.User.Id);
            activationToken.MarkAsUsed();
            await _activationTokenRepository.UpdateAsync(activationToken, cancellationToken);
            await _activationTokenRepository.SaveChangesAsync(cancellationToken);

            return new ActivateAccountResponse(
                activationToken.User.Id,
                "Your account is already activated. You can now log in.");
        }

        // Activate the user account
        activationToken.User.Activate();
        activationToken.MarkAsUsed();

        await _activationTokenRepository.UpdateAsync(activationToken, cancellationToken);
        await _activationTokenRepository.SaveChangesAsync(cancellationToken);

        // Story 2.1: Automatically create access request after activation
        var existingAccessRequest = await _accessRequestRepository.GetByUserIdAsync(activationToken.User.Id, cancellationToken);
        if (existingAccessRequest == null)
        {
            var accessRequest = AccessRequest.CreateForUser(activationToken.User.Id);
            await _accessRequestRepository.AddAsync(accessRequest, cancellationToken);
            await _activationTokenRepository.SaveChangesAsync(cancellationToken); // Reuse same UnitOfWork
            
            _logger.LogInformation("Access request created automatically for user {UserId}, RequestId: {AccessRequestId}", 
                activationToken.User.Id, accessRequest.Id);
        }
        else
        {
            _logger.LogInformation("Access request already exists for user {UserId}, skipping creation", activationToken.User.Id);
        }

        _logger.LogInformation("Account activated successfully for user {UserId}", activationToken.User.Id);

        return new ActivateAccountResponse(
            activationToken.User.Id,
            "Account activated successfully. Please set your password to continue.");
    }
}

