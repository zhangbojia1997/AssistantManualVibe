using System;
using System.Collections.Generic;

namespace RSP.AgileAssistant.Business.Meeting.Bo
{
    /// <summary>
    /// Internal representation of a meeting aggregate as stored across the
    /// <c>Vibe_Meeting_Table</c> and its child tables.
    /// </summary>
    public class MeetingBo
    {
        /// <summary>
        /// Meeting status value used for active sessions.
        /// </summary>
        public const int StatusRunning = 1;

        /// <summary>
        /// Meeting status value used for ended sessions.
        /// </summary>
        public const int StatusEnded = 0;

        /// <summary>
        /// Unique identifier of the meeting.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the user hosting (and owning) the meeting.
        /// </summary>
        public Guid HostId { get; set; }

        /// <summary>
        /// Discussion topic / title of the meeting.
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the deck used for estimation.
        /// </summary>
        public Guid DeckId { get; set; }

        /// <summary>
        /// Identifier of the topic currently being voted on, if any (FR-VOTE).
        /// </summary>
        public Guid? VotingOn { get; set; }

        /// <summary>
        /// Identifier of the current/most recent voting round, if any (FR-VOTE).
        /// </summary>
        public Guid? VotingRound { get; set; }

        /// <summary>
        /// Status of the meeting. The value is stored as an integer so the
        /// physical model can distinguish active and ended sessions.
        /// </summary>
        public int Status { get; set; } = StatusRunning;

        /// <summary>
        /// Timestamp of the most recent activity, in UTC.
        /// </summary>
        public DateTime LastActiveDate { get; set; }

        /// <summary>
        /// Email used to authenticate against Jira for this meeting.
        /// </summary>
        public string JiraEmail { get; set; } = string.Empty;

        /// <summary>
        /// API token used to authenticate against Jira for this meeting.
        /// </summary>
        public string JiraToken { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether Jira credentials are configured for the meeting.
        /// </summary>
        public bool JiraConnected { get; set; }

        /// <summary>
        /// Creation timestamp as Unix milliseconds.
        /// </summary>
        public long CreatedOn { get; set; }

        /// <summary>
        /// Groups associated with the meeting.
        /// </summary>
        public List<string> Groups { get; set; } = new List<string>();

        /// <summary>
        /// Participants currently in the meeting.
        /// </summary>
        public List<ParticipantBo> Participants { get; set; } = new List<ParticipantBo>();

        /// <summary>
        /// Voting rounds recorded for the meeting.
        /// </summary>
        public List<RoundBo> Rounds { get; set; } = new List<RoundBo>();

        /// <summary>
        /// Sub-topics (and their tasks) of the meeting.
        /// </summary>
        public List<SubTopicBo> SubTopics { get; set; } = new List<SubTopicBo>();
    }
}
