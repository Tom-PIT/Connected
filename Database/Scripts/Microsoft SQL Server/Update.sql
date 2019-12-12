/*
Run this script on:

        sys-db\sql2017.tompit_update    -  This database will be modified

to synchronize it with:

        sys-db\sql2017.tompit_sys

You are recommended to back up your database before running this script

Script created by SQL Compare Engine version 12.3.3.4490 from Red Gate Software Ltd at 12/10/2019 4:55:29 PM

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
PRINT N'Creating [tompit].[print_job]'
GO
CREATE TABLE [tompit].[print_job]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[created] [datetime] NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[status] [int] NOT NULL,
[error] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[component] [uniqueidentifier] NOT NULL,
[arguments] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[provider] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_print_job] on [tompit].[print_job]'
GO
ALTER TABLE [tompit].[print_job] ADD CONSTRAINT [PK_print_job] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_print_job] on [tompit].[print_job]'
GO
CREATE NONCLUSTERED INDEX [IX_print_job] ON [tompit].[print_job] ([token]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[print_job_del]'
GO
CREATE PROCEDURE [tompit].[print_job_del]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.print_job
	WHERE token = @token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[print_job_ins]'
GO
CREATE PROCEDURE [tompit].[print_job_ins]
	@token uniqueidentifier,
	@created datetime,
	@status int,
	@component uniqueidentifier,
	@arguments nvarchar(max) = NULL,
	@provider nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.print_job 
			(token, created, status, component, arguments, provider)
	VALUES	(@token, @created, @status, @component, @arguments, @provider);

	RETURN scope_identity();
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[print_job_sel]'
GO
CREATE PROCEDURE [tompit].[print_job_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM	tompit.print_job 
	WHERE token = @token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[print_job_upd]'
GO
CREATE PROCEDURE [tompit].[print_job_upd]
	@token uniqueidentifier,
	@error nvarchar(128) = NULL,
	@status int
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.print_job SET
		error = @error,
		status = @status
	WHERE token = @token;
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
