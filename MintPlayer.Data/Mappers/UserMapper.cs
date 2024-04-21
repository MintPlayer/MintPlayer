namespace MintPlayer.Data.Mappers;

internal interface IUserMapper
{
	MintPlayer.Dtos.Dtos.User? Entity2Dto(Entities.User? user, bool mapSensitiveData);
	Entities.User? Dto2Entity(MintPlayer.Dtos.Dtos.User? user);
}

internal class UserMapper : IUserMapper
{
	private readonly IServiceProvider serviceProvider;
	public UserMapper(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;
	}

	public MintPlayer.Dtos.Dtos.User? Entity2Dto(Entities.User? user, bool mapSensitiveData)
	{
		if (user == null) return null;
		return new MintPlayer.Dtos.Dtos.User
		{
			Id = mapSensitiveData ? user.Id : Guid.Empty,
			Email = mapSensitiveData ? user.Email : null,
			UserName = user.UserName,
			IsTwoFactorEnabled = mapSensitiveData ? user.TwoFactorEnabled : false,
			Bypass2faForExternalLogin = mapSensitiveData ? user.Bypass2faForExternalLogin : false,
			PictureUrl = user.PictureUrl,
		};
	}

	public Entities.User? Dto2Entity(MintPlayer.Dtos.Dtos.User? user)
	{
		if (user == null) return null;
		return new Entities.User
		{
			Id = user.Id,
			Email = user.Email,
			UserName = user.UserName,
			TwoFactorEnabled = user.IsTwoFactorEnabled,
			Bypass2faForExternalLogin = user.Bypass2faForExternalLogin,
			PictureUrl = user.PictureUrl,
		};
	}
}
