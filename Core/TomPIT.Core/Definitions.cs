namespace TomPIT
{
	public enum DataType
	{
		String = 1,
		Integer = 2,
		Float = 3,
		Date = 4,
		Bool = 5,
		Guid = 6,
		Binary = 7,
		Long = 8
	}

	public enum ExecutionContext
	{
		Distributed = 1,
		InProcess = 2
	}

	public enum InformationKind
	{
		None = 0,
		Primary = 1,
		Secondary = 2,
		Information = 3,
		Success = 4,
		Warning = 5,
		Danger = 6
	}

	public enum AccessModifier
	{
		Public = 1,
		Private = 2
	}
}
