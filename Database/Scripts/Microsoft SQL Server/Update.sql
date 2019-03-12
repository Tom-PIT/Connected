/*
Run this script on:

        sys-db\sql2017.tompit_update    -  This database will be modified

to synchronize it with:

        sys-db\sql2017.tompit_sys

You are recommended to back up your database before running this script

Script created by SQL Compare Engine version 12.3.3.4490 from Red Gate Software Ltd at 3/12/2019 8:22:22 AM

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
PRINT N'Creating [tompit].[mail_queue]'
GO
CREATE TABLE [tompit].[mail_queue]
(
[id] [bigint] NOT NULL,
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
PRINT N'Creating primary key [PK_mail_queue] on [tompit].[mail_queue]'
GO
ALTER TABLE [tompit].[mail_queue] ADD CONSTRAINT [PK_mail_queue] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[mail_reset]'
GO
CREATE PROCEDURE [tompit].[mail_reset]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	 UPDATE tompit.mail_queue SET
		error = null,
		next_visible = getutcdate(),
		pop_receipt = null,
		dequeue_count = 0
	where id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[mail_sel]'
GO
CREATE PROCEDURE [tompit].[mail_sel]
	@token uniqueidentifier,
	@pop_receipt uniqueidentifier
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
PRINT N'Creating [tompit].[mail_upd]'
GO
CREATE PROCEDURE [tompit].[mail_upd]
		@next_visible datetime,
		@error nvarchar(1024) = null,
		@pop_receipt uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.mail_queue SET
		error = @error,
		next_visible = @next_visible,
		pop_receipt = null
	where pop_receipt = @pop_receipt;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscriber]'
GO
CREATE TABLE [tompit].[subscriber]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[subscription] [bigint] NOT NULL,
[resource_type] [int] NOT NULL,
[resource_primary_key] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_subscriber] on [tompit].[subscriber]'
GO
ALTER TABLE [tompit].[subscriber] ADD CONSTRAINT [PK_subscriber] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscriber_del]'
GO
CREATE PROCEDURE [tompit].[subscriber_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.subscriber
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscriber_ins]'
GO
CREATE PROCEDURE [tompit].[subscriber_ins]
	@subscription bigint,
	@resource_type int,
	@resource_primary_key nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.subscriber (subscription, resource_type, resource_primary_key)
	VALUES (@subscription, @resource_type, @resource_primary_key);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscriber_que]'
GO
CREATE PROCEDURE [tompit].[subscriber_que]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.subscriber 
	WHERE (id = @id)
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscriber_sel]'
GO
CREATE PROCEDURE [tompit].[subscriber_sel]
	@subscription bigint,
	@resource_type int,
	@resource_primary_key nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.subscriber 
	WHERE (subscription = @subscription)
	AND (resource_type = @resource_type)
	AND (resource_primary_key = @resource_primary_key);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscription]'
GO
CREATE TABLE [tompit].[subscription]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[token] [uniqueidentifier] NOT NULL,
[handler] [uniqueidentifier] NOT NULL,
[topic] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[primary_key] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_subscription] on [tompit].[subscription]'
GO
ALTER TABLE [tompit].[subscription] ADD CONSTRAINT [PK_subscription] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [tompit].[subscription]'
GO
ALTER TABLE [tompit].[subscription] ADD CONSTRAINT [IX_subscription] UNIQUE NONCLUSTERED  ([handler], [primary_key], [topic]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscription_del]'
GO
CREATE PROCEDURE [tompit].[subscription_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.subscription
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscription_event]'
GO
CREATE TABLE [tompit].[subscription_event]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[subscription] [bigint] NOT NULL,
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[created] [datetime2] NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[arguments] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_subscription_event] on [tompit].[subscription_event]'
GO
ALTER TABLE [tompit].[subscription_event] ADD CONSTRAINT [PK_subscription_event] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscription_event_del]'
GO
CREATE PROCEDURE [tompit].[subscription_event_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.subscription_event
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscription_event_ins]'
GO
CREATE PROCEDURE [tompit].[subscription_event_ins]
	@token uniqueidentifier,
	@subscription bigint,
	@name nvarchar(128),
	@created datetime2(7),
	@arguments nvarchar(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.subscription_event (token, subscription, name, created, arguments)
	VALUES (@token, @subscription, @name, @created, @arguments);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscription_event_view]'
GO
CREATE VIEW [tompit].[subscription_event_view]
AS

SELECT	e.id, e.token, e.subscription, e.name, e.created, e.arguments,
		s.handler, s.primary_key, s.topic, s.token subscription_token
FROM tompit.subscription_event e
INNER JOIN tompit.subscription s ON e.subscription= s.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscription_event_que]'
GO
CREATE PROCEDURE [tompit].[subscription_event_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.subscription_event_view;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscription_event_sel]'
GO
CREATE PROCEDURE [tompit].[subscription_event_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.subscription_event_view
	WHERE token = @token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscription_ins]'
GO
CREATE PROCEDURE [tompit].[subscription_ins]
	@token uniqueidentifier,
	@handler uniqueidentifier,
	@topic nvarchar(128) = NULL,
	@primary_key nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.subscription (token, handler, topic, primary_key)
	VALUES (@token, @handler, @topic, @primary_key);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscription_sel]'
GO
CREATE PROCEDURE [tompit].[subscription_sel]
	@token uniqueidentifier = NULL,
	@handler uniqueidentifier = NULL,
	@topic nvarchar(128) = NULL,
	@primary_key nvarchar(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.subscription 
	WHERE (@token IS NULL OR token = @token)
	AND (@handler IS NULL OR handler = @handler)
	AND (@topic IS NULL OR topic = @topic)
	AND (@primary_key IS NULL OR primary_key = @primary_key)
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscription_clr]'
GO
CREATE PROCEDURE [tompit].[subscription_clr]
	@handler uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.subscription
	WHERE handler = @handler;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[alien]'
GO
CREATE TABLE [tompit].[alien]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[first_name] [nvarchar] (64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[last_name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[email] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[mobile] [varchar] (48) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[phone] [varchar] (48) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[token] [uniqueidentifier] NOT NULL,
[language] [int] NULL,
[timezone] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_alien] on [tompit].[alien]'
GO
ALTER TABLE [tompit].[alien] ADD CONSTRAINT [PK_alien] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[alien_ins]'
GO
CREATE PROCEDURE [tompit].[alien_ins]
	@first_name nvarchar(64) = NULL,
	@last_name nvarchar(128) = NULL,
	@email nvarchar(256) = NULL,
	@mobile varchar(48) = NULL,
	@phone varchar(48) = NULL,
	@token uniqueidentifier,
	@language int = NULL,
	@timezone nvarchar(256) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.alien (first_name, last_name, email, mobile, phone, token, language, timezone)
	VALUES (@first_name, @last_name, @email, @mobile, @phone, @token, @language, @timezone);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[alien_upd]'
GO
CREATE PROCEDURE [tompit].[alien_upd]
	@id bigint,
	@first_name nvarchar(64) = NULL,
	@last_name nvarchar(128) = NULL,
	@email nvarchar(256) = NULL,
	@mobile varchar(48) = NULL,
	@phone varchar(48) = NULL,
	@language int = NULL,
	@timezone nvarchar(256) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.alien SET
		first_name = @first_name, 
		last_name = @last_name, 
		email = @email, 
		mobile = @mobile, 
		phone = @phone, 
		language = @language, 
		timezone = @timezone
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[alien_del]'
GO
CREATE PROCEDURE [tompit].[alien_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.alien 
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[alien_view]'
GO
CREATE VIEW [tompit].[alien_view]
AS

SELECT a.id, a.token, a.first_name, a.last_name, a.email, a.mobile, a.phone, a.language, a.timezone,
		l.token language_token
FROM tompit.alien a
LEFT JOIN tompit.language l ON a.language = l.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[alien_que]'
GO
CREATE PROCEDURE [tompit].[alien_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.alien_view ;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[alien_sel]'
GO
CREATE PROCEDURE [tompit].[alien_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.alien_view 
	WHERE token = @token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_suite]'
GO
CREATE TABLE [tompit].[test_suite]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[suite] [uniqueidentifier] NOT NULL,
[run_count] [int] NOT NULL,
[success_count] [int] NOT NULL,
[service] [int] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_test_suite] on [tompit].[test_suite]'
GO
ALTER TABLE [tompit].[test_suite] ADD CONSTRAINT [PK_test_suite] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_suite_del]'
GO
CREATE PROCEDURE [tompit].[test_suite_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.test_suite
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session]'
GO
CREATE TABLE [tompit].[test_session]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[suite] [int] NOT NULL,
[start] [datetime2] NOT NULL,
[complete] [datetime2] NULL,
[status] [int] NOT NULL,
[result] [int] NOT NULL,
[error] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[token] [uniqueidentifier] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_test_session] on [tompit].[test_session]'
GO
ALTER TABLE [tompit].[test_session] ADD CONSTRAINT [PK_test_session] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_ins]'
GO
CREATE PROCEDURE [tompit].[test_session_ins]
	@suite int,
	@start datetime2(7),
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.test_session (suite, start, status, result, token)
	VALUES (@suite, @start, 1, 1, @token);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_scenario]'
GO
CREATE TABLE [tompit].[test_session_scenario]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[session] [bigint] NOT NULL,
[scenario] [uniqueidentifier] NOT NULL,
[start] [datetime2] NOT NULL,
[complete] [datetime2] NULL,
[status] [int] NOT NULL,
[result] [int] NOT NULL,
[error] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_test_session_scenario] on [tompit].[test_session_scenario]'
GO
ALTER TABLE [tompit].[test_session_scenario] ADD CONSTRAINT [PK_test_session_scenario] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_scenario_ins]'
GO
CREATE PROCEDURE [tompit].[test_session_scenario_ins]
	@session bigint,
	@scenario uniqueidentifier,
	@start datetime2(7)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.test_session_scenario ([session], scenario, start, status, result)
	VALUES (@session, @scenario, @start, 1, 1);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_scenario_view]'
GO
CREATE VIEW [tompit].[test_session_scenario_view]
AS

SELECT s.id, s.session, s.scenario, s.start, s.complete, s.status, s.result, s.error,
		ss.token session_token
FROM tompit.test_session_scenario s
INNER JOIN tompit.test_session ss ON s.session = ss.id;
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_scenario_que]'
GO
CREATE PROCEDURE [tompit].[test_session_scenario_que]
	@session bigint
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.test_session_scenario_view
	WHERE session = @session;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_view]'
GO

CREATE VIEW [tompit].[test_session_view]
AS

SELECT s.id, s.suite, s.start, s.complete, s.status, s.result, s.error, s.token,
		st.suite suite_token
FROM tompit.test_session s
INNER JOIN tompit.test_suite st ON s.suite = st.id;
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_que]'
GO
CREATE PROCEDURE [tompit].[test_session_que]
	@suite int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.test_session_view
	WHERE suite = @suite;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_case]'
GO
CREATE TABLE [tompit].[test_session_case]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[scenario] [bigint] NOT NULL,
[test_case] [uniqueidentifier] NOT NULL,
[start] [datetime2] NOT NULL,
[complete] [datetime2] NULL,
[status] [int] NOT NULL,
[result] [int] NOT NULL,
[error] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_test_session_case] on [tompit].[test_session_case]'
GO
ALTER TABLE [tompit].[test_session_case] ADD CONSTRAINT [PK_test_session_case] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_case_view]'
GO
CREATE VIEW [tompit].[test_session_case_view]
AS

SELECT s.id, s.scenario, s.test_case, s.start, s.complete, s.status, s.result, s.error,
		sc.scenario scenario_token
FROM tompit.test_session_case s
INNER JOIN tompit.test_session_scenario sc ON s.scenario = sc.id;
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_case_que]'
GO
CREATE PROCEDURE [tompit].[test_session_case_que]
	@scenario int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.test_session_case_view
	WHERE scenario = @scenario;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_scenario_sel]'
GO
CREATE PROCEDURE [tompit].[test_session_scenario_sel]
	@session int,
	@scenario uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.test_session_scenario_view
	WHERE [session] = @session
	AND scenario = @scenario;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_sel]'
GO
CREATE PROCEDURE [tompit].[test_session_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.test_session_view
	WHERE token = @token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_suite_sel]'
GO
CREATE PROCEDURE [tompit].[test_suite_sel]
	@suite uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.test_suite
	WHERE suite = @suite;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_case_sel]'
GO
CREATE PROCEDURE [tompit].[test_session_case_sel]
	@scenario bigint,
	@test_case uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.test_session_case_view
	WHERE scenario = @scenario
	AND test_case = @test_case;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_upd]'
GO
CREATE PROCEDURE [tompit].[test_session_upd]
	@id bigint,
	@status int,
	@result int,
	@complete datetime2(7) = NULL,
	@error nvarchar(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.test_session SET
		status = @status,
		result = @result,
		complete = @complete,
		error = @error
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_scenario_upd]'
GO
CREATE PROCEDURE [tompit].[test_session_scenario_upd]
	@id bigint,
	@status int,
	@result int,
	@complete datetime2(7) = NULL,
	@error nvarchar(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.test_session_scenario SET
		status = @status,
		result = @result,
		complete = @complete,
		error = @error
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[test_session_case_upd]'
GO
CREATE PROCEDURE [tompit].[test_session_case_upd]
	@id bigint,
	@status int,
	@result int,
	@complete datetime2(7) = NULL,
	@error nvarchar(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.test_session_case SET
		status = @status,
		result = @result,
		complete = @complete,
		error = @error
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[mail_dequeue]'
GO
CREATE PROCEDURE [tompit].[mail_dequeue]
	@next_visible datetime,
	@date datetime,
	@count int = 32
AS
BEGIN
	SET NOCOUNT ON;

	declare @ct table(num bigint);

	with q as
		(
			select top (@count) *
			from tompit.mail_queue with (readpast)
			where next_visible < @date
			and expire > @date
			order by next_visible, id
		)
	 update  q with (UPDLOCK, READPAST) set
		next_visible = @next_visible,
		dequeue_count = dequeue_count + 1,
		dequeue_timestamp = @date,
		pop_receipt = newid()
	output inserted.id into @ct;

	select * from tompit.mail_queue where id IN (select num from @ct);	
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[mail_queue_del]'
GO
CREATE PROCEDURE [tompit].[mail_queue_del]
	@pop_receipt uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.mail_queue
	WHERE pop_receipt = @pop_receipt;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[mail_ins]'
GO
CREATE PROCEDURE [tompit].[mail_ins]
	@created datetime,
	@token uniqueidentifier,
	@from nvarchar(256),
	@to nvarchar(256),
	@next_visible datetime,
	@expire smalldatetime,
	@subject nvarchar(256),
	@body nvarchar(MAX) = NULL,
	@headers nvarchar(MAX) = NULL,
	@attachment_count int,
	@format int
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.mail_queue (created, token, [from], [to], next_visible, expire, dequeue_count, subject, body, 
		headers, attachment_count, [format])
	VALUES (@created, @token, @from, @to, @next_visible, @expire, 0, @subject, @body, 
		@headers, @attachment_count, @format);

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[mail_clr]'
GO
CREATE PROCEDURE [tompit].[mail_clr]
AS
BEGIN
	SET NOCOUNT ON;

	delete tompit.mail_queue;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[mail_del]'
GO
CREATE PROCEDURE [tompit].[mail_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.mail_queue 
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[mail_que]'
GO
CREATE PROCEDURE [tompit].[mail_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.mail_queue;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscriber_claim]'
GO
CREATE TABLE [tompit].[subscriber_claim]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[claim] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[handler] [uniqueidentifier] NOT NULL,
[topic] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_subscriber_claim] on [tompit].[subscriber_claim]'
GO
ALTER TABLE [tompit].[subscriber_claim] ADD CONSTRAINT [PK_subscriber_claim] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[alien]'
GO
ALTER TABLE [tompit].[alien] ADD CONSTRAINT [FK_alien_language] FOREIGN KEY ([language]) REFERENCES [tompit].[language] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[subscriber]'
GO
ALTER TABLE [tompit].[subscriber] ADD CONSTRAINT [FK_subscriber_subscription] FOREIGN KEY ([subscription]) REFERENCES [tompit].[subscription] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[subscription_event]'
GO
ALTER TABLE [tompit].[subscription_event] ADD CONSTRAINT [FK_subscription_event_subscription] FOREIGN KEY ([subscription]) REFERENCES [tompit].[subscription] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[test_session_scenario]'
GO
ALTER TABLE [tompit].[test_session_scenario] ADD CONSTRAINT [FK_test_session_scenario_test_session] FOREIGN KEY ([session]) REFERENCES [tompit].[test_session] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[test_session]'
GO
ALTER TABLE [tompit].[test_session] ADD CONSTRAINT [FK_test_session_test_suite] FOREIGN KEY ([suite]) REFERENCES [tompit].[test_suite] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[test_session_case]'
GO
ALTER TABLE [tompit].[test_session_case] ADD CONSTRAINT [FK_test_session_case_test_session_scenario] FOREIGN KEY ([scenario]) REFERENCES [tompit].[test_session_scenario] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[test_suite]'
GO
ALTER TABLE [tompit].[test_suite] ADD CONSTRAINT [FK_test_suite_service] FOREIGN KEY ([service]) REFERENCES [tompit].[service] ([id]) ON DELETE CASCADE
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
