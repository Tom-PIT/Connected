
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
CREATE SCHEMA [migr]
AUTHORIZATION [dbo]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
CREATE SCHEMA [tompit]
AUTHORIZATION [dbo]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating types'
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
[timezone] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[resource_type] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[resource_primary_key] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
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
[password_change] [smalldatetime] NULL,
[security_code] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
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
PRINT N'Creating [tompit].[big_data_buffer]'
GO
CREATE TABLE [tompit].[big_data_buffer]
(
[id] [int] NOT NULL,
[partition] [uniqueidentifier] NOT NULL,
[next_visible] [datetime2] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_buffer] on [tompit].[big_data_buffer]'
GO
ALTER TABLE [tompit].[big_data_buffer] ADD CONSTRAINT [PK_big_data_buffer] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [tompit].[big_data_buffer]'
GO
ALTER TABLE [tompit].[big_data_buffer] ADD CONSTRAINT [IX_big_data_buffer] UNIQUE NONCLUSTERED  ([partition]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_buffer_data]'
GO
CREATE TABLE [tompit].[big_data_buffer_data]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[buffer] [int] NOT NULL,
[data] [varbinary] (max) NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_buffer_data] on [tompit].[big_data_buffer_data]'
GO
ALTER TABLE [tompit].[big_data_buffer_data] ADD CONSTRAINT [PK_big_data_buffer_data] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node]'
GO
CREATE TABLE [tompit].[big_data_node]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[connection_string] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
[key] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[lock_timeout] [datetime2] NULL,
[unlock_key] [uniqueidentifier] NULL,
[timezone] [int] NULL
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
PRINT N'Creating index [IX_big_data_index] on [tompit].[big_data_index]'
GO
CREATE NONCLUSTERED INDEX [IX_big_data_index] ON [tompit].[big_data_index] ([file]) ON [PRIMARY]
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
PRINT N'Creating [tompit].[big_data_timezone]'
GO
CREATE TABLE [tompit].[big_data_timezone]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[offset] [int] NOT NULL,
[token] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_big_data_timezone] on [tompit].[big_data_timezone]'
GO
ALTER TABLE [tompit].[big_data_timezone] ADD CONSTRAINT [PK_big_data_timezone] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
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
[field_name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[start_date] [datetime2] NULL,
[end_date] [datetime2] NULL
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
[partition] [int] NOT NULL,
[timezone] [int] NULL
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
PRINT N'Creating [tompit].[big_data_transaction_block]'
GO
CREATE TABLE [tompit].[big_data_transaction_block]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[transaction] [bigint] NOT NULL,
[token] [uniqueidentifier] NOT NULL
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
[draft] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
[modified] [datetime] NULL,
[service] [int] NULL,
[namespace] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
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
PRINT N'Creating [tompit].[service]'
GO
CREATE TABLE [tompit].[service]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (128) COLLATE Slovenian_CI_AS NOT NULL,
[url] [nvarchar] (136) COLLATE Slovenian_CI_AS NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[supported_stages] [int] NOT NULL CONSTRAINT [df_service_supported_stages] DEFAULT ((2147483647)),
[resource_group] [int] NOT NULL,
[template] [uniqueidentifier] NOT NULL,
[version] [varchar] (32) COLLATE Slovenian_CI_AS NULL,
[commit] [varchar] (32) COLLATE Slovenian_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_service] on [tompit].[service]'
GO
ALTER TABLE [tompit].[service] ADD CONSTRAINT [PK_service] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
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
PRINT N'Creating [tompit].[mru]'
GO
CREATE TABLE [tompit].[mru]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[type] [int] NOT NULL,
[primary_key] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[entity_type] [int] NOT NULL,
[entity_primary_key] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[date] [datetime2] NOT NULL,
[token] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_mru] on [tompit].[mru]'
GO
ALTER TABLE [tompit].[mru] ADD CONSTRAINT [PK_mru] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
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
PRINT N'Creating [tompit].[permission]'
GO
CREATE TABLE [tompit].[permission]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[evidence] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[schema] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[claim] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[descriptor] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[primary_key] [nvarchar] (4000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
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
PRINT N'Creating [tompit].[subscriber]'
GO
CREATE TABLE [tompit].[subscriber]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[subscription] [bigint] NOT NULL,
[resource_type] [int] NOT NULL,
[resource_primary_key] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[token] [uniqueidentifier] NULL,
[tags] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
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
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_subscription_event] on [tompit].[subscription_event]'
GO
ALTER TABLE [tompit].[subscription_event] ADD CONSTRAINT [PK_subscription_event] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
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
) ON [PRIMARY]
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
PRINT N'Creating [tompit].[user_state]'
GO
CREATE TABLE [tompit].[user_state]
(
[user_id] [int] NOT NULL,
[state] [varbinary] (max) NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_user_state] on [tompit].[user_state]'
GO
ALTER TABLE [tompit].[user_state] ADD CONSTRAINT [PK_user_state] PRIMARY KEY CLUSTERED  ([user_id]) ON [PRIMARY]
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
) ON [PRIMARY]
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
PRINT N'Creating [tompit].[view_component]'
GO








CREATE VIEW [tompit].[view_component]
AS
SELECT      c.id, c.name, c.token, c.type, c.category, c.modified,
			c.service, c.folder, c.namespace,
			f.token as folder_token, s.token AS [service_token]
FROM        tompit.component AS c 
LEFT JOIN	tompit.folder f on c.folder=f.id
INNER JOIN	tompit.service s on c.service = s.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[subscriber_ins]'
GO
CREATE PROCEDURE [tompit].[subscriber_ins]
	@subscription bigint,
	@resource_type int,
	@resource_primary_key nvarchar(128),
	@token uniqueidentifier,
	@tags nvarchar(1024) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.subscriber (subscription, resource_type, resource_primary_key, token, tags)
	VALUES (@subscription, @resource_type, @resource_primary_key, @token, @tags);
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
PRINT N'Creating [tompit].[view_subscriber]'
GO


