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
                RequestType = "U",
                WhereClause = "id=1"
            };

            IScriptGenerator scriptGenerator = new SqlServerScriptGenerator();
            var sqlScript = scriptGenerator.GetSqlDataScript(scriptRequest, sqlConnection);
            
The possible values for **RequestType** property in  **SqlServerDataScriptRequest** are 
1. U - Update 
2. I - Insert
3. D - Delete
