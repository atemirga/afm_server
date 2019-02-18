using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JQuery.DataTables.Extensions
{
    public static class DataTablesExtensions
    {
        public static IQueryable<T> ExecDataTableProcedure<T>(this DbContext dbContext, object filterModel) where T : class
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            if (filterModel is DataTablesFilterModel dataTablesFilterModel)
            {
                parameters.AddSqlParameter(nameof(dataTablesFilterModel.DataTablesRequest.Start), dataTablesFilterModel.IsReport ? 0 : dataTablesFilterModel.DataTablesRequest.Start);
                parameters.AddSqlParameter(nameof(dataTablesFilterModel.DataTablesRequest.Length), dataTablesFilterModel.IsReport ? int.MaxValue : dataTablesFilterModel.DataTablesRequest.Length);
                parameters.AddSqlParameter(nameof(dataTablesFilterModel.DataTablesRequest.Search), dataTablesFilterModel.DataTablesRequest.Search.Value);
            }

            var dynamicParameters = filterModel.GetType().GetProperties().Where(p => !p.IsDefined(typeof(IgnoreSqlParamAttribute)));

            foreach (var p in dynamicParameters)
            {
                parameters.AddSqlParameter(p.Name, p.GetValue(filterModel));
            }

            var sqlProc = filterModel.GetType().GetTypeInfo().GetCustomAttribute<SqlProcedureAttribute>();

            if (sqlProc == null)
            {
                throw new ArgumentNullException("SqlProcedureAttribute");
            }

            var sql = $"[{sqlProc.Name}] {string.Join(", ", parameters.Keys.Select((s, i) => $"@{s} = {{{i}}}"))}";
            return dbContext.Set<T>().FromSql(sql, parameters.Values.ToArray()).AsNoTracking();
        }

        public static IHtmlContent DataTableColumns<TModel>(this IHtmlHelper<TModel> html, Type tableDataType, IJsonHelper json, IStringLocalizer localizer)
        {
            var columns = new List<JObject>();
            var props = tableDataType.GetProperties();

            foreach (var p in props)
            {
                var camelCaseName = p.Name.ToCamelCase();

                var jColumn = new JObject();
                jColumn["name"] = camelCaseName;
                jColumn.Add("data", camelCaseName); 

                var columnAttr = p.GetCustomAttribute<DataTableColumnAttribute>();
                var displayNameAttr = p.GetCustomAttribute<DisplayNameAttribute>();
                var displayAttr = p.GetCustomAttribute<DisplayAttribute>();
                if (displayNameAttr != null)
                {
                    jColumn.Add("title", localizer[displayNameAttr.DisplayName].ToString());
                }
                else if (displayAttr != null)
                {
                    jColumn.Add("title", localizer[displayAttr.Name].ToString());
                }
                else
                {
                    jColumn.Add("title", localizer[p.Name].ToString());
                }

                if (columnAttr != null)
                {
                    //if (!string.IsNullOrEmpty(columnAttr.ClassName))
                    //{
                    //    jColumn["className"] = columnAttr.ClassName;
                    //}
                    //if (!string.IsNullOrEmpty(columnAttr.Title))
                    //{
                    //    jColumn["title"] = columnAttr.Title;
                    //}
                    //if (!string.IsNullOrEmpty(columnAttr.RenderJsFunction))
                    //{
                    //    jColumn["render"] = columnAttr.RenderJsFunction;
                    //}

                    var colProps = columnAttr.GetType().GetProperties();
                    foreach (var colProp in colProps)
                    {
                        if (colProp.GetIndexParameters().Length == 0)
                        {
                            var colPropVal = colProp.GetValue(columnAttr);
                            if (colPropVal != null)
                            {

                                if (colProp.Name == "TypeId")
                                {
                                    continue;
                                }
                                else if (colProp.Name == "Render")
                                {
                                    jColumn[colProp.Name.ToCamelCase()] = new JRaw(colPropVal);
                                }
                                else
                                {
                                    jColumn[colProp.Name.ToCamelCase()] = JToken.FromObject(colPropVal);
                                }
                            }
                        }
                    }
                }

                columns.Add(jColumn);
            }

            return html.Raw(json.Serialize(columns, 
                new JsonSerializerSettings()
                    {
                        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                    }));
        }

        private static void AddSqlParameter(this Dictionary<string, object> parameters, string name, object value)
        {
            if (value is string str && str == string.Empty)
            {
                value = null;
            }

            parameters.Add(name, value);
        }

        private static string ToCamelCase(this string str)
        {
            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}
