using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using RSP.AgileAssistant.Business.Meeting.Bo;

namespace RSP.AgileAssistant.Business.Meeting.Sql
{
    /// <summary>
    /// Parameterized SQL statements, parameter builders and row mappers for the
    /// meeting aggregate (FR-MEET / FR-VOTE). All values are bound through
    /// <see cref="DbParameter"/> instances to prevent SQL injection. GUIDs are
    /// stored as <c>NVARCHAR(36)</c> strings to match the ER 2.3.2 physical model.
    /// </summary>
    internal static class MeetingSql
    {
        // -------------------------------------------------------------------
        // SELECT statements
        // -------------------------------------------------------------------

        /// <summary>Selects a single meeting (root row) by identifier.</summary>
        internal const string SelectMeetingById = @"
SELECT Id, HostId, Topic, DeckId, VotingOn, VotingRound, Status,
       LastActiveDate, JiraEmail, JiraToken, JiraConnected, CreatedOn
FROM Vibe_Meeting_Table
WHERE Id = @Id";

        /// <summary>Selects the identifiers of all meetings.</summary>
        internal const string SelectAllMeetingIds = @"
SELECT Id
FROM Vibe_Meeting_Table
ORDER BY CreatedOn DESC";

        /// <summary>Selects the identifiers of all running meetings.</summary>
        internal const string SelectRunningMeetingIds = @"
SELECT Id
FROM Vibe_Meeting_Table
WHERE Status = 1
ORDER BY CreatedOn DESC";

        /// <summary>Selects the groups of a meeting.</summary>
        internal const string SelectGroupsByMeeting = @"
SELECT [Group]
FROM Vibe_MeetingGroup
WHERE MeetingId = @MeetingId";

        /// <summary>Selects the participants of a meeting.</summary>
        internal const string SelectParticipantsByMeeting = @"
SELECT Id, UserId, [Name], DeckId, [Group], IsPickedPoker, SelectedPokerId
FROM Vibe_Participant_Table
WHERE MeetingId = @MeetingId";

        /// <summary>Selects the selected cards of a meeting's participants.</summary>
        internal const string SelectPokersByMeeting = @"
SELECT p.Id, p.ParticipantName, p.PokerId, p.OriginalPokerValue, p.PokerValue, p.ParticipantId
FROM Vibe_ParticipantPoker_Table p
INNER JOIN Vibe_Participant_Table pa ON p.ParticipantId = pa.Id
WHERE pa.MeetingId = @MeetingId";

        /// <summary>Selects the rounds of a meeting.</summary>
        internal const string SelectRoundsByMeeting = @"
SELECT Id, TopicId, RoundNumber, [Status]
FROM Vibe_Round_Table
WHERE MeetingId = @MeetingId
ORDER BY RoundNumber";

        /// <summary>Selects the votes of a meeting's rounds.</summary>
        internal const string SelectVotesByMeeting = @"
SELECT v.RoundId, v.UserId, v.[Value]
FROM Vibe_RoundVote_Table v
INNER JOIN Vibe_Round_Table r ON v.RoundId = r.Id
WHERE r.MeetingId = @MeetingId";

        /// <summary>Selects the topic tasks (sub-topics and tasks) of a meeting.</summary>
        internal const string SelectTopicTasksByMeeting = @"
SELECT Id, Topic, [Desc], Points, JiraKey, IssueType, JiraStatus
FROM Vibe_TopicTask_Table
WHERE MeetingId = @MeetingId";

        /// <summary>Selects the parent/child relations of a meeting's topic tasks.</summary>
        internal const string SelectTaskRelationsByMeeting = @"
SELECT rel.Id1, rel.Id2
FROM Vibe_TopicTask_To_TopicTask rel
INNER JOIN Vibe_TopicTask_Table t ON rel.Id1 = t.Id
WHERE t.MeetingId = @MeetingId";

        // -------------------------------------------------------------------
        // INSERT statements
        // -------------------------------------------------------------------

        /// <summary>Inserts a meeting (root row).</summary>
        internal const string InsertMeeting = @"
INSERT INTO Vibe_Meeting_Table
    (Id, HostId, Topic, DeckId, VotingOn, VotingRound, Status, LastActiveDate, JiraEmail, JiraToken, JiraConnected, CreatedOn)
VALUES
    (@Id, @HostId, @Topic, @DeckId, @VotingOn, @VotingRound, @Status, @LastActiveDate, @JiraEmail, @JiraToken, @JiraConnected, @CreatedOn)";

