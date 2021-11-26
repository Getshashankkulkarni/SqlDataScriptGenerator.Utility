# SqlDataScriptGenerator.Utility
This is a data script generation utility.

This utility helps you to generate a data script for a given table. 
Currently I have added support for SQL Server but this is open for extensions to other databases as well.

How to use this utility ?

To demonstrate how to make use of this utility, I have also added a console application called "**SqlDataScriptGenerator.Console.Launcher**"

1. Create an instance of **SqlServerDataScriptRequest** class and specify the table name, request type, where clause, columns to be ignored etc
2. Create an instance of the class that implements **IScriptGenerator**, for SQL Server it is **SqlServerScriptGenerator**
3. Call the   **GetSqlDataScript** method like this -
   
            string connectionString = "Server=Shashank;Database=Test_DB;Trusted_Connection=True;";
            var sqlConnection = new SqlConnection(connectionString);
            SqlServerDataScriptRequest scriptRequest = new SqlServerDataScriptRequest
            {
                TableName = "CustInfo",
                RequestType = RequestType.INSERT,
                WhereClause = "id=1"
            };

            IScriptGenerator scriptGenerator = new SqlServerScriptGenerator();
            var sqlScript = scriptGenerator.GetSqlDataScript(scriptRequest, sqlConnection);
            
      Below is the INSERT script that gets generated since I passed RequestType as "I"
  
          IF NOT EXISTS(SELECT 1 FROM [dbo].[CustInfo] WHERE [id] = 1) INSERT INTO [dbo].[CustInfo]([id],[first_name],[last_name]) VALUES(1,'Shashank','Kulkarni')
            
The possible values for **RequestType** property in  **SqlServerDataScriptRequest** are **RequestType** enum - 
INSERT
UPDATE
DELETE
