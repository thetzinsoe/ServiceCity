using System.ComponentModel.DataAnnotations;

namespace ServiceCity.Models;

public class TrackViewModel
{
    [Required(ErrorMessage = "Search term is required.")]
    public string SearchTerm { get; set; } = string.Empty;
}