CREATE VIEW [tompit].[view_subscriber]
AS

SELECT s.id, s.subscription, s.resource_type, s.resource_primary_key, s.token, s.tags,
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
SELECT        s.id, s.name, s.url, s.token, s.supported_stages, s.resource_group, s.template,
				s.version, s.[commit],
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
	@modified datetime,
	@service int,
	@namespace nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	insert tompit.component (folder, name, token, type, category, modified, service, namespace)
	values (@folder, @name, @token, @type, @category, @modified, @service, @namespace);
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
) ON [PRIMARY]
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
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_log] on [tompit].[log]'
GO
ALTER TABLE [tompit].[log] ADD CONSTRAINT [PK_log] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
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
PRINT N'Creating [tompit].[component_upd]'
GO
CREATE PROCEDURE [tompit].[component_upd]
	@id int,
	@name nvarchar(128),
	@modified datetime,
	@folder int = NULL
AS
BEGIN
	SET NOCOUNT ON;

	update tompit.component set
		name = @name,
		modified = @modified,
		folder = @folder
	where id = @id;
END
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
	@timezone nvarchar(256) = NULL,
	@resource_type nvarchar(128) = NULL,
	@resource_primary_key nvarchar(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.alien (first_name, last_name, email, mobile, phone, token, language, timezone, resource_type, resource_primary_key)
	VALUES (@first_name, @last_name, @email, @mobile, @phone, @token, @language, @timezone, @resource_type, @resource_primary_key);
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
	@timezone nvarchar(256) = NULL,
	@resource_type nvarchar(128) = NULL,
	@resource_primary_key nvarchar(128) = NULL
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
		timezone = @timezone,
		resource_type = @resource_type,
		resource_primary_key = @resource_primary_key
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

SELECT a.id, a.token, a.first_name, a.last_name, a.email, a.mobile, a.phone, a.language, a.timezone, a.resource_type, a.resource_primary_key,
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
PRINT N'Creating [tompit].[xml_key]'
GO
CREATE TABLE [tompit].[xml_key]
(
[id] [nvarchar] (64) COLLATE Slovenian_CI_AS NOT NULL,
[friendly_name] [nvarchar] (64) COLLATE Slovenian_CI_AS NOT NULL,
[element] [nvarchar] (max) COLLATE Slovenian_CI_AS NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[xml_key_que]'
GO
CREATE PROCEDURE [tompit].[xml_key_que]
AS
BEGIN
	select * from [tompit].[xml_key]
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
PRINT N'Creating [tompit].[xml_key_sel]'
GO
CREATE PROCEDURE [tompit].[xml_key_sel]
	@id nvarchar(64)
AS
BEGIN
	select TOP 1 * from [tompit].[xml_key] where id = @id
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
PRINT N'Creating [tompit].[xml_key_ups]'
GO
CREATE PROCEDURE [tompit].[xml_key_ups]
	@id nvarchar(64),
	@friendly_name nvarchar(64),
	@element nvarchar(MAX)
AS
BEGIN
	IF NOT EXISTS (SELECT * FROM [tompit].[xml_key] WHERE id = @id)

    INSERT INTO [tompit].[xml_key](id, friendly_name, element)
    VALUES(@id, @friendly_name, @element)
ELSE
    UPDATE [tompit].[xml_key]
    SET friendly_name = @friendly_name, element = @element
    WHERE id = @id
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
PRINT N'Creating [tompit].[search]'
GO
CREATE TABLE [tompit].[search]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[catalog] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[created] [datetime2] NOT NULL,
[identifier] [uniqueidentifier] NOT NULL,
[arguments] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[service] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_search] on [tompit].[search]'
GO
ALTER TABLE [tompit].[search] ADD CONSTRAINT [PK_search] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[search_del]'
GO
CREATE PROCEDURE [tompit].[search_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.search
	WHERE id = @id;
END
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
PRINT N'Creating [tompit].[search_catalog_state]'
GO
CREATE TABLE [tompit].[search_catalog_state]
(
[id] [int] NOT NULL,
[catalog] [uniqueidentifier] NOT NULL,
[status] [int] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_search_catalog_state] on [tompit].[search_catalog_state]'
GO
ALTER TABLE [tompit].[search_catalog_state] ADD CONSTRAINT [PK_search_catalog_state] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [tompit].[search_catalog_state]'
GO
ALTER TABLE [tompit].[search_catalog_state] ADD CONSTRAINT [IX_search_catalog_state] UNIQUE NONCLUSTERED  ([catalog]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[search_catalog_state_del]'
GO
CREATE PROCEDURE [tompit].[search_catalog_state_del]
	@catalog uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.search_catalog_state
	WHERE catalog = @catalog;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[setting]'
GO
CREATE TABLE [tompit].[setting]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[value] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[type] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[primary_key] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[namespace] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
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
PRINT N'Creating [tompit].[setting_upd]'
GO
CREATE PROCEDURE [tompit].[setting_upd]
	@id int,
	@value nvarchar(1024) = null
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.setting SET
		value = @value
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[search_ins]'
GO
CREATE PROCEDURE [tompit].[search_ins]
	@catalog nvarchar(256),
	@created datetime2(7),
	@identifier uniqueidentifier,
	@arguments nvarchar(max) = NULL,
	@service uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.search (catalog, created, identifier, arguments, service)
	VALUES (@catalog, @created, @identifier, @arguments, @service);

	RETURN scope_identity();
	
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[setting_ins]'
GO
CREATE PROCEDURE [tompit].[setting_ins]
	@name nvarchar(128),
	@type nvarchar(128) = NULL,
	@primary_key nvarchar(128) = NULL,
	@value nvarchar(1024) = null,
	@namespace nvarchar(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.setting (name, type, primary_key, value, namespace)
	VALUES (@name, @type, @primary_key, @value, @namespace);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[search_catalog_state_ins]'
GO
CREATE PROCEDURE [tompit].[search_catalog_state_ins]
	@catalog uniqueidentifier,
	@status int
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.search_catalog_state(catalog, status)
	VALUES (@catalog, @status);

	RETURN scope_identity();
	
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[search_que]'
GO
CREATE PROCEDURE [tompit].[search_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.search;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[search_sel]'
GO
CREATE PROCEDURE [tompit].[search_sel]
	@identifier uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.search
	WHERE identifier = @identifier;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[search_catalog_state_sel]'
GO
CREATE PROCEDURE [tompit].[search_catalog_state_sel]
	@catalog uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.search_catalog_state
	WHERE catalog = @catalog;
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
	@supported_stages int,
	@resource_group int,
	@template uniqueidentifier,
	@version varchar(32) = NULL,
	@commit varchar(32) = NULL
	
AS
BEGIN
	SET NOCOUNT ON;

	insert [service] (name, url, token, supported_stages, resource_group, template, version, [commit])
	values (@name, @url, @token, @supported_stages, @resource_group, @template, @version, @commit);

	return scope_identity();
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[search_catalog_state_upd]'
GO
CREATE PROCEDURE [tompit].[search_catalog_state_upd]
	@catalog uniqueidentifier,
	@status int
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.search_catalog_state SET
		status = @status
	WHERE catalog = @catalog;
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
	@password_change smalldatetime = null,
	@security_code nvarchar(1024) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	insert [user] (token, url, email, status, first_name, last_name, description, language, timezone, notification_enabled, login_name,
		pin, mobile, phone, avatar, auth_token, password_change, security_code)
	values (@token, @url, @email, @status, @first_name, @last_name, @description, @language, @timezone, @notification_enabled, @login_name,
		@pin, @mobile, @phone, @avatar, @auth_token, @password_change, @security_code);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_big_data_partition]'
GO
CREATE VIEW [tompit].[view_big_data_partition]
AS
SELECT p.id, p.configuration, p.file_count, p.status, p.name, p.created, p.resource_group,
		g.token resource_group_token
FROM tompit.big_data_partition p
INNER JOIN tompit.resource_group g ON p.resource_group = g.id
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
	@password_change smalldatetime = null,
	@security_code nvarchar(1024) = NULL
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
		password_change = @password_change,
		security_code = @security_code
	where id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_block_view]'
GO


CREATE VIEW [tompit].[big_data_transaction_block_view]
AS
SELECT b.id, b.[transaction], b.token,
		t.token transaction_token, t.status transaction_status,
		p.configuration partition_configuration, p.status partition_status, p.resource_group,
		tz.token timezone_token
FROM tompit.big_data_transaction_block b
INNER JOIN tompit.big_data_transaction t ON b.[transaction] = t.id
INNER JOIN tompit.big_data_partition p ON t.partition = p.id
LEFT JOIN tompit.big_data_timezone tz ON t.timezone = tz.id;
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_block_sel]'
GO
CREATE PROCEDURE [tompit].[big_data_transaction_block_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.big_data_transaction_block_view
	WHERE token = @token;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_block_que]'
GO
CREATE PROCEDURE [tompit].[big_data_transaction_block_que]
	@transaction bigint
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.big_data_transaction_block_view
	WHERE [transaction] = @transaction;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_timezone_del]'
GO
CREATE PROCEDURE [tompit].[big_data_timezone_del]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.big_data_timezone
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_index_ins]'
GO
CREATE PROCEDURE [tompit].[big_data_index_ins]
	@partition int,
	@key nvarchar(128) = NULL,
	@node int,
	@start_timestamp datetime2,
	@status int,
	@file uniqueidentifier,
	@timezone int = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.big_data_index ([partition], [key], [node], start_timestamp, [count], [status], [file], timezone)
	VALUES (@partition, @key, @node, @start_timestamp, 0, @status, @file, @timezone);

	RETURN scope_identity();
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_timezone_ins]'
GO
CREATE PROCEDURE [tompit].[big_data_timezone_ins]
	@token uniqueidentifier,
	@name nvarchar(128),
	@offset int
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.big_data_timezone (token, name, offset)
	VALUES (@token, @name, @offset);

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_big_data_index]'
GO

