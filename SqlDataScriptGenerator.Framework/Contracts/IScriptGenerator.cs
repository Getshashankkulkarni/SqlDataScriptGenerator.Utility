// <copyright file="IScriptGenerator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SqlDataScriptGenerator.Framework.Contracts
{
    using System.Data;
    using SqlDataScriptGenerator.Framework.Models;

    /// <summary>
    /// Defines a contract for script generation.
    /// </summary>
    public interface IScriptGenerator
    {
        /// <summary>
        /// Method that returns the generated script in string.
        /// </summary>
        /// <param name="request">An instance of DataScriptRequest containing the request parameters.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>Returns the generated script in string.</returns>
        public string GetSqlDataScript(DataScriptRequest request, string connectionString);

        /// <summary>
        /// Method that returns the generated script in string.
        /// </summary>
        /// <param name="request">An instance of DataScriptRequest containing the request parameters.</param>
        /// <param name="dbConnection">An instance of IDbConnection.</param>
        /// <returns>Returns the generated script in string.</returns>
        public string GetSqlDataScript(DataScriptRequest request, IDbConnection dbConnection);
    }
}
