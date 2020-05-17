using MintPlayer.Dtos.Enums;

namespace MintPlayer.Dtos.Dtos
{
	public class MediumType
	{
		public int Id { get; set; }
		public string Description { get; set; }
		public ePlayerType PlayerType { get; set; }
		public bool Visible { get; set; }
	}
}
