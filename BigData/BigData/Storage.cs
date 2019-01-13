using Amt.Api.Serialization;
using Amt.Core.Configuration;
using Amt.Core.Exceptions;
using System;
using System.Data;
using System.IO;
using System.Threading;

namespace Amt.DataHub
{
	public static class Storage
	{
		private static string BlockStoragePath { get { return AmtShell.GetService<ISettingService>().GetValue<string>("Data hub block storage location"); } }

		public static void SavePostponed(Guid fileId, DataTable table)
		{
			var dir = GetDirectory();

			var buffer = Serialize(table);// StringUtils.ZipRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(table)));
			var path = string.Format("{0}/{1}.adb", dir, fileId.AsString());

			using (FileStream fs = File.Create(path))
			{
				fs.Write(buffer, 0, buffer.Length);
			}
		}

		public static void SaveBlock(Guid blockId, DataTable table)
		{
			var dir = GetDirectory();
			var buffer = Serialize(table);////StringUtils.ZipRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(table)));
			var path = string.Format("{0}/{1}.adb", dir, blockId.AsString());

			using (FileStream fs = File.Create(path))
			{
				fs.Write(buffer, 0, buffer.Length);
			}
		}

		public static void DeleteBlock(Guid blockId)
		{
			var dir = GetDirectory();
			var path = string.Format("{0}/{1}.adb", dir, blockId.AsString());

			File.Delete(path);
		}

		public static void DeletePostponed(Guid fileId)
		{
			var dir = GetDirectory();
			var path = string.Format("{0}/{1}.adb", dir, fileId.AsString());

			File.Delete(path);
		}

		public static DataTable LoadBlock(Guid blockId)
		{
			try
			{
				var dir = GetDirectory();
				var path = string.Format("{0}/{1}.adb", dir, blockId.AsString());

				if (!File.Exists(path))
				{
					Log.Warning(typeof(Storage), "Block file not found.", LogEvents.DhBlockFileNull, path);
					return null;
				}

				return ReadFile(path);
			}
			catch (Exception ex)
			{
				Log.Error(typeof(Storage), ex, LogEvents.DhLoadBlockError, blockId.AsString());
				return null;
			}
		}

		public static DataTable LoadPostponed(Guid fileId)
		{
			var dir = GetDirectory();
			var path = string.Format("{0}/{1}.adb", dir, fileId.AsString());

			if (!File.Exists(path))
				return null;

			return ReadFile(path);
		}

		private static DataTable ReadFile(string path)
		{
			var retryCount = 1;

			while (retryCount < 5)
			{
				try
				{
					using (var ms = new MemoryStream(StringUtils.Unzip(File.ReadAllBytes(path))))
					{
						return DataSerializer.DeserializeDataTable(ms);
					}
					//return JsonConvert.DeserializeObject<DataTable>(Encoding));
				}
				catch (IOException)
				{
					retryCount++;

					Thread.Sleep(100 * retryCount);
				}
			}

			return null;
		}
		public static Guid SavePartitionData(DataTable table)
		{
			Guid id = Guid.NewGuid();

			var dir = GetDirectory();
			var path = string.Format("{0}/{1}.apd", dir, id.AsString());

			var buffer = Serialize(table);//StringUtils.ZipRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(table)));

			using (FileStream fs = File.Create(path))
			{
				fs.Write(buffer, 0, buffer.Length);
			}

			return id;
		}

		public static DataTable LoadPartitionData(Guid fileId)
		{
			var dir = GetDirectory();
			var path = Path.Combine(dir, string.Format("{0}.apd", fileId.AsString()));

			if (!File.Exists(path))
				return null;

			return ReadFile(path);
		}

		private static string GetDirectory()
		{
			var dir = BlockStoragePath;

			if (dir == string.Empty)
				throw new AmtException("Setting 'Data hub block storage location' not found");

			if (!Directory.Exists(dir))
				throw new AmtException(string.Format("Directory '{0}' does not exist.", dir));

			return dir;
		}

		public static void DeleteDependecy(Guid fileId)
		{
			var dir = GetDirectory();
			var path = string.Format("{0}/{1}.apd", dir, fileId.AsString());

			File.Delete(path);
		}

		private static byte[] Serialize(DataTable table)
		{
			using (var ms = new MemoryStream())
			{
				DataSerializer.Serialize(ms, table, new ProtoDataWriterOptions());

				ms.Seek(0, SeekOrigin.Begin);

				return StringUtils.ZipRaw(ms.ToArray());
			}
		}
	}
}