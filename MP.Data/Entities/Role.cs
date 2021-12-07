using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MintPlayer.Data.Entities
{
	[Table("AspNetRoles", Schema = "mintplay")]
	internal class Role : IdentityRole<Guid>
    {
    }
}
