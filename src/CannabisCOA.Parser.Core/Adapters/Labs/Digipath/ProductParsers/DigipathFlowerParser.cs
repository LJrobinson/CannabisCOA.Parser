using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Parsers;

namespace CannabisCOA.Parser.Core.Adapters.Labs.Digipath.ProductParsers;

public static class DigipathFlowerParser
{
    private static readonly Dictionary<string, string[]> CannabinoidAliases = new()
    {
        ["THC"] = ["THC", "Δ9-THC", "DELTA-9 THC", "DELTA 9 THC", "D9-THC"],
        ["THCA"] = ["THCA", "THCa", "THC-A", "Δ9-THCA"],
        ["CBD"] = ["CBD"],
        ["CBDA"] = ["CBDA", "CBDa", "CBD-A"]
    };

    private static readonly string[] BlockedCannabinoidRowTerms =
    [
        "MME ID",
        "METRC",
        "LICENSE",
        "CERTIFICATE",
        "BATCH",
        "LOT",
        "FORMULA",
        "CALCULATION",
        "TOTAL THC =",
        "TOTAL CBD =",
        "TOTAL POTENTIAL THC",
        "TOTAL POTENTIAL CBD",
        "THCA *",
        "CBDA *",
        "THC /",
        "CBD /",
        "* 0.877",
        "/ 1"
    ];

