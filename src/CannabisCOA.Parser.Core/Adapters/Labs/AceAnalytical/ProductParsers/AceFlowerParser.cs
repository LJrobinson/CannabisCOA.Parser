using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Mappers;
using CannabisCOA.Parser.Core.Models;
using CannabisCOA.Parser.Core.Parsers;

namespace CannabisCOA.Parser.Core.Adapters.Labs.AceAnalytical.ProductParsers;

public static class AceFlowerParser
{
    private static readonly AnalyteDefinition[] AnalyteDefinitions =
    [
        new("THC", AnalyteKind.Cannabinoid, ["Δ9-THC", "D9-THC", "Delta-9 THC", "Delta 9 THC"]),
        new("THCA", AnalyteKind.Cannabinoid, ["THCa", "THCA"]),
        new("CBDA", AnalyteKind.Cannabinoid, ["CBDa", "CBDA"]),
        new("CBD", AnalyteKind.Cannabinoid, ["CBD"]),
        new("δ-Limonene", AnalyteKind.Terpene, ["δ-Limonene", "delta-Limonene", "Limonene"]),
        new("β-Myrcene", AnalyteKind.Terpene, ["β-Myrcene", "beta-Myrcene", "Myrcene"]),
        new("β-Caryophyllene", AnalyteKind.Terpene, ["β-Caryophyllene", "beta-Caryophyllene", "Caryophyllene"]),
        new("Linalool", AnalyteKind.Terpene, ["Linalool"]),
        new("α-Terpinene", AnalyteKind.Terpene, ["α-Terpinene", "alpha-Terpinene"]),
        new("Eucalyptol", AnalyteKind.Terpene, ["Eucalyptol"]),
        new("α-Pinene", AnalyteKind.Terpene, ["α-Pinene", "alpha-Pinene"]),
        new("β-Pinene", AnalyteKind.Terpene, ["β-Pinene", "beta-Pinene"]),
        new("α-Humulene", AnalyteKind.Terpene, ["α-Humulene", "alpha-Humulene", "Humulene"]),
        new("Terpinolene", AnalyteKind.Terpene, ["Terpinolene"]),
        new("Ocimene", AnalyteKind.Terpene, ["Ocimene"]),
        new("Camphene", AnalyteKind.Terpene, ["Camphene"]),
        new("Camphor", AnalyteKind.Terpene, ["Camphor"]),
        new("Geraniol", AnalyteKind.Terpene, ["Geraniol"]),
        new("Guaiol", AnalyteKind.Terpene, ["Guaiol"]),
        new("Nerolidol", AnalyteKind.Terpene, ["Nerolidol", "trans-Nerolidol", "cis-Nerolidol"]),
        new("α-Bisabolol", AnalyteKind.Terpene, ["α-Bisabolol", "alpha-Bisabolol", "Bisabolol"]),
        new("Isopulegol", AnalyteKind.Terpene, ["Isopulegol"]),
        new("δ-3-Carene", AnalyteKind.Terpene, ["δ-3-Carene", "delta-3-Carene", "3-Carene"]),
        new("p-Cymene", AnalyteKind.Terpene, ["p-Cymene"]),
        new("Fenchol", AnalyteKind.Terpene, ["Fenchol"]),
        new("Borneol", AnalyteKind.Terpene, ["Borneol"]),
        new("Valencene", AnalyteKind.Terpene, ["Valencene"]),
        new("Caryophyllene Oxide", AnalyteKind.Terpene, ["Caryophyllene Oxide"]),
        new("γ-Terpinene", AnalyteKind.Terpene, ["γ-Terpinene", "gamma-Terpinene"]),
        new("Farnesene", AnalyteKind.Terpene, ["Farnesene"])
    ];

