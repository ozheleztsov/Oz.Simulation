using System.ComponentModel.DataAnnotations;

namespace Oz.Blog.ViewModels;

public class CreateAnnotationViewModel
{
    [Display(Name = "Annotation text")]
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public Guid BlogPostId { get; set; }
}