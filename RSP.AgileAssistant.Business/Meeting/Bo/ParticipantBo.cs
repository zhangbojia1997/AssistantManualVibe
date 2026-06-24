using System;

namespace RSP.AgileAssistant.Business.Meeting.Bo
{
    /// <summary>
    /// Internal representation of a meeting participant as stored in the
    /// <c>Vibe_Participant_Table</c> table.
    /// </summary>
    public class ParticipantBo
    {
        /// <summary>
        /// Unique identifier of the participant within the meeting.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the underlying application user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Display name of the participant (logical identity within the meeting).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the deck the participant is currently using.
        /// </summary>
        public Guid DeckId { get; set; }

        /// <summary>
        /// Group the participant belongs to.
        /// </summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the participant has picked a card this round.
        /// </summary>
        public bool IsPickedPoker { get; set; }

        /// <summary>
        /// The participant's currently selected card, if any.
        /// </summary>
        public ParticipantPokerBo? SelectedPoker { get; set; }
    }
}
