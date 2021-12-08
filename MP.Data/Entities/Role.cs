using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MintPlayer.Data.Entities
{
	internal class Role : IdentityRole<Guid>
    {
    }
}
