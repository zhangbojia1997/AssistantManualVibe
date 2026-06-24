using System;

namespace RSP.AgileAssistant.Business.Meeting.Dto
{
    /// <summary>
    /// Card selection payload nested in <see cref="SelectPokersDto"/>.
    /// </summary>
    public class ParticipantPokerDto
    {
        /// <summary>
        /// Identifier of the selected-card record. When empty, a new identifier is
        /// generated server-side.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the participant who selected the card.
        /// </summary>
        public string ParticipantName { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the underlying poker card.
        /// </summary>
        public Guid PokerId { get; set; }

        /// <summary>
        /// The card's original (unmodified) face value.
        /// </summary>
        public string OriginalPokerValue { get; set; } = string.Empty;

        /// <summary>
        /// The card's current face value.
        /// </summary>
        public string PokerValue { get; set; } = string.Empty;
    }
}
