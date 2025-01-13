namespace Boutique.Core.Contracts.User
{
    public class UpdateUserDto
    {
        public string UserId { get; set; }
        public List<string> Roles { get; set; }
    }
}
