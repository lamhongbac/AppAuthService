namespace AuthService.Shared
{
    public class JwtData
    {
        //json web token
        public string Jwt { get; set; } = string.Empty;
        //refresh token
        public string Rft { get; set; } = string.Empty;
    }
}
