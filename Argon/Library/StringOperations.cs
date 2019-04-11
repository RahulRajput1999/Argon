using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Argon.Library
{
    class StringOperations
    {
        public static string TrimSearchString(string type, string originalValue)
        {
            int index = originalValue.IndexOf(type);
            string result = (index < 0)
                ? originalValue
                : originalValue.Remove(index, type.Length);
            return result;
        }
    }
}
