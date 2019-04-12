/*
Run this script on:

        sys-db\sql2017.tompit_update    -  This database will be modified

to synchronize it with:

        sys-db\sql2017.tompit_sys

You are recommended to back up your database before running this script

Script created by SQL Compare Engine version 12.3.3.4490 from Red Gate Software Ltd at 3/29/2019 8:37:18 AM

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
PRINT N'Dropping foreign keys from [tompit].[big_data_index]'
GO
ALTER TABLE [tompit].[big_data_index] DROP CONSTRAINT [FK_big_data_index_big_data_partition]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [tompit].[big_data_transaction_defer]'
GO
ALTER TABLE [tompit].[big_data_transaction_defer] DROP CONSTRAINT [FK_big_data_transaction_defer_big_data_partition]
GO
ALTER TABLE [tompit].[big_data_transaction_defer] DROP CONSTRAINT [FK_big_data_transaction_defer_big_data_transaction_worker]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [tompit].[big_data_transaction_block]'
GO
ALTER TABLE [tompit].[big_data_transaction_block] DROP CONSTRAINT [FK_big_data_transaction_block_big_data_transaction]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [tompit].[big_data_transaction_worker]'
GO
ALTER TABLE [tompit].[big_data_transaction_worker] DROP CONSTRAINT [FK_big_data_transaction_worker_big_data_transaction_block]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [tompit].[big_data_partition]'
GO
ALTER TABLE [tompit].[big_data_partition] DROP CONSTRAINT [PK_big_data_partition]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [tompit].[big_data_partition]'
GO
ALTER TABLE [tompit].[big_data_partition] DROP CONSTRAINT [IX_big_data_partition]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [tompit].[big_data_transaction]'
GO
ALTER TABLE [tompit].[big_data_transaction] DROP CONSTRAINT [PK_big_data_transaction]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [tompit].[big_data_transaction_block]'
GO
ALTER TABLE [tompit].[big_data_transaction_block] DROP CONSTRAINT [PK_big_data_transaction_block]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[mail_sel]'
GO
ALTER PROCEDURE [tompit].[mail_sel]
	@token uniqueidentifier = NULL,
	@pop_receipt uniqueidentifier = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT top 1 *
	FROM tompit.mail_queue 
	WHERE (@token IS NULL OR token = @token)
	AND (@pop_receipt IS NULL OR pop_receipt = @pop_receipt);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Rebuilding [tompit].[big_data_partition]'
GO
CREATE TABLE [tompit].[RG_Recovery_1_big_data_partition]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[configuration] [uniqueidentifier] NOT NULL,
[file_count] [int] NOT NULL,
[status] [int] NOT NULL,
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[created] [smalldatetime] NOT NULL,
[resource_group] [int] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
SET IDENTITY_INSERT [tompit].[RG_Recovery_1_big_data_partition] ON
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
INSERT INTO [tompit].[RG_Recovery_1_big_data_partition]([id], [configuration], [file_count], [status], [name], [created]) SELECT [id], [configuration], [file_count], [status], [name], [created] FROM [tompit].[big_data_partition]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
SET IDENTITY_INSERT [tompit].[RG_Recovery_1_big_data_partition] OFF
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @idVal BIGINT
SELECT @idVal = IDENT_CURRENT(N'[tompit].[big_data_partition]')
IF @idVal IS NOT NULL
    DBCC CHECKIDENT(N'[tompit].[RG_Recovery_1_big_data_partition]', RESEED, @idVal)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DROP TABLE [tompit].[big_data_partition]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
EXEC sp_rename N'[tompit].[RG_Recovery_1_big_data_partition]', N'big_data_partition', N'OBJECT'
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
PRINT N'Rebuilding [tompit].[big_data_transaction]'
GO
CREATE TABLE [tompit].[RG_Recovery_2_big_data_transaction]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[block_count] [int] NOT NULL,
[block_remaining] [int] NOT NULL,
[created] [datetime] NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[status] [int] NOT NULL,
[partition] [int] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
SET IDENTITY_INSERT [tompit].[RG_Recovery_2_big_data_transaction] ON
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
INSERT INTO [tompit].[RG_Recovery_2_big_data_transaction]([id], [block_count], [block_remaining], [created], [token], [status]) SELECT [id], [block_count], [block_remaining], [created], [token], [status] FROM [tompit].[big_data_transaction]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
SET IDENTITY_INSERT [tompit].[RG_Recovery_2_big_data_transaction] OFF
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @idVal BIGINT
SELECT @idVal = IDENT_CURRENT(N'[tompit].[big_data_transaction]')
IF @idVal IS NOT NULL
    DBCC CHECKIDENT(N'[tompit].[RG_Recovery_2_big_data_transaction]', RESEED, @idVal)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DROP TABLE [tompit].[big_data_transaction]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
EXEC sp_rename N'[tompit].[RG_Recovery_2_big_data_transaction]', N'big_data_transaction', N'OBJECT'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_transaction] on [tompit].[big_data_transaction]'
GO
ALTER TABLE [tompit].[big_data_transaction] ADD CONSTRAINT [PK_big_data_transaction] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_del]'
GO
CREATE PROCEDURE [tompit].[big_data_transaction_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.big_data_transaction
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Rebuilding [tompit].[big_data_transaction_block]'
GO
CREATE TABLE [tompit].[RG_Recovery_3_big_data_transaction_block]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[transaction] [bigint] NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[next_visible] [datetime2] NOT NULL,
[pop_receipt] [uniqueidentifier] NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
SET IDENTITY_INSERT [tompit].[RG_Recovery_3_big_data_transaction_block] ON
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
INSERT INTO [tompit].[RG_Recovery_3_big_data_transaction_block]([id], [transaction], [token]) SELECT [id], [transaction], [token] FROM [tompit].[big_data_transaction_block]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
SET IDENTITY_INSERT [tompit].[RG_Recovery_3_big_data_transaction_block] OFF
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @idVal BIGINT
SELECT @idVal = IDENT_CURRENT(N'[tompit].[big_data_transaction_block]')
IF @idVal IS NOT NULL
    DBCC CHECKIDENT(N'[tompit].[RG_Recovery_3_big_data_transaction_block]', RESEED, @idVal)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DROP TABLE [tompit].[big_data_transaction_block]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
EXEC sp_rename N'[tompit].[RG_Recovery_3_big_data_transaction_block]', N'big_data_transaction_block', N'OBJECT'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_transaction_block] on [tompit].[big_data_transaction_block]'
GO
ALTER TABLE [tompit].[big_data_transaction_block] ADD CONSTRAINT [PK_big_data_transaction_block] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_block_del]'
GO
CREATE PROCEDURE [tompit].[big_data_transaction_block_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.big_data_transaction_block
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_block_view]'
GO
CREATE VIEW [tompit].[big_data_transaction_block_view]
AS
SELECT b.id, b.[transaction], b.token, b.next_visible, b.pop_receipt,
		t.token transaction_token, t.status transaction_status,
		p.configuration partition_configuration, p.status partition_status, p.resource_group
FROM tompit.big_data_transaction_block b
INNER JOIN tompit.big_data_transaction t ON b.[transaction] = t.id
INNER JOIN tompit.big_data_partition p ON t.partition = p.id;
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_block_dequeue]'
GO
CREATE PROCEDURE [tompit].[big_data_transaction_block_dequeue]
	@resource_groups nvarchar(MAX),
	@next_visible datetime,
	@count int = 32,
	@date datetime
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ct table(num bigint);

	WITH q AS
		(
			SELECT TOP (@count) *
			FROM tompit.big_data_transaction_block_view WITH (READPAST)
			WHERE next_visible < @date
			AND partition_status =  1 
			AND transaction_status = 2 
			AND resource_group IN (SELECT resource_group FROM OPENJSON(@resource_groups) WITH (resource_group int))
			ORDER BY next_visible, id
		)
	 UPDATE q with (UPDLOCK, READPAST) set
		next_visible = @next_visible,
		pop_receipt = newid()
	output inserted.id into @ct;

	select * from tompit.big_data_transaction_block_view where id IN (select num from @ct);	
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_ins]'
GO
CREATE PROCEDURE [tompit].[big_data_transaction_ins]
	@partition int,
	@token uniqueidentifier,
	@block_count int,
	@created datetime
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.big_data_transaction (block_count, block_remaining, created, token, status, partition)
	VALUES (@block_count, @block_count, @created, @token, 1, @partition);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_block_ins]'
GO
CREATE PROCEDURE [tompit].[big_data_transaction_block_ins]
	@transaction bigint,
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.big_data_transaction_block ([transaction], token, next_visible, pop_receipt)
	VALUES (@transaction, @token, getutcdate(), null);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_block_upd]'
GO
CREATE PROCEDURE [tompit].[big_data_transaction_block_upd]
	@id bigint,
	@next_visible datetime
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.big_data_transaction_block SET
		next_visible = @next_visible,
		pop_receipt = NULL
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_upd]'
GO
CREATE PROCEDURE [tompit].[big_data_transaction_upd]
	@id bigint,
	@block_remaining int,
	@status int
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.big_data_transaction SET
		block_remaining = @block_remaining,
		status = @status
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_view]'
GO
CREATE VIEW [tompit].[big_data_transaction_view]
AS

SELECT t.id, t.block_count, t.block_remaining, t.created, t.token, t.status, t.partition,
		p.configuration partition_token
FROM tompit.big_data_transaction t
INNER JOIN tompit.big_data_partition p ON t.partition = p.id;
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_que]'
GO
CREATE PROCEDURE [tompit].[big_data_transaction_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.big_data_transaction_view;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_sel]'
GO
CREATE PROCEDURE [tompit].[big_data_transaction_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.big_data_transaction_view
	WHERE token = @token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[big_data_partition]'
GO
ALTER TABLE [tompit].[big_data_partition] ADD CONSTRAINT [FK_big_data_partition_resource_group] FOREIGN KEY ([resource_group]) REFERENCES [tompit].[resource_group] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[big_data_transaction]'
GO
ALTER TABLE [tompit].[big_data_transaction] ADD CONSTRAINT [FK_big_data_transaction_big_data_partition] FOREIGN KEY ([partition]) REFERENCES [tompit].[big_data_partition] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[big_data_transaction_defer]'
GO
ALTER TABLE [tompit].[big_data_transaction_defer] ADD CONSTRAINT [FK_big_data_transaction_defer_big_data_partition] FOREIGN KEY ([partition]) REFERENCES [tompit].[big_data_partition] ([id]) ON DELETE CASCADE
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
PRINT N'Adding foreign keys to [tompit].[big_data_index]'
GO
ALTER TABLE [tompit].[big_data_index] ADD CONSTRAINT [FK_big_data_index_big_data_partition] FOREIGN KEY ([partition]) REFERENCES [tompit].[big_data_partition] ([id]) ON DELETE CASCADE
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
