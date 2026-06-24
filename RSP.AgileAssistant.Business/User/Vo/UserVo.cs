namespace RSP.AgileAssistant.Business.User.Vo
{
    /// <summary>
    /// Output representation of a user returned to API clients. Sensitive fields
    /// such as the stored refresh token are deliberately excluded.
    /// </summary>
    public class UserVo
    {
        /// <summary>
        /// Unique identifier of the user.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Login name of the user.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Full display name of the user.
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Identifier of the user's default deck (FR-USER-2).
        /// </summary>
        public string? DefaultDeckId { get; set; }

        /// <summary>
        /// The user's default group (FR-USER-3).
        /// </summary>
        public string? DefaultGroup { get; set; }

        /// <summary>
        /// Email used to authenticate against Jira (FR-USER-4). The Jira API token
        /// itself is never returned to clients to avoid leaking the secret.
        /// </summary>
        public string? JiraEmail { get; set; }

        /// <summary>
        /// Indicates whether Jira credentials are stored for the user, allowing
        /// clients to reflect the connection state without exposing the token.
        /// </summary>
        public bool HasJiraCredentials { get; set; }

        /// <summary>
        /// Indicates whether the user's stored defaults should be applied to
        /// future meetings (FR-USER-5).
        /// </summary>
        public bool UseDefaults { get; set; }

        /// <summary>
        /// Indicates whether the user holds the scrum master role.
        /// </summary>
        public bool ScrumMaster { get; set; }

        /// <summary>
        /// Indicates whether the account is an anonymous guest account.
        /// </summary>
        public bool IsGuest { get; set; }

        /// <summary>
        /// Freshly issued JWT access token. Populated by login and refresh
        /// operations; empty for plain read operations.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;
    }
}
