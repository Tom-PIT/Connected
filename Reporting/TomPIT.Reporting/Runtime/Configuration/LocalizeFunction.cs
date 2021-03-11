using System;
using System.Threading;
using DevExpress.Data.Filtering;
using TomPIT.Globalization;
using TomPIT.Middleware;

namespace TomPIT.MicroServices.Reporting.Runtime.Configuration
{

	public class LocalizeFunction : ICustomFunctionOperator, ICustomFunctionOperatorBrowsable
	{
		public string Name => "Localize";

		public int MinOperandCount => 3;

		public int MaxOperandCount => 3;

		public string Description => "Get string from Tom PIT localization table";

		public FunctionCategory Category => FunctionCategory.Text;

		public object Evaluate(params object[] operands)
		{
			if (operands.Length < 3)
				return null;

			return MiddlewareDescriptor.Current.Tenant.GetService<ILocalizationService>().GetString(operands[0].ToString(), operands[1].ToString(), operands[2].ToString(), Thread.CurrentThread.CurrentUICulture.LCID, false);
		}

		public bool IsValidOperandCount(int count)
		{
			return count == 3;
		}

		public bool IsValidOperandType(int operandIndex, int operandCount, Type type)
		{
			return type == typeof(string);
		}

		public Type ResultType(params Type[] operands)
		{
			return typeof(string);
		}
	}
}
