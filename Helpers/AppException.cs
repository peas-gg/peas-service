using System;
using System.Globalization;

namespace PEAS.Helpers
{
    public class AppException : Exception
    {
        public AppException() : base() { }

        public AppException(string message) : base(message) { }

        public AppException(string message, params object[] args) : base(String.Format(CultureInfo.CurrentCulture, message, args))
        {
        }

        public static AppException ConstructException(Exception exception)
        {
            return exception switch
            {
                AppException => (AppException)exception,
                _ => new AppException(exception.Message),
            };
        }
    }
}