namespace RSP.AgileAssistant.Business.Consts
{
    /// <summary>
    /// String keys used to register and resolve IOC dependencies. Keys must match
    /// exactly between <c>Startup.RegisterIOC()</c> and the consuming Action's
    /// <c>[IOCInjection]</c> attribute.
    /// </summary>
    public static class IocConsts
    {
        /// <summary>
        /// IOC key for the JWT token service.
        /// </summary>
        public const string TOKEN_SERVICE = "TOKEN_SERVICE";
    }
}
