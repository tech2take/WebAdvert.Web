using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAdvert.Web.Models.Accounts
{
    public class ConfirmModel
    {
        [Required( ErrorMessage ="Error Message is required.")]
        [Display(Name ="Email")]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Security code is required.")]
        [Display(Name = "Security Code")]
        public string Code { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(6, ErrorMessage = "Password must six characters long.")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and Confirm Password shuold be same.")]
        public string ConfirmPassword { get; set; }

        public bool codeRevieved { get; set; }
    }
}
