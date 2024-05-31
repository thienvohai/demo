namespace EmailService
{
    public class MigrationScript
    {
        public static List<KeyValuePair<int, string>> SqlServerScripts = new List<KeyValuePair<int, string>>
        {
            new KeyValuePair<int, string>(20240411,
@"
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
            [TenantId] nvarchar(100) NULL,
			[Sender] nvarchar(500) NOT NULL,
			[Status] int NOT NULL,
			[IsDeleted] bit NOT NULL default 0,
			[Created] datetime2 NOT NULL,
			[Modified] datetime2 NOT NULL,
            [To] nvarchar (max) NOT NUll,
            [CC] nvarchar (max) NULL,
            [Bcc] nvarchar (max) NULL,
            [Comment] nvarchar (max) NULL,
			PRIMARY KEY CLUSTERED ([Id])
		);

        CREATE TABLE [@_tableSchema].[EmailPolicy](
			[Id] uniqueidentifier NOT NULL,
			[IsDeleted] bit NOT NULL default 0,
			[Created] datetime2 NOT NULL,
			[Modified] datetime2 NOT NULL,
            [TenantId] nvarchar(100) NOT NULL,
            [Content] nvarchar(max) NOT NULL,
			PRIMARY KEY CLUSTERED ([Id])
        );

        CREATE TABLE [@_tableSchema].[EmailCounter](
			[Id] uniqueidentifier NOT NULL,
			[IsDeleted] bit NOT NULL default 0,
			[Created] datetime2 NOT NULL,
			[Modified] datetime2 NOT NULL,
            [Type] int NOT NULL,
            [Target] nvarchar(max) NOT NULL,
            [Count] int NOT NULL,
            [Minutes] int NOT NULL,
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
"),
        };
        public static List<KeyValuePair<int, string>> PostgreSqlScripts = new List<KeyValuePair<int, string>>
        {
            new KeyValuePair<int, string>(20240411,
@"
DO $$ 
DECLARE 
    _version INT;
    _contextName VARCHAR(256);
    _ext TEXT;
BEGIN
    _version := 20240215;
    _contextName := 'EmailService';
    
    -- Create UpdateHistories table if it does not exist
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UpdateHistories' AND table_schema = '@_tableSchema')
    THEN
        CREATE TABLE ""@_tableSchema"".""UpdateHistories"" (
            ""ContextName"" VARCHAR(256) NOT NULL,
            ""ContextVersion"" BIGINT NOT NULL,
            ""UpdateTime"" TIMESTAMPTZ NOT NULL,
            ""Ext"" TEXT NULL,
            PRIMARY KEY (""ContextName"")
        );
    END IF;

    -- Check if the context has been updated
    IF (SELECT COUNT(*) FROM ""@_tableSchema"".""UpdateHistories"" WHERE ""ContextName"" = _contextName AND ""ContextVersion"" >= _version) = 0
    THEN
        -- Input and replace the upgrade script below
        -- Create Email table
        IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Email' AND table_schema = '@_tableSchema')
        THEN
            CREATE TABLE ""@_tableSchema"".""Email"" (
                ""Id"" UUID NOT NULL,
                ""TenantId"" VARCHAR(100) NOT NULL,
                ""Sender"" VARCHAR(500) NOT NULL,
                ""Status"" INT NOT NULL,
                ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                ""Created"" TIMESTAMPTZ NOT NULL,
                ""Modified"" TIMESTAMPTZ NOT NULL,
                ""To"" TEXT NOT NULL,
                ""CC"" TEXT NULL,
                ""Bcc"" TEXT NULL,
                ""Comment"" TEXT NULL,
                PRIMARY KEY (""Id"")
            );
            CREATE TABLE ""@_tableSchema"".""EmailPolicy"" (
                ""Id"" UUID NOT NULL,
                ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                ""Created"" TIMESTAMPTZ NOT NULL,
                ""Modified"" TIMESTAMPTZ NOT NULL,
                ""TenantId"" VARCHAR(100) NOT NULL,
                ""Content"" TEXT NOT NULL,
                PRIMARY KEY (""Id"")
            );
            CREATE TABLE ""@_tableSchema"".""EmailCounter"" (
                ""Id"" UUID NOT NULL,
                ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                ""Created"" TIMESTAMPTZ NOT NULL,
                ""Modified"" TIMESTAMPTZ NOT NULL,
                ""Type"" INT NOT NULL,
                ""Target"" TEXT NOT NULL,
                ""Count"" INT NOT NULL,
                ""Minutes"" INT NOT NULL,
                PRIMARY KEY (""Id"")
            );
        END IF;
        -- Input and replace the upgrade script above

        -- Insert or update the context version in UpdateHistories
        IF NOT EXISTS (SELECT 1 FROM ""@_tableSchema"".""UpdateHistories"" WHERE ""ContextName"" = _contextName)
        THEN
            INSERT INTO ""@_tableSchema"".""UpdateHistories"" (""ContextName"", ""ContextVersion"", ""UpdateTime"", ""Ext"")
            VALUES (_contextName, _version, CURRENT_TIMESTAMP, _ext);
        ELSE
            UPDATE ""@_tableSchema"".""UpdateHistories""
            SET ""ContextVersion"" = _version, ""UpdateTime"" = CURRENT_TIMESTAMP, ""Ext"" = _ext
            WHERE ""ContextName"" = _contextName;
        END IF;
    END IF;
END $$;
"),
        };
    }
}
