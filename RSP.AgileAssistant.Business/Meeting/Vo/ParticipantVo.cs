using System;
using System.Collections.Generic;

namespace RSP.AgileAssistant.Business.Meeting.Vo
{
    /// <summary>
    /// Output representation of a meeting participant returned to API clients.
    /// </summary>
    public class ParticipantVo
    {
        /// <summary>
        /// Unique identifier of the participant within the meeting.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Display name of the participant.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the underlying application user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Identifier of the deck the participant is currently using.
        /// </summary>
        public Guid DeckId { get; set; }

        /// <summary>
        /// Historical selected cards. Reserved for parity with the front-end
        /// contract; not populated by the current polling implementation.
        /// </summary>
        public List<ParticipantPokerVo> SelectedPokers { get; set; } = new List<ParticipantPokerVo>();

        /// <summary>
        /// The participant's currently selected card, if any.
        /// </summary>
        public ParticipantPokerVo? SelectedPoker { get; set; }

        /// <summary>
        /// Group the participant belongs to.
        /// </summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the participant has picked a card this round.
        /// </summary>
        public bool IsPickedPoker { get; set; }
    }
}
