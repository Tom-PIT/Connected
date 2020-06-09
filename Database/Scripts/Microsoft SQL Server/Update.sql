/*
Run this script on:

        sys-db\sql2017.tompit_update    -  This database will be modified

to synchronize it with:

        sys-db\sql2017.tompit_sys

You are recommended to back up your database before running this script

Script created by SQL Compare Engine version 12.3.3.4490 from Red Gate Software Ltd at 6/5/2020 9:48:45 AM

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
PRINT N'Dropping constraints from [tompit].[version_control_endpoint]'
GO
ALTER TABLE [tompit].[version_control_endpoint] DROP CONSTRAINT [PK_version_control_endpoint]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [tompit].[version_control_endpoint]'
GO
DROP TABLE [tompit].[version_control_endpoint]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[version_control_repository]'
GO
CREATE TABLE [tompit].[version_control_repository]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[url] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[user_name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[password] [varbinary] (128) NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_version_control_endpoint] on [tompit].[version_control_repository]'
GO
ALTER TABLE [tompit].[version_control_repository] ADD CONSTRAINT [PK_version_control_endpoint] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[service_binding]'
GO
CREATE TABLE [tompit].[service_binding]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[service] [int] NOT NULL,
[repository] [int] NOT NULL,
[commit] [bigint] NOT NULL,
[date] [datetime] NOT NULL,
[active] [bit] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_service_binding] on [tompit].[service_binding]'
GO
ALTER TABLE [tompit].[service_binding] ADD CONSTRAINT [PK_service_binding] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_service_binding]'
GO

CREATE VIEW [tompit].[view_service_binding]
AS
WITH raw AS 
(
	SELECT	service, MAX(created) as last_commit
	FROM	tompit.version_control_commit
	GROUP BY service
)
SELECT b.id, b.service, b.repository, b.[commit], b.date, b.active,
		r.name repository_name, r.url repository_url, 
		r1.last_commit,
		s.token service_token, s.name service_name
FROM	tompit.service_binding b
		INNER JOIN tompit.service s ON b.service = s.id
		INNER JOIN tompit.version_control_repository r ON b.repository = r.id
		INNER JOIN raw r1 ON r1.service = s.token;
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[version_control_repository_que]'
GO
CREATE PROCEDURE [tompit].[version_control_repository_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * FROM tompit.version_control_repository;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[service_binding_que_active]'
GO
CREATE PROCEDURE [tompit].[service_binding_que_active]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT s.id service, s.name service_name, s.token service_token, b.id, b.repository, b.[commit], b.date, b.active, b.repository_name, b.repository_url, b.last_commit
	FROM tompit.service s
	LEFT JOIN tompit.view_service_binding b ON s.id = b.service
	WHERE b.active IS NULL OR b.active = 1;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[service_binding_sel]'
GO
CREATE PROCEDURE [tompit].[service_binding_sel]
	@service int,
	@repository int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.view_service_binding
	WHERE service = @service
	AND repository = @repository;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[service_binding_que]'
GO
CREATE PROCEDURE [tompit].[service_binding_que]
	@service int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.view_service_binding
	WHERE service = @service;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[service_binding_upd]'
GO
CREATE PROCEDURE [tompit].[service_binding_upd]
	@service int,
	@repository int,
	@commit bigint,
	@date datetime,
	@active bit
AS
BEGIN
	SET NOCOUNT ON;

	IF (@active = 1)
		UPDATE tompit.service_binding SET active = 0 WHERE service = @service AND repository = @repository;

	MERGE tompit.service_binding AS t
	USING (SELECT @service, @repository) AS s (service, repository)
	ON (t.service = s.service AND t.repository = s.repository)
	WHEN NOT MATCHED THEN
		INSERT (service, repository, [commit], date, active)
		VALUES (@service, @repository, @commit, @date, 1)
	WHEN MATCHED THEN
		UPDATE SET
			[commit] = @commit,
			 date = @date,
			 active = @active;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[service_binding_del]'
GO
CREATE PROCEDURE [tompit].[service_binding_del]
	@service int,
	@repository int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.service_binding 
	WHERE service = @service AND repository = @repository;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[version_control_repository_ins]'
GO
CREATE PROCEDURE [tompit].[version_control_repository_ins]
	@name nvarchar(128),
	@url nvarchar(1024),
	@user_name nvarchar(128),
	@password varbinary(128)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.version_control_repository
		(name, url, user_name, password)
	VALUES
		(@name, @url, @user_name, @password);

	RETURN scope_identity();
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[version_control_repository_upd]'
GO
CREATE PROCEDURE [tompit].[version_control_repository_upd]
	@id int,
	@name nvarchar(128),
	@url nvarchar(1024),
	@user_name nvarchar(128),
	@password varbinary(128)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.version_control_repository SET
		name = @name, 
		url = @url, 
		user_name = @user_name, 
		password = @password
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[version_control_repository_del]'
GO
CREATE PROCEDURE [tompit].[version_control_repository_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.version_control_repository
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[version_control_repository_sel]'
GO
CREATE PROCEDURE [tompit].[version_control_repository_sel]
	@name nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.version_control_repository
	WHERE name = @name;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[big_data_node]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [tompit].[big_data_node] ALTER COLUMN [connection_string] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Refreshing [tompit].[view_big_data_index]'
GO
EXEC sp_refreshview N'[tompit].[view_big_data_index]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Refreshing [tompit].[view_big_data_index_field]'
GO
EXEC sp_refreshview N'[tompit].[view_big_data_index_field]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[big_data_node_ins]'
GO
ALTER PROCEDURE [tompit].[big_data_node_ins]
	@name nvarchar(128),
	@connection_string nvarchar(256) = NULL,
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
PRINT N'Altering [tompit].[big_data_node_upd]'
GO
ALTER PROCEDURE [tompit].[big_data_node_upd]
	@name nvarchar(128),
	@connection_string nvarchar(256) = NULL,
	@admin_connection_string nvarchar(256) = NULL,
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
PRINT N'Adding foreign keys to [tompit].[service_binding]'
GO
ALTER TABLE [tompit].[service_binding] ADD CONSTRAINT [FK_service_binding_service] FOREIGN KEY ([service]) REFERENCES [tompit].[service] ([id]) ON DELETE CASCADE
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
