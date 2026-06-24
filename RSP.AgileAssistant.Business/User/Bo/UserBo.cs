using System;

namespace RSP.AgileAssistant.Business.User.Bo
{
    /// <summary>
    /// Internal representation of an application user as stored in the database.
    /// </summary>
    public class UserBo
    {
        /// <summary>
        /// Unique identifier of the user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Login name of the user.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Most recently issued access token, used to validate refresh requests
        /// and to support logout (token revocation).
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Full display name of the user.
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Identifier of the user's default deck.
        /// </summary>
        public string? DefaultDeckId { get; set; }

        /// <summary>
        /// The user's default group.
        /// </summary>
        public string? DefaultGroup { get; set; }

        /// <summary>
        /// Email used to authenticate against Jira.
        /// </summary>
        public string? JiraEmail { get; set; }

        /// <summary>
        /// API token used to authenticate against Jira.
        /// </summary>
        public string? JiraToken { get; set; }

        /// <summary>
        /// Indicates whether the user holds the scrum master role.
        /// </summary>
        public bool ScrumMaster { get; set; }

        /// <summary>
        /// Indicates whether the user's stored defaults should be applied.
        /// </summary>
        public bool UseDefaults { get; set; }

        /// <summary>
        /// Indicates whether the account is an anonymous guest account.
        /// This flag is only used in business logic and is not persisted.
        /// </summary>
        public bool IsGuest { get; set; }

        /// <summary>
        /// Timestamp of the user's most recent activity, in UTC.
        /// </summary>
        public DateTime LastActiveDate { get; set; }
    }
}
