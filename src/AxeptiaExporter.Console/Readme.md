# Description

This program is used to retrieve data from a database (for now just supporting MSSQL), export that data to file in JSON format and then send it to Axeptia.

## Let's start

Before you run this program you must modify the configuration file, located in the program folder, named `axeptiaExportConfig.txt`

Each configuration section is seperated by `|[[Name of Section]]|`

For more info about the configuration, see the part **Description of the axeptiaExportConfig.txt** at det end of this file.

To use the program you must start it with a parameter. To have it list available parameters, just run the program without parameters: `AxeptiaExporter.ConsoleApp.exe`

Here are some valid parameters:

| Parameter | Description                              |
| --------- | ---------------------------------------- |
| --version | Write the program version to the console |
| --run     | Start the program                        |

`--run` parameter needs one of the following values:

| Value | Description                                                                                                              |
| ----- | ------------------------------------------------------------------------------------------------------------------------ |
| hello | Will just write hello to the console                                                                                     |
| test  | Will get 100 records (unless another limit is specified in the SQL) from the SQL set in config file and store it to file |
| real  | Will run the program                                                                                                     |

Sample:

Just write a hello message to the console:

`.\AxeptiaExporter.ConsoleApp.exe --run hello`

Check the version of the program:

`.\AxeptiaExporter.ConsoleApp.exe --version`

## Where to find the exported data

Important: If you run it with `--run real`, then the files will only be stored in the system for a short period, until it's been uploaded to Axeptia. Use `--run test` to prevent automatic uploading of files to Axeptia.

The job exports files to the `data` directory located where the program is located.

## Where to find the log file

The log files are located in the `log` directory located where the program is located. The log files will be rotated, so it will create a new one for each day. The last 31 log files are retained.

## Multiple file delivery

**You kan skip this part if you are not informed by Axeptia to do this**.

If you need to deliver multiple files, and the configuration to retrieve the data is mostly the same, you can add config files for each file in the `configs` child directory of the program location.

Those config files should only contain the specific sections that differ for each export.

Sections defined in these config files will override corresponding sections in `axeptiaExportConfig.txt` (located in program folder). The program will run one export for each file ending with `_config.txt` that is found in the `configs` directory.

Example of filenames for _config files:

`division1_config.txt`

`division2_config.txt`

`sale_dep_config.txt`

## Database Providers

In the configuration file, you need to set the connection string that can access the database. Here are some examples:

### MSSQL

#### Standard Security

```
Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;
```

#### Trusted Connection

```
Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;
```

##### More samples

Use [this Link](https://www.connectionstrings.com/sql-server/) to find more connection string samples

## Description of the axeptiaExportConfig.txt

The content of this file is just sample settings and must be replaced with your own settings.

```
|[[Company]]|
/*** Add your company name below this comment. ***/
axeptia

|[[GUID]]|
/*** This value is something you must get from Axeptia. ***/
f2cd5917-973a-41cc-99cd-2f7050336c91

|[[ConnectionString]]|
/*** A valid connection string that is used to connect to the database. Readme file points to sites with samples ***/
Server=local-server;Database=Transaction;User Id=myId;Password=myPwd;MultipleActiveResultSets=true

|[[Sql]]|
/***
Sql to retrieve records that should be sent to Axeptia.
It must contain the @runFrom parameter, which is used to restrict the number of records that should be exported.
@runFrom parameter is set in RunFrom section in this file
***/
SELECT [id]
      ,[name]
      ,[address]
      ,[zipCode]
      ,[city]
      ,[countryCodeAlpha2]
      ,[created]
      ,[updated]
      ,[deleted]
  FROM [dbo].[Table]
  WHERE created >= CAST(@runFrom AS datetime)

|[[RunFrom]]|
/***
A value, Date or Number, which is used in the SQL to prevent full export everytime. The value here will be updated when the job is finished.
Value is then set to the newest (Date) or highest (Number) found in the column specified in the RunFromUpdateByColumn section.
Samples:
Use of Date: 2020-01-21T14:10:00.000|DateTime
Use of Number: 2010|Number (this could also be a date, etc 20101225010510)
***/
2020-01-21T14:10:00.000|DateTime

|[[RunFromUpdateByColumn]]|
/***
Name of the colum that should be used when setting the new value in RunFrom section. This column must be part of the selected fields in the SQL
***/
created

|[[BlobStorageUrl]]|
/***
URL of the the blob storage where the file is uploaded to. This value is retrieved from Axeptia.
***/
https://dexeg878adfgasdiolk.blob.core.windows.net

|[[BlobContainer]]|
/***
Name of the the container where the file is uploaded to. This value is retrieved from Axeptia.
***/
axeptia

|[[BlobSasToken]]|
/***
DEPRECATED, uses ExchangeSasTokenCode instead
Token which must be used to have access to the blob storage. This value is retrieved from Axeptia.
***/
sv=2019-04-01&si=d-345NE&sr=c&sig=HcO5IGgUhaw%6h7lekOPvCi9cgfDknipOmoBSp6TxRP0UaY%34

|[[ExchangeSasTokenCode]]|
/***
Guid that is used to exchange to a valid SAS Token, used when uploading files to Axeptia. This value is retrieved from Axeptia.
***/
00930dee-11e2-4be8-a7e0-6832ee3ad075

|[[MaxRecordsInBatch]]|
/***
How many records each file should contain. Default 1000
***/
1000

|[[End]]|
/*** Just marks the end of the file ***/
END
```
