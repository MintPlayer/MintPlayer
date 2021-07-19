using Microsoft.Extensions.DependencyInjection;
using System;

namespace MintPlayer.Data.Mappers
{
    internal interface IMediumMapper
    {
		MintPlayer.Dtos.Dtos.Medium Entity2Dto(Entities.Medium medium, bool include_relations = false);
		Entities.Medium Dto2Entity(MintPlayer.Dtos.Dtos.Medium medium, MintPlayerContext mintplayer_context);
	}

	internal class MediumMapper : IMediumMapper
	{
        private readonly IServiceProvider serviceProvider;
        public MediumMapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

		public MintPlayer.Dtos.Dtos.Medium Entity2Dto(Entities.Medium medium, bool include_relations = false)
		{
			if (medium == null) return null;
			if (include_relations)
			{
				var mediumTypeMapper = serviceProvider.GetRequiredService<IMediumTypeMapper>();
				return new MintPlayer.Dtos.Dtos.Medium
				{
					Id = medium.Id,
					Type = mediumTypeMapper.Entity2Dto(medium.Type),
					Value = medium.Value
				};
			}
			else
			{
				return new MintPlayer.Dtos.Dtos.Medium
				{
					Id = medium.Id,
					Value = medium.Value
				};
			}
		}
	
		public Entities.Medium Dto2Entity(MintPlayer.Dtos.Dtos.Medium medium, MintPlayerContext mintplayer_context)
		{
			if (medium == null) return null;
			return new Entities.Medium
			{
				Id = medium.Id,
				Type = mintplayer_context.MediumTypes.Find(medium.Type.Id),
				Value = medium.Value
			};
		}
	}
}
