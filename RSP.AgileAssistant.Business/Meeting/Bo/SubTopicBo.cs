using System;
using System.Collections.Generic;

namespace RSP.AgileAssistant.Business.Meeting.Bo
{
    /// <summary>
    /// Internal representation of a sub-topic (user story) as stored in the
    /// <c>Vibe_TopicTask_Table</c> table. A sub-topic owns a collection of tasks
    /// linked through <c>Vibe_TopicTask_To_TopicTask</c>.
    /// </summary>
    public class SubTopicBo
    {
        /// <summary>
        /// Unique identifier of the sub-topic.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Title of the sub-topic.
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Description of the sub-topic.
        /// </summary>
        public string Desc { get; set; } = string.Empty;

        /// <summary>
        /// Estimated story points, if assigned.
        /// </summary>
        public float? Points { get; set; }

        /// <summary>
        /// Associated Jira issue key, if linked.
        /// </summary>
        public string? JiraKey { get; set; }

        /// <summary>
        /// Jira issue type, if linked.
        /// </summary>
        public string IssueType { get; set; } = string.Empty;

        /// <summary>
        /// Jira status, if linked.
        /// </summary>
        public string JiraStatus { get; set; } = string.Empty;

        /// <summary>
        /// Child tasks of the sub-topic.
        /// </summary>
        public List<TopicTaskBo> Tasks { get; set; } = new List<TopicTaskBo>();
    }
}
