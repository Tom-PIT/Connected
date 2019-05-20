
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
PRINT N'Creating schemas'
GO
CREATE SCHEMA [tompit]
AUTHORIZATION [dbo]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating types'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
CREATE TYPE [tompit].[message_recipient_list] AS TABLE
(
[id] [bigint] NULL,
[retry_count] [int] NULL,
[next_visible] [datetime2] NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
CREATE TYPE [tompit].[token_list] AS TABLE
(
[token] [uniqueidentifier] NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[mail_queue]'
GO
CREATE TABLE [tompit].[mail_queue]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
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
PRINT N'Creating [tompit].[instance_endpoint]'
GO
CREATE TABLE [tompit].[instance_endpoint]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[url] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[status] [int] NOT NULL,
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[type] [int] NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[verbs] [int] NOT NULL,
[reverse_proxy_url] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_instance_endpoint] on [tompit].[instance_endpoint]'
GO
ALTER TABLE [tompit].[instance_endpoint] ADD CONSTRAINT [PK_instance_endpoint] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[instance_endpoint_que]'
GO
CREATE PROCEDURE [tompit].[instance_endpoint_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from instance_endpoint;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[mail_sel]'
GO
CREATE PROCEDURE [tompit].[mail_sel]
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
PRINT N'Creating [tompit].[resource_group]'
GO
CREATE TABLE [tompit].[resource_group]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[storage_provider] [uniqueidentifier] NOT NULL,
[connection_string] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_resource_group] on [tompit].[resource_group]'
GO
ALTER TABLE [tompit].[resource_group] ADD CONSTRAINT [PK_resource_group] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob]'
GO
CREATE TABLE [tompit].[blob]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[file_name] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[token] [uniqueidentifier] NOT NULL,
[size] [int] NOT NULL,
[type] [int] NOT NULL,
[content_type] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[primary_key] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[service] [uniqueidentifier] NULL,
[draft] [uniqueidentifier] NULL,
[version] [int] NOT NULL,
[topic] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[modified] [datetime] NOT NULL,
[resource_group] [int] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_blob] on [tompit].[blob]'
GO
ALTER TABLE [tompit].[blob] ADD CONSTRAINT [PK_blob] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_blob_1] on [tompit].[blob]'
GO
CREATE NONCLUSTERED INDEX [IX_blob_1] ON [tompit].[blob] ([draft]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_blob] on [tompit].[blob]'
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_blob] ON [tompit].[blob] ([token]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_blob]'
GO




CREATE VIEW [tompit].[view_blob]
AS
SELECT        b.*, r.token AS resource_token
FROM            tompit.blob AS b INNER JOIN
                         tompit.resource_group AS r ON b.resource_group = r.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_sel]'
GO
CREATE PROCEDURE [tompit].[blob_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 *
	from view_blob
	where (token = @token);
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
PRINT N'Creating [tompit].[membership]'
GO
CREATE TABLE [tompit].[membership]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[user] [int] NOT NULL,
[role] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_membership] on [tompit].[membership]'
GO
ALTER TABLE [tompit].[membership] ADD CONSTRAINT [PK_membership] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [tompit].[membership]'
GO
ALTER TABLE [tompit].[membership] ADD CONSTRAINT [IX_membership] UNIQUE NONCLUSTERED  ([role], [user]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[membership_del]'
GO
CREATE PROCEDURE [tompit].[membership_del]
	@user int,
	@role uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.membership 
	WHERE [user] = @user
	AND role = @role;
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
PRINT N'Creating [tompit].[user]'
GO
CREATE TABLE [tompit].[user]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[token] [uniqueidentifier] NOT NULL,
[auth_token] [uniqueidentifier] NULL,
[url] [nvarchar] (136) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[email] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[status] [int] NOT NULL,
[first_name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[last_name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[description] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[password] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[language] [int] NULL,
[last_login] [smalldatetime] NULL,
[timezone] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[notification_enabled] [bit] NOT NULL,
[login_name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[pin] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[mobile] [varchar] (48) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[phone] [varchar] (48) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[avatar] [uniqueidentifier] NULL,
[password_change] [smalldatetime] NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_user] on [tompit].[user]'
GO
ALTER TABLE [tompit].[user] ADD CONSTRAINT [PK_user] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[service]'
GO
CREATE TABLE [tompit].[service]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[url] [nvarchar] (136) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[status] [int] NOT NULL,
[resource_group] [int] NOT NULL,
[template] [uniqueidentifier] NOT NULL,
[meta] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[license] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[package] [uniqueidentifier] NULL,
[update_status] [int] NOT NULL CONSTRAINT [DF_service_update_status] DEFAULT ((0)),
[commit_status] [int] NOT NULL CONSTRAINT [DF_service_commit_status] DEFAULT ((0)),
[version] [varchar] (32) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_service] on [tompit].[service]'
GO
ALTER TABLE [tompit].[service] ADD CONSTRAINT [PK_service] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
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
PRINT N'Creating [tompit].[component]'
GO
CREATE TABLE [tompit].[component]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[type] [nvarchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[folder] [int] NULL,
[category] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[runtime_configuration] [uniqueidentifier] NULL,
[modified] [datetime] NULL,
[service] [int] NULL,
[lock_status] [int] NOT NULL CONSTRAINT [DF_component_status] DEFAULT ((0)),
[lock_user] [int] NULL,
[lock_date] [datetime] NULL,
[lock_verb] [int] NOT NULL CONSTRAINT [DF_component_lock_verb] DEFAULT ((0))
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_component] on [tompit].[component]'
GO
ALTER TABLE [tompit].[component] ADD CONSTRAINT [PK_component] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_component]'
GO




CREATE VIEW [tompit].[view_component]
AS
SELECT      c.id, c.name, c.token, c.type, c.category, c.runtime_configuration, c.modified,
			c.service, c.folder, c.lock_status, c.lock_date, c.lock_user, c.lock_verb,
			u.token lock_user_token,
			f.token as folder_token, s.token AS [service_token]
FROM        tompit.component AS c 
LEFT JOIN	tompit.folder f on c.folder=f.id
LEFT JOIN	tompit.[user] u ON c.lock_user = u.id
INNER JOIN	tompit.service s on c.service = s.id

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
PRINT N'Creating [tompit].[resource_group_que]'
GO
CREATE PROCEDURE [tompit].[resource_group_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from resource_group;
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
PRINT N'Creating [tompit].[subscriber_que]'
GO
CREATE PROCEDURE [tompit].[subscriber_que]
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
PRINT N'Creating [tompit].[component_del]'
GO
CREATE PROCEDURE [tompit].[component_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	delete tompit.component 
	where id = @id;
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
	FROM tompit.view_subscriber 
	WHERE (subscription = @subscription)
	AND (resource_type = @resource_type)
	AND (resource_primary_key = @resource_primary_key);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[instance_endpoint_sel]'
GO
CREATE PROCEDURE [tompit].[instance_endpoint_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from instance_endpoint
	where token = @token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[resource_group_sel]'
GO
CREATE PROCEDURE [tompit].[resource_group_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from resource_group
	where token = @token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[environment_unit]'
GO
CREATE TABLE [tompit].[environment_unit]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[parent] [int] NULL,
[token] [uniqueidentifier] NOT NULL,
[ordinal] [int] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_environment_unit] on [tompit].[environment_unit]'
GO
ALTER TABLE [tompit].[environment_unit] ADD CONSTRAINT [PK_environment_unit] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[environment_unit_upd]'
GO
CREATE PROCEDURE [tompit].[environment_unit_upd]
	@id int,
	@name nvarchar(128),
	@parent int = null,
	@ordinal int
AS
BEGIN
	SET NOCOUNT ON;

	update environment_unit set
		name = @name,
		parent = @parent,
		ordinal = @ordinal
	where id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[instance_endpoint_upd]'
GO
CREATE PROCEDURE [tompit].[instance_endpoint_upd]
	@id int,
	@url nvarchar(1024)=NULL,
	@status int,
	@name nvarchar(128),
	@type int,
	@verbs int,
	@referse_proxy_url nvarchar(1024) =null
AS
BEGIN
	SET NOCOUNT ON;

	update instance_endpoint set
		url = @url,
		status = @status,
		name = @name,
		type = @type,
		verbs = @verbs,
		reverse_proxy_url = @referse_proxy_url
	where id = @id;
END
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
PRINT N'Creating [tompit].[view_service]'
GO







CREATE VIEW [tompit].[view_service]
AS
SELECT        s.id, s.name, s.url, s.token, s.status, s.resource_group, s.template, s.meta,
				s.license, s.package, s.update_status, s.commit_status, s.version,
				r.token AS resource_token
FROM            tompit.service AS s INNER JOIN
                         tompit.resource_group AS r ON s.resource_group = r.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[component_ins]'
GO
CREATE PROCEDURE [tompit].[component_ins]
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
PRINT N'Creating [tompit].[service_que]'
GO
CREATE PROCEDURE [tompit].[service_que]
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from view_service;
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
PRINT N'Creating [tompit].[resource_group_upd]'
GO
CREATE PROCEDURE [tompit].[resource_group_upd]
	@id int,
	@name nvarchar(128),
	@storage_provider uniqueidentifier,
	@connection_string nvarchar(1024) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	update resource_group set
		name = @name,
		storage_provider = @storage_provider,
		connection_string = @connection_string
	where id = @id;
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
PRINT N'Creating [tompit].[component_que]'
GO
CREATE PROCEDURE [tompit].[component_que]
	@category nvarchar(128) = NULL,
	@name nvarchar(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.view_component
	WHERE (@category IS NULL OR category = @category)
	AND (@name IS NULL OR name = @name);
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
PRINT N'Creating [tompit].[blob_content]'
GO
CREATE TABLE [tompit].[blob_content]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[blob] [uniqueidentifier] NOT NULL,
[content] [varbinary] (max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_blob_content] on [tompit].[blob_content]'
GO
ALTER TABLE [tompit].[blob_content] ADD CONSTRAINT [PK_blob_content] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_blob_content] on [tompit].[blob_content]'
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_blob_content] ON [tompit].[blob_content] ([blob]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_content_del]'
GO
CREATE PROCEDURE [tompit].[blob_content_del]
	@blob uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	delete blob_content
	where blob = @blob;

END
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
PRINT N'Creating [tompit].[dev_error]'
GO
CREATE TABLE [tompit].[dev_error]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[service] [int] NOT NULL,
[component] [int] NOT NULL,
[element] [uniqueidentifier] NULL,
[severity] [int] NOT NULL,
[message] [nvarchar] (1024) COLLATE Slovenian_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_dev_error] on [tompit].[dev_error]'
GO
ALTER TABLE [tompit].[dev_error] ADD CONSTRAINT [PK_dev_error] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[dev_error_view]'
GO

CREATE VIEW [tompit].[dev_error_view]
AS
SELECT	e.id, e.service, e.component, e.element, e.severity, e.message,
		s.token service_token,
		c.token component_token, c.name component_name, c.category component_category
FROM tompit.dev_error e
INNER JOIN tompit.service s ON e.service = s.id
INNER JOIN tompit.component c ON e.component = c.id
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
PRINT N'Creating [tompit].[log]'
GO
CREATE TABLE [tompit].[log]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[created] [datetime2] NOT NULL,
[message] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[trace_level] [int] NULL,
[source] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[category] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[event_id] [int] NULL,
[component] [uniqueidentifier] NULL,
[element] [uniqueidentifier] NULL,
[metric] [uniqueidentifier] NULL,
[date] [date] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_log] on [tompit].[log]'
GO
ALTER TABLE [tompit].[log] ADD CONSTRAINT [PK_log] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_log_2] on [tompit].[log]'
GO
CREATE NONCLUSTERED INDEX [IX_log_2] ON [tompit].[log] ([component]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_log_1] on [tompit].[log]'
GO
CREATE NONCLUSTERED INDEX [IX_log_1] ON [tompit].[log] ([date]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_log] on [tompit].[log]'
GO
CREATE NONCLUSTERED INDEX [IX_log] ON [tompit].[log] ([metric]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[log_clr]'
GO
CREATE PROCEDURE [tompit].[log_clr]
	
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.log
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[dev_error_que]'
GO
CREATE PROCEDURE [tompit].[dev_error_que]
	@service int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.dev_error_view
	WHERE service = @service;
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
PRINT N'Creating [tompit].[component_sel]'
GO
CREATE PROCEDURE [tompit].[component_sel]
	@service int = NULL,
	@name nvarchar(128) = null,
	@component uniqueidentifier = null,
	@category nvarchar(128) = null
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 *
	from view_component
	where (@service IS NULL OR service = @service)
	and (@name is null or name = @name)
	and (@category is null or category = @category)
	and (@component is null or token = @component)
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[dev_error_clr]'
GO
CREATE PROCEDURE [tompit].[dev_error_clr]
	@component int,
	@element uniqueidentifier = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.dev_error
	WHERE component = @component
	AND (@element IS NULL OR element = @element);
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
PRINT N'Creating [tompit].[log_del]'
GO
CREATE PROCEDURE [tompit].[log_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.log
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[dev_error_ins]'
GO
CREATE PROCEDURE [tompit].[dev_error_ins]
	@service int,
	@component int,
	@items nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.dev_error ([service], component, element, severity, message)
	SELECT @service, @component, element, severity, message FROM OPENJSON(@items) WITH (element uniqueidentifier, severity int, message nvarchar(1024));

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[component_upd]'
GO
CREATE PROCEDURE [tompit].[component_upd]
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
PRINT N'Creating [tompit].[language]'
GO
CREATE TABLE [tompit].[language]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[lcid] [int] NOT NULL,
[status] [int] NOT NULL,
[mappings] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[token] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_language] on [tompit].[language]'
GO
ALTER TABLE [tompit].[language] ADD CONSTRAINT [PK_language] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[language_del]'
GO
CREATE PROCEDURE [tompit].[language_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	delete language
	where id = @id;
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
PRINT N'Creating [tompit].[blob_list]'
GO
CREATE PROCEDURE [tompit].[blob_list]
	@items tompit.token_list READONLY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.view_blob
	WHERE token IN (SELECT token from @items);
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
PRINT N'Creating [tompit].[message_subscriber]'
GO
CREATE TABLE [tompit].[message_subscriber]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[connection] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[topic] [bigint] NOT NULL,
[created] [datetime2] NOT NULL,
[alive] [datetime2] NOT NULL,
[instance] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_message_subscriber] on [tompit].[message_subscriber]'
GO
ALTER TABLE [tompit].[message_subscriber] ADD CONSTRAINT [PK_message_subscriber] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_subscriber_ins]'
GO
CREATE PROCEDURE [tompit].[message_subscriber_ins]
	@topic bigint,
	@connection nvarchar(128),
	@created datetime2(7),
	@alive datetime2(7),
	@instance uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	insert tompit.message_subscriber (connection, topic, created, alive, instance)
	values (@connection, @topic, @created, @alive, @instance);