CREATE VIEW [tompit].[view_big_data_index]
AS
SELECT i.id, i.start_timestamp, i.end_timestamp, i.[count], i.[status], i.[node], i.[file], i.[partition], i.[key],
		n.token node_token, p.configuration partition_token,
		tz.token timezone_token
FROM tompit.big_data_index i
INNER JOIN tompit.big_data_partition p ON i.partition = p.id
INNER JOIN tompit.big_data_node n ON  i.node = n.id
LEFT JOIN tompit.big_data_timezone tz ON i.timezone = tz.id;
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_timezone_upd]'
GO
CREATE PROCEDURE [tompit].[big_data_timezone_upd]
	@id int,
	@name nvarchar(128),
	@offset int
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.big_data_timezone SET
		name = @name,
		offset = @offset
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
) ON [PRIMARY]
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
PRINT N'Creating [tompit].[big_data_index_sel]'
GO
CREATE PROCEDURE [tompit].[big_data_index_sel]
	@file uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.view_big_data_index 
	WHERE [file] = @file;
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
PRINT N'Creating [tompit].[big_data_timezone_sel]'
GO
CREATE PROCEDURE [tompit].[big_data_timezone_sel]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.big_data_timezone 
	WHERE token = @token;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_index_que]'
GO
CREATE PROCEDURE [tompit].[big_data_index_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.view_big_data_index;
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
PRINT N'Creating [tompit].[big_data_timezone_que]'
GO
CREATE PROCEDURE [tompit].[big_data_timezone_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.big_data_timezone;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_index_del]'
GO
CREATE PROCEDURE [tompit].[big_data_index_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.big_data_index
	WHERE id = @id;
END
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
PRINT N'Creating [tompit].[big_data_index_upd]'
GO
CREATE PROCEDURE [tompit].[big_data_index_upd]
	@id bigint,
	@start_timestamp datetime2(7) = NULL,
	@end_timestamp datetime2(7) = NULL,
	@count int,
	@status int

AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.big_data_index SET
		start_timestamp = @start_timestamp,
		end_timestamp = @end_timestamp,
		count = @count,
		status = @status
	WHERE id = @id;
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
PRINT N'Creating [tompit].[big_data_index_field_mdf]'
GO
CREATE PROCEDURE [tompit].[big_data_index_field_mdf]
	@index bigint,
	@start_string nvarchar(256) = NULL,
	@end_string nvarchar(256) = NULL,
	@start_number float = NULL,
	@end_number float = NULL,
	@start_date datetime2(7) = NULL,
	@end_date datetime2(7) = NULL,
	@field_name nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	MERGE tompit.big_data_index_field AS t
	USING (SELECT @index, @field_name, @start_string, @end_string, @start_date, @end_date, @start_number, @end_number) AS s 
	([index], field_name, start_string, end_string, start_date, end_date, start_number, end_number)
	ON (t.[index] = s.[index] AND t.field_name = s.field_name)
	WHEN NOT MATCHED THEN
		INSERT ([index], field_name, start_string, end_string, start_date, end_date, start_number, end_number)
		VALUES (s.[index], s.field_name, s.start_string, s.end_string, s.start_date, s.end_date, s.start_number, s.end_number)
	WHEN matched THEN
		UPDATE SET
			t.start_string = s.start_string,
			t.end_string = s.end_string,
			t.start_date= s.start_date,
			t.end_date = s.end_date,
			t.start_number = s.start_number,
			t.end_number = s.end_number;
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
PRINT N'Creating [tompit].[view_big_data_index_field]'
GO

CREATE VIEW [tompit].[view_big_data_index_field]
AS
SELECT f.id, f.[index], f.field_name, f.start_string, f.end_string, f.start_number, f.end_number, f.start_date, f.end_date,
		i.[file], i.[key]
FROM tompit.big_data_index_field f
INNER JOIN tompit.big_data_index i ON f.[index] = i.id;
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
PRINT N'Creating [tompit].[view_user]'
GO





CREATE VIEW [tompit].[view_user]
AS
SELECT			u.id, u.token, u.auth_token, u.url, u.email, u.status, u.first_name, u.last_name, u.description,
				u.password, u.language, u.last_login, u.timezone, u.notification_enabled, u.login_name, u.pin, 
				u.mobile, u.phone, u.avatar, u.password_change, u.security_code, l.token AS language_token
FROM            tompit.[user] AS u LEFT OUTER JOIN
                         tompit.language AS l ON u.language = l.id

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_index_field_sel]'
GO
CREATE PROCEDURE [tompit].[big_data_index_field_sel]
	@index bigint,
	@field_name nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.view_big_data_index_field
	WHERE [index] = @index
	AND field_name = @field_name;

END
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
PRINT N'Creating [tompit].[big_data_index_lock]'
GO
CREATE PROCEDURE [tompit].[big_data_index_lock]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ct TABLE
	(
		[unlock_key] [uniqueidentifier] NOT NULL
	);

	WITH idx AS
	(
		SELECT	TOP 1 *
		FROM	tompit.big_data_index
		WHERE	id = @id
		AND		(lock_timeout IS NULL OR lock_timeout <= GETUTCDATE())
		AND		status > 1
	)
	UPDATE idx SET
		lock_timeout = DATEADD(SECOND, 60, GETUTCDATE()),
		unlock_key = newid()
	OUTPUT inserted.unlock_key INTO @ct;

	SELECT unlock_key FROM @ct;
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
	@auth_token uniqueidentifier = null,
	@security_code nvarchar(1024) = NULL
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
	AND (@security_code IS NULL OR security_code = @security_code);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_index_unlock]'
