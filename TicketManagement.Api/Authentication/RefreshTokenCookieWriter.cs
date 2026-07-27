namespace TicketManagement.Api.Authentication
{
    public class RefreshTokenCookieWriter
    {
        public const string CookieName = "refreshToken";
        public static void Write(HttpResponse response, string refreshToken, DateTime expiresAt)
        {
            response.Cookies.Append(CookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt,
                Path = "/api/auth"
            });
        }

        public static void Clear(HttpResponse response)
        {
            response.Cookies.Delete(CookieName, new CookieOptions
            {
                Path = "/api/auth"
            });
        }
    }
}
