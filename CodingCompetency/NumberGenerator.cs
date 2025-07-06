using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodingCompetency
{
    public class NumberGenerator
    {
        private readonly Dictionary<int, string> rules = new Dictionary<int, string>();
        
        public void AddRule(int input, string output)
        {
            rules[input] = output;
        }
        
        public string Generate(int number)
        {
            // Check if there's an exact match first (highest priority)
            if (rules.ContainsKey(number))
            {
                return rules[number];
            }
            
            var result = new StringBuilder();
            
            // Check each rule in ascending order for divisibility
            foreach (var rule in rules.OrderBy(r => r.Key))
            {
                if (number % rule.Key == 0)
                {
                    result.Append(rule.Value);
                }
            }
            
            return result.Length > 0 ? result.ToString() : number.ToString();
        }
        
        public void PrintSequence(int n)
        {
            var output = new List<string>();
            for (int i = 1; i <= n; i++)
            {
                output.Add(Generate(i));
            }
            Console.WriteLine(">> " + string.Join(", ", output));
        }
    }
}