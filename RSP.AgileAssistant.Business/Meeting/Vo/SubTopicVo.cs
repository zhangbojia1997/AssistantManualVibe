using System;
using System.Collections.Generic;

namespace RSP.AgileAssistant.Business.Meeting.Vo
{
    /// <summary>
    /// Output representation of a meeting sub-topic (user story).
    /// </summary>
    public class SubTopicVo
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
        /// Estimated points for the sub-topic, if any.
        /// </summary>
        public float? Points { get; set; }

        /// <summary>
        /// Child tasks of the sub-topic.
        /// </summary>
        public List<TopicTaskVo> Tasks { get; set; } = new List<TopicTaskVo>();

        /// <summary>
        /// Linked Jira issue key, if any.
        /// </summary>
        public string? JiraKey { get; set; }

        /// <summary>
        /// Jira issue type, if any.
        /// </summary>
        public string IssueType { get; set; } = string.Empty;

        /// <summary>
        /// Jira issue status, if any.
        /// </summary>
        public string JiraStatus { get; set; } = string.Empty;
    }
}
