using System;
using System.Collections.Generic;

namespace RSP.AgileAssistant.Business.Meeting.Vo
{
    /// <summary>
    /// Output representation of a meeting returned to API clients (FR-MEET /
    /// FR-VOTE). Matches the front-end meeting contract.
    /// </summary>
    public class MeetingVo
    {
        /// <summary>
        /// Unique identifier of the meeting.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the host (owner) of the meeting.
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
        /// Identifier of the topic currently being voted on, if any.
        /// </summary>
        public Guid? VotingOn { get; set; }

        /// <summary>
        /// Identifier of the current/most recent voting round, if any.
        /// </summary>
        public Guid? VotingRound { get; set; }

        /// <summary>
        /// Identifier of the round whose results are currently displayed, if any.
        /// Derived from the current round's status under the polling model.
        /// </summary>
        public Guid? DisplayingRoundId { get; set; }

        /// <summary>
        /// Participants currently in the meeting.
        /// </summary>
        public List<ParticipantVo> Participants { get; set; } = new List<ParticipantVo>();

        /// <summary>
        /// Sub-topics (user stories) of the meeting.
        /// </summary>
        public List<SubTopicVo> SubTopics { get; set; } = new List<SubTopicVo>();

        /// <summary>
        /// Voting rounds recorded for the meeting.
        /// </summary>
        public List<RoundVo> Rounds { get; set; } = new List<RoundVo>();

        /// <summary>
        /// Groups associated with the meeting.
        /// </summary>
        public List<string> Groups { get; set; } = new List<string>();

        /// <summary>
        /// Indicates whether Jira credentials are configured for the meeting.
        /// </summary>
        public bool JiraConnected { get; set; }

        /// <summary>
        /// Creation timestamp as Unix milliseconds.
        /// </summary>
        public long CreatedOn { get; set; }

        /// <summary>
        /// Indicates whether the meeting has ended (equivalent to not running).
        /// </summary>
        public bool IsEnded { get; set; }

        /// <summary>
        /// Indicates whether the meeting is currently running.
        /// </summary>
        public bool IsRunning { get; set; }
    }
}
