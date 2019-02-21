using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WattchDB;

namespace WattchDog.Attributes
{
    public class UniqueUsernameAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var username = value as string;
            if (string.IsNullOrWhiteSpace(username))
                return false;

            var repo = new TempRepo();
            var user = repo.GetUser(u => u.Username, username).Result;

            if (user != null)
                return false;
            else
                return true;
        }
    }
}