GO
CREATE PROCEDURE [tompit].[big_data_index_unlock]
	@unlock_key uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.big_data_index SET
		lock_timeout = NULL,
		unlock_key = NULL
	WHERE unlock_key = @unlock_key;
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
	USING (SELECT resource_type, resource_primary_key, token, tags FROM OPENJSON(@items) WITH (resource_type int, resource_primary_key nvarchar(128), token uniqueidentifier, tags nvarchar(1024))) AS s (resource_type, resource_primary_key, token, tags)
	ON (d.subscription = @subscription AND d.resource_type = s.resource_type AND d.resource_primary_key = s.resource_primary_key AND ((d.tags IS NULL AND s.tags IS NULL) OR  d.tags = s.tags))
	WHEN NOT MATCHED THEN
	INSERT (subscription, resource_type, resource_primary_key, token, tags)
	VALUES (@subscription, s.resource_type, s.resource_primary_key, s.token, s.tags);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_buffer_data_que]'
GO
CREATE PROCEDURE [tompit].[big_data_buffer_data_que]
	@buffer bigint
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.big_data_buffer_data WITH (readpast)
	WHERE buffer = @buffer;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_buffer_data_clr]'
GO
CREATE PROCEDURE [tompit].[big_data_buffer_data_clr]
	@buffer bigint,
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.big_data_buffer_data
	WHERE buffer = @buffer AND id <= @id;
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
PRINT N'Creating [tompit].[big_data_buffer_data_ins]'
GO
CREATE PROCEDURE [tompit].[big_data_buffer_data_ins]
	@buffer bigint,
	@data varbinary(max)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.big_data_buffer_data (buffer, data)
	VALUES (@buffer, @data);
