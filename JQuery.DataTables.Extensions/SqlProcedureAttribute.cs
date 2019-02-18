using System;

namespace JQuery.DataTables.Extensions
{
    public class SqlProcedureAttribute : Attribute
    {
        public SqlProcedureAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}