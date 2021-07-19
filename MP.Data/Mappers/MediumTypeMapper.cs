namespace MintPlayer.Data.Mappers
{
    internal interface IMediumTypeMapper
    {
		MintPlayer.Dtos.Dtos.MediumType Entity2Dto(Entities.MediumType mediumType);
		Entities.MediumType Dto2Entity(MintPlayer.Dtos.Dtos.MediumType mediumType, MintPlayerContext mintplayer_context);
	}

	internal class MediumTypeMapper : IMediumTypeMapper
	{
		public MintPlayer.Dtos.Dtos.MediumType Entity2Dto(Entities.MediumType mediumType)
		{
			if (mediumType == null) return null;

			return new MintPlayer.Dtos.Dtos.MediumType
			{
				Id = mediumType.Id,
				Description = mediumType.Description,
				PlayerType = mediumType.PlayerType,
				Visible = mediumType.Visible
			};
		}

		public Entities.MediumType Dto2Entity(MintPlayer.Dtos.Dtos.MediumType mediumType, MintPlayerContext mintplayer_context)
		{
			if (mediumType == null) return null;

			return new Entities.MediumType
			{
				Id = mediumType.Id,
				Description = mediumType.Description,
				PlayerType = mediumType.PlayerType,
				Visible = mediumType.Visible
			};
		}
	}
}
