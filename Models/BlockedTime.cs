using System;
using System.ComponentModel.DataAnnotations;

namespace PRIV.Models
{
    public class BlockedTime
    {
        public int BlockId { get; set; }
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Note { get; set; }

        public User? User { get; set; }
    }
}