using DevExpress.Data.Filtering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TomPIT.Connected.Printing.Client.Printing
{
	public class LocalizeFunction : ICustomFunctionOperator, ICustomFunctionOperatorBrowsable
	{
		private readonly LocalizationProvider _provider;
		private readonly Guid _identity; 
		public LocalizeFunction(LocalizationProvider provider, Guid identity) 
		{
			this._provider = provider;
			this._identity = identity;
		}

		public string Name => "Localize";

		public int MinOperandCount => 3;

		public int MaxOperandCount => 3;

		public string Description => "Get string from Tom PIT localization table";

		public FunctionCategory Category => FunctionCategory.Text;

		public object Evaluate(params object[] operands)
		{
			if (operands.Length < 3)
				return null;

			return _provider.GetLocalization(operands[0].ToString(), operands[1].ToString(), operands[2].ToString(), _identity).ConfigureAwait(true).GetAwaiter().GetResult();
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
