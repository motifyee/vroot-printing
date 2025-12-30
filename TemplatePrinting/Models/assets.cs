
using System.ComponentModel.DataAnnotations;

namespace TemplatePrinting.Models;

public enum Assets {
  [Display(Name = "Assets/print_stamp.png")]
  PrintStamp,

  [Display(Name = "Assets/print_stamp_72.png")]
  PrintStamp72
}