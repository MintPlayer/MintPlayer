using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MintPlayer.Timestamps;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("MintPlayer.Data")]
namespace MintPlayer.Dtos.Dtos
{
	public abstract class Subject : IUpdateTimestamp
	{
		public int Id { get; set; }
		public List<Medium> Media { get; set; }
		public List<Tag> Tags { get; set; }
		public string Text { get; internal set; }

		#region DateUpdate
		[JsonIgnore]
		[Nest.Ignore]
		public DateTime DateUpdate { get; set; }

		[JsonProperty("dateUpdate")]
		private DateTime? DateUpdateJson
		{
			get => DateUpdate;
			set
			{
				if (value != null)
					DateUpdate = value.Value;
			}
		}
		#endregion
	}
}
