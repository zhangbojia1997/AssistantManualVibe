using System;

namespace RSP.AgileAssistant.Business.Meeting.Bo
{
    /// <summary>
    /// Internal representation of a task belonging to a sub-topic, as stored in
    /// the <c>Vibe_TopicTask_Table</c> table.
    /// </summary>
    public class TopicTaskBo
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
    }
}
