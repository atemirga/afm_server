using System;

namespace JQuery.DataTables.Extensions
{
    public class DataTableColumnAttribute : Attribute
    {
        public bool Visible { get; set; } = true;

        public string ClassName { get; set; }

        public string Title { get; set; }

        public string Width { get; set; }

        public string Render { get; set; }

    }
}