using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PersistentWorlds.Utils
{
    public static class DebugExtensions
    {
        public static string ToDebugString<TK, TV>(this Dictionary<TK, TV> dict)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{0} (keytype={1}, valuetype={2})", nameof(dict), nameof(TK), nameof(TV));
            sb.Append("{");
            
            for (var i = 0; i < dict.Count; i++)
            {
                var pair = dict.ElementAt(i);
                
                var key = pair.Key;
                var value = pair.Value;

                sb.AppendFormat("[key={0}, value={1}]", key.ToString(), value.ToString());
                
                if (i != dict.Count - 1)
                {
                    // Comma.
                    sb.Append(", ");
                }
            }

            sb.Append("}");
            
            return sb.ToString();
        }

        public static string ToDebugString<TE>(this List<TE> list)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{0} (type={1})", nameof(list), nameof(TE));
            sb.Append("{");

            for (var i = 0; i < list.Count; i++)
            {
                var element = list[i];

                sb.AppendFormat("[index={0}, element={1}]", i, element.ToString());

                if (i != list.Count - 1)
                {
                    sb.Append(", ");
                }
            }
            
            sb.Append("}");
            
            return sb.ToString();
        }

        public static string ToDebugString<TE>(this HashSet<TE> set)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{0} (type={1})", nameof(set), nameof(TE));
            sb.Append("{");

            var i = 0;
            
            foreach (var element in set)
            {
                sb.AppendFormat("[element={0}]", element.ToString());

                if (i != set.Count - 1)
                {
                    sb.Append(", ");
                }
                
                i++;
            }
            
            sb.Append("}");
            
            return sb.ToString();
        }
    }
}