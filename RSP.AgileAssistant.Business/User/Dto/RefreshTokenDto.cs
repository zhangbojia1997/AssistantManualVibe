using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.User.Dto
{
    /// <summary>
    /// Input payload for refreshing an access token.
    /// </summary>
    public class RefreshTokenDto : DtoBase
    {
        /// <summary>
        /// The current (possibly near-expiry) access token to be refreshed.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
