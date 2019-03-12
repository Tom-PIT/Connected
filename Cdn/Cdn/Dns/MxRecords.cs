using System.Collections;

namespace TomPIT.Cdn.Dns
{
	internal class MxRecords : CollectionBase
	{
		public void Add(MxRecord mxRecord)
		{
			List.Add(mxRecord);
		}

		public void Add(string domain, int preference)
		{
			MxRecord d = new MxRecord(domain, preference);

			List.Add(d);
		}

		public void Remove(int index)
		{
			if (index < Count || index >= 0)
				List.RemoveAt(index);
		}

		public MxRecord this[int index] { get { return (MxRecord)List[index]; } }

		public MxRecord GetPrefered()
		{
			int index, minIndex = 0;

			for (index = 0; index < List.Count; index++)
			{
				if (minIndex == -1 || minIndex > this[index].Preference)
					minIndex = index;
			}

			if (minIndex < this.Count && minIndex != -1)
				return this[minIndex];
			else
				return null;
		}
	}
}
