using System.ComponentModel.DataAnnotations;

namespace WordGame.Models
{
    public class PlayerModeSelectionViewModel
    {
        [Required]
        public string SelectedMode { get; set; } = "";
    }
}
