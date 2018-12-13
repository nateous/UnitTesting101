using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Awesome
{
    public class MyService
    {
        public Dictionary<string, string> GetMergeFields(params string[] templates)
        {
            Regex findMergeFields = new Regex(@"\{\{[^\}]+\}\}", RegexOptions.Compiled);
            return templates?
                .Where(t => t != null)
                .SelectMany(template => findMergeFields.Matches(template).Cast<Match>())
                .Select(m => m.Value)
                .ToDictionary(m => m)
                ?? new Dictionary<string, string>();
        }
    }
}
