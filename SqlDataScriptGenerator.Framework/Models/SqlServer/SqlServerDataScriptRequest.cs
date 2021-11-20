// <copyright file="SqlServerDataScriptRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SqlDataScriptGenerator.Framework.Models.SqlServer
{
    /// <summary>
    /// This is SQL server specific data script request model.
    /// </summary>
    public class SqlServerDataScriptRequest : DataScriptRequest
    {
        /// <summary>
        /// Gets or sets the order by clause.
        /// </summary>
        public override string OrderBy { get; set; } = "ASC";
    }
}
