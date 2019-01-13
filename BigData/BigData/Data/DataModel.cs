using Amt.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Amt.DataHub.Data
{
	public static class DataModel
	{
		public static DataTable Query(string commandText, List<Tuple<string, object>> parameters)
		{
			try
			{
				return new DataHubQuery(commandText, parameters).Execute();
			}
			catch (Exception ex)
			{
				AmtShell.GetService<ILoggingService>().Error("DataHub Query", UnwrappException(ex));

				throw;
			}
		}

		private static Exception UnwrappException(Exception ex)
		{
			if (!(ex is AggregateException))
				return ex;

			var sb = new StringBuilder();

			sb.AppendLine(ex.Source);
			sb.AppendLine(ex.Message);
			sb.AppendLine(ex.StackTrace);

			while (ex.InnerException != null)
			{
				ex = ex.InnerException;

				sb.AppendLine();
				sb.AppendLine("Inner exception");
				sb.AppendLine(ex.Message);
				sb.AppendLine(ex.StackTrace);
				sb.AppendLine("-----------------");
			}

			return new Exception(sb.ToString());
		}
	}
}