    private static readonly Regex ResultTokenRegex = new(
        @"<\s*LOQ|ND|NR|(?<prefix><)?\s*(?<value>\d{1,4}(?:\.\d+)?|\.\d+)\s*(?<unit>%|mg\s*/\s*g|mg/g|mg\/g)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly AnalyteDefinition[] AnalyteDefinitions =
    [
        new("THC", AnalyteKind.Cannabinoid, ["Δ9-THC", "D9-THC", "Delta-9 THC", "Delta 9 THC"]),
        new("THCA", AnalyteKind.Cannabinoid, ["THCa", "THCA"]),
        new("CBDA", AnalyteKind.Cannabinoid, ["CBDa", "CBDA"]),
        new("CBD", AnalyteKind.Cannabinoid, ["CBD"]),
        new("α-Pinene", AnalyteKind.Terpene, ["α-Pinene", "alpha-Pinene"]),
        new("β-Pinene", AnalyteKind.Terpene, ["β-Pinene", "beta-Pinene"]),
        new("β-Myrcene", AnalyteKind.Terpene, ["β-Myrcene", "beta-Myrcene", "Myrcene"]),
        new("δ-Limonene", AnalyteKind.Terpene, ["δ-Limonene", "delta-Limonene", "Limonene"]),
        new("Ocimene", AnalyteKind.Terpene, ["Ocimene"]),
        new("Terpinolene", AnalyteKind.Terpene, ["Terpinolene"]),
        new("Linalool", AnalyteKind.Terpene, ["Linalool"]),
        new("β-Caryophyllene", AnalyteKind.Terpene, ["β-Caryophyllene", "beta-Caryophyllene", "Caryophyllene"]),
        new("α-Humulene", AnalyteKind.Terpene, ["α-Humulene", "alpha-Humulene", "Humulene"]),
        new("α-Bisabolol", AnalyteKind.Terpene, ["α-Bisabolol", "alpha-Bisabolol", "Bisabolol"]),
        new("Eucalyptol", AnalyteKind.Terpene, ["Eucalyptol"]),
        new("Geraniol", AnalyteKind.Terpene, ["Geraniol"]),
        new("Guaiol", AnalyteKind.Terpene, ["Guaiol"]),
        new("Camphene", AnalyteKind.Terpene, ["Camphene"]),
        new("Camphor", AnalyteKind.Terpene, ["Camphor"]),
        new("Isopulegol", AnalyteKind.Terpene, ["Isopulegol"]),
        new("p-Cymene", AnalyteKind.Terpene, ["p-Cymene"]),
        new("δ-3-Carene", AnalyteKind.Terpene, ["δ-3-Carene", "delta-3-Carene", "3-Carene"]),
        new("α-Terpinene", AnalyteKind.Terpene, ["α-Terpinene", "alpha-Terpinene"]),
        new("Caryophyllene Oxide", AnalyteKind.Terpene, ["Caryophyllene Oxide"]),
        new("γ-Terpinene", AnalyteKind.Terpene, ["γ-Terpinene", "gamma-Terpinene"]),
        new("Farnesene", AnalyteKind.Terpene, ["Farnesene"]),
        new("Fenchol", AnalyteKind.Terpene, ["Fenchol"]),
        new("Borneol", AnalyteKind.Terpene, ["Borneol"]),
        new("Nerolidol", AnalyteKind.Terpene, ["Nerolidol", "trans-Nerolidol", "cis-Nerolidol"]),
        new("Valencene", AnalyteKind.Terpene, ["Valencene"])
    ];

    private static readonly Regex ResultWindowTokenRegex = new(
        @"(?<![\p{L}\p{N}.-])(?<raw><\s*LOQ|ND|NR|\d{1,4}(?:\.\d+)?|\.\d+)(?![\p{L}\p{N}.-])",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static CoaResult Parse(string text, string labName)
    {
        var productType = ProductTypeDetector.Detect(text);

        var cannabinoids = ParseDigipathCannabinoidsOrFallback(text);
        CannabinoidCalculator.CalculateTotals(cannabinoids);

        var testDate = GenericDateParser.ExtractTestDate(text);
        var freshness = FreshnessCalculator.Calculate(testDate);
        var compliance = ComplianceParser.Parse(text);
        var terpenes = ParseDigipathTerpenesOrFallback(text);

        return new CoaResult
        {
            LabName = labName,
            ProductType = productType,
            Cannabinoids = cannabinoids,
            Terpenes = terpenes,
            TestDate = testDate,
            Freshness = freshness,
            Compliance = compliance
        };
    }

    private static CannabinoidProfile ParseDigipathCannabinoidsOrFallback(string text)
    {
        if (TryParseDigipathCannabinoidTable(text, out var profile))
            return profile;

        return GenericCannabinoidTextParser.Parse(text);
    }

    private static TerpeneProfile ParseDigipathTerpenesOrFallback(string text)
    {
        if (TryParseDigipathTerpeneTable(text, out var profile))
            return profile;

        return GenericTerpeneTextParser.Parse(text);
    }

    private static bool TryParseDigipathTerpeneTable(string text, out TerpeneProfile profile)
    {
        profile = new TerpeneProfile();

        var rows = ExtractTerpeneSectionRows(text);

        if (rows.Count == 0)
            return false;

        var hasTableContext = HasDigipathTerpeneTableContext(rows);

        if (!hasTableContext)
            return false;

        foreach (var row in rows)
        {
            foreach (var parsedRow in ParseTerpeneRows(row))
            {
                profile.Terpenes[parsedRow.Name] = parsedRow.Value;
            }

            if (profile.TotalTerpenes == 0m &&
                TryParseDigipathTerpeneTotalRow(row, out var totalPercent))
            {
                profile.TotalTerpenes = totalPercent;
            }
        }

        return true;
    }

    private static List<string> ExtractTerpeneSectionRows(string text)
    {
        var rows = NormalizeRows(text);
        var sectionRows = new List<string>();
        var inTerpeneSection = false;

        foreach (var row in rows)
        {
            if (IsTerpeneSectionStart(row))
            {
                inTerpeneSection = true;
                sectionRows.Add(row);
                continue;
            }

            if (inTerpeneSection && IsTerpeneSectionEnd(row))
            {
                inTerpeneSection = false;
            }

            if (inTerpeneSection ||
                LooksLikeTerpeneTableRow(row) ||
                TryParseDigipathTerpeneTotalRow(row, out _))
            {
                sectionRows.Add(row);
            }
        }

        return sectionRows;
    }

    private static IEnumerable<ParsedDigipathTerpeneRow> ParseTerpeneRows(string row)
    {
        if (!LooksLikeSafeAnalyteRow(row))
            yield break;

        var anchors = FindAnalyteAnchors(row);

        for (var i = 0; i < anchors.Count; i++)
        {
            var anchor = anchors[i];

            if (anchor.Kind != AnalyteKind.Terpene)
                continue;

            var nextAnchor = i + 1 < anchors.Count ? anchors[i + 1] : null;

            if (TryParseTerpeneAnchorWindow(row, anchor, nextAnchor, out var parsedRow))
                yield return parsedRow;
        }
    }

    private static bool TryParseTerpeneAnchorWindow(
        string row,
        AnalyteAnchor anchor,
        AnalyteAnchor? nextAnchor,
        out ParsedDigipathTerpeneRow parsedRow)
    {
        parsedRow = new ParsedDigipathTerpeneRow(string.Empty, 0m);

        if (anchor.Kind != AnalyteKind.Terpene)
            return false;

        var segmentEnd = nextAnchor?.StartIndex ?? row.Length;

        if (segmentEnd <= anchor.EndIndex)
            return false;

        var segment = row[anchor.EndIndex..segmentEnd];
        var tokens = ExtractBoundedResultTokens(segment, 4);

        if (!TryParseDigipathTerpeneResultTriple(tokens, out var percent, out _))
            return false;

        parsedRow = new ParsedDigipathTerpeneRow(anchor.CanonicalName, percent);
        return true;
    }

    private static bool TryParseDigipathTerpeneResultTriple(
        IReadOnlyList<ResultToken> tokens,
        out decimal percent,
        out bool isLoq)
    {
        percent = 0m;
        isLoq = false;

        if (tokens.Count >= 3 &&
            tokens[1].IsNonDetect &&
            tokens[2].IsNonDetect &&
            !QualifiedTokensMatch(tokens[1], tokens[2]))
        {
            return false;
        }

        if (!TryParseDigipathResultTriple(tokens, out percent, out _, out isLoq))
            return false;

        return true;
    }

    private static bool TryParseDigipathTerpeneTotalRow(string row, out decimal totalPercent)
    {
        totalPercent = 0m;

        var totalMatches = Regex.Matches(
                row,
                @"(?<![\p{L}\p{N}])Total(?![\p{L}\p{N}])",
                RegexOptions.IgnoreCase)
            .Cast<Match>()
            .ToList();

        if (totalMatches.Count == 0)
            return false;

        var lastTotal = totalMatches[^1];
        var totalSegment = row[(lastTotal.Index + lastTotal.Length)..];
        var tokens = ExtractBoundedResultTokens(totalSegment, 2);

        if (tokens.Count < 2 ||
            tokens[0].Value is not decimal percent ||
            tokens[1].Value is not decimal mgPerGram)
        {
            return false;
        }

        if (!IsValidPercentMgPair(percent, mgPerGram))
            return false;

        totalPercent = percent;
        return true;
    }

    private static bool TryParseDigipathCannabinoidTable(string text, out CannabinoidProfile profile)
    {
        profile = CreateEmptyProfile();

        var rows = NormalizeRows(text);

        if (rows.Count == 0)
            return false;

        var hasTableContext = HasDigipathCannabinoidTableContext(rows);

        if (!hasTableContext)
            return false;

        foreach (var row in rows)
        {
            foreach (var parsedRow in ParseCannabinoidRows(row))
            {
                var field = new ParsedField<decimal>
                {
                    FieldName = parsedRow.FieldName,
                    Value = parsedRow.Value,
                    SourceText = parsedRow.SourceText,
                    Confidence = parsedRow.Confidence
                };

                switch (parsedRow.FieldName)
                {
                    case "THC" when profile.THC.Confidence == 0m:
                        profile.THC = field;
                        break;
                    case "THCA" when profile.THCA.Confidence == 0m:
                        profile.THCA = field;
                        break;
                    case "CBD" when profile.CBD.Confidence == 0m:
                        profile.CBD = field;
                        break;
                    case "CBDA" when profile.CBDA.Confidence == 0m:
                        profile.CBDA = field;
                        break;
                }
            }
        }

        return true;
    }

    private static IEnumerable<ParsedDigipathCannabinoidRow> ParseCannabinoidRows(string row)
    {
        if (!LooksLikeSafeAnalyteRow(row))
            yield break;

        var anchors = FindAnalyteAnchors(row);

        for (var i = 0; i < anchors.Count; i++)
        {
            var anchor = anchors[i];

            if (anchor.Kind != AnalyteKind.Cannabinoid)
                continue;

            var nextAnchor = i + 1 < anchors.Count ? anchors[i + 1] : null;

            if (TryParseCannabinoidAnchorWindow(row, anchor, nextAnchor, out var parsedRow))
                yield return parsedRow;
        }
    }

    private static bool TryParseCannabinoidSideRow(string row, out ParsedDigipathCannabinoidRow parsedRow)
    {
        parsedRow = ParseCannabinoidRows(row).FirstOrDefault()
            ?? new ParsedDigipathCannabinoidRow(string.Empty, 0m, string.Empty, 0m);

        return !string.IsNullOrWhiteSpace(parsedRow.FieldName);
    }

    private static IReadOnlyList<AnalyteAnchor> FindAnalyteAnchors(string row)
    {
        var anchors = new List<AnalyteAnchor>();

        foreach (var definition in AnalyteDefinitions)
        {
            foreach (var alias in definition.Aliases)
            {
                var pattern = BuildAnalytePattern(alias);

                foreach (Match match in Regex.Matches(row, pattern, RegexOptions.IgnoreCase))
                {
                    if (!match.Success)
                        continue;

                    anchors.Add(new AnalyteAnchor(
                        match.Index,
                        match.Index + match.Length,
                        definition.Kind,
                        definition.CanonicalName,
                        match.Value));
                }
            }
        }

        return anchors
            .OrderBy(anchor => anchor.StartIndex)
            .ThenByDescending(anchor => anchor.EndIndex - anchor.StartIndex)
            .Aggregate(new List<AnalyteAnchor>(), AddNonOverlappingAnchor)
            .OrderBy(anchor => anchor.StartIndex)
            .ToList();
    }

    private static bool TryParseCannabinoidAnchorWindow(
        string row,
        AnalyteAnchor anchor,
        AnalyteAnchor? nextAnchor,
        out ParsedDigipathCannabinoidRow parsedRow)
    {
        parsedRow = new ParsedDigipathCannabinoidRow(string.Empty, 0m, string.Empty, 0m);

        if (anchor.Kind != AnalyteKind.Cannabinoid)
            return false;

        var segmentEnd = nextAnchor?.StartIndex ?? row.Length;

        if (segmentEnd <= anchor.EndIndex)
            return false;

        var segment = row[anchor.EndIndex..segmentEnd];
        var tokens = ExtractBoundedResultTokens(segment, 4);

        if (!TryParseDigipathResultTriple(tokens, out var percent, out _, out var isLoq))
            return false;

        var confidence = isLoq ? 0.85m : 0.95m;

        parsedRow = new ParsedDigipathCannabinoidRow(anchor.CanonicalName, percent, row, confidence);
        return true;
    }

    private static IReadOnlyList<ResultToken> ExtractBoundedResultTokens(string segment, int maxTokens)
    {
        return ResultWindowTokenRegex.Matches(segment)
            .Cast<Match>()
            .Where(match => match.Success)
            .Take(maxTokens)
            .Select(ToResultToken)
            .Where(token => token != null)
            .Select(token => token!)
            .ToList();
    }

    private static bool TryParseDigipathResultTriple(
        IReadOnlyList<ResultToken> tokens,
        out decimal percent,
        out decimal mgPerGram,
        out bool isLoq)
    {
        percent = 0m;
        mgPerGram = 0m;
        isLoq = false;

        if (tokens.Count < 3)
            return false;

        var percentToken = tokens[1];
        var mgPerGramToken = tokens[2];

        if (percentToken.IsNonDetect)
        {
            if (!mgPerGramToken.IsNonDetect)
                return false;

            isLoq = true;
            return true;
        }

        if (mgPerGramToken.IsNonDetect ||
            percentToken.Value == null ||
            mgPerGramToken.Value == null)
        {
            return false;
        }

        percent = percentToken.Value.Value;
        mgPerGram = mgPerGramToken.Value.Value;

        if (percent < 0m || percent > 100m)
            return false;

        return IsValidPercentMgPair(percent, mgPerGram);
    }

    private static string BuildAnalytePattern(string alias)
    {
        var builder = new StringBuilder(@"(?<![\p{L}\p{N}])");

        for (var i = 0; i < alias.Length; i++)
        {
            if (StartsWithWord(alias, i, "alpha"))
            {
                builder.Append("(?:α|alpha)");
                i += "alpha".Length - 1;
                continue;
            }

            if (StartsWithWord(alias, i, "beta"))
            {
                builder.Append("(?:β|beta)");
                i += "beta".Length - 1;
                continue;
            }

            if (StartsWithWord(alias, i, "delta"))
            {
                builder.Append("(?:δ|Δ|delta)");
                i += "delta".Length - 1;
                continue;
            }

            if (StartsWithWord(alias, i, "gamma"))
            {
                builder.Append("(?:γ|gamma)");
                i += "gamma".Length - 1;
                continue;
            }

            var current = alias[i];

            builder.Append(current switch
            {
                'α' => "(?:α|alpha)",
                'β' => "(?:β|beta)",
                'δ' or 'Δ' => "(?:δ|Δ|delta)",
                'γ' => "(?:γ|gamma)",
                '-' or '‐' or '‑' or '‒' or '–' or '—' => @"[\s\-\u2010-\u2015]*",
                _ when char.IsWhiteSpace(current) => @"\s+",
                _ => Regex.Escape(current.ToString())
            });
        }

        builder.Append(@"(?![\p{L}\p{N}])");

        return builder.ToString();
    }

    private static bool StartsWithWord(string text, int startIndex, string word)
    {
        return text.AsSpan(startIndex).StartsWith(word.AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    private static List<AnalyteAnchor> AddNonOverlappingAnchor(
        List<AnalyteAnchor> anchors,
        AnalyteAnchor candidate)
    {
        if (anchors.Any(anchor => RangesOverlap(anchor, candidate)))
            return anchors;

        anchors.Add(candidate);
        return anchors;
    }

    private static bool RangesOverlap(AnalyteAnchor left, AnalyteAnchor right)
    {
        return left.StartIndex < right.EndIndex && right.StartIndex < left.EndIndex;
    }

    private static ResultToken? ToResultToken(Match match)
    {
        var raw = Regex.Replace(match.Groups["raw"].Value.Trim(), @"\s+", " ");

        if (IsNonDetectToken(raw))
            return new ResultToken(raw, null, IsNonDetect: true);

        if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return null;

        return new ResultToken(raw, value, IsNonDetect: false);
    }

    private static bool QualifiedTokensMatch(ResultToken left, ResultToken right)
    {
        return NormalizeQualifiedToken(left.RawText)
            .Equals(NormalizeQualifiedToken(right.RawText), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeQualifiedToken(string token)
    {
        return Regex.Replace(token.Trim(), @"\s+", string.Empty).ToUpperInvariant();
    }

    private static bool IsValidPercentMgPair(decimal percent, decimal mgPerGram)
    {
        var expectedMgPerGram = percent * 10m;
        var tolerance = Math.Max(0.01m, Math.Abs(mgPerGram) * 0.01m);

        return Math.Abs(expectedMgPerGram - mgPerGram) <= tolerance;
    }

    private static bool HasDigipathCannabinoidTableContext(IReadOnlyList<string> rows)
    {
        if (rows.Any(IsDigipathCannabinoidTableHeader))
            return true;

        return rows.Any(LooksLikeDigipathCannabinoidTableContextRow);
    }

    private static bool IsDigipathCannabinoidTableHeader(string row)
    {
        var upper = row.ToUpperInvariant();

        return upper.Contains("ANALYTE", StringComparison.OrdinalIgnoreCase) &&
               upper.Contains("LOQ", StringComparison.OrdinalIgnoreCase) &&
               upper.Contains("MASS", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeDigipathCannabinoidTableContextRow(string row)
    {
        var anchors = FindAnalyteAnchors(row);

        if (anchors.Any(anchor => anchor.Kind == AnalyteKind.Terpene))
            return anchors.Any(anchor => anchor.Kind == AnalyteKind.Cannabinoid);

        return anchors.Any(anchor =>
        {
            if (anchor.Kind != AnalyteKind.Cannabinoid)
                return false;

            var segment = row[anchor.EndIndex..];
            return ExtractBoundedResultTokens(segment, 3).Count >= 3;
        });
    }

    private static bool HasDigipathTerpeneTableContext(IReadOnlyList<string> rows)
    {
        if (rows.Any(IsTerpeneSectionStart))
            return true;

        if (rows.Any(row => TryParseDigipathTerpeneTotalRow(row, out _)))
            return true;

        return rows.Any(LooksLikeTerpeneTableRow);
    }

    private static bool LooksLikeTerpeneTableRow(string row)
    {
        var anchors = FindAnalyteAnchors(row);

        foreach (var anchor in anchors)
        {
            if (anchor.Kind != AnalyteKind.Terpene)
                continue;

            var segment = row[anchor.EndIndex..];

            if (ExtractBoundedResultTokens(segment, 3).Count >= 3)
                return true;
        }

        return false;
    }

    private static bool IsTerpeneSectionStart(string row)
    {
        return row.Contains("Terpene Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Terpenes Test Results", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTerpeneSectionEnd(string row)
    {
        return row.Contains("Safety & Quality Tests", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Pesticide", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Heavy Metals", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Mycotoxins", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Microbiological", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Cannabinoid Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Total Potential THC", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Total Potential CBD", StringComparison.OrdinalIgnoreCase) ||
               row.Equals("Safety", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryFindLeadingCannabinoidAlias(
        string row,
        out string fieldName,
        out string alias)
    {
        foreach (var cannabinoid in CannabinoidAliases)
        {
            foreach (var candidateAlias in cannabinoid.Value)
            {
                var escaped = Regex.Escape(candidateAlias);

                if (!Regex.IsMatch(row, $@"^\s*{escaped}(?=\s|:|$)", RegexOptions.IgnoreCase))
                    continue;

                fieldName = cannabinoid.Key;
                alias = candidateAlias;
                return true;
            }
        }

        fieldName = string.Empty;
        alias = string.Empty;
        return false;
    }

    private static bool IsNonDetectToken(string token)
    {
        return token.Equals("ND", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("NR", StringComparison.OrdinalIgnoreCase) ||
               Regex.IsMatch(token, @"^<\s*LOQ$", RegexOptions.IgnoreCase);
    }

    private static CannabinoidProfile CreateEmptyProfile()
    {
        return new CannabinoidProfile
        {
            THC = Empty("THC"),
            THCA = Empty("THCA"),
            CBD = Empty("CBD"),
            CBDA = Empty("CBDA")
        };
    }

    private static CannabinoidProfile ParseCannabinoidSection(IReadOnlyList<string> rows)
    {
        var context = DetectTableContext(rows);

        var profile = new CannabinoidProfile
        {
            THC = Extract(rows, "THC", context),
            THCA = Extract(rows, "THCA", context),
            CBD = Extract(rows, "CBD", context),
            CBDA = Extract(rows, "CBDA", context)
        };

        return profile;
    }

    private static ParsedField<decimal> Extract(
        IReadOnlyList<string> rows,
        string fieldName,
        DigipathTableContext context)
    {
        foreach (var row in rows)
        {
            if (!LooksLikeSafeCannabinoidRow(row))
                continue;

            var alias = FindLeadingAlias(row, fieldName);

            if (alias == null)
                continue;

            var parsedValue = ExtractResultValue(row, alias, context);

            if (parsedValue == null)
                continue;

            return new ParsedField<decimal>
            {
                FieldName = fieldName,
                Value = parsedValue.Value,
                SourceText = row,
                Confidence = parsedValue.Confidence
            };
        }

        return Empty(fieldName);
    }

    private static ParsedDigipathValue? ExtractResultValue(
        string row,
        string alias,
        DigipathTableContext context)
    {
        var aliasIndex = row.IndexOf(alias, StringComparison.OrdinalIgnoreCase);

        if (aliasIndex < 0)
            return null;

        var afterAlias = row[(aliasIndex + alias.Length)..];

        if (Regex.IsMatch(afterAlias, @"^\s*[\*/=]"))
            return null;

        var tokens = ResultTokenRegex.Matches(afterAlias)
            .Cast<Match>()
            .Select(ToToken)
            .Where(token => token != null)
            .Select(token => token!)
            .ToList();

        if (tokens.Count == 0)
            return null;

        var token = SelectResultToken(tokens, context);

        if (token == null)
            return null;

        if (token.IsNonDetect || token.IsLessThan)
            return new ParsedDigipathValue(0m, 0.85m);

        var unit = string.IsNullOrWhiteSpace(token.Unit)
            ? context.PrimaryResultUnit
            : token.Unit;

        var value = token.Value;

        if (unit == "MG/G")
            value *= 0.1m;

        if (value < 0m || value > 100m)
            return null;

        var confidence = unit == "MG/G" ? 0.9m : 0.95m;

        return new ParsedDigipathValue(value, confidence);
    }

    private static DigipathResultToken? SelectResultToken(
        IReadOnlyList<DigipathResultToken> tokens,
        DigipathTableContext context)
    {
        if (context.HasLoqColumn && tokens.Count >= 2)
            return tokens[1];

        var explicitPercent = tokens.LastOrDefault(token => token.Unit == "%");
        if (explicitPercent != null)
            return explicitPercent;

        var explicitMgPerGram = tokens.LastOrDefault(token => token.Unit == "MG/G");
        if (explicitMgPerGram != null)
            return explicitMgPerGram;

        return tokens.LastOrDefault();
    }

    private static DigipathResultToken? ToToken(Match match)
    {
        var raw = Regex.Replace(match.Value.Trim(), @"\s+", " ");

        if (raw.Equals("ND", StringComparison.OrdinalIgnoreCase) ||
            raw.Equals("NR", StringComparison.OrdinalIgnoreCase) ||
            Regex.IsMatch(raw, @"^<\s*LOQ$", RegexOptions.IgnoreCase))
        {
            return new DigipathResultToken(0m, string.Empty, IsLessThan: false, IsNonDetect: true);
        }

        if (!decimal.TryParse(
            match.Groups["value"].Value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var value))
        {
            return null;
        }

        var unit = NormalizeUnit(match.Groups["unit"].Value);
        var isLessThan = match.Groups["prefix"].Success;

        return new DigipathResultToken(value, unit, isLessThan, IsNonDetect: false);
    }

    private static DigipathTableContext DetectTableContext(IReadOnlyList<string> rows)
    {
        var hasLoqColumn = rows.Any(row =>
            row.Contains("ANALYTE", StringComparison.OrdinalIgnoreCase) &&
            row.Contains("LOQ", StringComparison.OrdinalIgnoreCase));

        foreach (var row in rows)
        {
            var upper = row.ToUpperInvariant();

            if (!upper.Contains("%") && !ContainsMgPerGram(upper))
                continue;

            var units = Regex.Matches(upper, @"%|MG\s*/\s*G|MG/G")
                .Cast<Match>()
                .Select(match => NormalizeUnit(match.Value))
                .ToList();

            if (hasLoqColumn && units.Count >= 2)
                return new DigipathTableContext(hasLoqColumn, units[1]);

            if (units.Count >= 1)
                return new DigipathTableContext(hasLoqColumn, units[0]);
        }

        return new DigipathTableContext(hasLoqColumn, string.Empty);
    }

    private static List<string> ExtractCannabinoidSectionRows(string text)
    {
        var rows = NormalizeRows(text);
        var startIndex = rows.FindIndex(IsCannabinoidSectionStart);

        if (startIndex < 0)
            return [];

        var sectionRows = new List<string>();

        for (var i = startIndex + 1; i < rows.Count; i++)
        {
            var row = rows[i];

            if (IsCannabinoidSectionEnd(row))
                break;

            sectionRows.Add(row);
        }

        return sectionRows;
    }

    private static bool IsCannabinoidSectionStart(string row)
    {
        return row.Contains("Cannabinoid Test Results", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCannabinoidSectionEnd(string row)
    {
        return row.Contains("Total Potential THC", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Total Potential CBD", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Terpene Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Safety & Quality Tests", StringComparison.OrdinalIgnoreCase) ||
               row.Equals("Safety", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeSafeCannabinoidRow(string row)
    {
        if (string.IsNullOrWhiteSpace(row))
            return false;

        if (row.Length > 180)
            return false;

        var upper = row.ToUpperInvariant();

        if (BlockedCannabinoidRowTerms.Any(term => upper.Contains(term.ToUpperInvariant())))
            return false;

        if (Regex.IsMatch(upper, @"\b(MME|ID|LICENSE|CERT|BATCH|LOT)\b.*\d{6,}"))
            return false;

        if (Regex.IsMatch(upper, @"\d{7,}"))
            return false;

        return true;
    }

    private static bool LooksLikeSafeAnalyteRow(string row)
    {
        if (string.IsNullOrWhiteSpace(row))
            return false;

        var upper = row.ToUpperInvariant();

        if (BlockedCannabinoidRowTerms.Any(term => upper.Contains(term.ToUpperInvariant())))
            return false;

        if (Regex.IsMatch(upper, @"\b(MME|ID|LICENSE|CERT|BATCH|LOT)\b.*\d{6,}"))
            return false;

        return true;
    }

    private static string? FindLeadingAlias(string row, string fieldName)
    {
        foreach (var alias in CannabinoidAliases[fieldName])
        {
            var escaped = Regex.Escape(alias);

            if (Regex.IsMatch(row, $@"^\s*{escaped}(?=\s|:|$)", RegexOptions.IgnoreCase))
                return alias;
        }

        return null;
    }

    private static bool HasAnyParsedCannabinoid(CannabinoidProfile profile)
    {
        return profile.THC.Confidence > 0m ||
               profile.THCA.Confidence > 0m ||
               profile.CBD.Confidence > 0m ||
               profile.CBDA.Confidence > 0m;
    }

    private static string NormalizeUnit(string unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
            return string.Empty;

        var normalized = unit.ToUpperInvariant().Replace(" ", "");

        return normalized switch
        {
            "MG/G" => "MG/G",
            "%" => "%",
            _ => normalized
        };
    }

    private static bool ContainsMgPerGram(string text)
    {
        return Regex.IsMatch(text, @"MG\s*/\s*G", RegexOptions.IgnoreCase)
            || text.Contains("MG/G", StringComparison.OrdinalIgnoreCase)
            || text.Contains("MG PER G", StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> NormalizeRows(string text)
    {
        return text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n')
            .Select(row => Regex.Replace(row.Trim(), @"\s+", " "))
            .Where(row => !string.IsNullOrWhiteSpace(row))
            .ToList();
    }

    private static ParsedField<decimal> Empty(string fieldName)
    {
        return new ParsedField<decimal>
        {
            FieldName = fieldName,
            Value = 0m,
            SourceText = string.Empty,
            Confidence = 0m
        };
    }

    private sealed record DigipathTableContext(bool HasLoqColumn, string PrimaryResultUnit);

    private sealed record DigipathResultToken(
        decimal Value,
        string Unit,
        bool IsLessThan,
        bool IsNonDetect);

    private sealed record ParsedDigipathValue(decimal Value, decimal Confidence);

    private sealed record ParsedDigipathCannabinoidRow(
        string FieldName,
        decimal Value,
        string SourceText,
        decimal Confidence);

    private sealed record ParsedDigipathTerpeneRow(
        string Name,
        decimal Value);

    private sealed record AnalyteDefinition(
        string CanonicalName,
        AnalyteKind Kind,
        string[] Aliases);

    private sealed record AnalyteAnchor(
        int StartIndex,
        int EndIndex,
        AnalyteKind Kind,
        string CanonicalName,
        string RawText);

    private sealed record ResultToken(
        string RawText,
        decimal? Value,
        bool IsNonDetect);

    private enum AnalyteKind
    {
        Cannabinoid,
        Terpene
    }
}
