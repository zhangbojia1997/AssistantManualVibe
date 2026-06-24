using System.Collections.Generic;
using System.Linq;
using RSP.AgileAssistant.Business.Meeting.Bo;
using RSP.AgileAssistant.Business.Meeting.Vo;

namespace RSP.AgileAssistant.Business.Meeting
{
    /// <summary>
    /// Pure mapping helpers between meeting business objects and value objects
    /// (FR-MEET / FR-VOTE). Computes the polling-derived <c>DisplayingRoundId</c>
    /// and <c>IsEnded</c> fields that are not stored physically.
    /// </summary>
    internal static class MeetingMapper
    {
        /// <summary>
        /// Projects a <see cref="MeetingBo"/> onto a <see cref="MeetingVo"/>.
        /// </summary>
        /// <param name="bo">Source meeting business object.</param>
        internal static MeetingVo ToVo(MeetingBo bo)
        {
            return new MeetingVo
            {
                Id = bo.Id,
                HostId = bo.HostId,
                Topic = bo.Topic,
                DeckId = bo.DeckId,
                VotingOn = bo.VotingOn,
                VotingRound = bo.VotingRound,
                DisplayingRoundId = ResolveDisplayingRoundId(bo),
                Participants = bo.Participants.Select(ToVo).ToList(),
                SubTopics = bo.SubTopics.Select(ToVo).ToList(),
                Rounds = bo.Rounds.Select(ToVo).ToList(),
                Groups = new List<string>(bo.Groups),
                JiraConnected = bo.JiraConnected,
                CreatedOn = bo.CreatedOn,
                IsEnded = bo.Status != MeetingBo.StatusRunning,
                Status = bo.Status,
            };
        }

        /// <summary>
        /// Derives the round whose results should be displayed under the polling
        /// model: the current voting round once it has been marked done.
        /// </summary>
        /// <param name="bo">Source meeting business object.</param>
        private static System.Guid? ResolveDisplayingRoundId(MeetingBo bo)
        {
            if (bo.VotingRound == null)
            {
                return null;
            }

            RoundBo? current = bo.Rounds.FirstOrDefault(r => r.Id == bo.VotingRound.Value);
            if (current != null && current.Status == RoundBo.StatusDone)
            {
                return bo.VotingRound;
            }

            return null;
        }

        /// <summary>Projects a <see cref="ParticipantBo"/> onto a <see cref="ParticipantVo"/>.</summary>
        private static ParticipantVo ToVo(ParticipantBo bo)
        {
            return new ParticipantVo
            {
                Id = bo.Id,
                Name = bo.Name,
                UserId = bo.UserId,
                DeckId = bo.DeckId,
                SelectedPoker = bo.SelectedPoker == null ? null : ToVo(bo.SelectedPoker),
                Group = bo.Group,
                IsPickedPoker = bo.IsPickedPoker,
            };
        }

        /// <summary>Projects a <see cref="ParticipantPokerBo"/> onto a <see cref="ParticipantPokerVo"/>.</summary>
        private static ParticipantPokerVo ToVo(ParticipantPokerBo bo)
        {
            return new ParticipantPokerVo
            {
                Id = bo.Id,
                ParticipantName = bo.ParticipantName,
                PokerId = bo.PokerId,
                OriginalPokerValue = bo.OriginalPokerValue,
                PokerValue = bo.PokerValue,
            };
        }

        /// <summary>Projects a <see cref="RoundBo"/> onto a <see cref="RoundVo"/>.</summary>
        private static RoundVo ToVo(RoundBo bo)
        {
            return new RoundVo
            {
                Id = bo.Id,
                TopicId = bo.TopicId,
                RoundNumber = bo.RoundNumber,
                Status = bo.Status,
                Votes = bo.Votes.Select(ToVo).ToList(),
            };
        }

        /// <summary>Projects a <see cref="RoundVoteBo"/> onto a <see cref="RoundVoteVo"/>.</summary>
        private static RoundVoteVo ToVo(RoundVoteBo bo)
        {
            return new RoundVoteVo
            {
                UserId = bo.UserId,
                Value = bo.Value,
            };
        }

        /// <summary>Projects a <see cref="SubTopicBo"/> onto a <see cref="SubTopicVo"/>.</summary>
        private static SubTopicVo ToVo(SubTopicBo bo)
        {
            return new SubTopicVo
            {
                Id = bo.Id,
                Topic = bo.Topic,
                Desc = bo.Desc,
                Points = bo.Points,
                Tasks = bo.Tasks.Select(ToVo).ToList(),
                JiraKey = bo.JiraKey,
                IssueType = bo.IssueType,
                JiraStatus = bo.JiraStatus,
            };
        }

        /// <summary>Projects a <see cref="TopicTaskBo"/> onto a <see cref="TopicTaskVo"/>.</summary>
        private static TopicTaskVo ToVo(TopicTaskBo bo)
        {
            return new TopicTaskVo
            {
                Id = bo.Id,
                Topic = bo.Topic,
                Desc = bo.Desc,
                Points = bo.Points,
                JiraKey = bo.JiraKey,
                IssueType = bo.IssueType,
                JiraStatus = bo.JiraStatus,
            };
        }
    }
}
