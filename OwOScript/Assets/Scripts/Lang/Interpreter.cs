using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using Mathos.Parser;
using System;

public class Interpreter : MonoBehaviour
{
    CultureInfo ci = CultureInfo.InvariantCulture;
    public Dictionary<string, dynamic> variables;
    private string code;
    private string[] lines;
    readonly string fileName = @"Assets\Scripts\Lang\main.owo";
	MathParser Math = new MathParser();

    void Start()
    {
        variables = new Dictionary<string, dynamic>();
        code = File.ReadAllText(fileName);
        lines = SeparateLines(code);
        foreach (var line in lines)
        {
            ParseLine(line);
        }

        // Output each key and value in the variables dictionary
        foreach (var item in variables)
        {
            Debug.Log(item);
        }
    }

    void PRINT(string text)
    {
        Debug.Log(text);
    }

    string[] SeparateLines(string _code)
    {
        string[] _lines = Regex.Split(_code,";");
        for (int i = 0; i < _lines.Length; i++)
        {
            // Remove all line breaks
            _lines[i] = _lines[i].Replace(System.Environment.NewLine, string.Empty);
        }
        return _lines;
    }

    void ParseLine(string _line)
    {
        // Variable assignment
        if (_line.Contains("="))
        {
            string _key;
            string _value;
            string[] _split = Regex.Split(_line,"=");
            _key = _split[0].Trim(' ', '\t');
            _value = _split[1].Trim(' ', '\t');
			ParseVariable(_key, _value);
        }
        // Statements (If/Methods/Loops)
        else if (_line.Contains("(") && _line.Contains(")"))
        {
            string _func;
            string _args;
            string[] _split = _line.Split('(');
            _func = _split[0];
            _args = _split[1];
            _args = _args.Replace(")", string.Empty);
            Debug.Log(float.Parse(_args));
        }
    }
	//this parses mathematical operations and variable value uses 
	bool ParseNumbers(string _expression, out double results)
	{
		Dictionary<string, dynamic> vars = new Dictionary<string, dynamic>();
		string _toParse = Regex.Replace(_expression, "(|)", string.Empty);
		string[] _Args = _toParse.Split(new[] {'+','-','/','*'});
		foreach (var item in _Args)
		{
			if(variables.TryGetValue(item,out dynamic value))
			{
				if (vars.ContainsKey(item))
				{
					break;
				}
				else if(variables[item] is double)
				{
					vars[item] = variables[item];
				}
				else
				{
					ThrowError();
					results = 0;
					return false;
				}
			}
		}
		string preParsed=_expression;
		foreach (var item in vars)
		{
			preParsed = Regex.Replace(preParsed, item.Key, item.Value.ToString());
		}
		preParsed = Regex.Replace(preParsed, ",", ".");
		double parsed = Math.ProgrammaticallyParse(preParsed);
		results = parsed;
		return true;
	}

    void ParseVariable(string _key, string _value)
    {
        // TODO: Fix this bug - `pleasefix = "this""should""not""work";` - As the variable's value suggests, this should not work.

        // Throw an error if the variable name has spaces in it
        if (_key.Contains(" "))
        {
            ThrowError();
            return;
        }

        dynamic parsedValue;

        // Strings
        if (_value[0] == '"' && _value[_value.Length - 1] == '"')
        {
            _value = _value.Substring(1, _value.Length - 2);
            parsedValue = _value;
        }
        // Numbers
        else if (Regex.IsMatch(_value, "1|2|3|4|5|6|7|8|9|0"))
        {
            if(ParseNumbers(_value,out double results))
			{
				parsedValue = results;
			}
			else
			{
				parsedValue = null;
			}
        }
        // Booleans (True/False)
        else if (bool.TryParse(_value, out bool _resultBool))
        {
            parsedValue = _resultBool;
        }
        else if (_value.ToLower() == "None")
        {
            parsedValue = null;
        }
        // TODO: Assigning methods to variables (now relocated to ParseExpression)
        // Otherwise - Handle errors
        else
        {
            ThrowError();
            return;
        }

		// Add the variable to the variables dictionary. If the variable does not already exist it will add it automatically.
		AddVariable(_key, parsedValue);
    }
	void AddVariable(string _key, dynamic _var)
	{
		variables[_key] = _var;
	}

    void ThrowError()
    {
        // TODO: Error handling

    }
}