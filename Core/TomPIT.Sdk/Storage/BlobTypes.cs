﻿using System;

namespace TomPIT.Storage
{
	public class BlobTypes
	{
		/*
		 * Blobs under 500 are deleted by installer
		 */
		public const int Configuration = 1;
		public const int Template = 2;
		[Obsolete("Runtime configuration is obsolete and will be removed in the next release.")]
		public const int RuntimeConfiguration = 3;
		public const int ComponentHistory = 4;

		public const int WorkerState = 504;
		public const int Package = 505;
		public const int InstallerConfiguration = 506;
		public const int Avatar = 511;
		public const int HtmlImage = 521;
		public const int BigDataPartitionSchema = 531;
		//public const int Manifest = 541;
		public const int ScriptManifest = 542;
		public const int ScriptManifestMetaData = 543;
		/*
		 * Blobs over 1000 are not notified by backplane by default
		 */
		public const int RuntimeConfigurationBackup = 1001;
		public const int RuntimeConfigurationState = 1002;
		public const int DatabaseState = 1003;
		public const int UserContent = 1103;
		public const int BigDataTransactionBlock = 1201;
		public const int MailAttachment = 1301;
	}
}
