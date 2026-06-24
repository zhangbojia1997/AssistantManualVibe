using System;

namespace RSP.AgileAssistant.Business.Meeting.Vo
{
    /// <summary>
    /// Output representation of a task belonging to a sub-topic.
    /// </summary>
    public class TopicTaskVo
    {
        /// <summary>
        /// Unique identifier of the task.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Title of the task.
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Description of the task.
        /// </summary>
        public string Desc { get; set; } = string.Empty;

        /// <summary>
        /// Estimated points for the task, if any.
        /// </summary>
        public float? Points { get; set; }

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
