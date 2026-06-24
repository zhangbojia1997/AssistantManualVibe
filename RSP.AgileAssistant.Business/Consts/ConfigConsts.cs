namespace RSP.AgileAssistant.Business.Consts
{
    /// <summary>
    /// Configuration keys read from appsettings. Centralized to avoid magic
    /// strings scattered across the codebase.
    /// </summary>
    public static class ConfigConsts
    {
        /// <summary>
        /// Configuration key holding the symmetric secret used to sign and
        /// validate JWT access tokens.
        /// </summary>
        public const string JWT_SECRET = "applicationSettings:jwtSecret";
    }
}
