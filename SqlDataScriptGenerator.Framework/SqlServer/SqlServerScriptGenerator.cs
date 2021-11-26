// <copyright file="SqlServerScriptGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SqlDataScriptGenerator.Framework.SqlServer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.SqlClient;
    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;
    using SqlDataScriptGenerator.Framework.Contracts;
    using SqlDataScriptGenerator.Framework.Models;
    using SqlDataScriptGenerator.Framework.Models.SqlServer;

    /// <summary>
    /// Implementation for the script generation logic.
    /// </summary>
    public class SqlServerScriptGenerator : IScriptGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerScriptGenerator"/> class.
        /// </summary>
        public SqlServerScriptGenerator()
        {
            this.ColumnList = new List<Column>();
            this.PkColumnList = new List<Column>();
            this.ScriptBuilder = new StringBuilder();
        }

        private List<Column> ColumnList { get; set; }

        private List<Column> PkColumnList { get; set; }

        private StringBuilder ScriptBuilder { get; set; }

        private SqlConnection SqlConnection { get; set; }

        private ServerConnection ServerConnection { get; set; }

        private Server Server { get; set; }

        private Database Database { get; set; }

        private Table Table { get; set; }

        private DataTable DataTable { get; set; }

        private IEnumerable<string> ColumnsToIgnore { get; set; }

        /// <inheritdoc/>
        public string GetSqlDataScript(DataScriptRequest request, string connectionString)
        {
            var scriptRequest = request as SqlServerDataScriptRequest;

            // Setup Database server and database objects
            this.SetupDatabaseServerAndObjects(connectionString, scriptRequest);

            // Setup Columns
            this.SetupColumns();

            // Setup the DataTable
            this.SetupDataTable(scriptRequest);

            // Build Script
            this.BuildScript(scriptRequest);

            return this.ScriptBuilder.ToString();
        }

        /// <inheritdoc/>
        public string GetSqlDataScript(DataScriptRequest request, IDbConnection dbConnection)
        {
            var scriptRequest = request as SqlServerDataScriptRequest;
            this.SetupDatabaseServerAndObjects(dbConnection, request as SqlServerDataScriptRequest);

            // Setup Columns
            this.SetupColumns();

            // Setup the DataTable
            this.SetupDataTable(scriptRequest);

            // Build the script
            this.BuildScript(scriptRequest);

            return this.ScriptBuilder.ToString();
        }

        private void SetupDatabaseServerAndObjects(string connectionString, SqlServerDataScriptRequest scriptRequest)
        {
            this.SqlConnection = new SqlConnection(connectionString);
            this.ServerConnection = new ServerConnection(this.SqlConnection);
            this.Server = new Server(this.ServerConnection);
            this.Database = this.Server.Databases[this.SqlConnection.Database];
            this.Table = this.Database.Tables[scriptRequest.TableName];
            this.ColumnsToIgnore = scriptRequest.ColumnsToIgnore;
        }

        private void SetupDatabaseServerAndObjects(IDbConnection dbConnection, SqlServerDataScriptRequest scriptRequest)
        {
            this.SqlConnection = dbConnection as SqlConnection;
            this.ServerConnection = new ServerConnection(this.SqlConnection);
            this.Server = new Server(this.ServerConnection);
            this.Database = this.Server.Databases[this.SqlConnection.Database];
            this.Table = this.Database.Tables[scriptRequest.TableName];
            this.ColumnsToIgnore = scriptRequest.ColumnsToIgnore;
        }

        private void SetupDataTable(SqlServerDataScriptRequest request)
        {
            var tableName = request.TableName;
            string whereClause = request.WhereClause;
            string sqlScript = string.Empty;

            if (string.IsNullOrEmpty(request.WhereClause))
            {
                sqlScript = $"SELECT * FROM {tableName}";
            }
            else
            {
                sqlScript = $"SELECT * FROM {tableName} WHERE {whereClause}";
            }

            // execute the select query on the database and get the rows from the table.
            this.DataTable = this.GetDataFromTable(this.SqlConnection, sqlScript);
        }

        private void SetupColumns()
        {
            var columns = this.Table.Columns;
            foreach (Column column in columns)
            {
                this.ColumnList.Add(column);
                if (column.InPrimaryKey)
                {
                    this.PkColumnList.Add(column);
                }
            }

            // Check if we have any columns to be ignored
            if (this.ColumnsToIgnore != null && this.ColumnsToIgnore.Any())
            {
                this.ColumnList.ToList().ForEach(a =>
                {
                    if (this.ColumnsToIgnore.Any(x => x == a.Name))
                    {
                        this.ColumnList.Remove(a);
                    }
                });
            }
        }

        private void BuildScript(SqlServerDataScriptRequest scriptRequest)
        {
            // Build Script
            if (scriptRequest.RequestType == RequestType.INSERT)
            {
                this.BuildInsertScript(this.ScriptBuilder, scriptRequest.TableName, this.DataTable, this.PkColumnList, this.ColumnList);
            }
            else if (scriptRequest.RequestType == RequestType.UPDATE)
            {
                this.BuildUpdateScript(this.ScriptBuilder, scriptRequest.TableName, this.DataTable, this.PkColumnList, this.ColumnList);
            }
            else if (scriptRequest.RequestType == RequestType.DELETE)
            {
                this.BuildDeleteScript(this.ScriptBuilder, scriptRequest.TableName, this.DataTable, this.PkColumnList, this.ColumnList);
            }
        }

        private DataTable GetDataFromTable(SqlConnection sqlConnection, string query)
        {
            DataTable dataTable = new DataTable();
            using (sqlConnection)
            {
                if (sqlConnection.State == ConnectionState.Closed)
                {
                    sqlConnection.Open();
                }

                using (SqlCommand cmd = sqlConnection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;

                    var reader = cmd.ExecuteReader();
                    dataTable.Load(reader);
                }
            }

            return dataTable;
        }

        private string GetColumnValue(Column column, DataRow row)
        {
            string columnValue = string.Empty;
            if (row[column.Name] == DBNull.Value)
            {
                columnValue = "NULL";
                return columnValue;
            }

            if (column.DataType.SqlDataType == SqlDataType.Bit)
            {
                var value = Convert.ToBoolean(row[column.Name]);
                columnValue = value ? "1" : "0";
                return columnValue;
            }

            bool addQuotes = this.IsQuoteRequired(column);

            if (addQuotes)
            {
                columnValue = "'" + row[column.Name].ToString() + "'";
            }
            else
            {
                columnValue = row[column.Name].ToString();
            }

            if (!string.IsNullOrEmpty(columnValue) && (column.DataType.SqlDataType == SqlDataType.Date || column.DataType.SqlDataType == SqlDataType.DateTime))
            {
                string strDateValue = Convert.ToDateTime(row[column.Name]).ToString("yyyy-MM-dd HH:mm:ss.fff");

                columnValue = "'" + strDateValue + "'";
            }

            return columnValue;
        }

        private void BuildInsertScript(StringBuilder scriptBuilder, string tableName, DataTable dataTable, List<Column> pkColumnList, List<Column> columnList)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                bool isFirstColumn = true;
                if (pkColumnList.Any(a => a.InPrimaryKey && a.Identity))
                {
                    // No need to have any IF NOT EXISTS condition since AUTO INCREMENT PK column(s)
                }
                else
                {
                    foreach (var column in pkColumnList)
                    {
                        scriptBuilder.Append($"IF NOT EXISTS(SELECT 1 FROM [dbo].[{tableName}] WHERE ");
                        if (isFirstColumn)
                        {
                            isFirstColumn = false;
                            scriptBuilder.Append($"{column} = {this.GetColumnValue(column, row)}");
                        }
                        else
                        {
                            scriptBuilder.Append($" AND {column} = {this.GetColumnValue(column, row)}");
                        }
                    }

                    scriptBuilder.Append(")");
                }

                scriptBuilder.Append($" INSERT INTO [dbo].[{tableName}](");

                var columnListExcludingIdentityColumns = columnList.Where(a => !a.Identity);
                int columnCount = columnListExcludingIdentityColumns.Count();
                int columnIterationCount = 0;
                bool isFirst = true;
                foreach (Column item in columnListExcludingIdentityColumns)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        scriptBuilder.Append($"[{item.Name}]");
                    }
                    else
                    {
                        if (columnIterationCount != (columnCount - 1))
                        {
                            scriptBuilder.Append($",[{item.Name}]");
                        }
                        else
                        {
                            scriptBuilder.Append($",[{item.Name}]");
                        }
                    }

                    columnIterationCount++;
                }

                scriptBuilder.Append(")");

                scriptBuilder.Append($" VALUES(");

                int valueCount = 0;
                bool isFirstValue = true;
                foreach (Column item in columnListExcludingIdentityColumns)
                {
                    if (isFirstValue)
                    {
                        isFirstValue = false;
                        scriptBuilder.Append($"{this.GetColumnValue(item, row)}");
                    }
                    else
                    {
                        if (valueCount != (columnCount - 1))
                        {
                            scriptBuilder.Append($",{this.GetColumnValue(item, row)}");
                        }
                        else
                        {
                            scriptBuilder.Append($",{this.GetColumnValue(item, row)}");
                        }
                    }

                    valueCount++;
                }

                scriptBuilder.Append(")");

                scriptBuilder.AppendLine();
            }
        }

        private void BuildUpdateScript(StringBuilder scriptBuilder, string tableName, DataTable dataTable, List<Column> pkColumnList, List<Column> columnList)
        {
            bool isFirstColumn = true;
            foreach (DataRow row in dataTable.Rows)
            {
                scriptBuilder.Append($"UPDATE [dbo].[{tableName}] SET ");
                foreach (Column column in columnList)
                {
                    if (isFirstColumn)
                    {
                        isFirstColumn = false;
                        scriptBuilder.Append($"[{column.Name}] = {this.GetColumnValue(column, row)}");
                    }
                    else
                    {
                        scriptBuilder.Append($",[{column.Name}] = {this.GetColumnValue(column, row)}");
                    }
                }

                scriptBuilder.Append(" WHERE ");
                isFirstColumn = true;
                foreach (Column column in pkColumnList)
                {
                    if (isFirstColumn)
                    {
                        isFirstColumn = false;
                        scriptBuilder.Append($"[{column.Name}] = {this.GetColumnValue(column, row)}");
                    }
                    else
                    {
                        scriptBuilder.Append($" AND [{column.Name}] = {this.GetColumnValue(column, row)}");
                    }
                }

                isFirstColumn = true;
                scriptBuilder.AppendLine();
            }
        }

        private void BuildDeleteScript(StringBuilder scriptBuilder, string tableName, DataTable dataTable, List<Column> pkColumnList, List<Column> columnList)
        {
            bool isFirstColumn = true;
            foreach (DataRow row in dataTable.Rows)
            {
                scriptBuilder.Append($"DELETE FROM [dbo].[{tableName}] WHERE");
                foreach (Column column in pkColumnList)
                {
                    if (isFirstColumn)
                    {
                        isFirstColumn = false;
                        scriptBuilder.Append($" [{column.Name}] = {this.GetColumnValue(column, row)}");
                    }
                    else
                    {
                        scriptBuilder.Append($" AND [{column.Name}] = {this.GetColumnValue(column, row)}");
                    }
                }

                scriptBuilder.AppendLine();
            }
        }

        private bool IsQuoteRequired(Column column)
        {
            switch (column.DataType.SqlDataType)
            {
                case SqlDataType.VarChar:
                case SqlDataType.VarCharMax:
                case SqlDataType.NText:
                case SqlDataType.NVarCharMax:
                case SqlDataType.NVarChar:
                case SqlDataType.Char:
                case SqlDataType.NChar:
                case SqlDataType.SmallDateTime:
                case SqlDataType.Text:
                case SqlDataType.UniqueIdentifier:
                case SqlDataType.VarBinary:
                case SqlDataType.VarBinaryMax:
                case SqlDataType.Date:
                case SqlDataType.DateTime:
                case SqlDataType.DateTime2:
                case SqlDataType.DateTimeOffset:
                    return true;
                default:
                    return false;
            }
        }
    }
}
