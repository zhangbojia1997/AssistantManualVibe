using System;
using System.ComponentModel.DataAnnotations;
using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.User.Dto
{
    /// <summary>
    /// Input payload for updating a user's persisted preferences (FR-USER):
    /// full name, default deck, default group, Jira credentials and the
    /// "apply defaults" toggle.
    /// </summary>
    public class UpdateUserSettingsDto : DtoBase
    {
        /// <summary>
        /// Unique identifier of the user whose settings are being updated.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Full display name to store for the user (FR-USER-1).
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Default group to apply to future meetings (FR-USER-3).
        /// </summary>
        public string? Group { get; set; }

        /// <summary>
        /// Jira account email used as default Jira credentials (FR-USER-4).
        /// </summary>
        public string? JiraEmail { get; set; }

        /// <summary>
        /// Jira API token used as default Jira credentials (FR-USER-4).
        /// </summary>
        public string? JiraToken { get; set; }

        /// <summary>
        /// Identifier of the default deck to apply to future meetings (FR-USER-2).
        /// </summary>
        public Guid? DeckId { get; set; }

        /// <summary>
        /// Indicates whether the stored defaults should be applied automatically
        /// to future meetings (FR-USER-5).
        /// </summary>
        public bool UseDefaults { get; set; }

        /// <summary>
        /// The caller's current access token, used to authorize the request.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
