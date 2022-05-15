using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WikipediaLinkSolver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string firstDefinition = "Map-Reduce";
            string searchedDefinition = "High-performance_computing";
            int maxDepth = 3;
            HttpClient client = new HttpClient();

            

            Regex regex = new Regex($"href=\"(.*?)\"");
            HashSet<string> links = new HashSet<string>();
            links.Add(firstDefinition);

            (string val, int depth, List<string> path) found = default;

            Queue<(string val, int depth, List<string> path)> queue = new Queue<(string val, int depth, List<string> path)>();
            queue.Enqueue((firstDefinition, 0, new List<string>() { firstDefinition }));
            while (found == default && queue.Any())
            {
                var curr = queue.Dequeue();

                Console.WriteLine($"Working on {curr.val} in depth {curr.depth}");

                if (curr.depth == maxDepth)
                {
                    continue;
                }

                var requestUri = $"https://en.wikipedia.org/wiki/{curr.val}";
                var res = await client.GetAsync(requestUri);
                var strValue = await res.Content.ReadAsStringAsync();

                foreach (Match match in regex.Matches(strValue))
                {
                    var s = match.Groups[1].ToString();


                    if (!s.StartsWith("/wiki/") || s.Contains('.') || s.Contains(':')) // avoid pics
                    {
                        continue;
                        
                    }

                    s = s.Replace("/wiki/", "");

                    
                    if (s == searchedDefinition)
                    {
                        curr.path.Add(s);
                        found = curr;
                        break;
                    }

                    var list = new List<string>(curr.path);
                    list.Add(s);
                    queue.Enqueue((s, curr.depth+1, list));
                }
            } ;



            Console.WriteLine($"Result is {found != default}");

            if (found != default)
            {
                Console.WriteLine($"The path is {string.Join('-', found.path)}");
            }
        }
    }
}
