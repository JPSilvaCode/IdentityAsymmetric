using System.Collections.Generic;

namespace IA.Identity.API.Models
{
	public class RoleUsersVIewModel
	{
		public int	 Id { get; set; }
		public string Name { get; set; }
		public IList<UserResponse> UsersResponse { get; set; }
	}
}
