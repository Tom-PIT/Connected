/*
Run this script on:

        sys-db\sql2017.tompit_update    -  This database will be modified

to synchronize it with:

        sys-db\sql2017.tompit_sys

You are recommended to back up your database before running this script

Script created by SQL Compare Engine version 12.3.3.4490 from Red Gate Software Ltd at 1/22/2020 9:39:32 AM

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
PRINT N'Altering [tompit].[subscriber]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [tompit].[subscriber] ADD
[token] [uniqueidentifier] NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[subscriber_ins]'
GO
ALTER PROCEDURE [tompit].[subscriber_ins]
	@subscription bigint,
	@resource_type int,
	@resource_primary_key nvarchar(128),
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.subscriber (subscription, resource_type, resource_primary_key, token)
	VALUES (@subscription, @resource_type, @resource_primary_key, @token);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[view_subscriber]'
GO

ALTER VIEW [tompit].[view_subscriber]
AS

SELECT s.id, s.subscription, s.resource_type, s.resource_primary_key, s.token,
	su.token subscription_token
FROM tompit.subscriber s
INNER JOIN tompit.subscription su ON s.subscription = su.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[subscriber_sel]'
GO
ALTER PROCEDURE [tompit].[subscriber_sel]
	@subscription bigint = NULL,
	@resource_type int = NULL,
	@resource_primary_key nvarchar(128) = NULL,
	@token uniqueidentifier = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.view_subscriber 
	WHERE (@subscription IS NULL OR subscription = @subscription)
	AND (@resource_type IS NULL OR resource_type = @resource_type)
	AND (@resource_primary_key IS NULL OR resource_primary_key = @resource_primary_key)
	AND (@token IS NULL OR token = @token);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[subscriber_ins_batch]'
GO
ALTER PROCEDURE [tompit].[subscriber_ins_batch]
	@subscription bigint,
	@items nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	MERGE tompit.subscriber d
	USING (SELECT resource_type, resource_primary_key, token FROM OPENJSON(@items) WITH (resource_type int, resource_primary_key nvarchar(128), token uniqueidentifier)) AS s (resource_type, resource_primary_key, token)
	ON (d.subscription = @subscription AND d.resource_type = s.resource_type AND d.resource_primary_key = s.resource_primary_key)
	WHEN NOT MATCHED THEN
	INSERT (subscription, resource_type, resource_primary_key, token)
	VALUES (@subscription, s.resource_type, s.resource_primary_key, s.token);
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
