# myicm.FormatSupervisor
Service to monitor Kafka message format. Also publishes JSON schemas for ADP (schemas.adp topic) on every start.

# highlights
confluent-kafka-dotnet, MsgPack.Cli, JSON schema, NLog to Kafka, winservice/console, graceful shutdown, no BO-DB access

# kafka topics
read: any
write: format.issues, schemas.adp

# schema rules
Schema is unique per topic/key. Schema can refer an another schema to avoid duplicate definitions. Missing schema is considered always valid.
Schemas are stored in ICM.FormatSupervisor\Schemas folder. 'ADP' folder contains schema files by key name (ADP/keyname.json) 
'Kafka' folder contains subfolders named by topic names, with files named after the keys (Kafka/topicname/keyname.json)
To temporarily disable schema, rename it so it has extension different from JSON (keyname._json)

# schema referencing
Sample schema referencing another (in file Schemas\Kafka\test.idk\lol.json):
{
    "$schema": "http://json-schema.org/draft-04/schema#",
    "$ref": "test.idk/msg.json"
}
'$ref' should contain local reference to existing file

# topics validated
All topics which has published and enabled schemas will be validated