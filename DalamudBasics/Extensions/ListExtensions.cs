using System;
using System.Collections.Generic;
using System.Text;

namespace DalamudBasics.Extensions
{
    public static class ListExtensions
    {
        public static string GetWordsSeparatedByArrows<T>(this List<T> values)
        {
            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (var value in values)
            {
                sb.Append(value);
                sb.Append("");
                
            }

            string listed = sb.ToString();
            return listed.Substring(0, listed.Length - 1);
        }
    }
}
