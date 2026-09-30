using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RO3.ThaiLocalization
{
    public sealed class SkillDictionary
    {
        private sealed class Row { public string English = ""; public string Thai = ""; }
        private sealed class Rule { public Regex Pattern = null!; public string Replacement = ""; }
        private readonly Dictionary<string, Row> byId = new Dictionary<string, Row>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> exact = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<Rule> rules = new List<Rule>();
        private readonly Dictionary<string, string> cache = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly object sync = new object();
        public int Count { get { return byId.Count; } }
        public int RuleCount { get { return rules.Count; } }
        private static string Decode(string value) { return value.Replace(@"\n", "\n").Replace(@"\r", "\r").Replace(@"\t", "\t"); }
        private static bool IsSkillId(string id) { return id.StartsWith("101103", StringComparison.Ordinal) || id.StartsWith("102203", StringComparison.Ordinal) || id.StartsWith("108001", StringComparison.Ordinal); }
        public static SkillDictionary Load(string table, string ruleFile)
        {
            var result = new SkillDictionary();
            foreach (string line in File.ReadAllLines(table))
            {
                if (line.StartsWith("#", StringComparison.Ordinal) || String.IsNullOrWhiteSpace(line)) continue;
                string[] cells = line.Split(new[] { '\t' }, 3);
                if (cells.Length != 3 || !IsSkillId(cells[0]) || cells[1].Length == 0 || cells[2].Length == 0)
                    throw new InvalidDataException("Invalid/unsupported skill row");
                if (result.byId.ContainsKey(cells[0])) throw new InvalidDataException("Duplicate skill ID");
                result.byId.Add(cells[0], new Row { English = cells[1], Thai = cells[2] });
                string existing;
                if (result.exact.TryGetValue(cells[1], out existing!) && existing != cells[2]) throw new InvalidDataException("Conflicting exact translations");
                result.exact[cells[1]] = cells[2];
                result.exact[Decode(cells[1])] = Decode(cells[2]);
            }
            foreach (string line in File.ReadAllLines(ruleFile))
            {
                if (line.StartsWith("#", StringComparison.Ordinal) || String.IsNullOrWhiteSpace(line)) continue;
                string[] cells = line.Split(new[] { '\t' }, 3);
                if (cells.Length != 3 || !result.byId.ContainsKey(cells[0])) throw new InvalidDataException("Rule not associated with approved skill");
                if (!cells[1].StartsWith(@"\A", StringComparison.Ordinal) || !cells[1].EndsWith(@"\z", StringComparison.Ordinal)) throw new InvalidDataException("Unanchored skill rule");
                result.rules.Add(new Rule { Pattern = new Regex(cells[1], RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(50)), Replacement = Decode(cells[2]) });
            }
            return result;
        }
        public bool TryId(string id, string original, out string translated)
        {
            translated = original;
            Row row;
            if (!byId.TryGetValue(id, out row!)) return false;
            // Refuse reused/mismatched IDs in a newer game build; never guess.
            if (original == row.English) { translated = row.Thai; return true; }
            if (original == Decode(row.English)) { translated = Decode(row.Thai); return true; }
            return false;
        }
        public string Translate(string input)
        {
            if (String.IsNullOrEmpty(input) || input.Length > 6000) return input;
            lock (sync)
            {
                string output;
                if (exact.TryGetValue(input, out output!)) return output;
                if (cache.TryGetValue(input, out output!)) return output;
                // Do not apply numeric template rules to names, small UI labels or Thai text.
                if (input.Length < 8 || Regex.IsMatch(input, @"[\u0E00-\u0E7F]")) return input;
                foreach (var rule in rules)
                {
                    try
                    {
                        Match match = rule.Pattern.Match(input);
                        if (!match.Success) continue;
                        output = match.Result(rule.Replacement);
                        if (cache.Count >= 2048) cache.Clear();
                        cache[input] = output;
                        return output;
                    }
                    catch (RegexMatchTimeoutException) { /* Keep the English input rather than stall the game. */ }
                }
                return input;
            }
        }
    }
}
