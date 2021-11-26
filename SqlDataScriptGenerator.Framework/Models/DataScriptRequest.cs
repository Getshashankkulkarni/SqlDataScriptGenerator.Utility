// <copyright file="DataScriptRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SqlDataScriptGenerator.Framework.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Model containing properties that are part of a data script request.
    /// </summary>
    public abstract class DataScriptRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataScriptRequest"/> class.
        /// </summary>
        public DataScriptRequest()
        {
            this.ColumnsToIgnore = new List<string>();
        }

        /// <summary>
        /// Gets or sets a flag indicating the type of script. I = Insert, U = Update, D = Delete.
        /// </summary>
        public RequestType RequestType { get; set; } = RequestType.INSERT;

        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the WHERE clause.
        /// </summary>
        public string WhereClause { get; set; }

        /// <summary>
        /// Gets or sets the order by clause.
        /// </summary>
        public virtual string OrderBy { get; set; }

        /// <summary>
        /// Gets or sets a collection of columns to be ignored in the data script.
        /// </summary>
        public IEnumerable<string> ColumnsToIgnore { get; set; }
    }
}