END
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
PRINT N'Creating [tompit].[service_meta_sel]'
GO
CREATE PROCEDURE [tompit].[service_meta_sel]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 *
	from [service]
	where id = @id;
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
PRINT N'Creating [tompit].[language_ins]'
GO
CREATE PROCEDURE [tompit].[language_ins]
	@name nvarchar(64),
	@lcid int,
	@status int,
	@mappings nvarchar(128)=null,
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	insert language(name, lcid, status, mappings, token)
	values (@name, @lcid, @status, @mappings, @token);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[service_sel]'
GO
CREATE PROCEDURE [tompit].[service_sel]
	@url nvarchar(136) = null,
	@name nvarchar(128) = null,
	@token uniqueidentifier = null,
	@id int = null
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 *
	from view_service
	where (@url is null or url = @url)
	and (@name is null or name = @name)
	and (@token is null or token = @token)
	and (@id is null or id = @id);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_subscriber_del]'
GO
CREATE PROCEDURE [tompit].[message_subscriber_del]
	@topic bigint,
	@connection nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	delete tompit.message_subscriber 
	where topic = @topic
	and connection = @connection;
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
PRINT N'Creating [tompit].[service_del]'
GO
CREATE PROCEDURE [tompit].[service_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	delete [service]
	where id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[language_que]'
GO
CREATE PROCEDURE [tompit].[language_que]
AS
BEGIN
	SET NOCOUNT ON;

	select * 
	from language
	
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[user_del]'
GO
CREATE PROCEDURE [tompit].[user_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	delete [user] 
	where id = @id;
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
PRINT N'Creating [tompit].[language_sel]'
GO
CREATE PROCEDURE [tompit].[language_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	select * 
	from language
	where token = @token;
	
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
PRINT N'Creating [tompit].[service_string]'
GO
CREATE TABLE [tompit].[service_string]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[service] [int] NOT NULL,
[element] [uniqueidentifier] NOT NULL,
[value] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[language] [int] NOT NULL,
[property] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_service_string] on [tompit].[service_string]'
GO
ALTER TABLE [tompit].[service_string] ADD CONSTRAINT [PK_service_string] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[service_string_del]'
GO
CREATE PROCEDURE [tompit].[service_string_del]
	@service int,
	@element uniqueidentifier,
	@property nvarchar(256)
AS
BEGIN
	SET NOCOUNT ON;

	delete service_string
	where (service = @service)
	and (element = @element)
	and (property = @property);
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
PRINT N'Creating [tompit].[language_upd]'
GO
CREATE PROCEDURE [tompit].[language_upd]
	@id int,
	@name nvarchar(64),
	@lcid int,
	@status int,
	@mappings nvarchar(128)=null
AS
BEGIN
	SET NOCOUNT ON;

	update language set
		name = @name,
		lcid = @lcid,
		status = @status,
		mappings = @mappings
	where id = @id;
	
END
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
PRINT N'Creating [tompit].[view_service_string]'
GO



CREATE VIEW [tompit].[view_service_string]
AS
SELECT        s.*, l.token AS language_token, svc.token as service_token
FROM            tompit.service_string s INNER JOIN
                         tompit.language l ON s.language = l.id
						 inner join tompit.[service] svc on s.service=svc.id
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
PRINT N'Creating [tompit].[service_string_sel]'
GO
CREATE PROCEDURE [tompit].[service_string_sel]
	@service int,
	@element uniqueidentifier,
	@property nvarchar(256),
	@language int
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 *
	from view_service_string
	where (service = @service)
	and (element = @element)
	and (property = @property)
	and (language = @language);
END
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
PRINT N'Creating [tompit].[service_string_que]'
GO
CREATE PROCEDURE [tompit].[service_string_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from view_service_string ;
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
PRINT N'Creating [tompit].[metric]'
GO
CREATE TABLE [tompit].[metric]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[session] [uniqueidentifier] NOT NULL,
[start] [datetime2] NOT NULL,
[end] [datetime2] NULL,
[result] [int] NULL,
[instance] [int] NOT NULL,
[request_ip] [varchar] (48) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[component] [uniqueidentifier] NOT NULL,
[element] [uniqueidentifier] NULL,
[parent] [uniqueidentifier] NULL,
[request] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[response] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[consumption_in] [bigint] NULL,
[consumption_out] [bigint] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_metric] on [tompit].[metric]'
GO
ALTER TABLE [tompit].[metric] ADD CONSTRAINT [PK_metric] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_metric_1] on [tompit].[metric]'
GO
CREATE NONCLUSTERED INDEX [IX_metric_1] ON [tompit].[metric] ([parent]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_metric] on [tompit].[metric]'
GO
CREATE NONCLUSTERED INDEX [IX_metric] ON [tompit].[metric] ([session]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[metric_clr]'
GO
CREATE PROCEDURE [tompit].[metric_clr]
	@component uniqueidentifier = NULL,
	@element uniqueidentifier = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DELETE 
	FROM tompit.metric
	WHERE (@component IS NULL OR component = @component)
	AND (@element IS NULL OR element = @element);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_recipient]'
GO
CREATE TABLE [tompit].[message_recipient]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[message] [bigint] NOT NULL,
[subscriber] [bigint] NOT NULL,
[retry_count] [int] NOT NULL,
[next_visible] [datetime2] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_message_recipient] on [tompit].[message_recipient]'
GO
ALTER TABLE [tompit].[message_recipient] ADD CONSTRAINT [PK_message_recipient] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message]'
GO
CREATE TABLE [tompit].[message]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[message] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[topic] [bigint] NOT NULL,
[created] [datetime2] NOT NULL,
[expire] [datetime2] NOT NULL,
[retry_interval] [int] NOT NULL,
[token] [uniqueidentifier] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_message] on [tompit].[message]'
GO
ALTER TABLE [tompit].[message] ADD CONSTRAINT [PK_message] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_ins]'
GO
CREATE PROCEDURE [tompit].[message_ins]
	@topic bigint,
	@message nvarchar(MAX) = null,
	@created datetime2(7),
	@expire datetime2(7),
	@retry_interval int,
	@token uniqueidentifier,
	@sender_instance uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	declare @id bigint;

	insert tompit.message (topic, message, created, expire, retry_interval, token)
	values (@topic, @message, @created, @expire, @retry_interval, @token);

	set @id = scope_identity();

	insert tompit.message_recipient (message, subscriber, retry_count, next_visible)
	select @id,  id, 0, DATEADD(s, @retry_interval, @created) from tompit.message_subscriber where topic = @topic AND instance != @sender_instance;

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
PRINT N'Creating [tompit].[metric_ins]'
GO
CREATE PROCEDURE [tompit].[metric_ins]
	@items nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.metric (session, start, [end], result, instance, request_ip, component, element, parent, request, response, consumption_in, consumption_out)
	SELECT session, start, [end], result, instance, request_ip, component, element, parent, request, response, consumption_in, consumption_out 
	FROM OPENJSON (@items) WITH (session uniqueidentifier, start datetime2(7), [end] datetime2(7), result int, instance int, request_ip varchar(48),
		component uniqueidentifier, element uniqueidentifier, parent uniqueidentifier, request nvarchar(max), response nvarchar(max), consumption_in bigint, consumption_out bigint); 
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[service_ins]'
GO
CREATE PROCEDURE [tompit].[service_ins]
	@name nvarchar(128),
	@url nvarchar(136),
	@token uniqueidentifier,
	@status int,
	@resource_group int,
	@template uniqueidentifier,
	@meta nvarchar(max),
	@version varchar(32) = NULL
	
