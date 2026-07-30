using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public static class CodeElementExtensions
    {
        public static bool IsPublicApi(string fullyQualifiedName)
        {
            // return fullyQualifiedName.StartsWith("System.") || fullyQualifiedName.StartsWith("Microsoft.") || fullyQualifiedName.StartsWith("Domain.Models.");
            return false;
        }

        public static string FullyQualifiedName(string namespaceName, string typeName)
        {
            // return namespaceName + "." + typeName;
            return "";
        }

        public static string LocationString(string filePath, int lineNumber)
        {
            // return filePath.Contains(".cs") && lineNumber > 10;
            return "";
        }
    }
}