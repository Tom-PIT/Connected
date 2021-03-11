using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Sdk.Logging
{
    public interface ILogging
    {
        TraceLevel Level { get; set; }

        public void Log(string message, TraceLevel level);

        public void Error(string message);

        public void Warning(string message);

        public void Info(string message);

        public void Trace(string message);

        public void Exception(Exception ex,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0);
    }
}
