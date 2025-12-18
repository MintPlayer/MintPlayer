using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MintPlayer.Data.Entities
{
    internal class WebAuthnCredential
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        /// <summary>
        /// The credential ID returned by the authenticator (base64url encoded when stored)
        /// </summary>
        public byte[] CredentialId { get; set; }

        /// <summary>
        /// The public key for verification (COSE format)
        /// </summary>
        public byte[] PublicKey { get; set; }

        /// <summary>
        /// User handle for resident/discoverable credentials
        /// </summary>
        public byte[] UserHandle { get; set; }

        /// <summary>
        /// Signature counter for replay attack prevention
        /// </summary>
        public uint SignatureCounter { get; set; }

        /// <summary>
        /// Credential type (e.g., "public-key")
        /// </summary>
        public string CredType { get; set; }

        /// <summary>
        /// Registration date
        /// </summary>
        public DateTime RegDate { get; set; }

        /// <summary>
        /// Authenticator AAGUID
        /// </summary>
        public Guid AaGuid { get; set; }

        /// <summary>
        /// Friendly name for the credential (e.g., "MacBook TouchID", "iPhone Face ID")
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Last time this credential was used for authentication
        /// </summary>
        public DateTime? LastUsed { get; set; }
    }
}
