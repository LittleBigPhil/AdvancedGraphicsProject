using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RuleHelper 
{
    public static List<string> Lex(string str)
    {
        return str.Split(' ').Where(s => s.Length > 0).ToList();
    }

    public static List<string> ApplyRules(List<string> generated, Dictionary<string, List<string>> dict)
    {
        return generated.SelectMany((string s) => {
            var hasRule = dict.TryGetValue(s, out var replaceTo);
            if (hasRule)
            {
                return replaceTo;
            } else
            {
                return new List<string>(){s};
            }
        }).ToList();
    }
}
