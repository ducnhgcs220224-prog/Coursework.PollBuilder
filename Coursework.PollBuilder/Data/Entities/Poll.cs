using System;
using System.Collections.Generic;

namespace Coursework.PollBuilder.Data.Entities
{
    public class Poll
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string Options { get; set; } = string.Empty;
        public bool IsClosed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property thể hiện mối quan hệ 1-n: 1 Poll có nhiều Vote
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}