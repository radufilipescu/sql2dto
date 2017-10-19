using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.QueryBuilder
{
    internal class QueryValidation<TQueryImpl>
        where TQueryImpl : Query<TQueryImpl>
    {
        internal static string JoinPath(TQueryImpl query, params string[] path)
        {
            var sb = new StringBuilder();
            bool isFirstPart = false;
            foreach (string p in path)
            {
                if (p.Contains(".") || p.Contains("@") || p.Contains(":"))
                {
                    throw new InvalidOperationException();
                }

                string part = p
                    .Replace(".", "")
                    .Replace("@", "")
                    .Replace(":", "")
                    .Replace("\'", "");

                if (!isFirstPart)
                {
                    isFirstPart = true;
                }
                else
                {
                    sb.Append(".");
                }

                if (path.Length > 1)
                {
                    sb.Append($"[{part}]");
                }
                else
                {
                    sb.Append($"@{part}");
                }
            }
            return sb.ToString();
        }
    }
}
