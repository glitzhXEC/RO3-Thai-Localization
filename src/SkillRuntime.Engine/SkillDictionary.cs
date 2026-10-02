using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RO3.ThaiLocalization
{
    public sealed class SkillDictionary
    {
        private sealed class Row { public string English = "", Thai = "", EnglishDecoded = "", ThaiDecoded = "", Cn = "", Tw = "", CnWire = "", TwWire = ""; }
        private sealed class Rule { public Regex Pattern = null!; public string Replacement = ""; }
        private readonly Dictionary<string, Row> byId = new Dictionary<string, Row>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> exact = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> exactOrigins = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<Rule> rules = new List<Rule>();
        private readonly Dictionary<string, string> cache = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly object sync = new object();
        public int Count { get { return byId.Count; } }
        public int RuleCount { get { return rules.Count; } }
        private static string Decode(string value)
        {
            var result = new System.Text.StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\\' && i + 1 < value.Length)
                {
                    char c = value[i + 1];
                    if (c == 'n' || c == 'r' || c == 't' || c == '\\') { result.Append(c == 'n' ? '\n' : c == 'r' ? '\r' : c == 't' ? '\t' : '\\'); i++; continue; }
                }
                result.Append(value[i]);
            }
            return result.ToString();
        }
        public static SkillDictionary Load(string table, string ruleFile, string? originsFile = null)
        {
            return LoadText(File.ReadAllText(table), File.ReadAllText(ruleFile), originsFile == null ? null : File.ReadAllText(originsFile));
        }
        public static SkillDictionary LoadText(string tableText, string rulesText, string? originsText = null)
        {
            if (tableText.Length > 8 * 1024 * 1024 || rulesText.Length > 8 * 1024 * 1024 || (originsText != null && originsText.Length > 8 * 1024 * 1024)) throw new InvalidDataException("Dictionary too large");
            var result = new SkillDictionary();
            var englishOnly = new HashSet<string>(StringComparer.Ordinal);
            foreach (string line in Lines(tableText))
            {
                if (line.StartsWith("#", StringComparison.Ordinal) || String.IsNullOrWhiteSpace(line)) continue;
                string[] cells = line.Split(new[] { '\t' }, 3);
                if (cells.Length != 3 || !Regex.IsMatch(cells[0], @"^\d{4,11}$") || cells[1].Length == 0 || cells[2].Length == 0)
                    throw new InvalidDataException("Invalid/unsupported translation row");
                if (cells[1] != cells[2]) ValidatePair(cells[1], cells[2]);
                else if (cells[1].Length > 10000 || cells[1].IndexOf('\uFFFD') >= 0) throw new InvalidDataException("Invalid English base");
                if (result.byId.Count >= 50000) throw new InvalidDataException("Too many translations");
                if (result.byId.ContainsKey(cells[0])) throw new InvalidDataException("Duplicate skill ID");
                result.byId.Add(cells[0], new Row { English = cells[1], Thai = cells[2], EnglishDecoded = Decode(cells[1]), ThaiDecoded = Decode(cells[2]) });
                if (cells[1] == cells[2]) { englishOnly.Add(cells[1]); englishOnly.Add(Decode(cells[1])); continue; } // English base is ID-only, never a global replacement.
                string existing;
                if (result.exact.TryGetValue(cells[1], out existing!) && existing != cells[2]) throw new InvalidDataException("Conflicting exact translations");
                result.exact[cells[1]] = cells[2];
                result.exact[Decode(cells[1])] = Decode(cells[2]);
            }
            foreach (string text in englishOnly) result.exact.Remove(text);
            if (originsText != null)
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (string line in Lines(originsText))
                {
                    if (String.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal)) continue;
                    var cells = line.Split(new[] { '\t' }, 3); Row row;
                    if (cells.Length != 3 || !result.byId.TryGetValue(cells[0], out row!) || !seen.Add(cells[0]) || cells[1].Length > 15000 || cells[2].Length > 15000)
                        throw new InvalidDataException("Invalid/duplicate language origin row");
                    row.CnWire = cells[1]; row.TwWire = cells[2]; row.Cn = Decode(cells[1]); row.Tw = Decode(cells[2]);
                }
                if (seen.Count != result.Count) throw new InvalidDataException("Incomplete language origin mapping");
                result.BuildExactOriginFallbacks();
            }
            foreach (string line in Lines(rulesText))
            {
                if (line.StartsWith("#", StringComparison.Ordinal) || String.IsNullOrWhiteSpace(line)) continue;
                string[] cells = line.Split(new[] { '\t' }, 3);
                if (cells.Length != 3 || !result.byId.ContainsKey(cells[0])) throw new InvalidDataException("Rule not associated with approved skill");
                if (englishOnly.Contains(result.byId[cells[0]].English)) throw new InvalidDataException("Rule would affect English-only IDs with the same text");
                if (cells[1].Length > 20000 || cells[2].Length > 10000 || result.rules.Count >= 50000) throw new InvalidDataException("Rule limit exceeded");
                if (!cells[1].StartsWith(@"\A", StringComparison.Ordinal) || !cells[1].EndsWith(@"\z", StringComparison.Ordinal)) throw new InvalidDataException("Unanchored skill rule");
                result.rules.Add(new Rule { Pattern = new Regex(cells[1], RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(50)), Replacement = Decode(cells[2]) });
            }
            if (result.Count == 0) throw new InvalidDataException("Empty translation table");
            return result;
        }
        private void BuildExactOriginFallbacks()
        {
            var ambiguous = new HashSet<string>(StringComparer.Ordinal);
            foreach (var pair in byId)
            {
                Row row = pair.Value;
                if (row.English == row.Thai) continue;
                AddExactOrigin(row.Cn, row.ThaiDecoded, ambiguous);
                AddExactOrigin(row.Tw, row.ThaiDecoded, ambiguous);
            }
            foreach (string text in ambiguous) exactOrigins.Remove(text);
        }
        private void AddExactOrigin(string source, string target, HashSet<string> ambiguous)
        {
            if (String.IsNullOrEmpty(source) || source == "None" || source.Length > 6000 || !HasChinese(source)) return;
            string existing;
            if (exactOrigins.TryGetValue(source, out existing!))
            {
                if (existing != target) ambiguous.Add(source);
                return;
            }
            exactOrigins.Add(source, target);
        }
        private static IEnumerable<string> Lines(string text)
        { using (var reader = new StringReader(text)) { string? line; while ((line = reader.ReadLine()) != null) yield return line; } }
        private static void ValidatePair(string english, string thai)
        {
            if (english.IndexOf('\t') >= 0 || thai.IndexOf('\t') >= 0 || english.Length > 6000 || thai.Length > 10000 || thai.IndexOf('\uFFFD') >= 0 || Regex.IsMatch(thai, @"ZX[QR]\d{4}[QR]XZ")) throw new InvalidDataException("Invalid text encoding or size");
            SameMatches(english, thai, @"[$@^]\{\d+\}|(?<![$@^])\{\d+\}|%(?:\d+\$)?[sdif]|<[^>]+>|\\[nrt]", false);
            SameMatches(english, thai, @"(?<![A-Za-z])\d+(?:\.\d+)?|[%*+]", true);
            var names = new Regex(@"【([^】]+)】|\[([^\]]+)\]");
            var a = new List<string>(); var b = new List<string>();
            foreach (Match m in names.Matches(english)) a.Add((m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value).Trim());
            foreach (Match m in names.Matches(thai)) b.Add((m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value).Trim());
            if (String.Join("\u001F", a) != String.Join("\u001F", b)) throw new InvalidDataException("Status names changed");
        }
        private static void SameMatches(string english, string thai, string pattern, bool sorted)
        {
            var a = new List<string>(); var b = new List<string>();
            foreach (Match m in Regex.Matches(english, pattern)) a.Add(m.Value);
            foreach (Match m in Regex.Matches(thai, pattern)) b.Add(m.Value);
            if (sorted) { a.Sort(StringComparer.Ordinal); b.Sort(StringComparer.Ordinal); }
            if (String.Join("\u001F", a) != String.Join("\u001F", b)) throw new InvalidDataException("Placeholder, formula or number changed");
        }
        public bool TryId(string id, string original, out string translated)
        {
            translated = original;
            Row row;
            if (!byId.TryGetValue(id, out row!)) return false;
            // Refuse reused/mismatched IDs in a newer game build; never guess.
            if (original == row.English) { translated = row.Thai; return true; }
            if (original == row.EnglishDecoded) { translated = row.ThaiDecoded; return true; }
            // Restore only known original Chinese for this exact ID. Never translate arbitrary CJK/player text.
            if (KnownOrigin(original, row.Cn) || KnownOrigin(original, row.Tw)) { translated = row.ThaiDecoded; return true; }
            if (KnownOrigin(original, row.CnWire) || KnownOrigin(original, row.TwWire)) { translated = row.Thai; return true; }
            return false;
        }
        public bool ContainsId(string id) { return byId.ContainsKey(id); }
        public bool TryUpdatedTarget(string id, string original, SkillDictionary? previous, out string translated)
        {
            translated = original;
            Row now, old;
            if (previous == null || !byId.TryGetValue(id, out now!) || !previous.byId.TryGetValue(id, out old!)) return false;
            if (original == old.ThaiDecoded) { translated = now.ThaiDecoded; return true; }
            if (original == old.Thai) { translated = now.Thai; return true; }
            return false;
        }
        public static bool HasChinese(string text)
        { return Regex.IsMatch(text.Replace("(╯▔皿▔)╯", ""), @"[\u3400-\u4DBF\u4E00-\u9FFF\uF900-\uFAFF]"); }
        // Only call from a trusted language-ID API or a named localization table.
        // This is NOT a global text/player-name translator.
        public bool TryLanguageId(string id, string original, out string translated, out bool restoredEnglish)
        {
            restoredEnglish = false;
            if (TryId(id, original, out translated)) return true;
            Row row;
            if (!byId.TryGetValue(id, out row!)) return false;
            if (original == row.ThaiDecoded || original == row.Thai) return true;
            if (original.Length > 15000 || !HasChinese(original)) return false;
            // A changed Chinese source must never be guessed into Thai. Restore the
            // trusted English by ID only when placeholders/tags/numbers still agree.
            try { ValidateLanguageShape(row.EnglishDecoded, Decode(original)); }
            catch (InvalidDataException) { return false; }
            translated = original == Decode(original) ? row.EnglishDecoded : row.English;
            restoredEnglish = true;
            return true;
        }
        private static void ValidateLanguageShape(string english, string chinese)
        {
            SameMatches(english, chinese, @"[$@^]\{\d+\}|(?<![$@^])\{\d+\}|%(?:\d+\$)?[sdif]|<[^>]+>", false);
            SameMatches(english, chinese, @"(?<![A-Za-z])\d+(?:\.\d+)?|[%*+]", true);
            if (english.Split('\n').Length != chinese.Split('\n').Length || english.Split('\r').Length != chinese.Split('\r').Length || english.Split('\t').Length != chinese.Split('\t').Length)
                throw new InvalidDataException("Changed localization layout");
        }
        private static bool KnownOrigin(string input, string value) { return value.Length != 0 && value != "None" && input == value; }
        public int ThaiCount { get { int n = 0; foreach (var row in byId.Values) if (row.English != row.Thai) n++; return n; } }
        public IEnumerable<KeyValuePair<long, string>> IdValues()
        { foreach (var row in byId) yield return new KeyValuePair<long, string>(long.Parse(row.Key, System.Globalization.CultureInfo.InvariantCulture), row.Value.ThaiDecoded); }
        public int SeedCache(IDictionary<long, string> native, SkillDictionary? previous = null)
        {
            int changed = 0;
            foreach (var pair in byId)
            {
                long key = long.Parse(pair.Key, System.Globalization.CultureInfo.InvariantCulture); string existing;
                bool has = native.TryGetValue(key, out existing!); Row old;
                bool ownPrevious = previous != null && previous.byId.TryGetValue(pair.Key, out old!) && existing == old.ThaiDecoded;
                string target = pair.Value.ThaiDecoded, matched;
                if (has && existing != target && !ownPrevious)
                {
                    if (!TryLanguageId(pair.Key, existing, out matched!, out _)) continue;
                    target = matched;
                }
                if (!has || existing != target) { native[key] = target; changed++; }
            }
            return changed;
        }
        public string Translate(string input)
        {
            if (String.IsNullOrEmpty(input) || input.Length > 6000) return input;
            lock (sync)
            {
                string output;
                // Some short UI labels are passed straight to text setters without a
                // localization ID. Translate only complete, known source strings whose
                // approved IDs all agree on the same Thai value.
                if (HasChinese(input) && exactOrigins.TryGetValue(input, out output!)) return output;
                // Global setters must not replace short UI labels/status names by coincidence.
                // Short descriptions remain available through the exact ID+English hook.
                if (input.Length >= 45 && exact.TryGetValue(input, out output!)) return output;
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
