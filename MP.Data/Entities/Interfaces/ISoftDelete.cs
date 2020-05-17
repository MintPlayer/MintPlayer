namespace MintPlayer.Data.Entities.Interfaces
{
	internal interface ISoftDelete
	{
		User UserInsert { get; set; }
		User UserUpdate { get; set; }
		User UserDelete { get; set; }
	}
}
