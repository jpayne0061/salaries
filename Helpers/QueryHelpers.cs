using SalaryExplorer.Models;
using System.Reflection;

namespace SalaryExplorer.Helpers
{
  public static class QueryHelpers
  {
    public static string LeadingOperator(string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return "";
      }

      switch (value[0])
      {
        case '<':
          return "<";
        case '*':
          return "LIKE %";
        case '>':
          return ">";
      }

      return "";
    }


    public static string TrailingOperator(string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return "";
      }

      switch (value[value.Length - 1])
      {
        case '*':
          return "%";
      }

      return "";
    }

    public static string GetClause(string propName, string value)
    {
      string leadingOperator = LeadingOperator(value);
      string trailingOperator = TrailingOperator(value);

      value = string.IsNullOrEmpty(leadingOperator) ? value : value.Remove(0, 1);
      value = string.IsNullOrEmpty(trailingOperator) ? value : value.Substring(0, (value.Length - 1));

      if (trailingOperator == "%")
      {
        leadingOperator = " LIKE ";
      }
      else if (string.IsNullOrEmpty(leadingOperator))
      {
        leadingOperator = "=";
      }

      string clause = propName + " " + leadingOperator + " '" + value + "' " + trailingOperator;

      clause = clause.Replace("% '", "'%");
      clause = clause.Replace("' %", "%'");

      return clause;
    }

    public static string BuildQuery(RecordInput record)
    {
      string query = "select top 100";

      PropertyInfo[] properties = record.GetType().GetProperties();

      for (int i = 0; i < properties.Length; i++)
      {
        string propName = properties[i].Name;
        query += i == 0 ? " " + propName : "," + propName;
      }

      query += " from dbo.SalaryData ";

      for (int i = 0; i < properties.Length; i++)
      {
        object val = properties[i].GetValue(record);
        if (val == null || string.IsNullOrEmpty(val.ToString()))
        {
          continue;
        }

        string propName = properties[i].Name;

        string clause = QueryHelpers.GetClause(propName, (string)val);

        query += query.Contains("where") ? " AND " + clause : "where " + clause;
      }

      return query;
    }

  }
}