        /// <summary>Inserts a meeting group.</summary>
        internal const string InsertGroup = @"
INSERT INTO Vibe_MeetingGroup ([Group], MeetingId)
VALUES (@Group, @MeetingId)";

        /// <summary>Inserts a participant.</summary>
        internal const string InsertParticipant = @"
INSERT INTO Vibe_Participant_Table
    (Id, UserId, [Name], DeckId, [Group], IsPickedPoker, SelectedPokerId, MeetingId)
VALUES
    (@Id, @UserId, @Name, @DeckId, @Group, @IsPickedPoker, @SelectedPokerId, @MeetingId)";

        /// <summary>Inserts a participant's selected card.</summary>
        internal const string InsertPoker = @"
INSERT INTO Vibe_ParticipantPoker_Table
    (Id, ParticipantName, PokerId, OriginalPokerValue, PokerValue, ParticipantId)
VALUES
    (@Id, @ParticipantName, @PokerId, @OriginalPokerValue, @PokerValue, @ParticipantId)";

        /// <summary>Inserts a voting round.</summary>
        internal const string InsertRound = @"
INSERT INTO Vibe_Round_Table (Id, TopicId, RoundNumber, [Status], MeetingId)
VALUES (@Id, @TopicId, @RoundNumber, @Status, @MeetingId)";

        /// <summary>Inserts a round vote (Id is identity, omitted).</summary>
        internal const string InsertVote = @"
INSERT INTO Vibe_RoundVote_Table ([Value], UserId, RoundId)
VALUES (@Value, @UserId, @RoundId)";

        /// <summary>Inserts a topic task (sub-topic or task).</summary>
        internal const string InsertTopicTask = @"
INSERT INTO Vibe_TopicTask_Table
    (Id, Topic, [Desc], Points, JiraKey, IssueType, JiraStatus, MeetingId)
VALUES
    (@Id, @Topic, @Desc, @Points, @JiraKey, @IssueType, @JiraStatus, @MeetingId)";

        /// <summary>Inserts a parent/child topic-task relation.</summary>
        internal const string InsertTaskRelation = @"
INSERT INTO Vibe_TopicTask_To_TopicTask (Id1, Id2)
VALUES (@Id1, @Id2)";

        // -------------------------------------------------------------------
        // UPDATE / DELETE statements
        // -------------------------------------------------------------------

        /// <summary>Updates a meeting's status and activity timestamp.</summary>
        internal const string UpdateStatus = @"
UPDATE Vibe_Meeting_Table
    SET Status = @Status, LastActiveDate = @LastActiveDate
WHERE Id = @Id";

        /// <summary>
        /// Deletes a meeting (root row). All child rows are removed by the
        /// cascading foreign keys defined in the physical model.
        /// </summary>
        internal const string DeleteMeeting = @"
DELETE FROM Vibe_Meeting_Table WHERE Id = @Id";

        /// <summary>Checks whether a meeting exists.</summary>
        internal const string MeetingExists = @"
SELECT COUNT(1) FROM Vibe_Meeting_Table WHERE Id = @Id";

        // -------------------------------------------------------------------
        // Parameter builders
        // -------------------------------------------------------------------

