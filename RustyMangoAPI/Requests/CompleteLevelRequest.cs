using System.ComponentModel.DataAnnotations;

namespace RustyMangoAPI.Requests
{
    public class CompleteLevelRequest
    {
        public int UserId { get; set; }
        public int LevelNumber { get; set; }
        public int Score { get; set; }
    }
}