END
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
[modified] [datetime2] NOT NULL,
[device] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
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
	USING (SELECT * FROM OPENJSON (@items) WITH (hub uniqueidentifier, field nvarchar(128), value nvarchar(1024), device nvarchar(128))) AS s (hub, field, value, device)
	ON (d.hub = s.hub AND d.field = s.field AND d.device = s.device)
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (hub, field, modified, device)
		VALUES (hub, field, GETUTCDATE(), device)
	WHEN MATCHED THEN
		UPDATE SET
			value = s.value,
			modified = GETUTCDATE();
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
	from setting;
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
PRINT N'Creating [tompit].[big_data_partition_ins]'
GO
CREATE PROCEDURE [tompit].[big_data_partition_ins]
	@configuration uniqueidentifier,
	@status int,
	@name nvarchar(128),
	@created smalldatetime,
	@resource_group int
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.big_data_partition (configuration, file_count, status, name, created, resource_group)
	VALUES (@configuration, 0, @status, @name, @created, @resource_group);
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
	FROM tompit.view_big_data_partition;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[blob_que_draft]'
GO
CREATE PROCEDURE [tompit].[blob_que_draft]
	@draft nvarchar(256)
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
	FROM tompit.view_big_data_partition
	WHERE configuration = @configuration;
END
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
	@draft nvarchar(256)
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
PRINT N'Creating [tompit].[big_data_buffer_que]'
GO
CREATE PROCEDURE [tompit].[big_data_buffer_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.big_data_buffer;
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
PRINT N'Creating [tompit].[service_upd]'
GO
CREATE PROCEDURE [tompit].[service_upd]
	@id int,
	@name nvarchar(128),
	@url nvarchar(136),
	@supported_stages int,
	@template uniqueidentifier,
	@resource_group int,
	@version varchar(32) = NULL,
	@commit varchar(32) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	update [service] set
		name = @name,
		url = @url,
		supported_stages = @supported_stages,
		template = @template,
		resource_group = @resource_group,
		[version] = @version,
		[commit] = @commit
	where id = @id;

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_node_ins]'
GO
CREATE PROCEDURE [tompit].[big_data_node_ins]
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
PRINT N'Creating [tompit].[big_data_node_upd]'
GO
CREATE PROCEDURE [tompit].[big_data_node_upd]
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
	and (@topic is null or topic = @topic);
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
PRINT N'Creating [tompit].[lock]'
GO
CREATE TABLE [tompit].[lock]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[entity] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[lock_timeout] [datetime2] NULL,
[unlock_key] [uniqueidentifier] NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_lock] on [tompit].[lock]'
GO
ALTER TABLE [tompit].[lock] ADD CONSTRAINT [PK_lock] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [tompit].[lock]'
GO
ALTER TABLE [tompit].[lock] ADD CONSTRAINT [IX_lock] UNIQUE NONCLUSTERED  ([entity]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[lock_lock]'
GO
CREATE PROCEDURE [tompit].[lock_lock]
	@entity nvarchar(128),
	@timeout datetime2(7),
	@date datetime2(7)
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT * FROM tompit.lock WHERE entity = @entity)
		INSERT tompit.lock(entity) VALUES (@entity);

	DECLARE @ct TABLE
	(
		[unlock_key] [uniqueidentifier] NOT NULL
	);

	WITH idx AS
	(
		SELECT	TOP 1 *
		FROM	tompit.lock
		WHERE	entity = @entity
		AND		(lock_timeout IS NULL OR lock_timeout <= @date)
	)
	UPDATE idx SET
		lock_timeout = @timeout,
		unlock_key = newid()
	OUTPUT inserted.unlock_key INTO @ct;

	SELECT TOP 1 *
	FROM tompit.lock
	WHERE unlock_key = (SELECT unlock_key FROM @ct);
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
PRINT N'Creating [tompit].[lock_unlock]'
GO
CREATE PROCEDURE [tompit].[lock_unlock]
	@unlock_key uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.lock SET
		lock_timeout = NULL,
		unlock_key = NULL
	WHERE unlock_key = @unlock_key;
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
PRINT N'Creating [tompit].[lock_ping]'
GO
CREATE PROCEDURE [tompit].[lock_ping]
	@unlock_key uniqueidentifier,
	@timeout datetime2(7)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.lock SET
		lock_timeout = @timeout
	WHERE unlock_key = @unlock_key;
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
PRINT N'Creating [tompit].[queue]'
GO
CREATE TABLE [tompit].[queue]
(
[id] [bigint] NOT NULL,
[message] [nvarchar] (1024) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[created] [datetime] NOT NULL,
[expire] [datetime] NOT NULL,
[next_visible] [datetime] NOT NULL,
[queue] [varchar] (32) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[pop_receipt] [uniqueidentifier] NULL,
[dequeue_count] [int] NOT NULL,
[scope] [int] NOT NULL,
[dequeue_timestamp] [datetime] NULL,
[buffer_key] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_queue] on [tompit].[queue]'
GO
ALTER TABLE [tompit].[queue] ADD CONSTRAINT [PK_queue] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[queue_upd]'
GO
CREATE PROCEDURE [tompit].[queue_upd]
	@items nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	TRUNCATE TABLE tompit.queue;

	INSERT tompit.queue (id, message, created, expire, next_visible, queue, pop_receipt, dequeue_count, scope, dequeue_timestamp, buffer_key)
	SELECT id, message, created, expire, next_visible, queue, pop_receipt, dequeue_count, scope, dequeue_timestamp, buffer_key 
		FROM OPENJSON (@items) 
		WITH (id bigint, message nvarchar(1024), created datetime2, expire datetime2, next_visible datetime2, queue varchar(32), pop_receipt uniqueidentifier, dequeue_count int, scope int, dequeue_timestamp datetime2, buffer_key nvarchar(128));
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[print_spooler]'
GO
CREATE TABLE [tompit].[print_spooler]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[created] [datetime] NOT NULL,
[content] [varbinary] (max) NOT NULL,
[mime] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[printer] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[token] [uniqueidentifier] NOT NULL,
[identity] [uniqueidentifier] NULL,
[copy_count] [int] NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_print_spooler] on [tompit].[print_spooler]'
GO
ALTER TABLE [tompit].[print_spooler] ADD CONSTRAINT [PK_print_spooler] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[print_spooler_ins]'
GO
CREATE PROCEDURE [tompit].[print_spooler_ins]
	@created datetime,
	@content varbinary(max),
	@mime nvarchar(128),
	@printer nvarchar(256),
	@token uniqueidentifier,
	@identity uniqueidentifier = null,
	@copy_count int = 1
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.print_spooler (created, content, mime, printer, token, [identity], copy_count)
	VALUES (@created, @content, @mime, @printer, @token, @identity, @copy_count);

	RETURN scope_identity();
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[print_spooler_sel]'
GO
CREATE PROCEDURE [tompit].[print_spooler_sel]
	@id bigint = NULL,
	@token uniqueidentifier = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.print_spooler 
	WHERE (@id IS NULL OR id = @id)
	AND (@token IS NULL OR token = @token);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[print_spooler_del]'
