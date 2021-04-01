using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Events;

public class LSystem : MonoBehaviour
{

    public string Axiom = "";


    [Serializable]
    public struct Rule
    {
        public string Key;
        public string Value;
    }
    public List<Rule> Rules = new List<Rule>();
    public List<Rule> Interpretations = new List<Rule>();

    public int Iterations = 3;

    public Action<List<string>> ConsumeGenerated = (List<string> xs) => {};
    private void Start()
    {
        var generated = RuleHelper.Lex(Axiom);

        for (int i = 0; i < Iterations; i++)
        {
            var rulesD = Rules.ToDictionary(r => r.Key, r => RuleHelper.Lex(r.Value));
            generated = RuleHelper.ApplyRules(generated, rulesD);
        }
        var interpretationsD = Interpretations.ToDictionary(r => r.Key, r => RuleHelper.Lex(r.Value));
        generated = RuleHelper.ApplyRules(generated, interpretationsD);

        ConsumeGenerated(generated);
    }

}