AS
BEGIN
	SET NOCOUNT ON;

	insert [service] (name, url, token, status, resource_group, template, meta, version)
	values (@name, @url, @token, @status, @resource_group, @template, @meta, @version);

	return scope_identity();
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
PRINT N'Creating [tompit].[metric_que]'
GO
CREATE PROCEDURE [tompit].[metric_que]
	@date date,
	@component uniqueidentifier,
	@element uniqueidentifier = NULL 
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.metric
	WHERE CAST([start] AS DATE) = @date
	AND (component = @component)
	AND (@element IS NULL OR element = @element); 
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[user_ins]'
GO
CREATE PROCEDURE [tompit].[user_ins]
	@token uniqueidentifier,
	@url nvarchar(136) = null,
	@email nvarchar(1024) = null,
	@status int,
	@first_name nvarchar(128) = null,
	@last_name nvarchar(128) = null,
	@description nvarchar(1024) = null,
	@language int = null,
	@timezone nvarchar(256) = null,
	@notification_enabled bit,
	@login_name nvarchar(128) = null,
	@pin nvarchar(128)=null,
	@mobile varchar(48) = null,
	@phone varchar(48) = null,
	@avatar uniqueidentifier = null,
	@auth_token uniqueidentifier,
	@password_change smalldatetime = null
AS
BEGIN
	SET NOCOUNT ON;

	insert [user] (token, url, email, status, first_name, last_name, description, language, timezone, notification_enabled, login_name,
		pin, mobile, phone, avatar, auth_token, password_change)
	values (@token, @url, @email, @status, @first_name, @last_name, @description, @language, @timezone, @notification_enabled, @login_name,
		@pin, @mobile, @phone, @avatar, @auth_token, @password_change);
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
PRINT N'Creating [tompit].[user_upd]'
GO
CREATE PROCEDURE [tompit].[user_upd]
	@id int,
	@url nvarchar(136) = null,
	@email nvarchar(1024) = null,
	@status int,
	@first_name nvarchar(128) = null,
	@last_name nvarchar(128) = null,
	@description nvarchar(1024) = null,
	@language int = null,
	@timezone nvarchar(256) = null,
	@notification_enabled bit,
	@login_name nvarchar(128) = null,
	@pin nvarchar(128)=null,
	@mobile varchar(48) = null,
	@phone varchar(48) = null,
	@avatar uniqueidentifier = null,
	@password_change smalldatetime = null
AS
BEGIN
	SET NOCOUNT ON;

	update [user] set
		url = @url, 
		email = @email, 
		status = @status, 
		first_name = @first_name, 
		last_name = @last_name, 
		description = @description, 
		language = @language, 
		timezone = @timezone, 
		notification_enabled = @notification_enabled, 
		login_name = @login_name,
		pin = @pin, 
		mobile = @mobile, 
		phone = @phone, 
		avatar = @avatar,
		password_change = @password_change
	where id = @id;
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
PRINT N'Creating [tompit].[api_test]'
GO
CREATE TABLE [tompit].[api_test]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[title] [nvarchar] (256) COLLATE Slovenian_CI_AS NOT NULL,
[description] [nvarchar] (1024) COLLATE Slovenian_CI_AS NULL,
[body] [nvarchar] (max) COLLATE Slovenian_CI_AS NULL,
[identifier] [uniqueidentifier] NOT NULL CONSTRAINT [DF_api_test_identifier] DEFAULT (newid()),
[api] [nvarchar] (1024) COLLATE Slovenian_CI_AS NOT NULL,
[tags] [nvarchar] (1024) COLLATE Slovenian_CI_AS NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_api_test] on [tompit].[api_test]'
GO
ALTER TABLE [tompit].[api_test] ADD CONSTRAINT [PK_api_test] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[api_test_del]'
GO
CREATE PROCEDURE [tompit].[api_test_del]
	@identifier uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.api_test
	WHERE identifier = @identifier;

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
PRINT N'Creating [tompit].[message_topic]'
GO
CREATE TABLE [tompit].[message_topic]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[resource_group] [int] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_message_topic] on [tompit].[message_topic]'
GO
ALTER TABLE [tompit].[message_topic] ADD CONSTRAINT [PK_message_topic] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_message_recipient]'
GO






CREATE VIEW [tompit].[view_message_recipient]
AS
SELECT      r.id, r.message, r.subscriber, r.retry_count, r.next_visible, m.token message_token, m.topic, s.connection, t.name topic_name, m.message content
FROM        tompit.message_recipient AS r 
INNER JOIN	tompit.[message] AS m ON r.[message] = m.id
INNER JOIN	tompit.message_subscriber s on r.subscriber = s.id
INNER JOIN	tompit.message_topic t on m.topic = t.id

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
PRINT N'Creating [tompit].[service_string_mdf]'
GO
CREATE PROCEDURE [tompit].[service_string_mdf]
	@service int,
	@language int,
	@element uniqueidentifier,
	@property nvarchar(256),
	@value nvarchar(max) = null
AS
BEGIN
	SET NOCOUNT ON;

	MERGE service_string AS d
	USING (SELECT @service, @language, @element, @property) AS s (service, language, element, property)
	ON (d.service = s.service AND d.language = s.language AND d.element = s.element AND d.property = s.property)
	WHEN NOT MATCHED THEN
		INSERT (service, language, element, value, property)
		VALUES (@service, @language, @element, @value, @property)
	WHEN MATCHED THEN
		UPDATE SET value = @value;
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
PRINT N'Creating [tompit].[message_recipient_que]'
GO
CREATE PROCEDURE [tompit].[message_recipient_que]
	@message bigint = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.view_message_recipient
	WHERE (@message IS NULL OR message = @message);
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
PRINT N'Creating [tompit].[message_subscriber_upd]'
GO
CREATE PROCEDURE [tompit].[message_subscriber_upd]
	@id bigint,
	@alive datetime2(7)
AS
BEGIN
	SET NOCOUNT ON;

	update tompit.message_subscriber set
		alive = @alive
	where id = @id;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_user]'
GO



CREATE VIEW [tompit].[view_user]
AS
SELECT        u.*, l.token AS language_token
FROM            tompit.[user] AS u LEFT OUTER JOIN
                         tompit.language AS l ON u.id = l.id

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_message_topic]'
GO

CREATE VIEW [tompit].[view_message_topic]
AS
SELECT        tompit.message_topic.id, tompit.message_topic.name, tompit.message_topic.resource_group, tompit.resource_group.token AS resource_group_token
FROM            tompit.resource_group INNER JOIN
                         tompit.message_topic ON tompit.resource_group.id = tompit.message_topic.resource_group
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[user_que]'
GO
CREATE PROCEDURE [tompit].[user_que]
AS
BEGIN
	SET NOCOUNT ON;

	select * 
	from view_user;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[user_sel]'
GO
CREATE PROCEDURE [tompit].[user_sel]
	@id int = null,
	@token uniqueidentifier = null,
	@email nvarchar(256) = null,
	@url nvarchar(136) = null,
	@login_name nvarchar(128)=null,
	@auth_token uniqueidentifier = null
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 * 
	from view_user
	where (@id is null or id = @id)
	and (@token is null or token = @token)
	and (@email is null or email = @email)
	and (@url is null or url = @url)
	and (@login_name is null or login_name = @login_name)
	and (@auth_token is null or auth_token = @auth_token)
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
PRINT N'Creating [tompit].[queue]'
GO
CREATE TABLE [tompit].[queue]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[message] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[created] [datetime] NOT NULL,
[expire] [datetime] NOT NULL,
[next_visible] [datetime] NOT NULL,
[queue] [varchar] (32) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[pop_receipt] [uniqueidentifier] NULL,
[dequeue_count] [int] NOT NULL,
[scope] [int] NOT NULL,
[dequeue_timestamp] [datetime] NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_queue] on [tompit].[queue]'
GO
ALTER TABLE [tompit].[queue] ADD CONSTRAINT [PK_queue] PRIMARY KEY NONCLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_queue_1] on [tompit].[queue]'
GO
CREATE NONCLUSTERED INDEX [IX_queue_1] ON [tompit].[queue] ([pop_receipt]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[queue_del]'
GO
CREATE PROCEDURE [tompit].[queue_del]
	@pop_receipt uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.queue
	WHERE pop_receipt = @pop_receipt;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_topic_que]'
GO
CREATE PROCEDURE [tompit].[message_topic_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.view_message_topic;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[api_test_ins]'
GO
CREATE PROCEDURE [tompit].[api_test_ins]
	@title nvarchar(128),
	@description nvarchar(1024) = NULL,
	@body nvarchar(max) = NULL,
	@api nvarchar(1024),
	@tags nvarchar(1024),
	@identifier uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.api_test (title, description, api, tags, body, identifier)
	VALUES (@title, @description, @api, @tags, @body, @identifier);

	RETURN scope_identity();

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_topic_sel]'
GO
CREATE PROCEDURE [tompit].[message_topic_sel]
	@name nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT top 1 *
	FROM tompit.view_message_topic
	where name = @name;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[setting]'
GO
CREATE TABLE [tompit].[setting]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[resource_group] [int] NULL,
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[value] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[visible] [bit] NOT NULL,
[data_type] [int] NOT NULL,
[tags] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_setting] on [tompit].[setting]'
GO
ALTER TABLE [tompit].[setting] ADD CONSTRAINT [PK_setting] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_setting]'
GO


CREATE VIEW [tompit].[view_setting]
AS
SELECT        s.*, r.token resource_token
FROM            tompit.setting AS s LEFT OUTER JOIN
                         tompit.resource_group AS r ON s.resource_group = r.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[iot_state]'
GO
CREATE TABLE [tompit].[iot_state]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[hub] [uniqueidentifier] NOT NULL,
[field] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[value] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[modified] [datetime2] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_iot_state] on [tompit].[iot_state]'
GO
ALTER TABLE [tompit].[iot_state] ADD CONSTRAINT [PK_iot_state] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_iot_state] on [tompit].[iot_state]'
GO
CREATE NONCLUSTERED INDEX [IX_iot_state] ON [tompit].[iot_state] ([hub]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[iot_state_upd]'
GO
CREATE PROCEDURE [tompit].[iot_state_upd]
	@items nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	MERGE tompit.iot_state AS d
	USING (SELECT * FROM OPENJSON (@items) WITH (hub uniqueidentifier, field nvarchar(128), value nvarchar(1024))) AS s (hub, field, value)
	ON (d.hub = s.hub AND d.field = s.field)
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (hub, field, modified)
		VALUES (hub, field, GETUTCDATE())
	WHEN MATCHED THEN
		UPDATE SET
			value = s.value,
			modified = GETUTCDATE();
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[service_meta_upd]'
GO
CREATE PROCEDURE [tompit].[service_meta_upd]
	@id int,
	@meta varbinary(max)
AS
BEGIN
	SET NOCOUNT ON;

	update [service] set
		meta = @meta
	where id = @id;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[setting_que]'
GO
CREATE PROCEDURE [tompit].[setting_que]
AS
BEGIN
	SET NOCOUNT ON;

	select * 
	from view_setting;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[iot_state_sel]'
GO
CREATE PROCEDURE [tompit].[iot_state_sel]
	@hub uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.iot_state
	WHERE hub = @hub;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[api_test_que]'
GO
CREATE PROCEDURE [tompit].[api_test_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT id, identifier, title, description, api, tags
	FROM tompit.api_test;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_partition]'
GO
CREATE TABLE [tompit].[big_data_partition]
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
PRINT N'Creating [tompit].[big_data_partition_ins]'
GO
CREATE PROCEDURE [tompit].[big_data_partition_ins]
	@configuration uniqueidentifier,
	@status int,
	@name nvarchar(128),
	@created smalldatetime
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.big_data_partition (configuration, file_count, status, name, created)
	VALUES (@configuration, 0, @status, @name, @created);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[queue_dequeue]'
GO
CREATE PROCEDURE [tompit].[queue_dequeue]
	@queue varchar(32),
	@next_visible datetime,
	@count int = 32,
	@date datetime
AS
BEGIN
	SET NOCOUNT ON;

	declare @ct table(num bigint);

	with q as
		(
			select top (@count) *
			from tompit.queue with (readpast)
			where next_visible < @date
			and expire > @date
			and queue = @queue
			and scope = 0
			order by next_visible, id
		)
	 update  q with (UPDLOCK, READPAST) set
		next_visible = @next_visible,
		dequeue_count = dequeue_count + 1,
		dequeue_timestamp = @date,
		pop_receipt = newid()
	output inserted.id into @ct;

	select * from tompit.queue where id IN (select num from @ct);	
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_partition_upd]'
GO
CREATE PROCEDURE [tompit].[big_data_partition_upd]
	@id int,
	@status int,
	@name nvarchar(128),
	@file_count int
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.big_data_partition SET
		file_count = @file_count, 
		status = @status, 
		name = @name
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_partition_que]'
GO
CREATE PROCEDURE [tompit].[big_data_partition_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.big_data_partition;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_que_draft]'
GO
CREATE PROCEDURE [tompit].[blob_que_draft]
	@draft uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from	view_blob
	WHERE	primary_key IS NULL
	and draft = @draft;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_partition_sel]'
