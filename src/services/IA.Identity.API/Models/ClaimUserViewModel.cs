namespace IA.Identity.API.Models
{
	public class ClaimUserViewModel
	{
		public string Type { get; set; }
		public string Value { get; set; }
		public UserResponse UserResponse { get; set; }
	}
}
