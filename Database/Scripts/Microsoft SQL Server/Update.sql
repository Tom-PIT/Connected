/*
Run this script on:

        sys-db\sql2017.tompit_update    -  This database will be modified

to synchronize it with:

        sys-db\sql2017.tompit_sys

You are recommended to back up your database before running this script

Script created by SQL Compare Engine version 12.3.3.4490 from Red Gate Software Ltd at 2/3/2019 1:14:48 AM

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
PRINT N'Dropping foreign keys from [tompit].[component]'
GO
ALTER TABLE [tompit].[component] DROP CONSTRAINT [FK_component_feature]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [tompit].[feature]'
GO
ALTER TABLE [tompit].[feature] DROP CONSTRAINT [FK_feature_service]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [tompit].[feature]'
GO
ALTER TABLE [tompit].[feature] DROP CONSTRAINT [PK_feature]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [tompit].[feature_sel]'
GO
DROP PROCEDURE [tompit].[feature_sel]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [tompit].[feature_que]'
GO
DROP PROCEDURE [tompit].[feature_que]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [tompit].[feature_ins]'
GO
DROP PROCEDURE [tompit].[feature_ins]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [tompit].[feature_del]'
GO
DROP PROCEDURE [tompit].[feature_del]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [tompit].[view_feature]'
GO
DROP VIEW [tompit].[view_feature]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [tompit].[feature_upd]'
GO
DROP PROCEDURE [tompit].[feature_upd]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [tompit].[feature]'
GO
DROP TABLE [tompit].[feature]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[folder]'
GO
CREATE TABLE [tompit].[folder]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[parent] [int] NULL,
[service] [int] NOT NULL,
[token] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_folder] on [tompit].[folder]'
GO
ALTER TABLE [tompit].[folder] ADD CONSTRAINT [PK_folder] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[component]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
EXEC sp_rename N'[tompit].[component].[feature]', N'folder', N'COLUMN'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[view_component]'
GO

ALTER VIEW [tompit].[view_component]
AS
SELECT      c.id, c.name, c.token, c.type, c.category, c.runtime_configuration, c.modified,
			c.service, c.folder, f.token as folder_token, s.token AS [service_token]
FROM        tompit.component AS c 
LEFT JOIN	tompit.folder f on c.folder=f.id
INNER JOIN	tompit.service s on c.service = s.id

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[component_ins]'
GO
ALTER PROCEDURE [tompit].[component_ins]
	@folder int = NULL,
	@name nvarchar(128),
	@token uniqueidentifier,
	@type nvarchar(512),
	@category nvarchar(128),
	@runtime_configuration uniqueidentifier = NULL,
	@modified datetime,
	@service int
AS
BEGIN
	SET NOCOUNT ON;

	insert tompit.component (folder, name, token, type, category, runtime_configuration, modified, service)
	values (@folder, @name, @token, @type, @category, @runtime_configuration, @modified, @service);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[component_upd]'
GO
ALTER PROCEDURE [tompit].[component_upd]
	@id int,
	@name nvarchar(128),
	@modified datetime,
	@runtime_configuration uniqueidentifier = NULL,
	@folder int = NULL
AS
BEGIN
	SET NOCOUNT ON;

	update tompit.component set
		name = @name,
		modified = @modified,
		runtime_configuration = @runtime_configuration,
		folder = @folder
	where id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[folder_ins]'
GO
CREATE PROCEDURE [tompit].[folder_ins]
	@service int,
	@name nvarchar(128),
	@token uniqueidentifier,
	@parent int = NULL
AS
BEGIN
	SET NOCOUNT ON;

	insert folder (service, name, token, parent)
	values (@service, @name, @token, @parent);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_folder]'
GO






CREATE VIEW [tompit].[view_folder]
AS
SELECT        f.id, f.service, f.name, f.parent, f.token, s.token AS [service_token], fp.token as parent_token
FROM            tompit.folder AS f 
INNER JOIN tompit.[service] AS s ON f.[service] = s.id
LEFT JOIN tompit.folder fp on f.parent = fp.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[folder_que]'
GO
CREATE PROCEDURE [tompit].[folder_que]
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from view_folder;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[folder_sel]'
GO
CREATE PROCEDURE [tompit].[folder_sel]
	@service int = NULL,
	@name nvarchar(128) = null,
	@token uniqueidentifier = null
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 *
	from view_folder
	where (@service IS NULL OR service = @service)
	and (@name is null or name = @name)
	and (@token is null or token = @token);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[folder_upd]'
GO
CREATE PROCEDURE [tompit].[folder_upd]
	@id int,
	@name nvarchar(128),
	@parent int = NULL
AS
BEGIN
	SET NOCOUNT ON;

	update tompit.folder set
		name = @name,
		parent = @parent
	where id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[folder_del]'
GO
CREATE PROCEDURE [tompit].[folder_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	delete folder 
	where id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[component]'
GO
ALTER TABLE [tompit].[component] ADD CONSTRAINT [FK_component_folder] FOREIGN KEY ([folder]) REFERENCES [tompit].[folder] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[folder]'
GO
ALTER TABLE [tompit].[folder] ADD CONSTRAINT [FK_folder_folder1] FOREIGN KEY ([parent]) REFERENCES [tompit].[folder] ([id])
GO
ALTER TABLE [tompit].[folder] ADD CONSTRAINT [FK_folder_service] FOREIGN KEY ([service]) REFERENCES [tompit].[service] ([id])
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
