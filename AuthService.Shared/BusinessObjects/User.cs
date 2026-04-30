namespace AuthService.Shared.BusinessObjects
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public string Mobile { get; set; }
        public string Password { get; set; }
        public List<AppRole> Roles { get; set; } = new List<AppRole>();
    }
}
