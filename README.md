# myicm.FormatSupervisor
Service to monitor Kafka message format. Also publishes JSON schemas for ADP usage, by call to localhost:60002/home/publish to topic 'schemas.adp'

# highlights
confluent-kafka-dotnet, MsgPack.Cli, JSON schema, aspnet core, mssql, NLog to Kafka, winservice/console, graceful shutdown, no BO-DB access

# kafka topics
read: any
write: format.issues, schemas.adp

# requirements
MS SQL instance, installed tables (Scripts\DB.sql). Uses 60002 tcp port
