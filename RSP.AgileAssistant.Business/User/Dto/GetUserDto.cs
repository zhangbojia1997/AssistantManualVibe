using System;
using System.ComponentModel.DataAnnotations;
using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.User.Dto
{
    /// <summary>
    /// Input payload for retrieving a single user by identifier.
    /// </summary>
    public class GetUserDto : DtoBase
    {
        /// <summary>
        /// Unique identifier of the user to retrieve.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// The caller's current access token, used to authorize the request.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
