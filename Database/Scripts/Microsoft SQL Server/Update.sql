/*
Run this script on:

        sys-db\sql2017.tompit_update    -  This database will be modified

to synchronize it with:

        sys-db\sql2017.tompit_sys

You are recommended to back up your database before running this script

Script created by SQL Compare Engine version 12.3.3.4490 from Red Gate Software Ltd at 3/18/2019 12:48:04 PM

*/
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
BEGIN TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [tompit].[mail_queue]'
GO
ALTER TABLE [tompit].[mail_queue] DROP CONSTRAINT [PK_mail_queue]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Rebuilding [tompit].[mail_queue]'
GO
CREATE TABLE [tompit].[RG_Recovery_1_mail_queue]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[created] [datetime] NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[from] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[to] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[next_visible] [datetime] NOT NULL,
[expire] [smalldatetime] NOT NULL,
[pop_receipt] [uniqueidentifier] NULL,
[dequeue_count] [int] NOT NULL,
[dequeue_timestamp] [datetime] NULL,
[subject] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[body] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[headers] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[attachment_count] [int] NOT NULL,
[error] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[format] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
SET IDENTITY_INSERT [tompit].[RG_Recovery_1_mail_queue] ON
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
INSERT INTO [tompit].[RG_Recovery_1_mail_queue]([id], [created], [token], [from], [to], [next_visible], [expire], [pop_receipt], [dequeue_count], [dequeue_timestamp], [subject], [body], [headers], [attachment_count], [error], [format]) SELECT [id], [created], [token], [from], [to], [next_visible], [expire], [pop_receipt], [dequeue_count], [dequeue_timestamp], [subject], [body], [headers], [attachment_count], [error], [format] FROM [tompit].[mail_queue]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
SET IDENTITY_INSERT [tompit].[RG_Recovery_1_mail_queue] OFF
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DROP TABLE [tompit].[mail_queue]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
EXEC sp_rename N'[tompit].[RG_Recovery_1_mail_queue]', N'mail_queue', N'OBJECT'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_mail_queue] on [tompit].[mail_queue]'
GO
ALTER TABLE [tompit].[mail_queue] ADD CONSTRAINT [PK_mail_queue] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_partition]'
GO
CREATE TABLE [tompit].[big_data_partition]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[configuration] [uniqueidentifier] NOT NULL,
[file_count] [int] NOT NULL,
[status] [int] NOT NULL,
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[created] [smalldatetime] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_partition] on [tompit].[big_data_partition]'
GO
ALTER TABLE [tompit].[big_data_partition] ADD CONSTRAINT [PK_big_data_partition] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [tompit].[big_data_partition]'
GO
ALTER TABLE [tompit].[big_data_partition] ADD CONSTRAINT [IX_big_data_partition] UNIQUE NONCLUSTERED  ([configuration]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_partition_ins]'
GO
CREATE PROCEDURE [tompit].[big_data_partition_ins]
	@configuration uniqueidentifier,
	@status int,
	@name nvarchar(128),
	@created smalldatetime
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.big_data_partition (configuration, file_count, status, name, created)
	VALUES (@configuration, 0, @status, @name, @created);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_partition_upd]'
GO
CREATE PROCEDURE [tompit].[big_data_partition_upd]
	@id int,
	@status int,
	@name nvarchar(128),
	@file_count int
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.big_data_partition SET
		file_count = @file_count, 
		status = @status, 
		name = @name
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_partition_que]'
GO
CREATE PROCEDURE [tompit].[big_data_partition_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.big_data_partition;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_partition_sel]'
GO
CREATE PROCEDURE [tompit].[big_data_partition_sel]
	@configuration uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.big_data_partition
	WHERE configuration = @configuration;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node]'
GO
CREATE TABLE [tompit].[big_data_node]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[connection_string] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[admin_connection_string] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[token] [uniqueidentifier] NOT NULL,
[status] [int] NOT NULL,
[size] [bigint] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_node] on [tompit].[big_data_node]'
GO
ALTER TABLE [tompit].[big_data_node] ADD CONSTRAINT [PK_big_data_node] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node_del]'
GO
CREATE PROCEDURE [tompit].[big_data_node_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.big_data_node
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node_ins]'
GO
CREATE PROCEDURE [tompit].[big_data_node_ins]
	@name nvarchar(128),
	@connection_string nvarchar(256),
	@admin_connection_string nvarchar(256) = NULL,
	@token uniqueidentifier,
	@status int
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.big_data_node (name, connection_string, admin_connection_string, token, status, size)
	VALUES (@name, @connection_string, @admin_connection_string, @token, @status, 0);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node_upd]'
GO
CREATE PROCEDURE [tompit].[big_data_node_upd]
	@name nvarchar(128),
	@connection_string nvarchar(256),
	@admin_connection_string nvarchar(256) = NULL,
	@token uniqueidentifier,
	@status int,
	@size bigint,
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.big_data_node SET
		name = @name, 
		connection_string = @connection_string, 
		admin_connection_string = @admin_connection_string, 
		status = @status, 
		size = @size
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node_que]'
GO
CREATE PROCEDURE [tompit].[big_data_node_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.big_data_node;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node_sel]'
GO
CREATE PROCEDURE [tompit].[big_data_node_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.big_data_node
	WHERE token = @token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_partition_del]'
