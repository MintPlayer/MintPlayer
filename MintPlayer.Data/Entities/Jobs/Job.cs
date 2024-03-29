﻿using MintPlayer.Data.Abstractions.Enums;
using MintPlayer.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MintPlayer.Data.Entities.Jobs
{
    internal class Job
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public EJobStatus Status { get; set; }
    }
}
