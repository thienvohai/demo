DECLARE @_version int, -- replace by the version of script, the format should be yyyyMMdd
		@_contextName nvarchar(256), -- replace by the context full name
		@_ext nvarchar(MAX) -- replace by additional meatadata if any;
			
SET @_version = 20240215;
SET @_contextName = 'EmailService';
	
IF NOT EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[@_tableSchema].[UpdateHistories]'))
BEGIN
CREATE TABLE [@_tableSchema].[UpdateHistories] (
  [ContextName] nvarchar(256) NOT NULL,
  [ContextVersion] bigint NOT NULL,
  [UpdateTime] datetime2 NOT NULL,
  [Ext] nvarchar(max) NULL,
  PRIMARY KEY CLUSTERED ([ContextName])
)
END

IF (SELECT COUNT(*) FROM [@_tableSchema].[UpdateHistories] WHERE [ContextName] = @_contextName AND [ContextVersion] >= @_version) = 0
BEGIN
	-- input and replace the upgrade script below --
	/* Create table */
    IF NOT EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[@_tableSchema].[Email]'))
    BEGIN
		CREATE TABLE [@_tableSchema].[Email](
			[Id] uniqueidentifier NOT NULL,
			[Subject] nvarchar(4000) NULL,
			[Sender] nvarchar(500) NOT NULL,
            [SenderName] nvarchar(4000) NULL,
			[Status] int NOT NULL,
            [RetryCount] int NOT NULL default 0,
			[IsDeleted] bit NOT NULL default 0,
			[Created] datetime2 NOT NULL,
			[Modified] datetime2 NOT NULL,
            [IsRetrying] bit NOT NULL default 0,
            [NextRetryTime] datetime2 NUll,
            [IsBodyHtml] bit NOT NULL default 0,
            [Body] nvarchar(max) NULL,
            [Attachment] nvarchar (max) NULL,
            [To] nvarchar (max) NOT NUll,
            [CC] nvarchar (max) NULL,
            [Bcc] nvarchar (max) NULL,
			PRIMARY KEY CLUSTERED ([Id])
		);
	END
    -- input and replace the upgrade script above --
    IF NOT EXISTS (SELECT * FROM [@_tableSchema].[UpdateHistories] WHERE [ContextName] = @_contextName)
    BEGIN
        INSERT INTO [@_tableSchema].[UpdateHistories] ([ContextName], [ContextVersion], [UpdateTime], [Ext]) VALUES (@_contextName, @_version, CURRENT_TIMESTAMP, @_ext);
    END
    ELSE
    BEGIN
        UPDATE [@_tableSchema].[UpdateHistories] SET [ContextVersion] = @_version, [UpdateTime] = CURRENT_TIMESTAMP, [Ext] = @_ext WHERE [ContextName] = @_contextName
    END
END;