using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace FakeApi.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string Address { get; set; }

		public ShoppingCart ShoppingCart { get; set; }

		public ICollection<Order> Orders { get; set; }

		/// <summary>加入基本IdentityUser模組</summary>
		public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; }
		public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }
		public virtual ICollection<IdentityUserLogin<string>> Logins { get; set; }
		public virtual ICollection<IdentityUserToken<string>> Tokens { get; set; }
	}
}
