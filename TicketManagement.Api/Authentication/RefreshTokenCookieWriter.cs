namespace TicketManagement.Api.Authentication
{
    public class RefreshTokenCookieWriter
    {
        public const string CookieName = "refreshToken";

        // A cookie's Path is a PREFIX match against the request path - it is
        // not "the auth feature," it is the literal string "/api/v1/auth".
        // AuthController's route moved from "api/auth" to
        // "api/v{version:apiVersion}/auth" (this sprint's versioning change),
        // so this had to move with it: left as the old "/api/auth", the
        // browser would silently stop attaching this cookie to
        // POST /api/v1/auth/refresh and /logout, since that path no longer
        // starts with "/api/auth" - version routing put "v1" in between.
        // Nothing would throw; refresh/logout would just quietly stop working.
        //
        // Tying a cookie's scope to one specific version string is its own
        // coupling, just moved rather than removed - the day a v2 Auth
        // controller exists, this needs a deliberate decision (exempt
        // refresh/logout from versioning entirely, since a session
        // mechanic isn't really a "resource representation" that should
        // vary by version; or re-scope the cookie broadly to "/api"). Not
        // solving that today since there's only one version to support -
        // recorded as the thing to revisit when v2 actually happens.
        private const string CookiePath = "/api/v1/auth";

        public static void Write(HttpResponse response, string refreshToken, DateTime expiresAt)
        {
            response.Cookies.Append(CookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt,
                Path = CookiePath
            });
        }

        public static void Clear(HttpResponse response)
        {
            response.Cookies.Delete(CookieName, new CookieOptions
            {
                Path = CookiePath
            });
        }
    }
}
