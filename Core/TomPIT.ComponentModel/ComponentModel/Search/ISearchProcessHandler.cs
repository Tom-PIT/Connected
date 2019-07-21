using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Search;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Search
{
	public enum SearchValidationBehavior
	{
		Retry = 1,
		Complete = 2
	}
	public interface ISearchProcessHandler : IProcessHandler
	{
		SearchVerb Verb { get; set; }
		SearchValidationBehavior ValidationFailed { get; }
	}
}