GO
CREATE PROCEDURE [tompit].[big_data_partition_sel]
	@configuration uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.big_data_partition
	WHERE configuration = @configuration;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[user_state]'
GO
CREATE TABLE [tompit].[user_state]
(
[user_id] [int] NOT NULL,
[state] [varbinary] (max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_user_state] on [tompit].[user_state]'
GO
ALTER TABLE [tompit].[user_state] ADD CONSTRAINT [PK_user_state] PRIMARY KEY CLUSTERED  ([user_id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[user_state_sel]'
GO
CREATE PROCEDURE [tompit].[user_state_sel]
	@user_id int
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 * 
	from user_state
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_commit]'
GO
CREATE PROCEDURE [tompit].[blob_commit]
	@primary_key nvarchar(256),
	@draft uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE blob SET
		primary_key = @primary_key,
		draft = NULL
	WHERE draft = @draft;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[api_test_sel]'
GO
CREATE PROCEDURE [tompit].[api_test_sel]
	@identifier uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT body
	FROM tompit.api_test
	WHERE identifier = @identifier;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_del]'
GO
CREATE PROCEDURE [tompit].[blob_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	delete blob 
	WHERE (id = @id)
END
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
[status] [int] NOT NULL,
[error] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[modified] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
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

	MERGE tompit.installer AS d
	USING (SELECT * FROM OPENJSON (@items) WITH (package uniqueidentifier, parent uniqueidentifier)) AS s (package, parent)
	ON (d.package = s.package)
	WHEN NOT MATCHED THEN
		INSERT (package, parent, status, modified)
		VALUES (s.package, s.parent, 1, GETUTCDATE())
	WHEN MATCHED THEN
		UPDATE SET
			status = 1,
			modified = GETUTCDATE(),
			parent = s.parent,
			error = NULL;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_topic_ins]'
GO
CREATE PROCEDURE [tompit].[message_topic_ins]
	@name nvarchar(128),
	@resource_group int
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.message_topic (name, resource_group)
	VALUES (@name, @resource_group);

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node]'
GO
CREATE TABLE [tompit].[big_data_node]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[connection_string] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[admin_connection_string] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[token] [uniqueidentifier] NOT NULL,
[status] [int] NOT NULL,
[size] [bigint] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_node] on [tompit].[big_data_node]'
GO
ALTER TABLE [tompit].[big_data_node] ADD CONSTRAINT [PK_big_data_node] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node_del]'
GO
CREATE PROCEDURE [tompit].[big_data_node_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.big_data_node
	WHERE id = @id;
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

	DELETE tompit.installer 
	WHERE package = @package;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[service_upd]'
GO
CREATE PROCEDURE [tompit].[service_upd]
	@id int,
	@name nvarchar(128),
	@url nvarchar(136),
	@status int,
	@template uniqueidentifier,
	@resource_group int,
	@package uniqueidentifier = NULL,
	@update_status int,
	@commit_status int
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
		update_status = @update_status,
		commit_status = @commit_status
	where id = @id;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node_ins]'
GO
CREATE PROCEDURE [tompit].[big_data_node_ins]
	@name nvarchar(128),
	@connection_string nvarchar(256),
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
PRINT N'Creating [tompit].[user_state_mdf]'
GO
CREATE PROCEDURE [tompit].[user_state_mdf]
	@user_id int,
	@state varbinary(max)=null
AS
BEGIN
	SET NOCOUNT ON;

	merge user_state d
	using (select @user_id) as s (user_id)
	on (d.user_id = s.user_id)
	when not matched then
		insert (user_id, state)
		values (@user_id, @state)
	when matched then
		update set state = @state;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[environment_unit_upd_batch]'
GO
CREATE PROCEDURE [tompit].[environment_unit_upd_batch]
	@items nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.environment_unit SET
		name = j.name,
		parent = j.parent,
		ordinal = j.ordinal
	FROM OPENJSON(@items)  WITH ([name] nvarchar(128), parent int, ordinal int, id int)  AS j
	WHERE tompit.environment_unit.id = j.id;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node_upd]'
GO
CREATE PROCEDURE [tompit].[big_data_node_upd]
	@name nvarchar(128),
	@connection_string nvarchar(256),
	@admin_connection_string nvarchar(256) = NULL,
	@token uniqueidentifier,
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
PRINT N'Creating [tompit].[installer_upd]'
GO
CREATE PROCEDURE [tompit].[installer_upd]
	@id int,
	@status int,
	@error nvarchar(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.installer SET
		status = @status,
		error = @error,
		modified = GETUTCDATE()
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[api_test_upd]'
GO
CREATE PROCEDURE [tompit].[api_test_upd]
	@identifier uniqueidentifier,
	@title nvarchar(128),
	@description nvarchar(1024) = NULL,
	@api nvarchar(1024),
	@body nvarchar(MAX) = NULL,
	@tags nvarchar(1024)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.api_test SET
		title = @title,
		description = @description,
		body = @body,
		api = @api,
		tags = @tags
	WHERE identifier = @identifier;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_que]'
GO
CREATE PROCEDURE [tompit].[blob_que]
	@resource_group int = null,
	@type int = null,
	@primary_key nvarchar(128) = null,
	@service uniqueidentifier = null,
	@topic nvarchar(128) = null
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from view_blob 
	WHERE (@resource_group is null or resource_group = @resource_group)
	and (@type is null or type = @type)
	and (@primary_key is null or primary_key = @primary_key)
	and (@service is null or service = @service)
	and (@resource_group is null or resource_group = @resource_group);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node_que]'
GO
CREATE PROCEDURE [tompit].[big_data_node_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.big_data_node;
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
PRINT N'Creating [tompit].[message_topic_del]'
GO
CREATE PROCEDURE [tompit].[message_topic_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.message_topic
	WHERE id = @id;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_que_draft_orphaned]'
GO
CREATE PROCEDURE [tompit].[blob_que_draft_orphaned]
	@modified smalldatetime
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from	view_blob
	WHERE	primary_key IS NULL
	and modified < @modified;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node_sel]'
GO
CREATE PROCEDURE [tompit].[big_data_node_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.big_data_node
	WHERE token = @token;
END
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
PRINT N'Creating [tompit].[queue_dequeue_content]'
GO
CREATE PROCEDURE [tompit].[queue_dequeue_content]
	@next_visible datetime,
	@count int = 32,
	@date datetime
AS
BEGIN
	SET NOCOUNT ON;

	declare @ct table(num bigint);

	with q as
		(
			select top (@count) *
			from tompit.queue with (readpast)
			where next_visible < @date
			and expire > @date
			and scope = 1
			order by next_visible, id
		)
	 update  q with (UPDLOCK, READPAST) set
		next_visible = @next_visible,
		dequeue_count = dequeue_count + 1,
		dequeue_timestamp = @date,
		pop_receipt = newid()
	output inserted.id into @ct;

	select * from tompit.queue where id = (select num from @ct);	
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_partition_del]'
GO
CREATE PROCEDURE [tompit].[big_data_partition_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.big_data_partition
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_message_subscriber]'
GO







CREATE VIEW [tompit].[view_message_subscriber]
AS
SELECT      s.id, s.connection, s.topic, s.created, s.alive, s.instance, t.name topic_name
FROM        tompit.message_subscriber AS s
INNER JOIN	tompit.message_topic t on s.topic = t.id

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[queue_enqueue]'
GO
CREATE PROCEDURE [tompit].[queue_enqueue]
	@queue varchar(32),
	@message nvarchar(256),
	@expire int = 172800,
	@next_visible datetime,
	@scope int,
	@created datetime
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.queue (message, created, expire, next_visible, queue, pop_receipt, dequeue_count, scope)
	VALUES (@message, @created, @expire, @next_visible, @queue, NULL, 0, @scope);

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_content_sel]'
GO
CREATE PROCEDURE [tompit].[blob_content_sel]
	@blob uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 *
	from blob_content
	where blob = @blob; 
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_subscriber_que]'
GO
CREATE PROCEDURE [tompit].[message_subscriber_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.view_message_subscriber;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_content_que]'
GO
CREATE PROCEDURE [tompit].[blob_content_que]
	@blobs token_list readonly
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from blob_content
	where blob = (select token from @blobs); 
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[queue_upd]'
GO
CREATE PROCEDURE [tompit].[queue_upd]
	@pop_receipt uniqueidentifier,
	@next_visible datetime
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.queue SET
		next_visible = @next_visible
	WHERE pop_receipt = @pop_receipt;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_subscriber_sel]'
GO
CREATE PROCEDURE [tompit].[message_subscriber_sel]
	@topic bigint,
	@connection nvarchar(128)
	
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.view_message_subscriber
	WHERE topic = @topic
	AND connection = @connection;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[queue_sel]'
GO
CREATE PROCEDURE [tompit].[queue_sel]
	@pop_receipt uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.queue
	WHERE pop_receipt = @pop_receipt;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[permission]'
GO
CREATE TABLE [tompit].[permission]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[evidence] [uniqueidentifier] NOT NULL,
[schema] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[claim] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[descriptor] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[primary_key] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[value] [int] NOT NULL,
[resource_group] [int] NULL,
[component] [nvarchar] (128) COLLATE Slovenian_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_permission] on [tompit].[permission]'
GO
ALTER TABLE [tompit].[permission] ADD CONSTRAINT [PK_permission] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[permission_upd]'
GO
CREATE PROCEDURE [tompit].[permission_upd]
	@id bigint,
	@value int
AS
BEGIN
	SET NOCOUNT ON;

	update permission set
		value = @value
	where id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_content_mdf]'
GO
CREATE PROCEDURE [tompit].[blob_content_mdf]
	@blob uniqueidentifier,
	@content varbinary(max) = null
AS
BEGIN
	SET NOCOUNT ON;

MERGE blob_content AS d
	USING (SELECT @blob, @content) AS s (blob, content)
	ON (d.blob = s.blob)
	WHEN NOT MATCHED THEN
		INSERT (blob, content)
		VALUES (@blob, @content)
	WHEN MATCHED THEN
		UPDATE SET content = @content;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[installer_configuration]'