    private static readonly AnalyteDefinition[] PesticideDefinitions =
    [
        new("Abamectin", AnalyteKind.Pesticide, ["Abamectin"]),
        new("Acequinocyl", AnalyteKind.Pesticide, ["Acequinocyl"]),
        new("Bifenazate", AnalyteKind.Pesticide, ["Bifenazate"]),
        new("Bifenthrin", AnalyteKind.Pesticide, ["Bifenthrin"]),
        new("Cyfluthrin", AnalyteKind.Pesticide, ["Cyfluthrin"]),
        new("Cypermethrin", AnalyteKind.Pesticide, ["Cypermethrin"]),
        new("Daminozide", AnalyteKind.Pesticide, ["Daminozide"]),
        new("Dimethomorph", AnalyteKind.Pesticide, ["Dimethomorph"]),
        new("Etoxazole", AnalyteKind.Pesticide, ["Etoxazole"]),
        new("Fenhexamid", AnalyteKind.Pesticide, ["Fenhexamid"]),
        new("Flonicamid", AnalyteKind.Pesticide, ["Flonicamid"]),
        new("Fludioxonil", AnalyteKind.Pesticide, ["Fludioxonil"]),
        new("Imidacloprid", AnalyteKind.Pesticide, ["Imidacloprid"]),
        new("Myclobutanil", AnalyteKind.Pesticide, ["Myclobutanil"]),
        new("Paclobutrazol", AnalyteKind.Pesticide, ["Paclobutrazol"]),
        new("Piperonyl Butoxide", AnalyteKind.Pesticide, ["Piperonyl Butoxide"]),
        new("Pyrethrins", AnalyteKind.Pesticide, ["Pyrethrins"]),
        new("Quintozene", AnalyteKind.Pesticide, ["Quintozene"]),
        new("Spinetoram", AnalyteKind.Pesticide, ["Spinetoram"]),
        new("Spinosad", AnalyteKind.Pesticide, ["Spinosad"]),
        new("Spirotetramat", AnalyteKind.Pesticide, ["Spirotetramat"]),
        new("Thiamethoxam", AnalyteKind.Pesticide, ["Thiamethoxam"]),
        new("Trifloxystrobin", AnalyteKind.Pesticide, ["Trifloxystrobin"])
    ];

    private static readonly AnalyteDefinition[] HeavyMetalDefinitions =
    [
        new("Lead", AnalyteKind.HeavyMetal, ["Lead"]),
        new("Cadmium", AnalyteKind.HeavyMetal, ["Cadmium"]),
        new("Arsenic", AnalyteKind.HeavyMetal, ["Arsenic"]),
        new("Mercury", AnalyteKind.HeavyMetal, ["Mercury"])
    ];

    private static readonly AnalyteDefinition[] MicrobialDefinitions =
    [
        new("Aspergillus flavus", AnalyteKind.Microbial, ["Aspergillus flavus"]),
        new("Aspergillus fumigatus", AnalyteKind.Microbial, ["Aspergillus fumigatus"]),
        new("Aspergillus niger", AnalyteKind.Microbial, ["Aspergillus niger"]),
        new("Aspergillus terreus", AnalyteKind.Microbial, ["Aspergillus terreus"]),
        new("Bile-Tolerant Gram-Negative Bacteria", AnalyteKind.Microbial, ["Bile-Tolerant Gram-Negative Bacteria"]),
        new("Coliforms", AnalyteKind.Microbial, ["Coliforms"]),
        new("E. Coli", AnalyteKind.Microbial, ["E. Coli", "E. coli", "STEC E. coli"]),
        new("Salmonella", AnalyteKind.Microbial, ["Salmonella"]),
        new("Yeast & Mold", AnalyteKind.Microbial, ["Yeast & Mold"])
    ];

