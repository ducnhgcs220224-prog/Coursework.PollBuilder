using System;

namespace Coursework.PollBuilder.Data.Entities
{
    public class Vote
    {
        public Guid Id { get; set; }
        public Guid PollId { get; set; }
        public int OptionIndex { get; set; }
        public string VoterToken { get; set; } = string.Empty;
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property: Trỏ về thực thể Poll cha
        public virtual Poll Poll { get; set; } = null!;
    }
}