GO
CREATE TABLE [tompit].[installer_configuration]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[package] [uniqueidentifier] NOT NULL,
[configuration] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_installer_configuration] on [tompit].[installer_configuration]'
GO
ALTER TABLE [tompit].[installer_configuration] ADD CONSTRAINT [PK_installer_configuration] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [tompit].[installer_configuration]'
GO
ALTER TABLE [tompit].[installer_configuration] ADD CONSTRAINT [IX_installer_configuration] UNIQUE NONCLUSTERED  ([package]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[installer_configuration_sel]'
GO
CREATE PROCEDURE [tompit].[installer_configuration_sel]
	@package uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 configuration
	FROM tompit.installer_configuration
	WHERE package = @package;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_ins]'
GO
CREATE PROCEDURE [tompit].[blob_ins]
	@resource_group int,
	@file_name nvarchar(128) = null,
	@token uniqueidentifier,
	@size int,
	@type int,
	@content_type nvarchar(128) = null,
	@primary_key nvarchar(256) = null,
	@service uniqueidentifier = null,
	@draft uniqueidentifier = null,
	@version int,
	@topic nvarchar(128) = null,
	@modified smalldatetime 
AS
BEGIN
	SET NOCOUNT ON;

	insert blob (file_name, token, size, type, content_type, primary_key, service, draft, version, topic, modified, resource_group)
	values (@file_name, @token, @size, @type, @content_type, @primary_key, @service, @draft, @version, @topic, @modified, @resource_group);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[installer_configuration_ins]'
GO
CREATE PROCEDURE [tompit].[installer_configuration_ins]
	@package uniqueidentifier,
	@configuration uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.installer_configuration (package, configuration)
	VALUES (@package, @configuration);
	
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[event]'
GO
CREATE TABLE [tompit].[event]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[created] [datetime2] NOT NULL,
[identifier] [uniqueidentifier] NOT NULL,
[arguments] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[callback] [varchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_event] on [tompit].[event]'
GO
ALTER TABLE [tompit].[event] ADD CONSTRAINT [PK_event] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[event_del]'
GO
CREATE PROCEDURE [tompit].[event_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.event
	WHERE id = @id;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_upd]'
GO
CREATE PROCEDURE [tompit].[blob_upd]
	@file_name nvarchar(128) = null,
	@id bigint,
	@size int,
	@content_type nvarchar(128) = null,
	@primary_key nvarchar(256) = null,
	@draft uniqueidentifier = null,
	@version int,
	@modified smalldatetime 
AS
BEGIN
	SET NOCOUNT ON;

	update blob set
		file_name = @file_name, 
		size = @size, 
		content_type = @content_type, 
		primary_key = @primary_key, 
		draft = @draft, 
		version = @version, 
		modified = @modified
	where id = @id
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_level_que]'
GO
CREATE PROCEDURE [tompit].[blob_level_que]
	@service uniqueidentifier,
	@level int
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from view_blob 
	WHERE (type < @level)
	and (service = @service);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[worker]'
GO
CREATE TABLE [tompit].[worker]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[start_time] [smalldatetime] NULL,
[end_time] [smalldatetime] NULL,
[interval_type] [int] NOT NULL,
[interval_value] [int] NOT NULL,
[start_date] [smalldatetime] NULL,
[end_date] [smalldatetime] NULL,
[limit] [int] NOT NULL,
[status] [int] NOT NULL,
[day_of_month] [int] NOT NULL,
[last_run] [datetime] NULL,
[last_complete] [datetime] NULL,
[next_run] [datetime2] NULL,
[day_mode] [int] NOT NULL,
[month_mode] [int] NOT NULL,
[year_mode] [int] NOT NULL,
[run_count] [bigint] NOT NULL,
[month_number] [int] NOT NULL,
[end_mode] [int] NOT NULL,
[interval_counter] [int] NOT NULL,
[month_part] [int] NOT NULL,
[elapsed] [int] NOT NULL,
[weekdays] [int] NOT NULL,
[fail_count] [int] NOT NULL,
[worker] [uniqueidentifier] NOT NULL,
[logging] [bit] NOT NULL,
[kind] [int] NOT NULL,
[state] [uniqueidentifier] NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_worker] on [tompit].[worker]'
GO
ALTER TABLE [tompit].[worker] ADD CONSTRAINT [PK_worker] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[worker_upd_stats]'
GO
CREATE PROCEDURE [tompit].[worker_upd_stats]
	@id bigint,
	@last_run datetime,
	@run_count int
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.worker SET
		last_run = @last_run,
		run_count = @run_count
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[auth_token]'
GO
CREATE TABLE [tompit].[auth_token]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[key] [nvarchar] (128) COLLATE Slovenian_CI_AS NOT NULL,
[claims] [int] NOT NULL,
[status] [int] NOT NULL,
[valid_from] [smalldatetime] NULL,
[valid_to] [smalldatetime] NULL,
[start_time] [time] NULL,
[end_time] [time] NULL,
[ip_restrictions] [varchar] (2048) COLLATE Slovenian_CI_AS NULL,
[resource_group] [int] NOT NULL,
[user] [int] NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[name] [nvarchar] (128) COLLATE Slovenian_CI_AS NULL,
[description] [nvarchar] (1024) COLLATE Slovenian_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_auth_token] on [tompit].[auth_token]'
GO
ALTER TABLE [tompit].[auth_token] ADD CONSTRAINT [PK_auth_token] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_auth_token]'
GO

CREATE VIEW [tompit].[view_auth_token]
AS
SELECT        tompit.auth_token.*, tompit.[user].token AS user_token, tompit.resource_group.token AS resource_group_token
FROM            tompit.auth_token INNER JOIN
                         tompit.[user] ON tompit.auth_token.[user] = tompit.[user].id INNER JOIN
                         tompit.resource_group ON tompit.auth_token.resource_group = tompit.resource_group.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[install_audit]'
GO
CREATE TABLE [tompit].[install_audit]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[package] [uniqueidentifier] NOT NULL,
[created] [datetime] NOT NULL,
[message] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[type] [int] NOT NULL,
[version] [varchar] (48) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_install_audit] on [tompit].[install_audit]'
GO
ALTER TABLE [tompit].[install_audit] ADD CONSTRAINT [PK_install_audit] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[install_audit_ins]'
GO
CREATE PROCEDURE [tompit].[install_audit_ins]
	@package uniqueidentifier,
	@created datetime,
	@message nvarchar(MAX) = NULL,
	@type int,
	@version varchar(48) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.install_audit (package, created, message, type, version)
	VALUES (@package, @created, @message, @type, @version);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_message]'
GO








