using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WattchDB;

namespace WattchDog.Attributes
{
    public class UniqueEmailAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var email = value as string;
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var repo = new TempRepo();
            var user = repo.GetUser(u => u.Email, email).Result;

            if (user != null)
                return false;
            else
                return true;
        }
    }
}