        /// <summary>Builds an <c>@Id</c> parameter.</summary>
        internal static DbParameter[] IdParameters(Guid id)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar, 36) { Value = id.ToString() },
            };
        }

        /// <summary>Builds a <c>@MeetingId</c> parameter.</summary>
        internal static DbParameter[] MeetingIdParameters(Guid meetingId)
        {
            return new DbParameter[]
            {
                new SqlParameter("@MeetingId", SqlDbType.NVarChar, 36) { Value = meetingId.ToString() },
            };
        }

        /// <summary>Builds parameters for <see cref="InsertMeeting"/>.</summary>
        internal static DbParameter[] InsertMeetingParameters(MeetingBo meeting)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar, 36) { Value = meeting.Id.ToString() },
                new SqlParameter("@HostId", SqlDbType.NVarChar, 36) { Value = meeting.HostId == Guid.Empty ? (object)DBNull.Value : meeting.HostId.ToString() },
                new SqlParameter("@Topic", SqlDbType.NVarChar) { Value = (object?)meeting.Topic ?? string.Empty },
                new SqlParameter("@DeckId", SqlDbType.NVarChar, 36) { Value = meeting.DeckId.ToString() },
                new SqlParameter("@VotingOn", SqlDbType.NVarChar, 36) { Value = (object?)meeting.VotingOn?.ToString() ?? DBNull.Value },
                new SqlParameter("@VotingRound", SqlDbType.NVarChar, 36) { Value = (object?)meeting.VotingRound?.ToString() ?? DBNull.Value },
                new SqlParameter("@Status", SqlDbType.Int) { Value = meeting.Status },
                new SqlParameter("@LastActiveDate", SqlDbType.DateTime2) { Value = meeting.LastActiveDate },
                new SqlParameter("@JiraEmail", SqlDbType.NVarChar) { Value = (object?)meeting.JiraEmail ?? string.Empty },
                new SqlParameter("@JiraToken", SqlDbType.NVarChar) { Value = (object?)meeting.JiraToken ?? string.Empty },
                new SqlParameter("@JiraConnected", SqlDbType.Bit) { Value = meeting.JiraConnected },
                new SqlParameter("@CreatedOn", SqlDbType.BigInt) { Value = meeting.CreatedOn },
            };
        }

        /// <summary>Builds parameters for <see cref="InsertGroup"/>.</summary>
        internal static DbParameter[] InsertGroupParameters(Guid meetingId, string group)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Group", SqlDbType.NVarChar, 100) { Value = (object?)group ?? string.Empty },
                new SqlParameter("@MeetingId", SqlDbType.NVarChar, 36) { Value = meetingId.ToString() },
            };
        }

        /// <summary>Builds parameters for <see cref="InsertParticipant"/>.</summary>
        internal static DbParameter[] InsertParticipantParameters(Guid meetingId, ParticipantBo participant)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar, 36) { Value = participant.Id.ToString() },
                new SqlParameter("@UserId", SqlDbType.NVarChar, 36) { Value = participant.UserId.ToString() },
                new SqlParameter("@Name", SqlDbType.NVarChar) { Value = (object?)participant.Name ?? string.Empty },
                new SqlParameter("@DeckId", SqlDbType.NVarChar, 36) { Value = participant.DeckId.ToString() },
                new SqlParameter("@Group", SqlDbType.NVarChar) { Value = (object?)participant.Group ?? string.Empty },
                new SqlParameter("@IsPickedPoker", SqlDbType.Bit) { Value = participant.IsPickedPoker },
                new SqlParameter("@SelectedPokerId", SqlDbType.NVarChar, 36) { Value = (object?)participant.SelectedPoker?.Id.ToString() ?? DBNull.Value },
                new SqlParameter("@MeetingId", SqlDbType.NVarChar, 36) { Value = meetingId.ToString() },
            };
        }

        /// <summary>Builds parameters for <see cref="InsertPoker"/>.</summary>
        internal static DbParameter[] InsertPokerParameters(Guid participantId, ParticipantPokerBo poker)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar, 36) { Value = poker.Id.ToString() },
                new SqlParameter("@ParticipantName", SqlDbType.NVarChar) { Value = (object?)poker.ParticipantName ?? string.Empty },
                new SqlParameter("@PokerId", SqlDbType.NVarChar, 36) { Value = poker.PokerId.ToString() },
                new SqlParameter("@OriginalPokerValue", SqlDbType.NVarChar) { Value = (object?)poker.OriginalPokerValue ?? string.Empty },
                new SqlParameter("@PokerValue", SqlDbType.NVarChar) { Value = (object?)poker.PokerValue ?? string.Empty },
                new SqlParameter("@ParticipantId", SqlDbType.NVarChar, 36) { Value = participantId.ToString() },
            };
        }

        /// <summary>Builds parameters for <see cref="InsertRound"/>.</summary>
        internal static DbParameter[] InsertRoundParameters(Guid meetingId, RoundBo round)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar, 36) { Value = round.Id.ToString() },
                new SqlParameter("@TopicId", SqlDbType.NVarChar, 36) { Value = (object?)round.TopicId?.ToString() ?? DBNull.Value },
                new SqlParameter("@RoundNumber", SqlDbType.Int) { Value = round.RoundNumber },
                new SqlParameter("@Status", SqlDbType.NVarChar, 100) { Value = (object?)round.Status ?? RoundBo.StatusVoting },
                new SqlParameter("@MeetingId", SqlDbType.NVarChar, 36) { Value = meetingId.ToString() },
            };
        }

        /// <summary>Builds parameters for <see cref="InsertVote"/>.</summary>
        internal static DbParameter[] InsertVoteParameters(Guid roundId, RoundVoteBo vote)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Value", SqlDbType.NVarChar, 100) { Value = (object?)vote.Value ?? string.Empty },
                new SqlParameter("@UserId", SqlDbType.NVarChar, 36) { Value = vote.UserId.ToString() },
                new SqlParameter("@RoundId", SqlDbType.NVarChar, 36) { Value = roundId.ToString() },
            };
        }

        /// <summary>Builds parameters for <see cref="InsertTopicTask"/>.</summary>
        internal static DbParameter[] InsertTopicTaskParameters(
            Guid meetingId,
            Guid id,
            string topic,
            string desc,
            float? points,
            string? jiraKey,
            string issueType,
            string jiraStatus)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar, 36) { Value = id.ToString() },
                new SqlParameter("@Topic", SqlDbType.NVarChar) { Value = (object?)topic ?? string.Empty },
                new SqlParameter("@Desc", SqlDbType.NVarChar) { Value = (object?)desc ?? string.Empty },
                new SqlParameter("@Points", SqlDbType.Float) { Value = (object?)points ?? DBNull.Value },
                new SqlParameter("@JiraKey", SqlDbType.NVarChar) { Value = (object?)jiraKey ?? DBNull.Value },
                new SqlParameter("@IssueType", SqlDbType.NVarChar) { Value = (object?)issueType ?? string.Empty },
                new SqlParameter("@JiraStatus", SqlDbType.NVarChar) { Value = (object?)jiraStatus ?? string.Empty },
                new SqlParameter("@MeetingId", SqlDbType.NVarChar, 36) { Value = meetingId.ToString() },
            };
        }

        /// <summary>Builds parameters for <see cref="InsertTaskRelation"/>.</summary>
        internal static DbParameter[] InsertTaskRelationParameters(Guid id1, Guid id2)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Id1", SqlDbType.NVarChar, 36) { Value = id1.ToString() },
                new SqlParameter("@Id2", SqlDbType.NVarChar, 36) { Value = id2.ToString() },
            };
        }

        /// <summary>Builds parameters for <see cref="UpdateStatus"/>.</summary>
        internal static DbParameter[] UpdateStatusParameters(Guid id, int status, DateTime lastActiveDate)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar, 36) { Value = id.ToString() },
                new SqlParameter("@Status", SqlDbType.Int) { Value = status },
                new SqlParameter("@LastActiveDate", SqlDbType.DateTime2) { Value = lastActiveDate },
            };
        }

        // -------------------------------------------------------------------
        // Row mappers
        // -------------------------------------------------------------------

        /// <summary>Maps a row from <see cref="SelectMeetingById"/> to a <see cref="MeetingBo"/>.</summary>
        internal static MeetingBo MapMeeting(DataRow row)
        {
            return new MeetingBo
            {
                Id = Guid.Parse((string)row["Id"]),
                HostId = row["HostId"] == DBNull.Value ? Guid.Empty : Guid.Parse((string)row["HostId"]),
                Topic = row["Topic"] == DBNull.Value ? string.Empty : (string)row["Topic"],
                DeckId = row["DeckId"] == DBNull.Value ? Guid.Empty : Guid.Parse((string)row["DeckId"]),
                VotingOn = row["VotingOn"] == DBNull.Value ? null : Guid.Parse((string)row["VotingOn"]),
                VotingRound = row["VotingRound"] == DBNull.Value ? null : Guid.Parse((string)row["VotingRound"]),
                Status = Convert.ToInt32(row["Status"]),
                LastActiveDate = (DateTime)row["LastActiveDate"],
                JiraEmail = row["JiraEmail"] == DBNull.Value ? string.Empty : (string)row["JiraEmail"],
                JiraToken = row["JiraToken"] == DBNull.Value ? string.Empty : (string)row["JiraToken"],
                JiraConnected = (bool)row["JiraConnected"],
                CreatedOn = Convert.ToInt64(row["CreatedOn"]),
            };
        }

        /// <summary>Maps a row from <see cref="SelectGroupsByMeeting"/> to a group name.</summary>
        internal static string MapGroup(DataRow row)
        {
            return row["Group"] == DBNull.Value ? string.Empty : (string)row["Group"];
        }

        /// <summary>
        /// Maps a row from <see cref="SelectParticipantsByMeeting"/> to a participant
        /// and its referenced selected-card identifier.
        /// </summary>
        internal static (ParticipantBo Participant, Guid? SelectedPokerId) MapParticipant(DataRow row)
        {
            ParticipantBo participant = new ParticipantBo
            {
                Id = Guid.Parse((string)row["Id"]),
                UserId = row["UserId"] == DBNull.Value ? Guid.Empty : Guid.Parse((string)row["UserId"]),
                Name = row["Name"] == DBNull.Value ? string.Empty : (string)row["Name"],
                DeckId = row["DeckId"] == DBNull.Value ? Guid.Empty : Guid.Parse((string)row["DeckId"]),
                Group = row["Group"] == DBNull.Value ? string.Empty : (string)row["Group"],
                IsPickedPoker = (bool)row["IsPickedPoker"],
            };

            Guid? selectedPokerId = row["SelectedPokerId"] == DBNull.Value
                ? null
                : Guid.Parse((string)row["SelectedPokerId"]);

            return (participant, selectedPokerId);
        }

        /// <summary>
        /// Maps a row from <see cref="SelectPokersByMeeting"/> to a poker and its
        /// owning participant identifier.
        /// </summary>
        internal static (Guid ParticipantId, ParticipantPokerBo Poker) MapPoker(DataRow row)
        {
            ParticipantPokerBo poker = new ParticipantPokerBo
            {
                Id = Guid.Parse((string)row["Id"]),
                ParticipantName = row["ParticipantName"] == DBNull.Value ? string.Empty : (string)row["ParticipantName"],
                PokerId = row["PokerId"] == DBNull.Value ? Guid.Empty : Guid.Parse((string)row["PokerId"]),
                OriginalPokerValue = row["OriginalPokerValue"] == DBNull.Value ? string.Empty : (string)row["OriginalPokerValue"],
                PokerValue = row["PokerValue"] == DBNull.Value ? string.Empty : (string)row["PokerValue"],
            };

            Guid participantId = row["ParticipantId"] == DBNull.Value
                ? Guid.Empty
                : Guid.Parse((string)row["ParticipantId"]);

            return (participantId, poker);
        }

        /// <summary>Maps a row from <see cref="SelectRoundsByMeeting"/> to a <see cref="RoundBo"/>.</summary>
        internal static RoundBo MapRound(DataRow row)
        {
            return new RoundBo
            {
                Id = Guid.Parse((string)row["Id"]),
                TopicId = row["TopicId"] == DBNull.Value ? null : Guid.Parse((string)row["TopicId"]),
                RoundNumber = Convert.ToInt32(row["RoundNumber"]),
                Status = row["Status"] == DBNull.Value ? RoundBo.StatusVoting : (string)row["Status"],
            };
        }

        /// <summary>
        /// Maps a row from <see cref="SelectVotesByMeeting"/> to a vote and its
        /// owning round identifier.
        /// </summary>
        internal static (Guid RoundId, RoundVoteBo Vote) MapVote(DataRow row)
        {
            RoundVoteBo vote = new RoundVoteBo
            {
                UserId = row["UserId"] == DBNull.Value ? Guid.Empty : Guid.Parse((string)row["UserId"]),
                Value = row["Value"] == DBNull.Value ? string.Empty : (string)row["Value"],
            };

            Guid roundId = Guid.Parse((string)row["RoundId"]);
            return (roundId, vote);
        }

        /// <summary>
        /// Maps a row from <see cref="SelectTopicTasksByMeeting"/> to a sub-topic
        /// business object (also used to build child tasks).
        /// </summary>
        internal static SubTopicBo MapTopicTask(DataRow row)
        {
            return new SubTopicBo
            {
                Id = Guid.Parse((string)row["Id"]),
                Topic = row["Topic"] == DBNull.Value ? string.Empty : (string)row["Topic"],
                Desc = row["Desc"] == DBNull.Value ? string.Empty : (string)row["Desc"],
                Points = row["Points"] == DBNull.Value ? null : Convert.ToSingle(row["Points"]),
                JiraKey = row["JiraKey"] == DBNull.Value ? null : (string)row["JiraKey"],
                IssueType = row["IssueType"] == DBNull.Value ? string.Empty : (string)row["IssueType"],
                JiraStatus = row["JiraStatus"] == DBNull.Value ? string.Empty : (string)row["JiraStatus"],
            };
        }

        /// <summary>Maps a row from <see cref="SelectTaskRelationsByMeeting"/> to a parent/child pair.</summary>
        internal static (Guid Id1, Guid Id2) MapTaskRelation(DataRow row)
        {
            return (Guid.Parse((string)row["Id1"]), Guid.Parse((string)row["Id2"]));
        }
    }
}
