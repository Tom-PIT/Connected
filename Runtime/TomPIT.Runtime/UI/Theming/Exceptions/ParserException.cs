namespace TomPIT.UI.Theming.Exceptions
{
	using System;
	using TomPIT.UI.Theming.Parser;

	public class ParserException : Exception
    {
        public ParserException(string message)
            : base(message)
        {
        }

        public ParserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ParserException(string message, Exception innerException, Zone errorLocation)
            : base(message, innerException) {
            ErrorLocation = errorLocation;
        }

        public Zone ErrorLocation { get; set; }
    }
}