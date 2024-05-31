# HOW TO SET UP API
## Prepare resource:
- We need a K8s cluster to run our Email Serice
- We need Certificate for Email Service connection
- We need DB server (should use SQL Server), our Pods can connect to DB Server
- We need create a SendGrid client and SendGrid API key to send email
## Setup for cluster
## Setup for container
### Certificate file:
- We need to have certificate file in root folder of container: "/app/tls.crt" and "/app/tls.key". Need to mount the file

    ```C#
    options.ConfigureHttpsDefaults(listenOptions =>
    {
        listenOptions.ServerCertificate = new X509Certificate2(X509Certificate2.CreateFromPemFile("tls.crt", "tls.key").Export(X509ContentType.Pfx));
    });
    ```

### Configuration:
- We need to have configuration file: "/app/appsetting.json" or have configMap for the below key and value:
    * QueueCapacity: The capacity of the queue message, used when our api send message to its background service to handle email, write to queue function will wait for available capacity
    * FilterRecipient:BlockList: Recipient email address will be blocked, support for .* regex (for example: "*@gmail.com")
    * FilterRecipient:AllowList: Recipient email address will be allowed, support for .* regex (for example: "*" - should be default value)
    * BackgroundWorker:MaxConcurrentTask: The maximum of Task can be create to handle email or retry email in Background Service, the service will wait for create other task
    * Database:DatabaseType: The type of db server, should be: "SqlServer"/"PostgreSql"/"MySql"
    * Database:ConnectionString: The connection string to connect to db, use @_databaseName for the Db name, for example: "Data Source=localhost;Initial Catalog=@_databaseName;User ID=sa;Password=1qaz2wsxE;TrustServerCertificate=True;"
    * Database:TableSchema: the table schema, for example: "dbo"
    * EmailClient:EmailClientType: Type of the email client service, should be: "SendGrid" (will support other client later)
    * EmailClient:SendGrid:ApiKey: The api key of send grid client, use to send request to SendGrid service
    * AuthProvider: use to set up the Authentication Scheme for Email Service (will update later)
    ```json
    {
        "Logging": {
            "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
            }
        },
        "AllowedHosts": "*",
        "QueueCapacity": 1000,
        "FilterRecipient": {
            "BlockList": [
            ],
            "AllowList": [
            "*"
            ]
        },
        "BackgroundWorker": {
            "MaxConcurrentTask": 100
        },
        "Database": {
            "DatabaseType": "", //SqlServer, PostgreSql, Mysql
            "ConnectionString": "",
            "TableSchema": ""
        },
        "EmailClient": {
            "EmailClientType": "", //SendGrid
            "SendGrid": {
            "ApiKey": ""
            }
        },
        "AuthProvider": {
            "ValidIssuers": [

            ],
            "Authority": "",
            "Audience": ""
        }
    }
    ```