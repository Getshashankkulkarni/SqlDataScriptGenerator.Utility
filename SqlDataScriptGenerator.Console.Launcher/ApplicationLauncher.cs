using Microsoft.Data.SqlClient;
using SqlDataScriptGenerator.Framework.Contracts;
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
            string connectionString = "Server=Shashank;Database=PalashWebDB_SGHM_Client;Trusted_Connection=True;";
            var sqlConnection = new SqlConnection(connectionString);
            SqlServerDataScriptRequest scriptRequest = new SqlServerDataScriptRequest
            {
                TableName = "T_Charges",
                RequestType = "U",
                WhereClause = "Opd_Ipd_Id=1681",
                ColumnsToIgnore = new List<string> { "UpdatedBy","UpdatedWindowsLoginName" }
            };

            IScriptGenerator scriptGenerator = new SqlServerScriptGenerator();
            var sqlScript = scriptGenerator.GetSqlDataScript(scriptRequest, sqlConnection);

            System.Console.WriteLine(sqlScript.ToString());

            System.Console.ReadKey();
        }

    }
}
