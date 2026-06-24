using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RSP.AgileAssistant.Business.Auth;
using RSP.AgileAssistant.Business.Base.Actions;
using RSP.AgileAssistant.Business.Consts;
using RSP.AgileAssistant.Business.Meeting.Bo;
using RSP.AgileAssistant.Business.Meeting.Sql;
using RSP.AgileAssistant.Business.User.Bo;
using RSP.AgileAssistant.Business.User.Sql;
using RSP.Common;
using RSP.Common.DataAccess;

namespace RSP.AgileAssistant.Business.Meeting.Actions
{
    /// <summary>
    /// Base class for meeting actions (FR-MEET / FR-VOTE). Provides shared,
    /// transaction-aware helpers to load a meeting aggregate from its physical
    /// tables and to persist it back using a snapshot-replace strategy.
    /// </summary>
    /// <typeparam name="TResult">Result type produced by the action.</typeparam>
    public abstract class MeetingActionBase<TResult> : SqlActionAsyncBase<TResult>
    {
        /// <summary>
        /// Initializes the action with the supplied ADO configuration.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        protected MeetingActionBase(IADOConfigurable adoConfigurable)
            : base(adoConfigurable)
        {
        }

        /// <summary>
        /// JWT token service used to authorize requests. The setter is public so the
        /// IOC container (and the unit-test mock injector) can write it through the
        /// derived action types.
        /// </summary>
        [IOCInjection(InjectionKey = IocConsts.TOKEN_SERVICE)]
        public ITokenService TokenService { get; set; } = null!;

        /// <summary>
        /// Loads a complete meeting aggregate (root, groups, participants, rounds,
        /// votes, sub-topics and tasks) by identifier.
        /// </summary>
        /// <param name="meetingId">Identifier of the meeting to load.</param>
        /// <returns>The assembled meeting, or <c>null</c> when not found.</returns>
        protected async Task<MeetingBo?> LoadMeetingAsync(Guid meetingId)
        {
            MeetingBo? meeting = await this.RunObjQueryAsync(
                MeetingSql.SelectMeetingById,
                MeetingSql.IdParameters(meetingId),
                MeetingSql.MapMeeting);

            if (meeting == null)
            {
                return null;
            }

            await this.PopulateMeetingAsync(meeting);
            return meeting;
        }

        /// <summary>
        /// Loads all meetings (optionally only running ones) as fully assembled
        /// aggregates.
        /// </summary>
        /// <param name="runningOnly">When <c>true</c>, only running meetings are returned.</param>
        /// <returns>The assembled meetings ordered most-recent first.</returns>
        protected async Task<List<MeetingBo>> LoadMeetingsAsync(bool runningOnly)
        {
            string sql = runningOnly ? MeetingSql.SelectRunningMeetingIds : MeetingSql.SelectAllMeetingIds;
            IEnumerable<Guid> ids = await this.RunListQueryAsync(
                sql,
                Array.Empty<System.Data.Common.DbParameter>(),
                row => Guid.Parse((string)row["Id"]));

            List<MeetingBo> meetings = new List<MeetingBo>();
            foreach (Guid id in ids.ToList())
            {
                MeetingBo? meeting = await this.LoadMeetingAsync(id);
                if (meeting != null)
                {
                    meetings.Add(meeting);
                }
            }

            return meetings;
        }

        /// <summary>
        /// Persists a meeting aggregate using a snapshot-replace strategy: the root
        /// row is deleted (cascading to all children) and every part is reinserted.
        /// Must be invoked within a transactional ADO configuration.
        /// </summary>
        /// <param name="meeting">Meeting aggregate to persist.</param>
        protected async Task SaveMeetingSnapshotAsync(MeetingBo meeting)
        {
            await this.RunNonQueryAsync(MeetingSql.DeleteMeeting, MeetingSql.IdParameters(meeting.Id));
            await this.RunNonQueryAsync(MeetingSql.InsertMeeting, MeetingSql.InsertMeetingParameters(meeting));

            foreach (string group in meeting.Groups)
            {
                await this.RunNonQueryAsync(MeetingSql.InsertGroup, MeetingSql.InsertGroupParameters(meeting.Id, group));
            }

            foreach (ParticipantBo participant in meeting.Participants)
            {
                await this.RunNonQueryAsync(
                    MeetingSql.InsertParticipant,
                    MeetingSql.InsertParticipantParameters(meeting.Id, participant));

                if (participant.SelectedPoker != null)
                {
                    await this.RunNonQueryAsync(
                        MeetingSql.InsertPoker,
                        MeetingSql.InsertPokerParameters(participant.Id, participant.SelectedPoker));
                }
            }

            foreach (SubTopicBo subTopic in meeting.SubTopics)
            {
                await this.RunNonQueryAsync(
                    MeetingSql.InsertTopicTask,
                    MeetingSql.InsertTopicTaskParameters(
                        meeting.Id,
                        subTopic.Id,
                        subTopic.Topic,
                        subTopic.Desc,
                        subTopic.Points,
                        subTopic.JiraKey,
                        subTopic.IssueType,
                        subTopic.JiraStatus));

                foreach (TopicTaskBo task in subTopic.Tasks)
                {
                    await this.RunNonQueryAsync(
                        MeetingSql.InsertTopicTask,
                        MeetingSql.InsertTopicTaskParameters(
                            meeting.Id,
                            task.Id,
                            task.Topic,
                            task.Desc,
                            task.Points,
                            task.JiraKey,
                            task.IssueType,
                            task.JiraStatus));

                    await this.RunNonQueryAsync(
                        MeetingSql.InsertTaskRelation,
                        MeetingSql.InsertTaskRelationParameters(subTopic.Id, task.Id));
                }
            }

            foreach (RoundBo round in meeting.Rounds)
            {
                await this.RunNonQueryAsync(MeetingSql.InsertRound, MeetingSql.InsertRoundParameters(meeting.Id, round));

                foreach (RoundVoteBo vote in round.Votes)
                {
                    await this.RunNonQueryAsync(MeetingSql.InsertVote, MeetingSql.InsertVoteParameters(round.Id, vote));
                }
            }
        }

