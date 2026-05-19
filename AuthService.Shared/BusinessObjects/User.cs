namespace AuthService.Shared.BusinessObjects
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public List<AppRole> Roles { get; set; } = new List<AppRole>();
    }
}
