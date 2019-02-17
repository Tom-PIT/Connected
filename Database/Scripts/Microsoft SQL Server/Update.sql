/*
Run this script on:

        sys-db\sql2017.tompit_update    -  This database will be modified

to synchronize it with:

        sys-db\sql2017.tompit_sys

You are recommended to back up your database before running this script

Script created by SQL Compare Engine version 12.3.3.4490 from Red Gate Software Ltd at 2/17/2019 1:37:49 PM

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
PRINT N'Creating [tompit].[service_string_restore]'
GO
CREATE PROCEDURE [tompit].[service_string_restore]
	@items nvarchar(max)
AS
BEGIN
	SET NOCOUNT ON;

	MERGE service_string AS d
	USING (SELECT * FROM OPENJSON(@items) WITH ([service] int, element uniqueidentifier, property nvarchar(128), value nvarchar(MAX), language int)) AS s (service, element, property, value, language)
	ON (d.service = s.service AND d.language = s.language AND d.element = s.element AND d.property = s.property)
	WHEN NOT MATCHED THEN
		INSERT (service, language, element, value, property)
		VALUES (s.service, s.language, s.element, s.value, s.property)
	WHEN MATCHED THEN
		UPDATE SET value = s.value;
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
