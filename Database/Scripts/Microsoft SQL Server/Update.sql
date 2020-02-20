/*
Run this script on:

        sys-db\sql2017.tompit_update    -  This database will be modified

to synchronize it with:

        sys-db\sql2017.tompit_sys

You are recommended to back up your database before running this script

Script created by SQL Compare Engine version 12.3.3.4490 from Red Gate Software Ltd at 2/17/2020 12:24:22 PM

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
PRINT N'Creating [tompit].[mru_tag]'
GO
CREATE TABLE [tompit].[mru_tag]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[mru] [bigint] NOT NULL,
[tag] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_mru_tag] on [tompit].[mru_tag]'
GO
ALTER TABLE [tompit].[mru_tag] ADD CONSTRAINT [PK_mru_tag] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[mru]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [tompit].[mru] DROP
COLUMN [tags]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[mru_mdf]'
GO
ALTER PROCEDURE [tompit].[mru_mdf]
	@type int,
	@primary_key nvarchar(128),
	@entity_type int,
	@entity_primary_key nvarchar(128),
	@token uniqueidentifier,
	@date datetime2(7),
	@tags nvarchar(max)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @id bigint
	DECLARE @rv bigint

	MERGE tompit.mru AS t
	USING
	(
		SELECT @type, @primary_key, @entity_type, @entity_primary_key, @date
	) AS s ([type], primary_key, entity_type, entity_primary_key, date)
	ON (t.type = s.type AND t.primary_key = s.primary_key AND t.entity_type = s.entity_type AND t.entity_primary_key = s.entity_primary_key)
	WHEN NOT MATCHED THEN
		INSERT ([type], primary_key, entity_type, entity_primary_key, [date], token)
		VALUES (s.[type], s.primary_key, s.entity_type, s.entity_primary_key, s.date, @token)
	WHEN MATCHED THEN
		UPDATE SET
			date = @date,
			@id = id;

	IF(@id IS NULL)
	BEGIN
		SET @id = SCOPE_IDENTITY()
		SET @rv = @id
	END

	MERGE tompit.mru_tag AS t
	USING
	(
		SELECT	@id, tag
		FROM	OPENJSON(@tags) WITH (tag nvarchar(128))
	) AS s (mru, tag)
	ON (t.mru = s.mru AND t.tag = s.tag)
	WHEN NOT MATCHED THEN
		INSERT
		(mru, tag)
		VALUES
		(s.mru, s.tag)
	WHEN NOT MATCHED BY SOURCE AND t.mru = @id THEN
		DELETE;

	RETURN @rv
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[mru_que]'
GO
ALTER PROCEDURE [tompit].[mru_que]
	@entity_type int,
	@entity_primary_key nvarchar(128),
	@tags nvarchar(max)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT m.*, t.tag
	FROM tompit.mru m
	inner join tompit.mru_tag t ON t.mru = m.id
	inner join
	(
		SELECT tag
		FROM OPENJSON(@tags) WITH(tag nvarchar(128))
	) tt ON tt.tag = t.tag
	WHERE m.entity_type = @entity_type 
	AND m.entity_primary_key = @entity_primary_key 
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[mru_tag]'
GO
ALTER TABLE [tompit].[mru_tag] ADD CONSTRAINT [FK_mru_tag_mru] FOREIGN KEY ([mru]) REFERENCES [tompit].[mru] ([id]) ON DELETE CASCADE
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
