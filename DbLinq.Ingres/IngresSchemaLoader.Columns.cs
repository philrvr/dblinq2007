﻿#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
//        Thomas Glaser
////////////////////////////////////////////////////////////////////
#endregion

using System.Collections.Generic;
using System.Data;
using DbLinq.Util;
using DbLinq.Vendor;

namespace DbLinq.Ingres
{
    partial class IngresSchemaLoader
    {
        protected virtual string GetFullType(IDataTableColumn column)
        {
            switch (column.Type.ToLower())
            {
            case "c":
            case "char":
            case "nchar":
            case "varchar":
            case "nvarchar":
            case "long varchar":
            case "text":
            case "integer":
                return column.Type + "(" + column.Length + ")";

            case "decimal":
                return column.Type + "(" + column.Length + ", " + column.Scale + ")";

            default:
                return column.Type;
            }
        }

        protected virtual IDataTableColumn ReadColumn(IDataRecord rdr)
        {
            var t = new DataTableColumn();
            int field = 0;
            t.TableSchema = rdr.GetAsString(field++).Trim();
            t.TableName = rdr.GetAsString(field++).Trim();
            t.ColumnName = rdr.GetAsString(field++).Trim();
            string nullableStr = rdr.GetAsString(field++);
            t.Nullable = nullableStr == "Y";
            t.Type = rdr.GetAsString(field++).Trim();
            t.DefaultValue = rdr.GetAsString(field++);
            t.Length = rdr.GetAsNullableNumeric<long>(field++);
            t.Scale = rdr.GetAsNullableNumeric<int>(field++);
            t.FullType = GetFullType(t);
            return t;
        }

        public override IList<IDataTableColumn> ReadColumns(IDbConnection connectionString, string databaseName)
        {
            string sql = @"
SELECT t.table_owner, t.table_name, column_name
    ,column_nulls, column_datatype, column_default_val
    ,column_length, column_scale
FROM iicolumns c join iitables t on (c.table_name=t.table_name and c.table_owner=t.table_owner) 
            WHERE t.table_owner <> '$ingres' and t.table_type in ('T', 'V')
ORDER BY column_sequence
";

            return DataCommand.Find<IDataTableColumn>(connectionString, sql, ReadColumn);
        }
    }
}