GO
CREATE PROCEDURE [tompit].[print_spooler_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.print_spooler 
	WHERE id = @id;
END
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
	@draft nvarchar(256) = null,
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
PRINT N'Creating [tompit].[blob_upd]'
GO
CREATE PROCEDURE [tompit].[blob_upd]
	@file_name nvarchar(128) = null,
	@id bigint,
	@size int,
	@content_type nvarchar(128) = null,
	@primary_key nvarchar(256) = null,
	@draft nvarchar(256) = null,
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
[state] [uniqueidentifier] NULL,
[retry_interval] [int] NOT NULL CONSTRAINT [DF_worker_retry_interval] DEFAULT ((10)),
[disable_treshold] [int] NOT NULL CONSTRAINT [DF_worker_disable_treshold] DEFAULT ((3))
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
PRINT N'Creating [tompit].[event]'
GO
CREATE TABLE [tompit].[event]
(
[name] [nvarchar] (256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[created] [datetime2] NOT NULL,
[identifier] [uniqueidentifier] NOT NULL,
[arguments] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[callback] [varchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[service] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[event_que]'
GO
CREATE PROCEDURE [tompit].[event_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
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
	@kind int,
	@retry_interval int,
	@disable_treshold int
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.worker (start_time, end_time, interval_type, interval_value, start_date, end_date, limit, status, day_of_month,
		last_run, last_complete, next_run, day_mode, month_mode, year_mode, run_count, month_number, end_mode, interval_counter,
		month_part, elapsed, weekdays, fail_count, worker, logging, kind, retry_interval, disable_treshold)
	VALUES (@start_time, @end_time, @interval_type, @interval_value, @start_date, @end_date, @limit, @status, @day_of_month,
		@last_run, @last_complete, @next_run, @day_mode, @month_mode, @year_mode, @run_count, @month_number, @end_mode, @interval_counter,
		@month_part, @elapsed, @weekdays, @fail_count, @worker, @logging, @kind, @retry_interval, @disable_treshold);
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
	@state uniqueidentifier = NULL,
	@retry_interval int,
	@disable_treshold int
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
		state = @state,
		retry_interval = @retry_interval,
		disable_treshold = @disable_treshold
	WHERE id = @id;
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
) ON [PRIMARY]
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
[provider] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[user] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[serial_number] [bigint] NULL,
[category] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[copy_count] [int] NULL
) ON [PRIMARY]
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
PRINT N'Creating [tompit].[print_job_upd]'
GO
CREATE PROCEDURE [tompit].[print_job_upd]
	@items nvarchar(max)
AS
BEGIN
	SET NOCOUNT ON;

	TRUNCATE TABLE tompit.print_job;

	INSERT tompit.print_job (created, token, status, error, component, arguments, provider, [user], serial_number, category, copy_count)
	SELECT created, token, status, error, component, arguments, provider, user, serial_number, category, copy_count
		FROM OPENJSON (@items) 
		WITH (created datetime, token uniqueidentifier, status int, error nvarchar(1024), component uniqueidentifier, arguments nvarchar(max), 
			provider nvarchar(128), [user] nvarchar(128), serial_number bigint, category nvarchar(128), copy_count int);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[setting_sel]'
GO
CREATE PROCEDURE [tompit].[setting_sel]
	@name nvarchar(128),
	@type nvarchar(128) = NULL,
	@primary_key nvarchar(128) = NULL,
	@namespace nvarchar(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 *
	from setting
	where (name = @name)
	AND ((@type IS NULL AND type IS NULL) OR (type = @type))
	AND ((@namespace IS NULL AND namespace IS NULL) OR (namespace = @namespace))
	AND ((@primary_key IS NULL AND primary_key IS NULL) OR (primary_key = @primary_key));
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
PRINT N'Creating [tompit].[client]'
GO
CREATE TABLE [tompit].[client]
(
[id] [bigint] NOT NULL IDENTITY(1, 1),
[token] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[name] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[created] [smalldatetime] NOT NULL,
[status] [int] NOT NULL,
[type] [nvarchar] (64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_client] on [tompit].[client]'
GO
ALTER TABLE [tompit].[client] ADD CONSTRAINT [PK_client] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [tompit].[client]'
GO
ALTER TABLE [tompit].[client] ADD CONSTRAINT [IX_client] UNIQUE NONCLUSTERED  ([token]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[client_ins]'
GO
CREATE PROCEDURE [tompit].[client_ins]
	@token nvarchar(128),
	@name nvarchar(128),
	@created smalldatetime,
	@status int,
	@type nvarchar(64) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.client (token, name, created, status, type)
	VALUES (@token, @name, @created, @status, @type);
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
PRINT N'Creating [tompit].[client_upd]'
GO
CREATE PROCEDURE [tompit].[client_upd]
	@id bigint,
	@name nvarchar(128),
	@status int,
	@type nvarchar(64) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.client SET
		name = @name,
		status = @status,
		type = @type
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[client_del]'
GO
CREATE PROCEDURE [tompit].[client_del]
	@id bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.client
	WHERE id = @id;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[view_permission]'
GO

CREATE VIEW [tompit].[view_permission]
AS
SELECT        tompit.permission.*, tompit.resource_group.token AS resource_group_token
FROM            tompit.permission LEFT JOIN
                         tompit.resource_group ON tompit.permission.resource_group = tompit.resource_group.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[event_upd]'
GO
CREATE PROCEDURE [tompit].[event_upd]
	@items nvarchar(max)
AS
BEGIN
	SET NOCOUNT ON;

	TRUNCATE TABLE tompit.event;

	INSERT tompit.event (name, created, identifier, arguments, callback, service)
	SELECT name, created, identifier, arguments, callback, service 
		FROM OPENJSON (@items) 
		WITH (name nvarchar(256), created datetime2(7), identifier uniqueidentifier, arguments nvarchar(max), callback varchar(128), service uniqueidentifier);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[client_sel]'
GO
CREATE PROCEDURE [tompit].[client_sel]
	@id bigint = NULL,
	@token nvarchar(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 *
	FROM tompit.client
	WHERE (@id IS NULL OR id = @id)
	AND (@token IS NULL OR token = @token);
END
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
PRINT N'Creating [tompit].[client_que]'
GO
CREATE PROCEDURE [tompit].[client_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.client;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[queue_que]'
GO
CREATE PROCEDURE [tompit].[queue_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM tompit.queue;
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_transaction_ins]'
GO
CREATE PROCEDURE [tompit].[big_data_transaction_ins]
	@partition int,
	@token uniqueidentifier,
	@block_count int,
	@created datetime,
	@timezone int = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.big_data_transaction (block_count, block_remaining, created, token, status, partition, timezone)
	VALUES (@block_count, @block_count, @created, @token, 1, @partition, @timezone);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[big_data_buffer_mdf]'
GO
CREATE PROCEDURE [tompit].[big_data_buffer_mdf]
	@items nvarchar(max)
AS
BEGIN
	SET NOCOUNT ON;

	MERGE tompit.big_data_buffer AS t
	USING (SELECT id, partition, next_visible FROM OPENJSON(@items) WITH (id int, partition uniqueidentifier, next_visible datetime2(7))) AS s 
	(id, partition, next_visible)
	ON (t.partition = s.partition)
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (id, partition, next_visible)
		VALUES (s.id, s.partition, s.next_visible)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
	WHEN matched THEN
		UPDATE SET
			t.next_visible = s.next_visible;
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

	INSERT tompit.big_data_transaction_block ([transaction], token)
	VALUES (@transaction, @token);
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
		p.configuration partition_token,
		tz.token timezone_token
FROM tompit.big_data_transaction t
INNER JOIN tompit.big_data_partition p ON t.partition = p.id
LEFT JOIN tompit.big_data_timezone tz ON t.timezone = tz.id
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
PRINT N'Creating [tompit].[mru_mdf]'
GO
CREATE PROCEDURE [tompit].[mru_mdf]
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
PRINT N'Creating [tompit].[mru_que]'
GO
CREATE PROCEDURE [tompit].[mru_que]
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
PRINT N'Creating [tompit].[mru_del]'
GO
CREATE PROCEDURE [tompit].[mru_del]
	@token uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	DELETE tompit.mru 
	WHERE token = @token
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[setting_del]'
GO
CREATE PROCEDURE [tompit].[setting_del]
	@name nvarchar(128),
	@type nvarchar(128) = NULL,
	@primary_key nvarchar(128) = NULL,
	@namespace nvarchar(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	delete setting
	where (name = @name)
	AND ((@type IS NULL AND type IS NULL) OR (type = @type))
	AND ((@namespace IS NULL AND namespace IS NULL) OR (namespace = @namespace))
	AND ((@primary_key IS NULL AND primary_key IS NULL) OR (primary_key = @primary_key));
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
PRINT N'Creating [tompit].[print_job_serial_number]'
GO
CREATE TABLE [tompit].[print_job_serial_number]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[category] [nvarchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[serial_number] [bigint] NOT NULL
) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_print_job_serial_number] on [tompit].[print_job_serial_number]'
GO
ALTER TABLE [tompit].[print_job_serial_number] ADD CONSTRAINT [PK_print_job_serial_number] PRIMARY KEY CLUSTERED  ([id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [tompit].[print_job_serial_number]'
GO
ALTER TABLE [tompit].[print_job_serial_number] ADD CONSTRAINT [IX_print_job_serial_number] UNIQUE NONCLUSTERED  ([category]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[print_job_serial_number_ins]'
GO
CREATE PROCEDURE [tompit].[print_job_serial_number_ins]
	@category nvarchar(128),
	@serial_number bigint
AS
BEGIN
	SET NOCOUNT ON;

	INSERT tompit.print_job_serial_number (category, serial_number) VALUES (@category, @serial_number);
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [tompit].[permission_ins]'
GO
CREATE PROCEDURE [tompit].[permission_ins]
	@evidence nvarchar(128),
	@schema nvarchar(128),
	@claim nvarchar(128),
	@descriptor nvarchar(128),
	@primary_key nvarchar(4000),
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
PRINT N'Creating [tompit].[print_job_serial_number_upd]'
GO
CREATE PROCEDURE [tompit].[print_job_serial_number_upd]
	@id int,
	@serial_number bigint
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE tompit.print_job_serial_number SET
		serial_number = @serial_number
	WHERE id = @id;
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
PRINT N'Creating [tompit].[print_job_serial_number_que]'
GO
CREATE PROCEDURE [tompit].[print_job_serial_number_que]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.print_job_serial_number;
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
PRINT N'Creating [tompit].[print_job_serial_number_sel]'
GO
CREATE PROCEDURE [tompit].[print_job_serial_number_sel]
	@category nvarchar(128)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 * 
	FROM tompit.print_job_serial_number
	WHERE category = @category;
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
PRINT N'Creating [tompit].[print_job_que]'
GO
CREATE PROCEDURE [tompit].[print_job_que]
	
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM tompit.print_job;
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
PRINT N'Creating [tompit].[blob_content_que_by_type]'
GO
CREATE PROCEDURE [tompit].[blob_content_que_by_type]
	@resource_group int,
	@types nvarchar(max)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT c.*
	FROM tompit.blob_content c
	INNER JOIN tompit.blob b ON c.blob = b.token
	WHERE b.type IN (SELECT type FROM OPENJSON(@types) WITH (type int))
	AND resource_group = @resource_group;
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
	@evidence nvarchar(128) = null,
	@schema nvarchar(128) = null,
	@claim nvarchar(128) = null,
	@primary_key nvarchar(128),
	@descriptor nvarchar(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	select top 1 *
	from view_permission
	where (@evidence is null or evidence = @evidence)
	and (@schema is null or [schema] = @schema)
	and (@claim is null or claim = @claim)
	and (@descriptor is null or descriptor = @descriptor)
	and primary_key = @primary_key;
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
PRINT N'Creating [migr].[test]'
GO
CREATE PROCEDURE [migr].[test]
AS
BEGIN
	SET NOCOUNT ON;
END
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
PRINT N'Adding foreign keys to [tompit].[big_data_buffer_data]'
GO
ALTER TABLE [tompit].[big_data_buffer_data] ADD CONSTRAINT [FK_big_data_buffer_data_big_data_buffer] FOREIGN KEY ([buffer]) REFERENCES [tompit].[big_data_buffer] ([id]) ON DELETE CASCADE
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
ALTER TABLE [tompit].[big_data_index] ADD CONSTRAINT [FK_big_data_index_big_data_timezone] FOREIGN KEY ([timezone]) REFERENCES [tompit].[big_data_timezone] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[big_data_transaction]'
GO
ALTER TABLE [tompit].[big_data_transaction] ADD CONSTRAINT [FK_big_data_transaction_big_data_partition] FOREIGN KEY ([partition]) REFERENCES [tompit].[big_data_partition] ([id])
GO
ALTER TABLE [tompit].[big_data_transaction] ADD CONSTRAINT [FK_big_data_transaction_big_data_timezone] FOREIGN KEY ([timezone]) REFERENCES [tompit].[big_data_timezone] ([id])
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
PRINT N'Adding foreign keys to [tompit].[component]'
GO
ALTER TABLE [tompit].[component] ADD CONSTRAINT [FK_component_folder] FOREIGN KEY ([folder]) REFERENCES [tompit].[folder] ([id])
GO
ALTER TABLE [tompit].[component] ADD CONSTRAINT [FK_component_service] FOREIGN KEY ([service]) REFERENCES [tompit].[service] ([id])
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
PRINT N'Adding foreign keys to [tompit].[mru_tag]'
GO
ALTER TABLE [tompit].[mru_tag] ADD CONSTRAINT [FK_mru_tag_mru] FOREIGN KEY ([mru]) REFERENCES [tompit].[mru] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[permission]'
GO
ALTER TABLE [tompit].[permission] ADD CONSTRAINT [FK_permission_resource_group] FOREIGN KEY ([resource_group]) REFERENCES [tompit].[resource_group] ([id]) ON DELETE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [tompit].[service]'
GO
ALTER TABLE [tompit].[service] ADD CONSTRAINT [FK_solution_resource_group] FOREIGN KEY ([resource_group]) REFERENCES [tompit].[resource_group] ([id]) ON DELETE CASCADE
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
-- This statement writes to the SQL Server Log so SQL Monitor can show this deployment.
IF HAS_PERMS_BY_NAME(N'sys.xp_logevent', N'OBJECT', N'EXECUTE') = 1
BEGIN
    DECLARE @databaseName AS nvarchar(2048), @eventMessage AS nvarchar(2048)
    SET @databaseName = REPLACE(REPLACE(DB_NAME(), N'\', N'\\'), N'"', N'\"')
    SET @eventMessage = N'Redgate SQL Compare: { "deployment": { "description": "Redgate SQL Compare deployed to ' + @databaseName + N'", "database": "' + @databaseName + N'" }}'
    EXECUTE sys.xp_logevent 55000, @eventMessage
END
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
