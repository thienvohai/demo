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
        CREATE TABLE "@_tableSchema"."UpdateHistories" (
            "ContextName" VARCHAR(256) NOT NULL,
            "ContextVersion" BIGINT NOT NULL,
            "UpdateTime" TIMESTAMPTZ NOT NULL,
            "Ext" TEXT NULL,
            PRIMARY KEY ("ContextName")
        );
    END IF;

    -- Check if the context has been updated
    IF (SELECT COUNT(*) FROM "@_tableSchema"."UpdateHistories" WHERE "ContextName" = _contextName AND "ContextVersion" >= _version) = 0
    THEN
        -- Input and replace the upgrade script below
        -- Create Email table
        IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Email' AND table_schema = '@_tableSchema')
        THEN
            CREATE TABLE "@_tableSchema"."Email" (
                "Id" UUID NOT NULL,
                "Subject" VARCHAR(4000) NULL,
                "Sender" VARCHAR(500) NOT NULL,
                "SenderName" VARCHAR(4000) NULL,
                "Status" INT NOT NULL,
                "RetryCount" INT NOT NULL DEFAULT 0,
                "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
                "Created" TIMESTAMPTZ NOT NULL,
                "Modified" TIMESTAMPTZ NOT NULL,
                "IsRetrying" BOOLEAN NOT NULL DEFAULT FALSE,
                "NextRetryTime" TIMESTAMPTZ NULL,
                "IsBodyHtml" BOOLEAN NOT NULL DEFAULT FALSE,
                "Body" TEXT NULL,
                "Attachment" TEXT NULL,
                "To" TEXT NOT NULL,
                "CC" TEXT NULL,
                "Bcc" TEXT NULL,
                PRIMARY KEY ("Id")
            );
        END IF;
        -- Input and replace the upgrade script above

        -- Insert or update the context version in UpdateHistories
        IF NOT EXISTS (SELECT 1 FROM "@_tableSchema"."UpdateHistories" WHERE "ContextName" = _contextName)
        THEN
            INSERT INTO "@_tableSchema"."UpdateHistories" ("ContextName", "ContextVersion", "UpdateTime", "Ext")
            VALUES (_contextName, _version, CURRENT_TIMESTAMP, _ext);
        ELSE
            UPDATE "@_tableSchema"."UpdateHistories"
            SET "ContextVersion" = _version, "UpdateTime" = CURRENT_TIMESTAMP, "Ext" = _ext
            WHERE "ContextName" = _contextName;
        END IF;
    END IF;
END $$;