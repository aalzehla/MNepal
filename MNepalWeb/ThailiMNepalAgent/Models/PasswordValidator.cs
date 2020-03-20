using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ThailiMNepalAgent.Models
{
    public class PasswordValidator
    {
        private int _FewUpperCaseCharacter = 1,
            _FewDigitAllowed = 1,
            _MinLength = 5,
            _MaxLength = 12;

        public bool IsValid(string password)
        {
            return password.Length >= MinimumLength;
                //&& uppercaseCharacterMatcher.Matches(password).Count
                //    >= FewestUppercaseCharactersAllowed
                //&& digitsMatcher.Matches(password).Count >= FewestDigitsAllowed;
        }

        public int FewestUppercaseCharactersAllowed {
            get { return _FewUpperCaseCharacter; }
            set { _FewUpperCaseCharacter = value; }
        }
        public int FewestDigitsAllowed {
            get { return _FewDigitAllowed; }
            set { _FewDigitAllowed = value; }
        }
        public int MinimumLength {
            get { return _MinLength; }
            set { _MinLength = value; }
        }
        public int MaximumLength {
            get { return _MaxLength; }
            set { _MaxLength = value; }
        }

        //private Regex uppercaseCharacterMatcher = new Regex("[A-Z]");
        //private Regex digitsMatcher = new Regex("[0-9]");


    }
}