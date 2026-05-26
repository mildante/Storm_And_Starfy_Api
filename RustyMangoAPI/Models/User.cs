using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RustyMangoApi.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Login { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int TotalScore { get; set; } = 0;

    }
}