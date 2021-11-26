﻿using Microsoft.Data.SqlClient;
using SqlDataScriptGenerator.Framework.Contracts;
using SqlDataScriptGenerator.Framework.Models;
using SqlDataScriptGenerator.Framework.Models.SqlServer;
using SqlDataScriptGenerator.Framework.SqlServer;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlDataScriptGenerator.Console.Launcher
{
    public class ApplicationLauncher
    {
        public static void Main(string[] args)
        {
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

            System.Console.WriteLine(sqlScript.ToString());

            System.Console.ReadKey();
        }

    }
}
