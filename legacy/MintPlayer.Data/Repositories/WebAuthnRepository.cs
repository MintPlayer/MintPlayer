using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MintPlayer.Data.Entities;
using MintPlayer.Data.Exceptions.Account;
using MintPlayer.Data.Mappers;
using MintPlayer.Dtos.Dtos;
using MintPlayer.Dtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MintPlayer.Data.Repositories
{
    internal interface IWebAuthnRepository
    {
        Task<CredentialCreateOptions> GetRegistrationOptions(ClaimsPrincipal userProperty, string displayName);
        Task<WebAuthnCredentialInfo> CompleteRegistration(ClaimsPrincipal userProperty, AuthenticatorAttestationRawResponse attestationResponse, CredentialCreateOptions originalOptions, string displayName);
        Task<AssertionOptions> GetAssertionOptions(string email);
        Task<LocalLoginResult> ValidateAssertion(AuthenticatorAssertionRawResponse assertionResponse, AssertionOptions originalOptions);
        Task<IEnumerable<WebAuthnCredentialInfo>> GetCredentials(ClaimsPrincipal userProperty);
        Task<bool> RemoveCredential(ClaimsPrincipal userProperty, int credentialId);
    }

    internal class WebAuthnRepository : IWebAuthnRepository
    {
        private readonly MintPlayerContext mintPlayerContext;
        private readonly UserManager<Entities.User> userManager;
        private readonly SignInManager<Entities.User> signInManager;
        private readonly IFido2 fido2;
        private readonly IUserMapper userMapper;

        public WebAuthnRepository(
            MintPlayerContext mintPlayerContext,
            UserManager<Entities.User> userManager,
            SignInManager<Entities.User> signInManager,
            IFido2 fido2,
            IUserMapper userMapper)
        {
            this.mintPlayerContext = mintPlayerContext;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.fido2 = fido2;
            this.userMapper = userMapper;
        }

        public async Task<CredentialCreateOptions> GetRegistrationOptions(ClaimsPrincipal userProperty, string displayName)
        {
            var user = await userManager.GetUserAsync(userProperty);
            if (user == null)
                throw new UnauthorizedAccessException();

            // Get existing credentials to exclude them
            var existingCredentials = await mintPlayerContext.WebAuthnCredentials
                .Where(c => c.UserId == user.Id)
                .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
                .ToListAsync();

            // Create Fido2User
            var fido2User = new Fido2User
            {
                Id = user.Id.ToByteArray(),
                Name = user.Email,
                DisplayName = user.UserName ?? user.Email
            };

            // Generate registration options with resident key for discoverable credentials
            var authenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = true,
                UserVerification = UserVerificationRequirement.Preferred
            };

            var options = fido2.RequestNewCredential(
                fido2User,
                existingCredentials,
                authenticatorSelection,
                AttestationConveyancePreference.None
            );

            return options;
        }

        public async Task<WebAuthnCredentialInfo> CompleteRegistration(
            ClaimsPrincipal userProperty,
            AuthenticatorAttestationRawResponse attestationResponse,
            CredentialCreateOptions originalOptions,
            string displayName)
        {
            var user = await userManager.GetUserAsync(userProperty);
            if (user == null)
                throw new UnauthorizedAccessException();

            // Verify the attestation response
            var result = await fido2.MakeNewCredentialAsync(
                attestationResponse,
                originalOptions,
                async (args, cancellationToken) =>
                {
                    // Check if credential ID already exists
                    var exists = await mintPlayerContext.WebAuthnCredentials
                        .AnyAsync(c => c.CredentialId == args.CredentialId, cancellationToken);
                    return !exists;
                });

            if (result.Status != "ok")
            {
                throw new Exception($"WebAuthn registration failed: {result.ErrorMessage}");
            }

            // Store the credential
            var credential = new WebAuthnCredential
            {
                UserId = user.Id,
                CredentialId = result.Result.CredentialId,
                PublicKey = result.Result.PublicKey,
                UserHandle = result.Result.User.Id,
                SignatureCounter = result.Result.Counter,
                CredType = result.Result.CredType.ToString(),
                RegDate = DateTime.UtcNow,
                AaGuid = result.Result.Aaguid,
                DisplayName = displayName ?? "Passkey"
            };

            mintPlayerContext.WebAuthnCredentials.Add(credential);
            await mintPlayerContext.SaveChangesAsync();

            return new WebAuthnCredentialInfo
            {
                Id = credential.Id,
                DisplayName = credential.DisplayName,
                RegDate = credential.RegDate,
                LastUsed = credential.LastUsed
            };
        }

        public async Task<AssertionOptions> GetAssertionOptions(string email)
        {
            List<PublicKeyCredentialDescriptor> allowedCredentials = null;

            if (!string.IsNullOrEmpty(email))
            {
                // Non-discoverable flow: get credentials for specific user
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    allowedCredentials = await mintPlayerContext.WebAuthnCredentials
                        .Where(c => c.UserId == user.Id)
                        .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
                        .ToListAsync();
                }
            }

            // Generate assertion options
            // If allowedCredentials is null or empty, it will use discoverable credentials
            var options = fido2.GetAssertionOptions(
                allowedCredentials ?? new List<PublicKeyCredentialDescriptor>(),
                UserVerificationRequirement.Preferred
            );

            return options;
        }

        public async Task<LocalLoginResult> ValidateAssertion(
            AuthenticatorAssertionRawResponse assertionResponse,
            AssertionOptions originalOptions)
        {
            // Find the credential by ID
            var credential = await mintPlayerContext.WebAuthnCredentials
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CredentialId == assertionResponse.Id);

            if (credential == null)
            {
                throw new LoginException();
            }

            // Verify the assertion
            var result = await fido2.MakeAssertionAsync(
                assertionResponse,
                originalOptions,
                credential.PublicKey,
                credential.SignatureCounter,
                async (args, cancellationToken) =>
                {
                    // Verify user handle matches if provided
                    if (args.UserHandle != null && args.UserHandle.Length > 0)
                    {
                        var userIdFromHandle = new Guid(args.UserHandle);
                        return credential.UserId == userIdFromHandle;
                    }
                    return true;
                });

            if (result.Status != "ok")
            {
                throw new LoginException();
            }

            // Update signature counter
            credential.SignatureCounter = result.Counter;
            credential.LastUsed = DateTime.UtcNow;
            await mintPlayerContext.SaveChangesAsync();

            // Sign in the user
            await signInManager.SignInAsync(credential.User, isPersistent: true);

            return new LocalLoginResult
            {
                Status = LoginStatus.Success,
                User = userMapper.Entity2Dto(credential.User, true)
            };
        }

        public async Task<IEnumerable<WebAuthnCredentialInfo>> GetCredentials(ClaimsPrincipal userProperty)
        {
            var user = await userManager.GetUserAsync(userProperty);
            if (user == null)
                throw new UnauthorizedAccessException();

            var credentials = await mintPlayerContext.WebAuthnCredentials
                .Where(c => c.UserId == user.Id)
                .OrderByDescending(c => c.RegDate)
                .Select(c => new WebAuthnCredentialInfo
                {
                    Id = c.Id,
                    DisplayName = c.DisplayName,
                    RegDate = c.RegDate,
                    LastUsed = c.LastUsed
                })
                .ToListAsync();

            return credentials;
        }

        public async Task<bool> RemoveCredential(ClaimsPrincipal userProperty, int credentialId)
        {
            var user = await userManager.GetUserAsync(userProperty);
            if (user == null)
                throw new UnauthorizedAccessException();

            var credential = await mintPlayerContext.WebAuthnCredentials
                .FirstOrDefaultAsync(c => c.Id == credentialId && c.UserId == user.Id);

            if (credential == null)
                return false;

            mintPlayerContext.WebAuthnCredentials.Remove(credential);
            await mintPlayerContext.SaveChangesAsync();
            return true;
        }
    }
}
