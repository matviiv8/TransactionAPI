﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionAPI.Domain.Models
{
	public class Tokens
	{
		public string Token { get; set; }

		public string RefreshToken { get; set; }
	}
}