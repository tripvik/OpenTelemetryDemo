using System.ComponentModel.DataAnnotations;

namespace SampleWebApp.Models
{
    public class ExceptionModel
    {
        [Required]
        [Display(Name = "Exception Type")]
        public string Type { get; set; } = default!;

        [Required]
        public bool IsHandled { get; set; }
    }
}