        /// <summary>
        /// Loads a user by identifier, reusing the user feature's SQL.
        /// </summary>
        /// <param name="userId">Identifier of the user to load.</param>
        /// <returns>The user, or <c>null</c> when not found.</returns>
        protected async Task<UserBo?> GetUserAsync(Guid userId)
        {
            return await this.RunObjQueryAsync(
                UserSql.SelectById,
                UserSql.SelectByIdParameters(userId),
                UserSql.MapUser);
        }

        /// <summary>
        /// Loads the child collections of a meeting and attaches them to it.
        /// </summary>
        /// <param name="meeting">Meeting whose children are loaded.</param>
        private async Task PopulateMeetingAsync(MeetingBo meeting)
        {
            // Groups.
            IEnumerable<string> groups = await this.RunListQueryAsync(
                MeetingSql.SelectGroupsByMeeting,
                MeetingSql.MeetingIdParameters(meeting.Id),
                MeetingSql.MapGroup);
            meeting.Groups = groups.ToList();

            // Participants and their selected cards.
            List<(ParticipantBo Participant, Guid? SelectedPokerId)> participants =
                (await this.RunListQueryAsync(
                    MeetingSql.SelectParticipantsByMeeting,
                    MeetingSql.MeetingIdParameters(meeting.Id),
                    MeetingSql.MapParticipant)).ToList();

            List<(Guid ParticipantId, ParticipantPokerBo Poker)> pokers =
                (await this.RunListQueryAsync(
                    MeetingSql.SelectPokersByMeeting,
                    MeetingSql.MeetingIdParameters(meeting.Id),
                    MeetingSql.MapPoker)).ToList();

            Dictionary<Guid, ParticipantPokerBo> pokerById = pokers
                .GroupBy(p => p.Poker.Id)
                .ToDictionary(g => g.Key, g => g.First().Poker);

            foreach ((ParticipantBo participant, Guid? selectedPokerId) in participants)
            {
                if (selectedPokerId != null && pokerById.TryGetValue(selectedPokerId.Value, out ParticipantPokerBo? selected))
                {
                    participant.SelectedPoker = selected;
                }

                meeting.Participants.Add(participant);
            }

            // Rounds and their votes.
            List<RoundBo> rounds = (await this.RunListQueryAsync(
                MeetingSql.SelectRoundsByMeeting,
                MeetingSql.MeetingIdParameters(meeting.Id),
                MeetingSql.MapRound)).ToList();

            List<(Guid RoundId, RoundVoteBo Vote)> votes = (await this.RunListQueryAsync(
                MeetingSql.SelectVotesByMeeting,
                MeetingSql.MeetingIdParameters(meeting.Id),
                MeetingSql.MapVote)).ToList();

            ILookup<Guid, RoundVoteBo> votesByRound = votes.ToLookup(v => v.RoundId, v => v.Vote);
            foreach (RoundBo round in rounds)
            {
                round.Votes = votesByRound[round.Id].ToList();
            }

            meeting.Rounds = rounds;

            // Sub-topics and tasks.
            List<SubTopicBo> topicTasks = (await this.RunListQueryAsync(
                MeetingSql.SelectTopicTasksByMeeting,
                MeetingSql.MeetingIdParameters(meeting.Id),
                MeetingSql.MapTopicTask)).ToList();

            List<(Guid Id1, Guid Id2)> relations = (await this.RunListQueryAsync(
                MeetingSql.SelectTaskRelationsByMeeting,
                MeetingSql.MeetingIdParameters(meeting.Id),
                MeetingSql.MapTaskRelation)).ToList();

            meeting.SubTopics = BuildSubTopics(topicTasks, relations);
        }

        /// <summary>
        /// Reconstructs the sub-topic / task hierarchy from the flat topic-task rows
        /// and their parent/child relations. Rows referenced as a child are treated
        /// as tasks; all other rows are sub-topics.
        /// </summary>
        /// <param name="rows">All topic-task rows for the meeting.</param>
        /// <param name="relations">Parent (Id1) to child (Id2) relations.</param>
        private static List<SubTopicBo> BuildSubTopics(
            List<SubTopicBo> rows,
            List<(Guid Id1, Guid Id2)> relations)
        {
            Dictionary<Guid, SubTopicBo> rowById = rows.ToDictionary(r => r.Id);
            HashSet<Guid> childIds = new HashSet<Guid>(relations.Select(r => r.Id2));
            ILookup<Guid, Guid> childrenByParent = relations.ToLookup(r => r.Id1, r => r.Id2);

            List<SubTopicBo> subTopics = new List<SubTopicBo>();
            foreach (SubTopicBo row in rows)
            {
                if (childIds.Contains(row.Id))
                {
                    continue;
                }

                foreach (Guid childId in childrenByParent[row.Id])
                {
                    if (rowById.TryGetValue(childId, out SubTopicBo? child))
                    {
                        row.Tasks.Add(new TopicTaskBo
                        {
                            Id = child.Id,
                            Topic = child.Topic,
                            Desc = child.Desc,
                            Points = child.Points,
                            JiraKey = child.JiraKey,
                            IssueType = child.IssueType,
                            JiraStatus = child.JiraStatus,
                        });
                    }
                }

                subTopics.Add(row);
            }

            return subTopics;
        }
    }
}
