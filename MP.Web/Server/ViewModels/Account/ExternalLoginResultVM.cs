﻿using MintPlayer.Dtos.Enums;

namespace MintPlayer.Web.Server.ViewModels.Account
{
	public class ExternalLoginResultVM
	{
		public LoginStatus Status { get; set; }
		public string Medium { get; set; }
		public string Platform { get; set; }

		public Dtos.Dtos.User User { get; set; }

		public string Error { get; set; }
		public string ErrorDescription { get; set; }
	}
}