CREATE VIEW [tompit].[view_message]
AS
SELECT      m.id, m.message, m.topic, m.created, m.expire, m.retry_interval, m.token, t.name topic_name
FROM        tompit.message AS m
INNER JOIN	tompit.message_topic t on m.topic = t.id

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[environment_unit_del]'
GO
CREATE PROCEDURE [tompit].[environment_unit_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	delete environment_unit
	where id = @id;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[install_audit_que]'
GO
CREATE PROCEDURE [tompit].[install_audit_que]
	@package uniqueidentifier = NULL,
	@created datetime = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.install_audit
	WHERE (@package IS NULL OR package = @package)
	AND (@created IS NULL OR created >= @created);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[worker_del]'
GO
CREATE PROCEDURE [tompit].[worker_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.worker
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[auth_token_que]'
GO
CREATE PROCEDURE [tompit].[auth_token_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.view_auth_token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[event_que]'
GO
CREATE PROCEDURE [tompit].[event_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	SELECT id, identifier, name, created, callback
	FROM tompit.event
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[auth_token_sel]'
GO
CREATE PROCEDURE [tompit].[auth_token_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.view_auth_token
	WHERE token = @token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[worker_ins]'
GO
CREATE PROCEDURE [tompit].[worker_ins]
	@start_time smalldatetime  = NULL,
	@end_time smalldatetime = NULL,
	@interval_type int,
	@interval_value int,
	@start_date smalldatetime = NULL,
	@end_date smalldatetime = NULL,
	@limit int,
	@status int,
	@day_of_month int,
	@last_run datetime = NULL,
	@last_complete datetime = NULL,
	@next_run datetime2(7) = NULL,
	@day_mode int,
	@month_mode int,
	@year_mode int,
	@run_count bigint,
	@month_number int,
	@end_mode int,
	@interval_counter int,
	@month_part int,
	@elapsed int,
	@weekdays int,
	@fail_count int,
	@worker uniqueidentifier,
	@logging bit,
	@kind int
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.worker (start_time, end_time, interval_type, interval_value, start_date, end_date, limit, status, day_of_month,
		last_run, last_complete, next_run, day_mode, month_mode, year_mode, run_count, month_number, end_mode, interval_counter,
		month_part, elapsed, weekdays, fail_count, worker, logging, kind)
	VALUES (@start_time, @end_time, @interval_type, @interval_value, @start_date, @end_date, @limit, @status, @day_of_month,
		@last_run, @last_complete, @next_run, @day_mode, @month_mode, @year_mode, @run_count, @month_number, @end_mode, @interval_counter,
		@month_part, @elapsed, @weekdays, @fail_count, @worker, @logging, @kind);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[auth_token_del]'
GO
CREATE PROCEDURE [tompit].[auth_token_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.auth_token
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[component_lock_upd]'
GO
CREATE PROCEDURE [tompit].[component_lock_upd]
	@id int,
	@lock_status int,
	@lock_user int = NULL,
	@lock_date datetime = NULL,
	@lock_verb int
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.component SET
		lock_status = @lock_status,
		lock_user = @lock_user,
		lock_date = @lock_date,
		lock_verb = @lock_verb
	WHERE id = @id; 
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_que]'
GO
CREATE PROCEDURE [tompit].[message_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.view_message;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[worker_upd]'
GO
CREATE PROCEDURE [tompit].[worker_upd]
	@id int,
	@start_time smalldatetime  = NULL,
	@end_time smalldatetime = NULL,
	@interval_type int,
	@interval_value int,
	@start_date smalldatetime = NULL,
	@end_date smalldatetime = NULL,
	@limit int,
	@status int,
	@day_of_month int,
	@last_run datetime = NULL,
	@last_complete datetime = NULL,
	@next_run datetime2(7) = NULL,
	@day_mode int,
	@month_mode int,
	@year_mode int,
	@run_count bigint,
	@month_number int,
	@end_mode int,
	@interval_counter int,
	@month_part int,
	@elapsed int,
	@weekdays int,
	@fail_count int,
	@logging bit,
	@state uniqueidentifier = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.worker SET
		start_time = @start_time, 
		end_time = @end_time, 
		interval_type = @interval_type, 
		interval_value = @interval_value, 
		start_date = @start_date, 
		end_date = @end_date, 
		limit = @limit, 
		status = @status, 
		day_of_month = @day_of_month,
		last_run = @last_run, 
		last_complete = @last_complete, 
		next_run = @next_run, 
		day_mode = @day_mode, 
		month_mode = @month_mode, 
		year_mode = @year_mode, 
		run_count = @run_count, 
		month_number = @month_number, 
		end_mode = @end_mode, 
		interval_counter = @interval_counter,
		month_part = @month_part, 
		elapsed = @elapsed, 
		weekdays = @weekdays, 
		fail_count = @fail_count, 
		logging = @logging,
		state = @state
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[event_sel]'
GO
CREATE PROCEDURE [tompit].[event_sel]
	@identifier uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.event
	WHERE (identifier = @identifier);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[environment_var]'
GO
CREATE TABLE [tompit].[environment_var]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[value] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_environment_var] on [tompit].[environment_var]'
GO
ALTER TABLE [tompit].[environment_var] ADD CONSTRAINT [PK_environment_var] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[environment_var_mdf]'
GO
CREATE PROCEDURE [tompit].[environment_var_mdf]
	@name nvarchar(128),
	@value nvarchar(max) = null
AS
BEGIN
	SET NOCOUNT ON;

	MERGE environment_var AS d
	USING (SELECT @name, @value) AS s (name, value)
	ON (d.name = s.name)
	WHEN NOT MATCHED THEN
		INSERT (name, value)
		VALUES (@name, @value)
	WHEN MATCHED THEN
		UPDATE SET value = @value;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[version_control_commit]'
GO
CREATE TABLE [tompit].[version_control_commit]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[created] [datetime] NOT NULL,
[user] [int] NOT NULL,
[comment] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[service] [uniqueidentifier] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_version_control_commit] on [tompit].[version_control_commit]'
GO
ALTER TABLE [tompit].[version_control_commit] ADD CONSTRAINT [PK_version_control_commit] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[version_control_commit_del]'
GO
CREATE PROCEDURE [tompit].[version_control_commit_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.version_control_commit
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_sel]'
GO
CREATE PROCEDURE [tompit].[message_sel]
	@message uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.view_message
	WHERE token = @message;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[auth_token_ins]'
GO
CREATE PROCEDURE [tompit].[auth_token_ins]
	@token uniqueidentifier,
	@key nvarchar(128),
	@claims int,
	@status int,
	@valid_from smalldatetime = NULL,
	@valid_to smalldatetime = NULL,
	@start_time time(7) = NULL,
	@end_time time(7) = NULL,
	@ip_restrictions varchar(2048) = NULL,
	@resource_group int,
	@user int,
	@name nvarchar(128),
	@description nvarchar(1024) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.auth_token ([key], claims, status, valid_from, valid_to, start_time, end_time, ip_restrictions, resource_group, [user], token, [name], description)
	VALUES (@key, @claims, @status, @valid_from, @valid_to, @start_time, @end_time, @ip_restrictions, @resource_group, @user, @token, @name, @description);

	return scope_identity();
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[event_ins]'
GO
CREATE PROCEDURE [tompit].[event_ins]
	@name nvarchar(256),
	@arguments nvarchar(MAX) = NULL,
	@identifier uniqueidentifier,
	@created datetime2(7),
	@callback varchar(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.event (name, created, identifier, arguments, callback)
	VALUES (@name, @created, @identifier, @arguments, @callback);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[auth_token_upd]'
GO
CREATE PROCEDURE [tompit].[auth_token_upd]
	@id int,
	@key nvarchar(128),
	@claims int,
	@status int,
	@valid_from smalldatetime = NULL,
	@valid_to smalldatetime = NULL,
	@start_time time(7) = NULL,
	@end_time time(7) = NULL,
	@ip_restrictions varchar(2048) = NULL,
	@user int,
	@name nvarchar(128),
	@description nvarchar(1024) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.auth_token SET
		[key] = @key, 
		claims = @claims, 
		status = @status, 
		valid_from = @valid_from, 
		valid_to = @valid_to, 
		start_time = @start_time, 
		end_time = @end_time, 
		ip_restrictions = @ip_restrictions, 
		[user] = @user,
		name = @name,
		description = @description
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_recipient_sel]'
GO
CREATE PROCEDURE [tompit].[message_recipient_sel]
	@message bigint,
	@subscriber bigint
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.view_message_recipient
	WHERE message = @message
	AND subscriber = @subscriber;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[setting_sel]'
GO
CREATE PROCEDURE [tompit].[setting_sel]
	@resource_group int = NULL,
	@name nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 *
	from view_setting
	where (@resource_group IS NULL OR resource_group = @resource_group)
	and (name = @name);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_recipient_del]'
GO
CREATE PROCEDURE [tompit].[message_recipient_del]
	@message bigint,
	@subscriber bigint = null
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM tompit.message_recipient 
	WHERE message = @message
	AND (@subscriber IS NULL OR subscriber = @subscriber);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[instance_endpoint_del]'
GO
CREATE PROCEDURE [tompit].[instance_endpoint_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	delete instance_endpoint
	where id = @id;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[component_history]'
GO
CREATE TABLE [tompit].[component_history]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[created] [datetime] NOT NULL,
[configuration] [uniqueidentifier] NOT NULL,
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[user] [int] NOT NULL,
[commit] [int] NULL,
[component] [uniqueidentifier] NOT NULL,
[verb] [int] NOT NULL CONSTRAINT [DF_component_history_verb] DEFAULT ((0))
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_component_history] on [tompit].[component_history]'
GO
ALTER TABLE [tompit].[component_history] ADD CONSTRAINT [PK_component_history] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [tompit].[component_history]'
GO
ALTER TABLE [tompit].[component_history] ADD CONSTRAINT [IX_component_history] UNIQUE NONCLUSTERED  ([configuration]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[version_control_commit_ins]'
GO
CREATE PROCEDURE [tompit].[version_control_commit_ins]
	@token uniqueidentifier,
	@service uniqueidentifier,
	@user int,
	@created datetime,
	@comment nvarchar(MAX),
	@components nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	declare @id int;

	INSERT tompit.version_control_commit (created, [user], service, comment, token)
	VALUES (@created, @user, @service, @comment, @token);

	set @id = scope_identity();

	UPDATE tompit.component SET
		lock_status = 0,
		lock_date = NULL,
		lock_user = NULL,
		lock_verb = 0
	WHERE token IN (SELECT  token FROM OPENJSON(@components) WITH (token uniqueidentifier));

	UPDATE tompit.component_history SET
		[commit] = @id
	WHERE component IN (SELECT token FROM OPENJSON(@components) WITH (token uniqueidentifier));
		
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[worker_sel]'
GO
CREATE PROCEDURE [tompit].[worker_sel]
	@worker uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.worker 
	WHERE worker = @worker;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[resource_group_del]'
GO
CREATE PROCEDURE [tompit].[resource_group_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	delete resource_group
	where id = @id;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction]'
GO
CREATE TABLE [tompit].[big_data_transaction]
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
PRINT N'Creating [tompit].[component_commit_que]'
GO
CREATE PROCEDURE [tompit].[component_commit_que]
	@commit int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT c.*
	FROM tompit.view_component c
	INNER JOIN tompit.component_history h
	ON c.token = h.component
	WHERE h.[commit] = @commit;
		
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[user_upd_login_info]'
GO
CREATE PROCEDURE [tompit].[user_upd_login_info]
	@id int,
	@auth_token uniqueidentifier,
	@last_login smalldatetime
AS
BEGIN
	SET NOCOUNT ON;

	update [user] set
		auth_token = @auth_token,
		last_login=@last_login
	where id = @id;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[environment_unit_ins]'
GO
CREATE PROCEDURE [tompit].[environment_unit_ins]
	@token uniqueidentifier,
	@name nvarchar(128),
	@parent int = null,
	@ordinal int
AS
BEGIN
	SET NOCOUNT ON;

	insert environment_unit (name, parent, token, ordinal)
	values (@name, @parent, @token, @ordinal);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[version_control_commit_que]'
GO
CREATE PROCEDURE [tompit].[version_control_commit_que]
	@service uniqueidentifier,
	@component uniqueidentifier = NULL,
	@user int = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF (@component IS NULL)
		SELECT *
		FROM tompit.version_control_commit
		WHERE service = @service
		AND (@user IS NULL OR user = @user);
	ELSE
		SELECT c.*
		FROM  tompit.component_history h
		INNER JOIN tompit.version_control_commit c ON h.[commit] = c.id
		INNER JOIN tompit.component cmp ON h.component = cmp.token
		WHERE (cmp.token = @component)
		AND (@user IS NULL OR c.[user] = @user);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_permission]'
GO
CREATE VIEW [tompit].[view_permission]
AS
SELECT        tompit.permission.*, tompit.resource_group.token AS resource_group_token
FROM            tompit.permission INNER JOIN
                         tompit.resource_group ON tompit.permission.resource_group = tompit.resource_group.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[version_control_commit_sel]'
GO
CREATE PROCEDURE [tompit].[version_control_commit_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.version_control_commit
	WHERE token = @token;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_block]'
GO
CREATE TABLE [tompit].[big_data_transaction_block]
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
PRINT N'Creating [tompit].[component_history_ins]'
GO
CREATE PROCEDURE [tompit].[component_history_ins]
	@created datetime,
	@configuration uniqueidentifier,
	@name nvarchar(128),
	@user int,
	@component uniqueidentifier,
	@verb int
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.component_history (created, configuration, name, [user], [commit], component, verb)
	VALUES (@created, @configuration, @name, @user, NULL, @component, @verb);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[role]'
GO
CREATE TABLE [tompit].[role]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (128) COLLATE Slovenian_CI_AS NOT NULL,
[token] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_role] on [tompit].[role]'
GO
ALTER TABLE [tompit].[role] ADD CONSTRAINT [PK_role] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[role_del]'
GO
CREATE PROCEDURE [tompit].[role_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.role
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
PRINT N'Creating [tompit].[component_history_undo]'
GO
CREATE PROCEDURE [tompit].[component_history_undo]
	@component uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.component_history 
	WHERE component = @component
	AND [commit] IS NULL;
END
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
PRINT N'Creating [tompit].[component_history_view]'
GO



CREATE VIEW [tompit].[component_history_view]
AS

SELECT h.id, h.created, h.configuration, h.name, h.[user], h.[commit], h.component, h.verb, 
		c.token commit_token,
		u.token user_token
FROM tompit.component_history h
INNER JOIN tompit.[user] u ON h.[user] = u.id
LEFT JOIN tompit.version_control_commit c ON h.[commit] = c.id;
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
PRINT N'Creating [tompit].[component_history_que]'
GO
CREATE PROCEDURE [tompit].[component_history_que]
	@component uniqueidentifier = NULL,
	@commit int = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.component_history_view 
	WHERE (@component IS NULL OR component = @component)
	AND (@commit IS NULL OR [commit] = @commit);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[audit]'
GO
CREATE TABLE [tompit].[audit]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[created] [datetime] NOT NULL,
[user] [uniqueidentifier] NULL,
[identifier] [uniqueidentifier] NOT NULL,
[category] [nvarchar] (64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[event] [nvarchar] (64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[primary_key] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[property] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[value] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ip] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[description] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_audit] on [tompit].[audit]'
GO
ALTER TABLE [tompit].[audit] ADD CONSTRAINT [PK_audit] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_audit] on [tompit].[audit]'
GO
CREATE NONCLUSTERED INDEX [IX_audit] ON [tompit].[audit] ([category]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_audit_1] on [tompit].[audit]'
GO
CREATE NONCLUSTERED INDEX [IX_audit_1] ON [tompit].[audit] ([event]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_audit_2] on [tompit].[audit]'
GO
CREATE NONCLUSTERED INDEX [IX_audit_2] ON [tompit].[audit] ([primary_key]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[audit_ins]'
GO
CREATE PROCEDURE [tompit].[audit_ins]
	@user uniqueidentifier = null,
	@created datetime,
	@primary_key nvarchar(128),
	@category nvarchar(64),
	@event nvarchar(64),
	@description nvarchar(1024) = NULL,
	@ip nvarchar(128) = NULL,
	@property nvarchar(128),
	@value nvarchar(1024) = NULL,
	@identifier uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.[audit] ([user], created, primary_key, category, event, description, ip, property, value, identifier)
	VALUES (@user, @created, @primary_key, @category, @event, @description, @ip, @property, @value, @identifier);
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
PRINT N'Creating [tompit].[role_ins]'
GO
CREATE PROCEDURE [tompit].[role_ins]
	@token uniqueidentifier,
	@name nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.role (token, name)
	VALUES (@token, @name);

	RETURN scope_identity();
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
PRINT N'Creating [tompit].[message_recipient_clr]'
GO
CREATE PROCEDURE [tompit].[message_recipient_clr]
	@topic bigint, 
	@subscriber bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE r 
	FROM tompit.message_recipient r
	INNER JOIN tompit.message m ON r.message = m.id
	WHERE m.topic = @topic
	AND subscriber = @subscriber;
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
PRINT N'Creating [tompit].[audit_que]'
GO
CREATE PROCEDURE [tompit].[audit_que]
	@category nvarchar(64),
	@event nvarchar(64) = NULL,
	@primary_key nvarchar(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.[audit]
	WHERE (category = @category)
	AND (@event IS NULL OR [event] = @event)
	AND (@primary_key IS NULL OR primary_key = @primary_key);
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
PRINT N'Creating [tompit].[user_data]'
GO
CREATE TABLE [tompit].[user_data]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[topic] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[user] [int] NOT NULL,
[primary_key] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[value] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_user_data] on [tompit].[user_data]'
GO
ALTER TABLE [tompit].[user_data] ADD CONSTRAINT [PK_user_data] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [tompit].[user_data]'
GO
ALTER TABLE [tompit].[user_data] ADD CONSTRAINT [IX_user_data] UNIQUE NONCLUSTERED  ([user], [primary_key], [topic]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[user_data_mdf]'
GO
CREATE PROCEDURE [tompit].[user_data_mdf]
	@items nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	MERGE tompit.user_data AS d
	USING (SELECT topic, [user], primary_key, value FROM OPENJSON(@items) WITH (topic nvarchar(128), [user] int, primary_key nvarchar(256), value nvarchar(MAX))) 
	AS s (topic, [user], primary_key, value)
	ON ((s.topic IS NULL OR d.topic = s.topic) AND d.[user] = s.[user] AND d.primary_key = s.primary_key)
	WHEN NOT MATCHED THEN
		INSERT (topic, [user], primary_key, value)
		VALUES (s.topic, s.[user], s.primary_key, s.value)
	WHEN MATCHED THEN
		UPDATE SET
			value = s.value;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[message_recipient_upd]'
GO

CREATE PROCEDURE [tompit].[message_recipient_upd]
	@items [tompit].[message_recipient_list] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	MERGE INTO tompit.message_recipient r
	USING @items s
	ON r.id = s.id
	WHEN MATCHED THEN
	UPDATE SET
		retry_count = s.retry_count,
		next_visible = s.next_visible;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_membership]'
GO
CREATE VIEW [tompit].[view_membership]
AS
SELECT        m.id, m.[user], m.role, u.token AS user_token
FROM            tompit.membership AS m INNER JOIN
                         tompit.[user] AS u ON m.[user] = u.id
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
PRINT N'Creating [tompit].[user_data_que]'
GO
CREATE PROCEDURE [tompit].[user_data_que]
	@user int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.user_data
	WHERE [user] = @user
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[permission_del]'
GO
CREATE PROCEDURE [tompit].[permission_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	delete permission
	where id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_environment_unit]'
GO



CREATE VIEW [tompit].[view_environment_unit]
AS
SELECT        u.*, p.token AS parent_token
FROM            tompit.environment_unit AS u LEFT OUTER JOIN
                         tompit.environment_unit AS p ON u.parent = p.id

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
PRINT N'Creating [tompit].[message_del]'
GO
CREATE PROCEDURE [tompit].[message_del]
	@message bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.message
	WHERE id = @message;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[environment_var_sel]'
GO
CREATE PROCEDURE [tompit].[environment_var_sel]
	@name nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 *
	from environment_var 
	where name = @name;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[role_que]'
GO
CREATE PROCEDURE [tompit].[role_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.role;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[log_ins]'
GO
CREATE PROCEDURE [tompit].[log_ins]
	@items nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.log (created, message, trace_level, source, category, event_id, metric, component, element, date)
	SELECT created, message, trace_level, source, category, event_id, metric, component, element, date FROM OPENJSON (@items) WITH (created datetime2(7), message nvarchar(max), trace_level int, source nvarchar(1024), category nvarchar(128),
		event_id int, metric uniqueidentifier, component uniqueidentifier, element uniqueidentifier, date date); 
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[membership_que]'
GO
CREATE PROCEDURE [tompit].[membership_que]
	@user int = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.view_membership
	WHERE (@user IS NULL OR [user] = @user);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[setting_del]'
GO
CREATE PROCEDURE [tompit].[setting_del]
	@resource_group int = NULL,
	@name nvarchar(128) = null
AS
BEGIN
	SET NOCOUNT ON;

	delete setting
	where (@resource_group IS NULL OR resource_group = @resource_group)
	and (name = @name);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[environment_unit_sel]'
GO
CREATE PROCEDURE [tompit].[environment_unit_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 *
	from view_environment_unit
	where token = @token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[log_que]'
GO
CREATE PROCEDURE [tompit].[log_que]
	@date date = NULL,
	@component uniqueidentifier = NULL,
	@element uniqueidentifier = NULL,
	@metric uniqueidentifier = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF (@metric IS NOT NULL)
	BEGIN
		WITH
		  cteMetric (session, parent, level)
		  AS
		  (
			SELECT session, parent, 1
			FROM tompit.metric
			WHERE session = @metric
			UNION ALL
			SELECT p.session, p.parent, m.level + 1
			FROM tompit.metric p
			  INNER JOIN cteMetric m
				ON p.parent = m.session
		  )
		SELECT * 
		FROM tompit.log
		WHERE (metric IN (SELECT session FROM cteMetric))
	END
	ELSE
	BEGIN
		SELECT * 
		FROM tompit.log
		WHERE (@date IS NULL OR [date] = @date)
		AND (@component IS NULL OR component = @component)
		AND (@element IS NULL OR element = @element);
	END
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[environment_var_que]'
GO
CREATE PROCEDURE [tompit].[environment_var_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from environment_var 
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[role_sel]'
GO
CREATE PROCEDURE [tompit].[role_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.role
	WHERE token = @token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[worker_que]'
GO
CREATE PROCEDURE [tompit].[worker_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.worker ;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[membership_sel]'
GO
CREATE PROCEDURE [tompit].[membership_sel]
	@user int,
	@role uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.view_membership
	WHERE ([user] = @user)
	AND (role = @role);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[role_upd]'
GO
CREATE PROCEDURE [tompit].[role_upd]
	@id int,
	@name nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.role SET
		name = @name
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[instance_endpoint_ins]'
GO
CREATE PROCEDURE [tompit].[instance_endpoint_ins]
	@url nvarchar(1024)=NULL,
	@status int,
	@name nvarchar(128),
	@type int,
	@token uniqueidentifier,
	@verbs int,
	@reverse_proxy_url nvarchar(1024) = null
AS
BEGIN
	SET NOCOUNT ON;

	insert instance_endpoint (url, status, name, type, token, verbs, reverse_proxy_url)
	values (@url, @status, @name, @type, @token, @verbs, @reverse_proxy_url);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[permission_ins]'
GO
CREATE PROCEDURE [tompit].[permission_ins]
	@evidence uniqueidentifier,
	@schema nvarchar(128),
	@claim nvarchar(128),
	@descriptor nvarchar(128),
	@primary_key nvarchar(128),
	@value int,
	@resource_group int = NULL,
	@component nvarchar(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	insert permission (evidence, [schema], claim, descriptor, primary_key, value, resource_group, component)
	values (@evidence, @schema, @claim, @descriptor, @primary_key, @value, @resource_group, @component);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[environment_var_del]'
GO
CREATE PROCEDURE [tompit].[environment_var_del]
	@name nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	delete environment_var 
	where name = @name;
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
PRINT N'Creating [tompit].[user_sel_password]'
GO
CREATE PROCEDURE [tompit].[user_sel_password]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [password]
	FROM tompit.[user]
	WHERE id = @id;
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
PRINT N'Creating [tompit].[setting_mdf]'
GO
CREATE PROCEDURE [tompit].[setting_mdf]
	@resource_group int = NULL,
	@name nvarchar(128),
	@visible bit,
	@data_type int,
	@tags nvarchar(256) = null,
	@value nvarchar(1024) = null
AS
BEGIN
	SET NOCOUNT ON;

	MERGE setting AS d
	USING (SELECT @name, @resource_group) AS s (name, resource_group)
	ON (d.name = s.name) and (s.resource_group IS NULL OR d.resource_group = s.resource_group)
	WHEN NOT MATCHED THEN
		INSERT (resource_group, name, visible, data_type, tags, value)
		VALUES (@resource_group, @name, @visible, @data_type, @tags, @value)
	WHEN MATCHED THEN
		UPDATE SET visible = @visible, data_type = @data_type, tags = @tags, value = @value;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[membership_ins]'
GO
CREATE PROCEDURE [tompit].[membership_ins]
	@user int,
	@role uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.membership ([user], role)
	VALUES (@user, @role);
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
PRINT N'Creating [tompit].[user_upd_password]'
GO
CREATE PROCEDURE [tompit].[user_upd_password]
	@id int,
	@password nvarchar(1024) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.[user] SET
		password = @password
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[resource_group_ins]'
GO
CREATE PROCEDURE [tompit].[resource_group_ins]
	@token uniqueidentifier,
	@name nvarchar(128),
	@storage_provider uniqueidentifier,
	@connection_string nvarchar(1024) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	insert resource_group (token, name, storage_provider, connection_string)
	values (@token, @name, @storage_provider, @connection_string);
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
PRINT N'Creating [tompit].[permission_que]'
GO
CREATE PROCEDURE [tompit].[permission_que]
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from view_permission;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[component_history_del]'
GO
CREATE PROCEDURE [tompit].[component_history_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.component_history
	WHERE id = @id;
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
PRINT N'Creating [tompit].[permission_sel]'
GO
CREATE PROCEDURE [tompit].[permission_sel]
	@evidence uniqueidentifier = null,
	@schema nvarchar(128) = null,
	@claim nvarchar(128) = null,
	@primary_key nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 *
	from view_permission
	where (@evidence is null or evidence = @evidence)
	and (@schema is null or [schema] = @schema)
	and (@claim is null or claim = @claim)
	and primary_key = @primary_key;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[environment_unit_que]'
GO
CREATE PROCEDURE [tompit].[environment_unit_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	select *
	from view_environment_unit ;
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
PRINT N'Creating [tompit].[big_data_index]'
GO
CREATE TABLE [tompit].[big_data_index]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[start_timestamp] [datetime2] NULL,
[end_timestamp] [datetime2] NULL,
[count] [int] NOT NULL,
[status] [int] NOT NULL,
[node] [int] NOT NULL,
[file] [uniqueidentifier] NOT NULL,
[partition] [int] NOT NULL,
[key] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_index] on [tompit].[big_data_index]'
GO
ALTER TABLE [tompit].[big_data_index] ADD CONSTRAINT [PK_big_data_index] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_index_field]'
GO
CREATE TABLE [tompit].[big_data_index_field]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[index] [bigint] NOT NULL,
[start_string] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[end_string] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[start_number] [float] NULL,
[end_number] [float] NULL,
[field_name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_index_field] on [tompit].[big_data_index_field]'
GO
ALTER TABLE [tompit].[big_data_index_field] ADD CONSTRAINT [PK_big_data_index_field] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_defer]'
GO
CREATE TABLE [tompit].[big_data_transaction_defer]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[partition] [int] NOT NULL,
[file] [uniqueidentifier] NOT NULL,
[next_visible] [datetime2] NOT NULL,
[worker] [bigint] NOT NULL,
[pop_receipt] [uniqueidentifier] NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_transaction_defer] on [tompit].[big_data_transaction_defer]'
GO
ALTER TABLE [tompit].[big_data_transaction_defer] ADD CONSTRAINT [PK_big_data_transaction_defer] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[metric_agg_day]'
GO
CREATE TABLE [tompit].[metric_agg_day]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[component] [uniqueidentifier] NOT NULL,
[element] [uniqueidentifier] NULL,
[date] [date] NOT NULL,
[count] [int] NOT NULL,
[success] [int] NOT NULL,
[duration] [bigint] NOT NULL,
[max] [int] NOT NULL,
[min] [int] NOT NULL,
[consumption_in] [bigint] NOT NULL,
[consumption_out] [bigint] NOT NULL,
[max_consumption_in] [bigint] NOT NULL,
[min_consumption_out] [bigint] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_metric_agg_day] on [tompit].[metric_agg_day]'
GO
ALTER TABLE [tompit].[metric_agg_day] ADD CONSTRAINT [PK_metric_agg_day] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[alien]'
GO
ALTER TABLE [tompit].[alien] ADD CONSTRAINT [FK_alien_language] FOREIGN KEY ([language]) REFERENCES [tompit].[language] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[auth_token]'
GO
ALTER TABLE [tompit].[auth_token] ADD CONSTRAINT [FK_auth_token_resource_group] FOREIGN KEY ([resource_group]) REFERENCES [tompit].[resource_group] ([id]) ON DELETE CASCADE
GO
ALTER TABLE [tompit].[auth_token] ADD CONSTRAINT [FK_auth_token_user] FOREIGN KEY ([user]) REFERENCES [tompit].[user] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[big_data_index_field]'
GO
ALTER TABLE [tompit].[big_data_index_field] ADD CONSTRAINT [FK_big_data_index_field_big_data_index] FOREIGN KEY ([index]) REFERENCES [tompit].[big_data_index] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[big_data_index]'
GO
ALTER TABLE [tompit].[big_data_index] ADD CONSTRAINT [FK_big_data_index_big_data_node] FOREIGN KEY ([node]) REFERENCES [tompit].[big_data_node] ([id]) ON DELETE CASCADE
GO
ALTER TABLE [tompit].[big_data_index] ADD CONSTRAINT [FK_big_data_index_big_data_partition] FOREIGN KEY ([partition]) REFERENCES [tompit].[big_data_partition] ([id]) ON DELETE CASCADE
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
PRINT N'Adding foreign keys to [tompit].[big_data_partition]'
GO
ALTER TABLE [tompit].[big_data_partition] ADD CONSTRAINT [FK_big_data_partition_resource_group] FOREIGN KEY ([resource_group]) REFERENCES [tompit].[resource_group] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[big_data_transaction_block]'
GO
ALTER TABLE [tompit].[big_data_transaction_block] ADD CONSTRAINT [FK_big_data_transaction_block_big_data_transaction] FOREIGN KEY ([transaction]) REFERENCES [tompit].[big_data_transaction] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[blob]'
GO
ALTER TABLE [tompit].[blob] ADD CONSTRAINT [FK_blob_resource_group] FOREIGN KEY ([resource_group]) REFERENCES [tompit].[resource_group] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[dev_error]'
GO
ALTER TABLE [tompit].[dev_error] ADD CONSTRAINT [FK_dev_error_component] FOREIGN KEY ([component]) REFERENCES [tompit].[component] ([id]) ON DELETE CASCADE
GO
ALTER TABLE [tompit].[dev_error] ADD CONSTRAINT [FK_dev_error_service] FOREIGN KEY ([service]) REFERENCES [tompit].[service] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[component]'
GO
ALTER TABLE [tompit].[component] ADD CONSTRAINT [FK_component_folder] FOREIGN KEY ([folder]) REFERENCES [tompit].[folder] ([id])
GO
ALTER TABLE [tompit].[component] ADD CONSTRAINT [FK_component_service] FOREIGN KEY ([service]) REFERENCES [tompit].[service] ([id])
GO
ALTER TABLE [tompit].[component] ADD CONSTRAINT [FK_component_user] FOREIGN KEY ([lock_user]) REFERENCES [tompit].[user] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[component_history]'
GO
ALTER TABLE [tompit].[component_history] ADD CONSTRAINT [FK_component_history_user] FOREIGN KEY ([user]) REFERENCES [tompit].[user] ([id])
GO
ALTER TABLE [tompit].[component_history] ADD CONSTRAINT [FK_component_history_version_control_commit] FOREIGN KEY ([commit]) REFERENCES [tompit].[version_control_commit] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[environment_unit]'
GO
ALTER TABLE [tompit].[environment_unit] ADD CONSTRAINT [FK_environment_unit_environment_unit] FOREIGN KEY ([parent]) REFERENCES [tompit].[environment_unit] ([id])
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
PRINT N'Adding foreign keys to [tompit].[service_string]'
GO
ALTER TABLE [tompit].[service_string] ADD CONSTRAINT [FK_service_string_language] FOREIGN KEY ([language]) REFERENCES [tompit].[language] ([id]) ON DELETE CASCADE
GO
ALTER TABLE [tompit].[service_string] ADD CONSTRAINT [FK_service_string_solution] FOREIGN KEY ([service]) REFERENCES [tompit].[service] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[user]'
GO
ALTER TABLE [tompit].[user] ADD CONSTRAINT [FK_user_language] FOREIGN KEY ([language]) REFERENCES [tompit].[language] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[membership]'
GO
ALTER TABLE [tompit].[membership] ADD CONSTRAINT [FK_membership_user] FOREIGN KEY ([user]) REFERENCES [tompit].[user] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[message_recipient]'
GO
ALTER TABLE [tompit].[message_recipient] ADD CONSTRAINT [FK_message_recipient_message] FOREIGN KEY ([message]) REFERENCES [tompit].[message] ([id])
GO
ALTER TABLE [tompit].[message_recipient] ADD CONSTRAINT [FK_message_recipient_message_subscriber] FOREIGN KEY ([subscriber]) REFERENCES [tompit].[message_subscriber] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[message]'
GO
ALTER TABLE [tompit].[message] ADD CONSTRAINT [FK_message_message_topic] FOREIGN KEY ([topic]) REFERENCES [tompit].[message_topic] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[message_subscriber]'
GO
ALTER TABLE [tompit].[message_subscriber] ADD CONSTRAINT [FK_message_subscriber_message_topic] FOREIGN KEY ([topic]) REFERENCES [tompit].[message_topic] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[message_topic]'
GO
ALTER TABLE [tompit].[message_topic] ADD CONSTRAINT [FK_message_topic_resource_group] FOREIGN KEY ([resource_group]) REFERENCES [tompit].[resource_group] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[permission]'
GO
ALTER TABLE [tompit].[permission] ADD CONSTRAINT [FK_permission_resource_group] FOREIGN KEY ([resource_group]) REFERENCES [tompit].[resource_group] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[setting]'
GO
ALTER TABLE [tompit].[setting] ADD CONSTRAINT [FK_setting_resource_group] FOREIGN KEY ([resource_group]) REFERENCES [tompit].[resource_group] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[service]'
GO
ALTER TABLE [tompit].[service] ADD CONSTRAINT [FK_solution_resource_group] FOREIGN KEY ([resource_group]) REFERENCES [tompit].[resource_group] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[test_suite]'
GO
ALTER TABLE [tompit].[test_suite] ADD CONSTRAINT [FK_test_suite_service] FOREIGN KEY ([service]) REFERENCES [tompit].[service] ([id]) ON DELETE CASCADE
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
PRINT N'Adding foreign keys to [tompit].[user_data]'
GO
ALTER TABLE [tompit].[user_data] ADD CONSTRAINT [FK_user_data_user] FOREIGN KEY ([user]) REFERENCES [tompit].[user] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[user_state]'
GO
ALTER TABLE [tompit].[user_state] ADD CONSTRAINT [FK_user_state_user] FOREIGN KEY ([user_id]) REFERENCES [tompit].[user] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating extended properties'
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_DiagramPane1', N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "auth_token (tompit)"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 323
               Right = 209
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "user (tompit)"
            Begin Extent = 
               Top = 182
               Left = 321
               Bottom = 312
               Right = 518
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "resource_group (tompit)"
            Begin Extent = 
               Top = 6
               Left = 482
               Bottom = 136
               Right = 666
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', 'SCHEMA', N'tompit', 'VIEW', N'view_auth_token', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	DECLARE @xp int
SELECT @xp=1
EXEC sp_addextendedproperty N'MS_DiagramPaneCount', @xp, 'SCHEMA', N'tompit', 'VIEW', N'view_auth_token', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_DiagramPane1', N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "u"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 235
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "m"
            Begin Extent = 
               Top = 6
               Left = 273
               Bottom = 119
               Right = 443
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', 'SCHEMA', N'tompit', 'VIEW', N'view_membership', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	DECLARE @xp int
SELECT @xp=1
EXEC sp_addextendedproperty N'MS_DiagramPaneCount', @xp, 'SCHEMA', N'tompit', 'VIEW', N'view_membership', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_DiagramPane1', N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "resource_group (tompit)"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 224
               Right = 222
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "message_topic (tompit)"
            Begin Extent = 
               Top = 6
               Left = 260
               Bottom = 119
               Right = 431
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', 'SCHEMA', N'tompit', 'VIEW', N'view_message_topic', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	DECLARE @xp int
SELECT @xp=1
EXEC sp_addextendedproperty N'MS_DiagramPaneCount', @xp, 'SCHEMA', N'tompit', 'VIEW', N'view_message_topic', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_DiagramPane1', N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "permission (tompit)"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 209
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "resource_group (tompit)"
            Begin Extent = 
               Top = 6
               Left = 247
               Bottom = 136
               Right = 431
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', 'SCHEMA', N'tompit', 'VIEW', N'view_permission', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	DECLARE @xp int
SELECT @xp=1
EXEC sp_addextendedproperty N'MS_DiagramPaneCount', @xp, 'SCHEMA', N'tompit', 'VIEW', N'view_permission', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_DiagramPane1', N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "language (tompit)"
            Begin Extent = 
               Top = 6
               Left = 246
               Bottom = 233
               Right = 416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "solution_string (tompit)"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 230
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', 'SCHEMA', N'tompit', 'VIEW', N'view_service_string', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	DECLARE @xp int
SELECT @xp=1
EXEC sp_addextendedproperty N'MS_DiagramPaneCount', @xp, 'SCHEMA', N'tompit', 'VIEW', N'view_service_string', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_DiagramPane1', N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "s"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 209
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "r"
            Begin Extent = 
               Top = 6
               Left = 247
               Bottom = 136
               Right = 424
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', 'SCHEMA', N'tompit', 'VIEW', N'view_setting', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	DECLARE @xp int
SELECT @xp=1
EXEC sp_addextendedproperty N'MS_DiagramPaneCount', @xp, 'SCHEMA', N'tompit', 'VIEW', N'view_setting', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_DiagramPane1', N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "u"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 235
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "l"
            Begin Extent = 
               Top = 6
               Left = 273
               Bottom = 208
               Right = 443
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', 'SCHEMA', N'tompit', 'VIEW', N'view_user', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	DECLARE @xp int
SELECT @xp=1
EXEC sp_addextendedproperty N'MS_DiagramPaneCount', @xp, 'SCHEMA', N'tompit', 'VIEW', N'view_user', NULL, NULL
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
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
