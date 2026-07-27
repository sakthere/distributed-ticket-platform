namespace TicketManagement.Api.Contract
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }

        public class RegisterResponse : AuthResponse
        {
            public int UserId { get; set; }
        }
    }
}