GO
CREATE PROCEDURE [tompit].[big_data_partition_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.big_data_partition
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_index]'
GO
CREATE TABLE [tompit].[big_data_index]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[start_timestamp] [datetime2] NULL,
[end_timestamp] [datetime2] NULL,
[count] [int] NOT NULL,
[status] [int] NOT NULL,
[node] [int] NOT NULL,
[file] [uniqueidentifier] NOT NULL,
[partition] [int] NOT NULL,
[key] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_index] on [tompit].[big_data_index]'
GO
ALTER TABLE [tompit].[big_data_index] ADD CONSTRAINT [PK_big_data_index] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_index_field]'
GO
CREATE TABLE [tompit].[big_data_index_field]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[index] [bigint] NOT NULL,
[start_string] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[end_string] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[start_number] [float] NULL,
[end_number] [float] NULL,
[field_name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_index_field] on [tompit].[big_data_index_field]'
GO
ALTER TABLE [tompit].[big_data_index_field] ADD CONSTRAINT [PK_big_data_index_field] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction]'
GO
CREATE TABLE [tompit].[big_data_transaction]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[block_count] [int] NOT NULL,
[block_remaining] [int] NOT NULL,
[created] [datetime] NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[status] [int] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_transaction] on [tompit].[big_data_transaction]'
GO
ALTER TABLE [tompit].[big_data_transaction] ADD CONSTRAINT [PK_big_data_transaction] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_block]'
GO
CREATE TABLE [tompit].[big_data_transaction_block]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[transaction] [bigint] NOT NULL,
[worker_remaining] [int] NOT NULL,
[token] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_transaction_block] on [tompit].[big_data_transaction_block]'
GO
ALTER TABLE [tompit].[big_data_transaction_block] ADD CONSTRAINT [PK_big_data_transaction_block] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_defer]'
GO
CREATE TABLE [tompit].[big_data_transaction_defer]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[partition] [int] NOT NULL,
[file] [uniqueidentifier] NOT NULL,
[next_visible] [datetime2] NOT NULL,
[worker] [bigint] NOT NULL,
[pop_receipt] [uniqueidentifier] NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_transaction_defer] on [tompit].[big_data_transaction_defer]'
GO
ALTER TABLE [tompit].[big_data_transaction_defer] ADD CONSTRAINT [PK_big_data_transaction_defer] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_worker]'
GO
CREATE TABLE [tompit].[big_data_transaction_worker]
(
[id] [bigint] NOT NULL,
[block] [bigint] NOT NULL,
[status] [int] NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[configuration] [uniqueidentifier] NOT NULL,
[next_visible] [datetime2] NOT NULL,
[has_dependencies] [bit] NOT NULL,
[pop_receipt] [uniqueidentifier] NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_transaction_worker] on [tompit].[big_data_transaction_worker]'
GO
ALTER TABLE [tompit].[big_data_transaction_worker] ADD CONSTRAINT [PK_big_data_transaction_worker] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[big_data_index_field]'
GO
ALTER TABLE [tompit].[big_data_index_field] ADD CONSTRAINT [FK_big_data_index_field_big_data_index] FOREIGN KEY ([index]) REFERENCES [tompit].[big_data_index] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[big_data_index]'
GO
ALTER TABLE [tompit].[big_data_index] ADD CONSTRAINT [FK_big_data_index_big_data_node] FOREIGN KEY ([node]) REFERENCES [tompit].[big_data_node] ([id]) ON DELETE CASCADE
GO
ALTER TABLE [tompit].[big_data_index] ADD CONSTRAINT [FK_big_data_index_big_data_partition] FOREIGN KEY ([partition]) REFERENCES [tompit].[big_data_partition] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[big_data_transaction_defer]'
GO
ALTER TABLE [tompit].[big_data_transaction_defer] ADD CONSTRAINT [FK_big_data_transaction_defer_big_data_partition] FOREIGN KEY ([partition]) REFERENCES [tompit].[big_data_partition] ([id]) ON DELETE CASCADE
GO
ALTER TABLE [tompit].[big_data_transaction_defer] ADD CONSTRAINT [FK_big_data_transaction_defer_big_data_transaction_worker] FOREIGN KEY ([worker]) REFERENCES [tompit].[big_data_transaction_worker] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[big_data_transaction_block]'
GO
ALTER TABLE [tompit].[big_data_transaction_block] ADD CONSTRAINT [FK_big_data_transaction_block_big_data_transaction] FOREIGN KEY ([transaction]) REFERENCES [tompit].[big_data_transaction] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[big_data_transaction_worker]'
GO
ALTER TABLE [tompit].[big_data_transaction_worker] ADD CONSTRAINT [FK_big_data_transaction_worker_big_data_transaction_block] FOREIGN KEY ([block]) REFERENCES [tompit].[big_data_transaction_block] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
COMMIT TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @Success AS BIT
SET @Success = 1
SET NOEXEC OFF
IF (@Success = 1) PRINT 'The database update succeeded'
ELSE BEGIN
	IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
	PRINT 'The database update failed'
END
GO
