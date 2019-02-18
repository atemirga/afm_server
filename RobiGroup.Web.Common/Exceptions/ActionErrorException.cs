using System;

namespace RobiGroup.Web.Common.Exceptions
{
    public class ActionErrorException : Exception
    {
        public string ErrorName { get; private set; }
        
        public ActionErrorException(string errorMessage) : this(string.Empty, errorMessage)
        {
        }

        public ActionErrorException(string errorName, string errorMessage):base(errorMessage)
        {
            ErrorName = errorName;
        }
    }
}