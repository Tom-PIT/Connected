﻿namespace TomPIT.Cdn
{
	public interface IPrintingProvider
	{
		string Name { get; }
		void Print(IPrintJob job);
	}
}