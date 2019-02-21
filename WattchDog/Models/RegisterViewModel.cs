using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WattchDog.Attributes;

namespace WattchDog.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Required")]
        [MinLength(8, ErrorMessage = "Must be at least 8 characters long")]
        [UniqueUsername(ErrorMessage = "Username is already taken")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Required")]
        [EmailAddress(ErrorMessage = "Must be a valid email address")]
        [UniqueEmail(ErrorMessage = "Email is already registered to an account")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required")]
        [MinLength(8, ErrorMessage = "Must be at least 8 characters long")]
        public string Password { get; set; }
    }
}