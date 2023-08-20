using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionAPI.Infrastructure.ViewModels.Tokens
{
	public class TokensViewModel
	{
		public string AccessToken { get; set; }

		public string RefreshToken { get; set; }
	}
}
