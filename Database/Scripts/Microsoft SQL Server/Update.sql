/*
Run this script on:

        sys-db\sql2017.tompit_update    -  This database will be modified

to synchronize it with:

        sys-db\sql2017.tompit_sys

You are recommended to back up your database before running this script

Script created by SQL Compare Engine version 12.3.3.4490 from Red Gate Software Ltd at 3/12/2019 10:05:53 AM

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
PRINT N'Dropping constraints from [tompit].[subscriber_claim]'
GO
ALTER TABLE [tompit].[subscriber_claim] DROP CONSTRAINT [PK_subscriber_claim]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [tompit].[subscriber_claim]'
GO
DROP TABLE [tompit].[subscriber_claim]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_subscriber]'
GO
CREATE VIEW [tompit].[view_subscriber]
AS

SELECT s.id, s.subscription, s.resource_type, s.resource_primary_key,
	su.token subscription_token
FROM tompit.subscriber s
INNER JOIN tompit.subscription su ON s.subscription = su.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[subscriber_que]'
GO
ALTER PROCEDURE [tompit].[subscriber_que]
	@subscription bigint
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.view_subscriber
	WHERE (subscription = @subscription)
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [tompit].[subscriber_sel]'
GO
ALTER PROCEDURE [tompit].[subscriber_sel]
	@subscription bigint,
	@resource_type int,
	@resource_primary_key nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.view_subscriber 
	WHERE (subscription = @subscription)
	AND (resource_type = @resource_type)
	AND (resource_primary_key = @resource_primary_key);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscriber_ins_batch]'
GO
CREATE PROCEDURE [tompit].[subscriber_ins_batch]
	@subscription bigint,
	@items nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	MERGE tompit.subscriber d
	USING (SELECT resource_type, resource_primary_key FROM OPENJSON(@items) WITH (resource_type int, resource_primary_key nvarchar(128))) AS s (resource_type, resource_primary_key)
	ON (d.subscription = @subscription AND d.resource_type = s.resource_type AND d.resource_primary_key = s.resource_primary_key)
	WHEN NOT MATCHED THEN
	INSERT (subscription, resource_type, resource_primary_key)
	VALUES (@subscription, s.resource_type, s.resource_primary_key);
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
