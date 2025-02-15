namespace TomPIT.UI.Theming.Engine
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using Cache;
	using TomPIT.UI.Theming.Loggers;

	public class CacheDecorator : ILessEngine
	{
		public readonly ILessEngine Underlying;
		public readonly ICache Cache;
		public ILogger Logger { get; set; }

		public CacheDecorator(ILessEngine underlying, ICache cache) : this(underlying, cache, NullLogger.Instance)
		{ }

		public CacheDecorator(ILessEngine underlying, ICache cache, ILogger logger)
		{
			Underlying = underlying;
			Cache = cache;
			Logger = logger;
		}

		public string TransformToCss(string source, string fileName)
		{
			//Compute Cache Key
			var hash = ComputeContentHash(source);
			var cacheKey = fileName + hash;
			if (!Cache.Exists(cacheKey))
			{
				Logger.Debug(String.Format("Inserting cache entry for {0}", cacheKey));

				var css = Underlying.TransformToCss(source, fileName);
				var dependancies = new[] { fileName }.Concat(GetImports());

				Cache.Insert(cacheKey, dependancies, css);

				return css;
			}
			Logger.Debug(String.Format("Retrieving cache entry {0}", cacheKey));
			return Cache.Retrieve(cacheKey);
		}

		private string ComputeContentHash(string source)
		{
			SHA1 sha1 = SHA1.Create();
			byte[] computeHash = sha1.ComputeHash(Encoding.Default.GetBytes(source));
			return Convert.ToBase64String(computeHash);
		}

		public IEnumerable<string> GetImports()
		{
			return Underlying.GetImports();
		}

		public void ResetImports()
		{
			Underlying.ResetImports();
		}

		public bool LastTransformationSuccessful
		{
			get
			{
				return Underlying.LastTransformationSuccessful;
			}
		}

		public string CurrentDirectory
		{
			get { return Underlying.CurrentDirectory; }
			set { Underlying.CurrentDirectory = value; }
		}
	}
}