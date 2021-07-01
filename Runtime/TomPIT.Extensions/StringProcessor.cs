using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TomPIT
{
    public class StringProcessor
    {
        private enum ReplacementType
        {
            Link = 1,
            Image = 2,
            Span = 3
        }

        private static readonly Regex Expression = new Regex(@"\[.:\[(.*?)\]\]|\[.:(.*?)\]");

        private const char LinkCharacter = 'L';
        private const char ImageCharacter = 'I';
        private const char SpanCharacter = 'S';

        public StringProcessor(string value, params IEnumerable<(string property, object value)>[] replacements)
        {
            Value = value;

            Replacements = new Queue<IEnumerable<(string property, object value)>>();

            foreach (var replacement in replacements)
                Replacements.Enqueue(replacement);

            ProcessValue();
        }


        private string Value { get; }
        private Queue<IEnumerable<(string property, object value)>> Replacements { get; }
        public string Result { get; private set; }

        public static string Process(string value, params IEnumerable<(string property, object value)>[] replacements)
        {
            return new StringProcessor(value, replacements).Result;
        }

        private void ProcessValue()
        {
            if (string.IsNullOrWhiteSpace(Value) || Replacements.Count == 0)
            {
                Result = Value;
                return;
            }

            Result = Expression.Replace(Value, new MatchEvaluator(OnReplace));
        }

        private string OnReplace(Match match)
        {
            var value = match.Value[1..^1];

            if (value.Length < 3)
                return match.Value;

            var tokens = value.Split(':', 2);

            if (string.Compare(tokens[0], LinkCharacter.ToString(), true) == 0)
                return OnReplace(ReplacementType.Link, tokens[1]);
            else if (string.Compare(tokens[0], ImageCharacter.ToString(), true) == 0)
                return OnReplace(ReplacementType.Image, tokens[1]);
            else if (string.Compare(tokens[0], SpanCharacter.ToString(), true) == 0)
                return OnReplace(ReplacementType.Span, tokens[1]);

            return match.Value;
        }

        private string OnReplace(ReplacementType type, string value)
        {
            if (Replacements.Count == 0)
                return value;

            var replacements = Replacements.Dequeue();

            Func<object, string, string> getValue = (object propertyValue, string replacementValue) => 
            {
                if (propertyValue is string str && str.Equals("{.}"))
                {
                    return replacementValue;
                }
                else
                {
                    return propertyValue.ToString();
                }
            };

            var properties = replacements.Select(e => GenerateProperty((e.property, getValue(e.value, value)))).ToArray();

            return type switch
            {
                ReplacementType.Link => OnReplaceLink(value, properties),
                ReplacementType.Image => OnReplaceImage(value, properties),
                ReplacementType.Span => OnReplaceSpan(value, properties),
                _ => throw new NotSupportedException(),
            };
        }

        private string OnReplaceLink(string value, string[] properties)
        {
            return $"<a {string.Join(" ", properties)}>{value}</a>";
        }

        private string OnReplaceImage(string value, string[] properties)
        {
            return $"<img {string.Join(" ", properties)} alt=\"{value}\"/>";
        }

        private string OnReplaceSpan(string value, string[] properties)
        {
            return $"<span {string.Join(" ", properties)}>{value}</span>";
        }

        private string GenerateProperty((string property, object value) parameter)
        {
            return $"{parameter.property}=\"{parameter.value}\"";
        }
    }
}