    private static readonly HashSet<string> QuantitativeMicrobialNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Bile-Tolerant Gram-Negative Bacteria",
        "Coliforms",
        "Yeast & Mold"
    };

    private static readonly AnalyteDefinition[] MycotoxinDefinitions =
    [
        new("Aflatoxins", AnalyteKind.Mycotoxin, ["Aflatoxins"]),
        new("Aflatoxin B1", AnalyteKind.Mycotoxin, ["Aflatoxin B1"]),
        new("Aflatoxin B2", AnalyteKind.Mycotoxin, ["Aflatoxin B2"]),
        new("Aflatoxin G1", AnalyteKind.Mycotoxin, ["Aflatoxin G1"]),
        new("Aflatoxin G2", AnalyteKind.Mycotoxin, ["Aflatoxin G2"]),
        new("Ochratoxin A", AnalyteKind.Mycotoxin, ["Ochratoxin A"])
    ];

    private static readonly Regex ResultTokenRegex = new(
        @"(?<![\p{L}\p{N}.-])(?<raw><\s*LOQ|<\s*LOD|>\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)(?![\p{L}\p{N}.-])",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ContaminantFourColumnRegex = new(
        @"^\s*(?<loq>\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<limit>\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<mass><\s*LOQ|<\s*LOD|>\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<status>Pass|Fail|NT|NR)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AcePesticideFiveColumnRegex = new(
        @"^\s*(?<lod>\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<loq>\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<limit>\d{1,6}(?:\.\d+)?|\.\d+|>\s*LOD|<\s*LOQ|<\s*LOD|ND|NR|NT)\s+(?<mass><\s*LOQ|<\s*LOD|>\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<status>Pass|Fail|NT|NR)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex BinaryMicrobialRegex = new(
        @"^\s*(?<result>Not\s+Detected|Negative|Positive|Detected|ND|NR|NT)\s+(?<status>Pass|Fail|NT|NR)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);


    //LEGACY COA RESULT

    public static CoaResult Parse(string text, string labName)
    {
        var cannabinoids = ParseAceCannabinoidsOrFallback(text);
        CannabinoidCalculator.CalculateTotals(cannabinoids);
        var productType = ProductTypeDetector.Detect(text);

        if (productType == ProductType.Unknown && LooksLikeAceFlower(text))
            productType = ProductType.Flower;

        var testDate = GenericDateParser.ExtractTestDate(text);
        var harvestDate = GenericDateParser.ExtractHarvestDate(text);
        var packageDate = GenericDateParser.ExtractPackageDate(text);

        return new CoaResult
        {
            ProductType = productType,
            IsAmended = CoaMetadataParser.IsAmended(text),
            LabName = labName,
            HarvestDate = harvestDate,
            TestDate = testDate,
            PackageDate = packageDate,
            Cannabinoids = cannabinoids,
            Terpenes = ParseAceTerpenesOrFallback(text),
            Compliance = ParseAceComplianceOrFallback(text),
            Freshness = FreshnessCalculator.Calculate(testDate)
        };
    }

    //FUTURE COA RESULT

    public static CoaDocument ParseDocument(
        string text,
        string labName,
        string? sourceFileName = null)
    {
        var result = Parse(text, labName);

        return CoaDocumentMapper.FromCoaResult(
            result,
            sourceFileName,
            parserName: nameof(AceFlowerParser));
    }

    private static CannabinoidProfile ParseAceCannabinoidsOrFallback(string text)
    {
        if (!TryParseAceCannabinoidTable(text, out var profile))
            return GenericCannabinoidTextParser.Parse(text);

        return profile;
    }

    private static bool TryParseAceCannabinoidTable(string text, out CannabinoidProfile profile)
    {
        profile = CreateEmptyCannabinoidProfile();
        var hasTableContext = NormalizeRows(text).Any(IsCannabinoidSectionStart);
        var rows = ExtractSectionRows(text, IsCannabinoidSectionStart, IsCannabinoidSectionEnd, row => LooksLikeAnalyteRow(row, AnalyteKind.Cannabinoid, AnalyteDefinitions));

        if (rows.Count == 0)
            return hasTableContext;

        foreach (var row in rows)
        {
            var anchors = FindAnchors(row, AnalyteDefinitions);

            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];
                if (anchor.Kind != AnalyteKind.Cannabinoid)
                    continue;

                var nextAnchor = i + 1 < anchors.Count ? anchors[i + 1] : null;
                var segment = GetSegment(row, anchor, nextAnchor);

                if (!TryParseCannabinoidTriple(segment, out var value, out var confidence))
                    continue;

                var field = new ParsedField<decimal>
                {
                    FieldName = anchor.CanonicalName,
                    Value = value,
                    SourceText = row,
                    Confidence = confidence
                };

                switch (anchor.CanonicalName)
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

        return hasTableContext || HasCannabinoidResult(profile);
    }

    private static bool TryParseCannabinoidTriple(string segment, out decimal value, out decimal confidence)
    {
        value = 0m;
        confidence = 0m;
        var tokens = ExtractResultTokens(segment, 3);

        if (tokens.Count < 3)
            return false;

        var percentToken = tokens[1];
        var mgPerGramToken = tokens[2];

        if (percentToken.IsQualified)
        {
            if (!mgPerGramToken.IsQualified)
                return false;

            confidence = 0m;
            return true;
        }

        if (percentToken.Value is not decimal percent ||
            mgPerGramToken.Value is not decimal mgPerGram)
        {
            return false;
        }

        if (!IsValidPercentMgPair(percent, mgPerGram))
            return false;

        value = percent;
        confidence = 0.95m;
        return true;
    }

    private static TerpeneProfile ParseAceTerpenesOrFallback(string text)
    {
        if (!TryParseAceTerpeneTable(text, out var profile))
            return GenericTerpeneTextParser.Parse(text);

        return profile;
    }

    private static bool TryParseAceTerpeneTable(string text, out TerpeneProfile profile)
    {
        profile = new TerpeneProfile();
        var hasTableContext = NormalizeRows(text).Any(IsTerpeneSectionStart);
        var rows = ExtractSectionRows(text, IsTerpeneSectionStart, IsTerpeneSectionEnd, row => LooksLikeAnalyteRow(row, AnalyteKind.Terpene, AnalyteDefinitions) || TryParseAceTerpeneTotalRow(row, out _));

        if (rows.Count == 0)
            return hasTableContext;

        foreach (var row in rows)
        {
            var anchors = FindAnchors(row, AnalyteDefinitions);

            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];
                if (anchor.Kind != AnalyteKind.Terpene)
                    continue;

                var nextAnchor = i + 1 < anchors.Count ? anchors[i + 1] : null;
                var segment = GetSegment(row, anchor, nextAnchor);

                if (TryParseAceTerpeneTriple(segment, out var value))
                    profile.Terpenes[anchor.CanonicalName] = value;
            }

            if (profile.TotalTerpenes == 0m &&
                TryParseAceTerpeneTotalRow(row, out var totalPercent))
            {
                profile.TotalTerpenes = totalPercent;
            }
        }

        return hasTableContext || profile.Terpenes.Count > 0 || profile.TotalTerpenes > 0m;
    }

    private static bool TryParseAceTerpeneTriple(string segment, out decimal value)
    {
        value = 0m;
        var tokens = ExtractResultTokens(segment, 3);

        if (tokens.Count < 3)
            return false;

        var mgPerGramToken = tokens[1];
        var percentToken = tokens[2];

        if (mgPerGramToken.IsQualified && percentToken.IsQualified)
            return QualifiedTokensMatch(mgPerGramToken, percentToken);

        if (mgPerGramToken.Value is not decimal mgPerGram ||
            percentToken.Value is not decimal percent)
        {
            return false;
        }

        if (!IsValidPercentMgPair(percent, mgPerGram))
            return false;

        value = percent;
        return true;
    }

    private static bool TryParseAceTerpeneTotalRow(string row, out decimal totalPercent)
    {
        totalPercent = 0m;

        if (!Regex.IsMatch(row, @"^\s*Total\b", RegexOptions.IgnoreCase))
            return false;

        var totalIndex = row.IndexOf("Total", StringComparison.OrdinalIgnoreCase);
        var tokens = ExtractResultTokens(row[(totalIndex + "Total".Length)..], 2);

        if (tokens.Count < 2 ||
            tokens[0].Value is not decimal mgPerGram ||
            tokens[1].Value is not decimal percent)
        {
            return false;
        }

        if (!IsValidPercentMgPair(percent, mgPerGram))
            return false;

        totalPercent = percent;
        return true;
    }

    private static ComplianceResult ParseAceComplianceOrFallback(string text)
    {
        var generic = ComplianceParser.Parse(text);
        var hasExplicitOverall = TryParseExplicitOverallComplianceStatus(text, out var explicitOverall);
        var hasPesticideContext = TryParseContaminantTable(text, PesticideDefinitions, IsPesticideSectionStart, IsPesticideSectionEnd, TryParseAcePesticideRow, out var pesticideTable);
        var hasHeavyMetalContext = TryParseContaminantTable(text, HeavyMetalDefinitions, IsHeavyMetalSectionStart, IsHeavyMetalSectionEnd, TryParseStandardContaminantRow, out var heavyMetalTable);
        var hasMicrobialContext = TryParseMicrobialTable(text, out var microbialTable);
        var hasMycotoxinContext = TryParseContaminantTable(text, MycotoxinDefinitions, IsMycotoxinSectionStart, IsMycotoxinSectionEnd, TryParseStandardContaminantRow, out var mycotoxinTable);
        var hasPesticideRows = hasPesticideContext && pesticideTable.ParsedRowCount > 0;
        var hasHeavyMetalRows = hasHeavyMetalContext && heavyMetalTable.ParsedRowCount > 0;
        var hasMicrobialRows = hasMicrobialContext && microbialTable.ParsedRowCount > 0;
        var hasMycotoxinRows = hasMycotoxinContext && mycotoxinTable.ParsedRowCount > 0;

        if (!hasPesticideContext && !hasHeavyMetalContext && !hasMicrobialContext && !hasMycotoxinContext)
            return generic;

        if (!hasPesticideRows && !hasHeavyMetalRows && !hasMicrobialRows && !hasMycotoxinRows)
            return hasExplicitOverall ? explicitOverall : UnknownCompliance();

        if ((hasPesticideRows && pesticideTable.FailingRowCount > 0) ||
            (hasHeavyMetalRows && heavyMetalTable.FailingRowCount > 0) ||
            (hasMicrobialRows && microbialTable.FailingRowCount > 0) ||
            (hasMycotoxinRows && mycotoxinTable.FailingRowCount > 0))
        {
            return FailedCompliance();
        }

        var hasUnknownRows =
            (hasPesticideRows && pesticideTable.UnknownRowCount > 0) ||
            (hasHeavyMetalRows && heavyMetalTable.UnknownRowCount > 0) ||
            (hasMicrobialRows && microbialTable.UnknownRowCount > 0) ||
            (hasMycotoxinRows && mycotoxinTable.UnknownRowCount > 0);

        if (hasUnknownRows)
            return hasExplicitOverall ? explicitOverall : UnknownCompliance();

        return hasExplicitOverall
            ? explicitOverall
            : new ComplianceResult { Passed = false, ContaminantsPassed = true, Status = "unknown" };
    }

    private static bool TryParseContaminantTable(
        string text,
        AnalyteDefinition[] definitions,
        Func<string, bool> isSectionStart,
        Func<string, bool> isSectionEnd,
        TryParseContaminantRow tryParseRow,
        out ContaminantTableResult result)
    {
        result = new ContaminantTableResult(0, 0, 0, 0);
        var rows = ExtractSectionRows(text, isSectionStart, isSectionEnd, row => LooksLikeAnyAnalyteRow(row, definitions));

        if (rows.Count == 0)
            return false;

        var parsed = 0;
        var passing = 0;
        var failing = 0;
        var unknown = 0;

        foreach (var row in rows)
        {
            var anchors = FindAnchors(row, definitions);

            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];
                var nextAnchor = i + 1 < anchors.Count ? anchors[i + 1] : null;
                var segment = GetSegment(row, anchor, nextAnchor);

                if (!tryParseRow(segment, out var parsedRow))
                    continue;

                parsed++;

                switch (EvaluateContaminantRow(parsedRow))
                {
                    case ContaminantRowOutcome.Pass:
                        passing++;
                        break;
                    case ContaminantRowOutcome.Fail:
                        failing++;
                        break;
                    case ContaminantRowOutcome.Unknown:
                        unknown++;
                        break;
                }
            }
        }

        result = new ContaminantTableResult(parsed, passing, failing, unknown);
        return true;
    }

    private static bool TryParseAcePesticideRow(string segment, out ParsedContaminantRow row)
    {
        row = EmptyContaminantRow();

        var fiveColumnMatch = AcePesticideFiveColumnRegex.Match(segment);

        if (fiveColumnMatch.Success)
        {
            if (!decimal.TryParse(fiveColumnMatch.Groups["loq"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var loq))
                return false;

            var limit = ParseOptionalDecimal(fiveColumnMatch.Groups["limit"].Value);
            var mass = ParseContaminantMass(fiveColumnMatch.Groups["mass"].Value);

            row = new ParsedContaminantRow(loq, limit, mass, fiveColumnMatch.Groups["status"].Value.ToLowerInvariant());
            return true;
        }

        return TryParseStandardContaminantRow(segment, out row);
    }

    private static bool TryParseStandardContaminantRow(string segment, out ParsedContaminantRow row)
    {
        row = EmptyContaminantRow();
        var match = ContaminantFourColumnRegex.Match(segment);

        if (!match.Success)
            return false;

        if (!decimal.TryParse(match.Groups["loq"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var loq))
            return false;

        if (!decimal.TryParse(match.Groups["limit"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var limit))
            return false;

        var mass = ParseContaminantMass(match.Groups["mass"].Value);
        row = new ParsedContaminantRow(loq, limit, mass, match.Groups["status"].Value.ToLowerInvariant());
        return true;
    }

    private static bool TryParseMicrobialTable(string text, out ContaminantTableResult result)
    {
        result = new ContaminantTableResult(0, 0, 0, 0);
        var rows = ExtractSectionRows(text, IsMicrobialSectionStart, IsMicrobialSectionEnd, row => LooksLikeAnyAnalyteRow(row, MicrobialDefinitions));

        if (rows.Count == 0)
            return false;

        var parsed = 0;
        var passing = 0;
        var failing = 0;
        var unknown = 0;

        foreach (var row in rows)
        {
            var anchors = FindAnchors(row, MicrobialDefinitions);

            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];
                var nextAnchor = i + 1 < anchors.Count ? anchors[i + 1] : null;
                var segment = GetSegment(row, anchor, nextAnchor);

                if (!TryParseMicrobialRow(anchor, segment, out var parsedRow))
                    continue;

                parsed++;

                switch (EvaluateMicrobialRow(parsedRow))
                {
                    case ContaminantRowOutcome.Pass:
                        passing++;
                        break;
                    case ContaminantRowOutcome.Fail:
                        failing++;
                        break;
                    case ContaminantRowOutcome.Unknown:
                        unknown++;
                        break;
                }
            }
        }

        result = new ContaminantTableResult(parsed, passing, failing, unknown);
        return true;
    }

    private static bool TryParseMicrobialRow(AnalyteAnchor anchor, string segment, out ParsedMicrobialRow row)
    {
        row = new ParsedMicrobialRow(false, null, null, new MicrobialResult(null, null), string.Empty);

        if (QuantitativeMicrobialNames.Contains(anchor.CanonicalName) &&
            TryParseStandardContaminantRow(segment, out var quantitative))
        {
            row = new ParsedMicrobialRow(true, quantitative.Loq, quantitative.Limit, new MicrobialResult(quantitative.Mass.Value, quantitative.Mass.Qualifier), quantitative.Status);
            return true;
        }

        var match = BinaryMicrobialRegex.Match(segment);

        if (!match.Success)
            return false;

        row = new ParsedMicrobialRow(false, null, null, new MicrobialResult(null, NormalizeToken(match.Groups["result"].Value)), match.Groups["status"].Value.ToLowerInvariant());
        return true;
    }

    private static ContaminantRowOutcome EvaluateMicrobialRow(ParsedMicrobialRow row)
    {
        if (row.Status.Equals("fail", StringComparison.OrdinalIgnoreCase))
            return ContaminantRowOutcome.Fail;

        if (row.Status.Equals("nt", StringComparison.OrdinalIgnoreCase) ||
            row.Status.Equals("nr", StringComparison.OrdinalIgnoreCase))
        {
            return ContaminantRowOutcome.Unknown;
        }

        if (row.IsQuantitative)
        {
            if (!row.Status.Equals("pass", StringComparison.OrdinalIgnoreCase))
                return ContaminantRowOutcome.Unknown;

            if (row.Result.Qualifier is "NR" or "NT")
                return ContaminantRowOutcome.Unknown;

            if (row.Result.Qualifier == "ND" || row.Result.Qualifier == "<LOQ" || row.Result.Qualifier == "<LOD")
                return ContaminantRowOutcome.Pass;

            if (row.Result.Value is decimal value && row.Limit is decimal limit)
                return value > limit ? ContaminantRowOutcome.Fail : ContaminantRowOutcome.Pass;

            return ContaminantRowOutcome.Unknown;
        }

        var qualifier = row.Result.Qualifier ?? string.Empty;

        if (qualifier is "POSITIVE" or "DETECTED")
            return ContaminantRowOutcome.Fail;

        if (qualifier is "NR" or "NT")
            return ContaminantRowOutcome.Unknown;

        if (row.Status.Equals("pass", StringComparison.OrdinalIgnoreCase) &&
            (qualifier is "ND" or "NEGATIVE" or "NOTDETECTED"))
        {
            return ContaminantRowOutcome.Pass;
        }

        return ContaminantRowOutcome.Unknown;
    }

    private static ContaminantRowOutcome EvaluateContaminantRow(ParsedContaminantRow row)
    {
        if (row.Status.Equals("fail", StringComparison.OrdinalIgnoreCase))
            return ContaminantRowOutcome.Fail;

        if (row.Status.Equals("nt", StringComparison.OrdinalIgnoreCase) ||
            row.Status.Equals("nr", StringComparison.OrdinalIgnoreCase))
        {
            return ContaminantRowOutcome.Unknown;
        }

        if (!row.Status.Equals("pass", StringComparison.OrdinalIgnoreCase))
            return ContaminantRowOutcome.Unknown;

        if (!string.IsNullOrWhiteSpace(row.Mass.Qualifier))
            return row.Mass.Qualifier is "NR" or "NT"
                ? ContaminantRowOutcome.Unknown
                : ContaminantRowOutcome.Pass;

        if (row.Mass.Value is not decimal mass)
            return ContaminantRowOutcome.Unknown;

        if (row.Limit.HasValue)
            return mass > row.Limit.Value ? ContaminantRowOutcome.Fail : ContaminantRowOutcome.Pass;

        return ContaminantRowOutcome.Unknown;
    }

    private static ContaminantMass ParseContaminantMass(string raw)
    {
        var normalized = NormalizeToken(raw);

        if (normalized is "<LOQ" or "<LOD" or ">LOD" or "ND" or "NR" or "NT")
            return new ContaminantMass(null, normalized);

        return decimal.TryParse(raw.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? new ContaminantMass(value, null)
            : new ContaminantMass(null, normalized);
    }

    private static decimal? ParseOptionalDecimal(string raw)
    {
        return decimal.TryParse(raw.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static List<string> ExtractSectionRows(
        string text,
        Func<string, bool> isSectionStart,
        Func<string, bool> isSectionEnd,
        Func<string, bool> looksLikeRow)
    {
        var rows = NormalizeRows(text);
        var sectionRows = new List<string>();
        var inSection = false;

        foreach (var row in rows)
        {
            if (isSectionStart(row))
            {
                inSection = true;
                sectionRows.Add(row);
                continue;
            }

            if (inSection && isSectionEnd(row))
                inSection = false;

            if (inSection || looksLikeRow(row))
                sectionRows.Add(row);
        }

        return sectionRows;
    }

    private static IReadOnlyList<AnalyteAnchor> FindAnchors(string row, AnalyteDefinition[] definitions)
    {
        var anchors = new List<AnalyteAnchor>();

        foreach (var definition in definitions)
        {
            foreach (var alias in definition.Aliases)
            {
                var pattern = BuildAnalytePattern(alias);

                foreach (Match match in Regex.Matches(row, pattern, RegexOptions.IgnoreCase))
                {
                    if (!match.Success)
                        continue;

                    anchors.Add(new AnalyteAnchor(match.Index, match.Index + match.Length, definition.Kind, definition.CanonicalName, match.Value));
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

    private static List<AnalyteAnchor> AddNonOverlappingAnchor(List<AnalyteAnchor> anchors, AnalyteAnchor candidate)
    {
        if (anchors.Any(anchor => anchor.StartIndex < candidate.EndIndex && candidate.StartIndex < anchor.EndIndex))
            return anchors;

        anchors.Add(candidate);
        return anchors;
    }

    private static string GetSegment(string row, AnalyteAnchor anchor, AnalyteAnchor? nextAnchor)
    {
        var segmentEnd = nextAnchor?.StartIndex ?? row.Length;
        return segmentEnd <= anchor.EndIndex ? string.Empty : row[anchor.EndIndex..segmentEnd];
    }

    private static IReadOnlyList<ResultToken> ExtractResultTokens(string segment, int maxTokens)
    {
        return ResultTokenRegex.Matches(segment)
            .Cast<Match>()
            .Where(match => match.Success)
            .Take(maxTokens)
            .Select(ToResultToken)
            .Where(token => token != null)
            .Select(token => token!)
            .ToList();
    }

    private static ResultToken? ToResultToken(Match match)
    {
        var raw = Regex.Replace(match.Groups["raw"].Value.Trim(), @"\s+", string.Empty);

        if (raw.StartsWith("<", StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith(">", StringComparison.OrdinalIgnoreCase) ||
            raw.Equals("ND", StringComparison.OrdinalIgnoreCase) ||
            raw.Equals("NR", StringComparison.OrdinalIgnoreCase) ||
            raw.Equals("NT", StringComparison.OrdinalIgnoreCase))
        {
            return new ResultToken(raw.ToUpperInvariant(), null, true);
        }

        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? new ResultToken(raw, value, false)
            : null;
    }

    private static bool QualifiedTokensMatch(ResultToken left, ResultToken right)
    {
        return NormalizeToken(left.RawText).Equals(NormalizeToken(right.RawText), StringComparison.OrdinalIgnoreCase);
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

    private static bool IsValidPercentMgPair(decimal percent, decimal mgPerGram)
    {
        var expectedMgPerGram = percent * 10m;
        var tolerance = Math.Max(0.01m, Math.Abs(mgPerGram) * 0.01m);

        return Math.Abs(expectedMgPerGram - mgPerGram) <= tolerance;
    }

    private static bool LooksLikeAnalyteRow(string row, AnalyteKind kind, AnalyteDefinition[] definitions)
    {
        return FindAnchors(row, definitions).Any(anchor => anchor.Kind == kind);
    }

    private static bool LooksLikeAnyAnalyteRow(string row, AnalyteDefinition[] definitions)
    {
        return FindAnchors(row, definitions).Count > 0;
    }

    private static bool IsCannabinoidSectionStart(string row)
    {
        return row.Contains("Cannabinoid Test Results", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCannabinoidSectionEnd(string row)
    {
        return row.Contains("Terpene Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Total Potential THC", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Safety & Quality Tests", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeAceFlower(string text)
    {
        var rows = NormalizeRows(text);

        return rows.Any(row => Regex.IsMatch(row, @"\bPlant\s*,?\s*Flower\b|\bFlower\s*-\s*Cured\b", RegexOptions.IgnoreCase)) ||
               (rows.Any(IsCannabinoidSectionStart) && rows.Any(IsTerpeneSectionStart));
    }

    private static bool IsTerpeneSectionStart(string row)
    {
        return row.Contains("Terpene Test Results", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTerpeneSectionEnd(string row)
    {
        return row.Contains("Safety & Quality Tests", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Pesticides", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Heavy Metals", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Microbials", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Mycotoxins", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Cannabinoid Test Results", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPesticideSectionStart(string row)
    {
        return row.Contains("Pesticides", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPesticideSectionEnd(string row)
    {
        return row.Contains("Heavy Metals", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Microbials", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Mycotoxins", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHeavyMetalSectionStart(string row)
    {
        return row.Contains("Heavy Metals", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHeavyMetalSectionEnd(string row)
    {
        return row.Contains("Microbials", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Mycotoxins", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Pesticides", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMicrobialSectionStart(string row)
    {
        return row.Contains("Microbials", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMicrobialSectionEnd(string row)
    {
        return row.Contains("Heavy Metals", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Mycotoxins", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Pesticides", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMycotoxinSectionStart(string row)
    {
        return row.Contains("Mycotoxins", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMycotoxinSectionEnd(string row)
    {
        return row.Contains("Residual Solvents", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Solvents", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Heavy Metals", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Microbials", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Pesticides", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseExplicitOverallComplianceStatus(string text, out ComplianceResult compliance)
    {
        compliance = UnknownCompliance();

        foreach (var row in NormalizeRows(text))
        {
            if (!IsExplicitOverallComplianceRow(row))
                continue;

            if (row.Contains("FAIL", StringComparison.OrdinalIgnoreCase))
            {
                compliance = FailedCompliance();
                return true;
            }

            if (row.Contains("PASS", StringComparison.OrdinalIgnoreCase))
            {
                compliance = new ComplianceResult { Passed = true, ContaminantsPassed = true, Status = "pass" };
                return true;
            }
        }

        return false;
    }

    private static bool IsExplicitOverallComplianceRow(string row)
    {
        return row.Contains("Overall Result", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Final Result", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Overall Status", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Compliance Status", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Result Status", StringComparison.OrdinalIgnoreCase);
    }

    private static CannabinoidProfile CreateEmptyCannabinoidProfile()
    {
        return new CannabinoidProfile
        {
            THC = EmptyField("THC"),
            THCA = EmptyField("THCA"),
            CBD = EmptyField("CBD"),
            CBDA = EmptyField("CBDA")
        };
    }

    private static ParsedField<decimal> EmptyField(string fieldName)
    {
        return new ParsedField<decimal>
        {
            FieldName = fieldName,
            Value = 0m,
            SourceText = string.Empty,
            Confidence = 0m
        };
    }

    private static bool HasCannabinoidResult(CannabinoidProfile profile)
    {
        return profile.THC.Confidence > 0m ||
               profile.THCA.Confidence > 0m ||
               profile.CBD.Confidence > 0m ||
               profile.CBDA.Confidence > 0m;
    }

    private static List<string> NormalizeRows(string text)
    {
        var rows = text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n')
            .Select(row => Regex.Replace(row.Trim(), @"\s+", " "))
            .Where(row => !string.IsNullOrWhiteSpace(row))
            .ToList();

        for (var i = 0; i < rows.Count - 1; i++)
        {
            if (rows[i].Equals("Piperonyl", StringComparison.OrdinalIgnoreCase) &&
                rows[i + 1].StartsWith("Butoxide", StringComparison.OrdinalIgnoreCase))
            {
                rows[i] = $"{rows[i]} {rows[i + 1]}";
                rows.RemoveAt(i + 1);
                break;
            }
        }

        return rows;
    }

    private static string NormalizeToken(string token)
    {
        return Regex.Replace(token.Trim(), @"\s+", string.Empty).ToUpperInvariant();
    }

    private static ParsedContaminantRow EmptyContaminantRow()
    {
        return new ParsedContaminantRow(0m, null, new ContaminantMass(null, null), string.Empty);
    }

    private static ComplianceResult UnknownCompliance()
    {
        return new ComplianceResult { Passed = false, ContaminantsPassed = null, Status = "unknown" };
    }

    private static ComplianceResult FailedCompliance()
    {
        return new ComplianceResult { Passed = false, ContaminantsPassed = false, Status = "fail" };
    }

    private delegate bool TryParseContaminantRow(string segment, out ParsedContaminantRow row);

    private sealed record AnalyteDefinition(string CanonicalName, AnalyteKind Kind, string[] Aliases);

    private sealed record AnalyteAnchor(int StartIndex, int EndIndex, AnalyteKind Kind, string CanonicalName, string RawText);

    private sealed record ResultToken(string RawText, decimal? Value, bool IsQualified);

    private sealed record ContaminantMass(decimal? Value, string? Qualifier);

    private sealed record ParsedContaminantRow(decimal Loq, decimal? Limit, ContaminantMass Mass, string Status);

    private sealed record ParsedMicrobialRow(bool IsQuantitative, decimal? Loq, decimal? Limit, MicrobialResult Result, string Status);

    private sealed record MicrobialResult(decimal? Value, string? Qualifier);

    private sealed record ContaminantTableResult(int ParsedRowCount, int PassingRowCount, int FailingRowCount, int UnknownRowCount);

    private enum AnalyteKind
    {
        Cannabinoid,
        Terpene,
        Pesticide,
        HeavyMetal,
        Microbial,
        Mycotoxin
    }

    private enum ContaminantRowOutcome
    {
        Pass,
        Fail,
        Unknown
    }
}
