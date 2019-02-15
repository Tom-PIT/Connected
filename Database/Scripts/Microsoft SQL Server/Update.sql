/*
Run this script on:

        sys-db\sql2017.tompit_update    -  This database will be modified

to synchronize it with:

        sys-db\sql2017.tompit_sys

You are recommended to back up your database before running this script

Script created by SQL Compare Engine version 12.3.3.4490 from Red Gate Software Ltd at 2/15/2019 10:29:01 PM

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
PRINT N'Altering [tompit].[service]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [tompit].[service] ADD
[configuration] [uniqueidentifier] NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Refreshing [tompit].[view_component]'
GO
EXEC sp_refreshview N'[tompit].[view_component]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[view_service]'
GO




ALTER VIEW [tompit].[view_service]
AS
SELECT        s.id, s.name, s.url, s.token, s.status, s.resource_group, s.template, s.meta,
				s.license, s.package, s.configuration, r.token AS resource_token
FROM            tompit.service AS s INNER JOIN
                         tompit.resource_group AS r ON s.resource_group = r.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Refreshing [tompit].[view_service_string]'
GO
EXEC sp_refreshview N'[tompit].[view_service_string]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Refreshing [tompit].[view_folder]'
GO
EXEC sp_refreshview N'[tompit].[view_folder]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[installer]'
GO
CREATE TABLE [tompit].[installer]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[package] [uniqueidentifier] NOT NULL,
[parent] [uniqueidentifier] NULL,
[status] [int] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_installer] on [tompit].[installer]'
GO
ALTER TABLE [tompit].[installer] ADD CONSTRAINT [PK_installer] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [tompit].[installer]'
GO
ALTER TABLE [tompit].[installer] ADD CONSTRAINT [IX_installer] UNIQUE NONCLUSTERED  ([package]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[installer_ins]'
GO
CREATE PROCEDURE [tompit].[installer_ins]
	@items nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.installer (package, parent, status)
	SELECT package, parent, 1 FROM OPENJSON(@items) WITH (package uniqueidentifier, parent uniqueidentifier);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[installer_del]'
GO
CREATE PROCEDURE [tompit].[installer_del]
	@package uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.installer SET parent = NULL WHERE parent = @package;

	UPDATE tompit.installer SET
		parent = NULL
	DELETE tompit.installer 
	WHERE package = @package;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[service_upd]'
GO
ALTER PROCEDURE [tompit].[service_upd]
	@id int,
	@name nvarchar(128),
	@url nvarchar(136),
	@status int,
	@template uniqueidentifier,
	@resource_group int,
	@package uniqueidentifier = NULL,
	@configuration uniqueidentifier = NULL
AS
BEGIN
	SET NOCOUNT ON;

	update [service] set
		name = @name,
		url = @url,
		status = @status,
		template = @template,
		resource_group = @resource_group,
		package = @package,
		configuration = @configuration
	where id = @id;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[installer_que]'
GO
CREATE PROCEDURE [tompit].[installer_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.installer;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[installer_upd]'
GO
CREATE PROCEDURE [tompit].[installer_upd]
	@id int,
	@status int 
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.installer SET
		status = @status
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[installer_sel]'
GO
CREATE PROCEDURE [tompit].[installer_sel]
	@package uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.installer 
	WHERE package = @package;
END
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
