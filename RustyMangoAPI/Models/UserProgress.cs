using RustyMangoApi.Models;

namespace RustyMangoAPI.Models
{
    public class UserProgress
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int LevelNumber { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsCompleted { get; set; }
    }
}
