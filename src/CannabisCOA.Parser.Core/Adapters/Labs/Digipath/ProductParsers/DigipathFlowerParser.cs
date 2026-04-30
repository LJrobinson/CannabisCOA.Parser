using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CannabisCOA.Parser.Core.Calculators;
using CannabisCOA.Parser.Core.Enums;
using CannabisCOA.Parser.Core.Mappers;
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
        @"<\s*LOQ|ND|NR|NT|Not\s+Detected|(?<prefix><)?\s*(?<value>\d{1,4}(?:\.\d+)?|\.\d+)\s*(?<unit>%|mg\s*/\s*g|mg/g|mg\/g)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly AnalyteDefinition[] AnalyteDefinitions =
    [
        new("THC", AnalyteKind.Cannabinoid, ["Δ9-THC", "D9-THC", "Delta-9 THC", "Delta 9 THC"]),
        new("D8-THC", AnalyteKind.Cannabinoid, ["Δ8-THC", "D8-THC", "Delta-8 THC", "Delta 8 THC"]),
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

    private static readonly AnalyteDefinition[] PesticideTableDefinitions =
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
        new("Trifloxystrobin", AnalyteKind.Pesticide, ["Trifloxystrobin"]),
        new("Aerobic Bacteria", AnalyteKind.Microbial, ["Aerobic Bacteria"]),
        new("Bile-Tolerant Gram-Negative Bacteria", AnalyteKind.Microbial, ["Bile-Tolerant Gram-Negative Bacteria"]),
        new("Coliforms", AnalyteKind.Microbial, ["Coliforms"]),
        new("Yeast & Mold", AnalyteKind.Microbial, ["Yeast & Mold"]),
        new("Powdery Mildew", AnalyteKind.Microbial, ["Powdery Mildew"]),
        new("STEC E. coli", AnalyteKind.Microbial, ["STEC E. coli"]),
        new("Salmonella", AnalyteKind.Microbial, ["Salmonella"]),
        new("Aspergillus niger", AnalyteKind.Microbial, ["Aspergillus niger"]),
        new("Aspergillus flavus", AnalyteKind.Microbial, ["Aspergillus flavus"]),
        new("Aspergillus fumigatus", AnalyteKind.Microbial, ["Aspergillus fumigatus"]),
        new("Aspergillus terreus", AnalyteKind.Microbial, ["Aspergillus terreus"])
    ];

    private static readonly AnalyteDefinition[] HeavyMetalTableDefinitions =
    [
        new("Arsenic", AnalyteKind.HeavyMetal, ["Arsenic"]),
        new("Cadmium", AnalyteKind.HeavyMetal, ["Cadmium"]),
        new("Lead", AnalyteKind.HeavyMetal, ["Lead"]),
        new("Mercury", AnalyteKind.HeavyMetal, ["Mercury"])
    ];

    private static readonly AnalyteDefinition[] MycotoxinTableDefinitions =
    [
        new("Aflatoxins", AnalyteKind.Mycotoxin, ["Aflatoxins"]),
        new("Aflatoxin B1", AnalyteKind.Mycotoxin, ["Aflatoxin B1"]),
        new("Aflatoxin B2", AnalyteKind.Mycotoxin, ["Aflatoxin B2"]),
        new("Aflatoxin G1", AnalyteKind.Mycotoxin, ["Aflatoxin G1"]),
        new("Aflatoxin G2", AnalyteKind.Mycotoxin, ["Aflatoxin G2"]),
        new("Ochratoxin A", AnalyteKind.Mycotoxin, ["Ochratoxin A"])
    ];

    private static readonly HashSet<string> QuantitativeMicrobialNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Aerobic Bacteria",
        "Bile-Tolerant Gram-Negative Bacteria",
        "Coliforms",
        "Yeast & Mold",
        "Powdery Mildew"
    };

    private static readonly HashSet<string> BinaryMicrobialNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "STEC E. coli",
        "Salmonella",
        "Aspergillus niger",
        "Aspergillus flavus",
        "Aspergillus fumigatus",
        "Aspergillus terreus"
    };

    private static readonly Regex ResultWindowTokenRegex = new(
        @"(?<![\p{L}\p{N}.-])(?<raw><\s*LOQ|ND|NR|NT|Not\s+Detected|\d{1,4}(?:\.\d+)?|\.\d+)(?![\p{L}\p{N}.-])",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PesticideColumnRegex = new(
        @"^\s*(?<loq>\d{1,4}(?:\.\d+)?|\.\d+)\s+(?<limit>\d{1,4}(?:\.\d+)?|\.\d+)\s+(?<mass><\s*LOQ|<\s*LOD|ND|NR|\d{1,4}(?:\.\d+)?|\.\d+)\s+(?<status>Pass|Fail|NT|NR)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AcceptedPesticideMassQualifierRegex = new(
        @"^(?:<\s*LOQ|<\s*LOD|ND|NR)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex HeavyMetalColumnRegex = new(
        @"^\s*(?<loq>\d{1,4}(?:\.\d+)?|\.\d+)\s+(?<limit>\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<result><\s*LOQ|<\s*LOD|ND|NR|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<status>Pass|Fail|NT|NR)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AcceptedHeavyMetalResultQualifierRegex = new(
        @"^(?:<\s*LOQ|<\s*LOD|ND|NR)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MycotoxinColumnRegex = new(
        @"^\s*(?<loq>\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<limit>\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<mass><\s*LOQ|<\s*LOD|ND|NR|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<status>Pass|Fail|NT|NR)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AcceptedMycotoxinMassQualifierRegex = new(
        @"^(?:<\s*LOQ|<\s*LOD|ND|NR)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex QuantitativeMicrobialColumnRegex = new(
        @"^\s*(?<loq>\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<limit>\d{1,6}(?:\.\d+)?|\.\d+|NR|NT)\s+(?<result><\s*\d{1,6}(?:\.\d+)?|<\s*LOQ|<\s*LOD|ND|NR|NT|\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<status>Pass|Fail|NT|NR)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex QuantitativeMicrobialExceptionRegex = new(
        @"^\s*(?<loq>\d{1,6}(?:\.\d+)?|\.\d+)\s+(?<result>NR|NT)\s+(?<status>NT|NR|Pass|Fail)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex BinaryMicrobialColumnRegex = new(
        @"^\s*(?<result>Not\s+Detected|Negative|Positive|Detected|NR|NT)\s+(?<status>Pass|Fail|NT|NR)(?=\s|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    //LEGACY PARSING RETURN
    public static CoaResult Parse(string text, string labName)
    {
        var productType = ProductTypeDetector.Detect(text);

        var cannabinoids = ParseDigipathCannabinoidsOrFallback(text, productType);

        var testDate = GenericDateParser.ExtractTestDate(text);
        var freshness = FreshnessCalculator.Calculate(testDate);
        var compliance = ParseDigipathComplianceOrFallback(text);
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


    //FUTURE COA PARSING RESULT
    public static CoaDocument ParseDocument(
        string text,
        string labName,
        string? sourceFileName = null)
    {
        var result = Parse(text, labName);

        return CoaDocumentMapper.FromCoaResult(
            result,
            sourceFileName,
            parserName: nameof(DigipathFlowerParser));
    }

    private static CannabinoidProfile ParseDigipathCannabinoidsOrFallback(string text, ProductType productType)
    {
        if (TryParseDigipathCannabinoidTable(text, productType, out var profile))
            return profile;

        if (HasDigipathCannabinoidTableContext(NormalizeRows(text)))
            return profile;

        var generic = GenericCannabinoidTextParser.Parse(text);
        CannabinoidCalculator.CalculateTotals(generic);
        return generic;
    }

    private static TerpeneProfile ParseDigipathTerpenesOrFallback(string text)
    {
        if (TryParseDigipathTerpeneTable(text, out var profile))
            return profile;

        return GenericTerpeneTextParser.Parse(text);
    }

    private static ComplianceResult ParseDigipathComplianceOrFallback(string text)
    {
        var generic = ComplianceParser.Parse(text);
        var hasExplicitOverallCompliance = TryParseExplicitOverallComplianceStatus(
            text,
            out var explicitOverallCompliance);
        var hasPesticideContext = TryParseDigipathPesticideTable(text, out var pesticideTable);
        var hasHeavyMetalContext = TryParseDigipathHeavyMetalTable(text, out var heavyMetalTable);
        var hasMicrobialContext = TryParseDigipathMicrobialTable(text, out var microbialTable);
        var hasMycotoxinContext = TryParseDigipathMycotoxinTable(text, out var mycotoxinTable);
        var hasPesticideTable = hasPesticideContext && pesticideTable.ParsedRowCount > 0;
        var hasHeavyMetalTable = hasHeavyMetalContext && heavyMetalTable.ParsedRowCount > 0;
        var hasMicrobialTable = hasMicrobialContext && microbialTable.ParsedRowCount > 0;
        var hasMycotoxinTable = hasMycotoxinContext && mycotoxinTable.ParsedRowCount > 0;

        if (!hasPesticideContext && !hasHeavyMetalContext && !hasMicrobialContext && !hasMycotoxinContext)
            return generic;

        if (!hasPesticideTable && !hasHeavyMetalTable && !hasMicrobialTable && !hasMycotoxinTable)
        {
            if (hasExplicitOverallCompliance)
                return explicitOverallCompliance;

            return new ComplianceResult
            {
                Passed = false,
                ContaminantsPassed = null,
                Status = "unknown"
            };
        }

        if ((hasPesticideTable && pesticideTable.FailingRowCount > 0) ||
            (hasHeavyMetalTable && heavyMetalTable.FailingRowCount > 0) ||
            (hasMicrobialTable && microbialTable.FailingRowCount > 0) ||
            (hasMycotoxinTable && mycotoxinTable.FailingRowCount > 0))
        {
            return new ComplianceResult
            {
                Passed = false,
                ContaminantsPassed = false,
                Status = "fail"
            };
        }

        var hasUnknownRows =
            (hasPesticideTable && pesticideTable.UnknownRowCount > 0) ||
            (hasHeavyMetalTable && heavyMetalTable.UnknownRowCount > 0) ||
            (hasMicrobialTable && microbialTable.UnknownRowCount > 0) ||
            (hasMycotoxinTable && mycotoxinTable.UnknownRowCount > 0);

        if (hasUnknownRows)
        {
            if (hasExplicitOverallCompliance)
                return explicitOverallCompliance;

            return new ComplianceResult
            {
                Passed = false,
                ContaminantsPassed = null,
                Status = "unknown"
            };
        }

        if (hasExplicitOverallCompliance)
            return explicitOverallCompliance;

        return new ComplianceResult
        {
            Passed = false,
            ContaminantsPassed = true,
            Status = "unknown"
        };
    }

    private static bool TryParseExplicitOverallComplianceStatus(string text, out ComplianceResult compliance)
    {
        compliance = new ComplianceResult
        {
            Passed = false,
            ContaminantsPassed = null,
            Status = "unknown"
        };

        foreach (var row in NormalizeRows(text))
        {
            if (!IsExplicitOverallComplianceRow(row))
                continue;

            if (row.Contains("FAIL", StringComparison.OrdinalIgnoreCase))
            {
                compliance = new ComplianceResult
                {
                    Passed = false,
                    ContaminantsPassed = false,
                    Status = "fail"
                };

                return true;
            }

            if (row.Contains("PASS", StringComparison.OrdinalIgnoreCase))
            {
                compliance = new ComplianceResult
                {
                    Passed = true,
                    ContaminantsPassed = true,
                    Status = "pass"
                };

                return true;
            }
        }

        return false;
    }

    private static bool TryParseDigipathMycotoxinTable(string text, out DigipathMycotoxinTableResult result)
    {
        result = new DigipathMycotoxinTableResult(0, 0, 0, 0);

        var rows = ExtractMycotoxinSectionRows(text);

        if (rows.Count == 0)
            return false;

        var hasTableContext = rows.Any(IsMycotoxinSectionStart) ||
                              rows.Any(row => FindMycotoxinAnchors(row)
                                  .Any(anchor => anchor.Kind == AnalyteKind.Mycotoxin));

        if (!hasTableContext)
            return false;

        var parsedRowCount = 0;
        var passingRowCount = 0;
        var failingRowCount = 0;
        var unknownRowCount = 0;

        foreach (var row in rows)
        {
            if (!LooksLikeSafeAnalyteRow(row))
                continue;

            var anchors = FindMycotoxinAnchors(row);

            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];

                if (anchor.Kind != AnalyteKind.Mycotoxin)
                    continue;

                var nextAnchor = i + 1 < anchors.Count ? anchors[i + 1] : null;

                if (!TryParseMycotoxinAnchorWindow(row, anchor, nextAnchor, out var mycotoxinRow))
                    continue;

                parsedRowCount++;

                switch (EvaluateMycotoxinRow(mycotoxinRow))
                {
                    case MycotoxinRowOutcome.Pass:
                        passingRowCount++;
                        break;
                    case MycotoxinRowOutcome.Fail:
                        failingRowCount++;
                        break;
                    case MycotoxinRowOutcome.Unknown:
                        unknownRowCount++;
                        break;
                }
            }
        }

        result = new DigipathMycotoxinTableResult(
            parsedRowCount,
            passingRowCount,
            failingRowCount,
            unknownRowCount);

        return hasTableContext;
    }

    private static List<string> ExtractMycotoxinSectionRows(string text)
    {
        var rows = NormalizeRows(text);
        var sectionRows = new List<string>();
        var inMycotoxinSection = false;

        foreach (var row in rows)
        {
            if (IsMycotoxinSectionStart(row))
            {
                inMycotoxinSection = true;
                sectionRows.Add(row);
                continue;
            }

            if (inMycotoxinSection && IsMycotoxinSectionEnd(row))
                inMycotoxinSection = false;

            if (inMycotoxinSection || LooksLikeMycotoxinTableRow(row))
                sectionRows.Add(row);
        }

        return sectionRows;
    }

    private static IReadOnlyList<AnalyteAnchor> FindMycotoxinAnchors(string row)
    {
        var anchors = new List<AnalyteAnchor>();

        foreach (var definition in MycotoxinTableDefinitions)
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

    private static bool TryParseMycotoxinAnchorWindow(
        string row,
        AnalyteAnchor anchor,
        AnalyteAnchor? nextAnchor,
        out ParsedDigipathMycotoxinRow rowResult)
    {
        rowResult = new ParsedDigipathMycotoxinRow(
            string.Empty,
            0m,
            0m,
            new MycotoxinMassResult(null, null),
            string.Empty,
            string.Empty);

        if (anchor.Kind != AnalyteKind.Mycotoxin)
            return false;

        var segmentEnd = nextAnchor?.StartIndex ?? row.Length;

        if (segmentEnd <= anchor.EndIndex)
            return false;

        var segment = row[anchor.EndIndex..segmentEnd];

        if (!TryParseMycotoxinResultColumns(segment, out var loq, out var limit, out var mass, out var status))
            return false;

        rowResult = new ParsedDigipathMycotoxinRow(
            anchor.CanonicalName,
            loq,
            limit,
            mass,
            status,
            row);

        return true;
    }

    private static bool TryParseMycotoxinResultColumns(
        string segment,
        out decimal loq,
        out decimal limit,
        out MycotoxinMassResult mass,
        out string status)
    {
        loq = 0m;
        limit = 0m;
        mass = new MycotoxinMassResult(null, null);
        status = string.Empty;

        var match = MycotoxinColumnRegex.Match(segment);

        if (!match.Success)
            return false;

        if (!decimal.TryParse(match.Groups["loq"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out loq))
            return false;

        if (!decimal.TryParse(match.Groups["limit"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out limit))
            return false;

        var rawMass = match.Groups["mass"].Value.Trim();

        if (AcceptedMycotoxinMassQualifierRegex.IsMatch(rawMass))
        {
            mass = new MycotoxinMassResult(null, NormalizeQualifiedToken(rawMass));
        }
        else
        {
            if (!decimal.TryParse(rawMass, NumberStyles.Number, CultureInfo.InvariantCulture, out var massValue))
                return false;

            mass = new MycotoxinMassResult(massValue, null);
        }

        status = match.Groups["status"].Value.ToLowerInvariant();
        return true;
    }

    private static MycotoxinRowOutcome EvaluateMycotoxinRow(ParsedDigipathMycotoxinRow row)
    {
        if (row.Status.Equals("fail", StringComparison.OrdinalIgnoreCase))
            return MycotoxinRowOutcome.Fail;

        if (row.Status.Equals("nt", StringComparison.OrdinalIgnoreCase) ||
            row.Status.Equals("nr", StringComparison.OrdinalIgnoreCase))
        {
            return MycotoxinRowOutcome.Unknown;
        }

        if (!row.Status.Equals("pass", StringComparison.OrdinalIgnoreCase))
            return MycotoxinRowOutcome.Unknown;

        if (row.Mass.Value is decimal massValue)
        {
            if (row.Limit > 0m && massValue > row.Limit)
                return MycotoxinRowOutcome.Fail;

            if (row.Limit == 0m && massValue > 0m)
                return MycotoxinRowOutcome.Fail;

            return MycotoxinRowOutcome.Pass;
        }

        if (!string.IsNullOrWhiteSpace(row.Mass.Qualifier))
            return MycotoxinRowOutcome.Pass;

        return MycotoxinRowOutcome.Unknown;
    }

    private static bool TryParseDigipathMicrobialTable(string text, out DigipathMicrobialTableResult result)
    {
        result = new DigipathMicrobialTableResult(0, 0, 0, 0);

        var rows = ExtractMicrobialSectionRows(text);

        if (rows.Count == 0)
            return false;

        var hasTableContext = rows.Any(IsMicrobialSectionStart) ||
                              rows.Any(row => FindMicrobialTableAnchors(row)
                                  .Any(anchor => anchor.Kind == AnalyteKind.Microbial));

        if (!hasTableContext)
            return false;

        var parsedRowCount = 0;
        var passingRowCount = 0;
        var failingRowCount = 0;
        var unknownRowCount = 0;

        foreach (var row in rows)
        {
            if (!LooksLikeSafeAnalyteRow(row))
                continue;

            var anchors = FindMicrobialTableAnchors(row);

            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];

                if (anchor.Kind != AnalyteKind.Microbial)
                    continue;

                var nextAnchor = i + 1 < anchors.Count ? anchors[i + 1] : null;

                if (!TryParseMicrobialAnchorWindow(row, anchor, nextAnchor, out var microbialRow))
                    continue;

                parsedRowCount++;

                switch (EvaluateMicrobialRow(microbialRow))
                {
                    case MicrobialRowOutcome.Pass:
                        passingRowCount++;
                        break;
                    case MicrobialRowOutcome.Fail:
                        failingRowCount++;
                        break;
                    case MicrobialRowOutcome.Unknown:
                        unknownRowCount++;
                        break;
                }
            }
        }

        result = new DigipathMicrobialTableResult(
            parsedRowCount,
            passingRowCount,
            failingRowCount,
            unknownRowCount);

        return hasTableContext;
    }

    private static List<string> ExtractMicrobialSectionRows(string text)
    {
        var rows = NormalizeRows(text);
        var sectionRows = new List<string>();
        var inMicrobialSection = false;

        foreach (var row in rows)
        {
            if (IsMicrobialSectionStart(row))
            {
                inMicrobialSection = true;
                sectionRows.Add(row);
                continue;
            }

            if (inMicrobialSection && IsMicrobialSectionEnd(row))
                inMicrobialSection = false;

            if (inMicrobialSection || LooksLikeMicrobialTableRow(row))
                sectionRows.Add(row);
        }

        return sectionRows;
    }

    private static IReadOnlyList<AnalyteAnchor> FindMicrobialTableAnchors(string row)
    {
        var anchors = new List<AnalyteAnchor>();

        foreach (var definition in PesticideTableDefinitions)
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

    private static bool TryParseMicrobialAnchorWindow(
        string row,
        AnalyteAnchor anchor,
        AnalyteAnchor? nextAnchor,
        out ParsedDigipathMicrobialRow rowResult)
    {
        rowResult = new ParsedDigipathMicrobialRow(
            string.Empty,
            MicrobialRowType.Quantitative,
            null,
            null,
            new MicrobialResult(null, null, null, null),
            string.Empty,
            string.Empty);

        if (anchor.Kind != AnalyteKind.Microbial)
            return false;

        var segmentEnd = nextAnchor?.StartIndex ?? row.Length;

        if (segmentEnd <= anchor.EndIndex)
            return false;

        var segment = row[anchor.EndIndex..segmentEnd];

        if (QuantitativeMicrobialNames.Contains(anchor.CanonicalName) &&
            TryParseQuantitativeMicrobialColumns(segment, out var loq, out var limit, out var quantitativeResult, out var quantitativeStatus))
        {
            rowResult = new ParsedDigipathMicrobialRow(
                anchor.CanonicalName,
                MicrobialRowType.Quantitative,
                loq,
                limit,
                quantitativeResult,
                quantitativeStatus,
                row);

            return true;
        }

        if (BinaryMicrobialNames.Contains(anchor.CanonicalName) &&
            TryParseBinaryMicrobialColumns(segment, out var binaryResult, out var binaryStatus))
        {
            rowResult = new ParsedDigipathMicrobialRow(
                anchor.CanonicalName,
                MicrobialRowType.Binary,
                null,
                null,
                binaryResult,
                binaryStatus,
                row);

            return true;
        }

        return false;
    }

    private static bool TryParseQuantitativeMicrobialColumns(
        string segment,
        out decimal? loq,
        out decimal? limit,
        out MicrobialResult result,
        out string status)
    {
        loq = null;
        limit = null;
        result = new MicrobialResult(null, null, null, null);
        status = string.Empty;

        var match = QuantitativeMicrobialColumnRegex.Match(segment);

        if (!match.Success)
        {
            match = QuantitativeMicrobialExceptionRegex.Match(segment);

            if (!match.Success)
                return false;
        }

        if (!decimal.TryParse(match.Groups["loq"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedLoq))
            return false;

        loq = parsedLoq;

        if (match.Groups["limit"].Success &&
            decimal.TryParse(match.Groups["limit"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedLimit))
        {
            limit = parsedLimit;
        }

        if (!TryParseMicrobialResultToken(match.Groups["result"].Value, out result))
            return false;

        status = match.Groups["status"].Value.ToLowerInvariant();
        return true;
    }

    private static bool TryParseBinaryMicrobialColumns(
        string segment,
        out MicrobialResult result,
        out string status)
    {
        result = new MicrobialResult(null, null, null, null);
        status = string.Empty;

        var match = BinaryMicrobialColumnRegex.Match(segment);

        if (!match.Success)
            return false;

        var binaryValue = Regex.Replace(match.Groups["result"].Value.Trim(), @"\s+", " ");
        result = new MicrobialResult(null, null, null, binaryValue.ToUpperInvariant());
        status = match.Groups["status"].Value.ToLowerInvariant();
        return true;
    }

    private static bool TryParseMicrobialResultToken(string rawResult, out MicrobialResult result)
    {
        result = new MicrobialResult(null, null, null, null);
        var normalized = Regex.Replace(rawResult.Trim(), @"\s+", string.Empty).ToUpperInvariant();

        if (normalized is "NR" or "NT" or "ND")
        {
            result = new MicrobialResult(null, null, normalized, null);
            return true;
        }

        var lessThanMatch = Regex.Match(rawResult, @"^<\s*(?<value>\d{1,6}(?:\.\d+)?|\.\d+)$", RegexOptions.IgnoreCase);

        if (lessThanMatch.Success &&
            decimal.TryParse(lessThanMatch.Groups["value"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var lessThanValue))
        {
            result = new MicrobialResult(null, lessThanValue, null, null);
            return true;
        }

        if (Regex.IsMatch(rawResult, @"^<\s*(LOQ|LOD)$", RegexOptions.IgnoreCase))
        {
            result = new MicrobialResult(null, null, normalized, null);
            return true;
        }

        if (!decimal.TryParse(rawResult, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return false;

        result = new MicrobialResult(value, null, null, null);
        return true;
    }

    private static MicrobialRowOutcome EvaluateMicrobialRow(ParsedDigipathMicrobialRow row)
    {
        if (row.Status.Equals("fail", StringComparison.OrdinalIgnoreCase))
            return MicrobialRowOutcome.Fail;

        if (row.Status.Equals("nt", StringComparison.OrdinalIgnoreCase) ||
            row.Status.Equals("nr", StringComparison.OrdinalIgnoreCase))
        {
            return MicrobialRowOutcome.Unknown;
        }

        return row.RowType switch
        {
            MicrobialRowType.Quantitative => EvaluateQuantitativeMicrobialRow(row),
            MicrobialRowType.Binary => EvaluateBinaryMicrobialRow(row),
            _ => MicrobialRowOutcome.Unknown
        };
    }

    private static MicrobialRowOutcome EvaluateQuantitativeMicrobialRow(ParsedDigipathMicrobialRow row)
    {
        if (!row.Status.Equals("pass", StringComparison.OrdinalIgnoreCase))
            return MicrobialRowOutcome.Unknown;

        if (!row.Limit.HasValue)
            return MicrobialRowOutcome.Unknown;

        if (!string.IsNullOrWhiteSpace(row.Result.Qualifier))
            return MicrobialRowOutcome.Unknown;

        if (row.Result.Value is decimal value)
            return value > row.Limit.Value ? MicrobialRowOutcome.Fail : MicrobialRowOutcome.Pass;

        if (row.Result.LessThanValue is decimal lessThanValue)
            return lessThanValue > row.Limit.Value ? MicrobialRowOutcome.Fail : MicrobialRowOutcome.Pass;

        return MicrobialRowOutcome.Unknown;
    }

    private static MicrobialRowOutcome EvaluateBinaryMicrobialRow(ParsedDigipathMicrobialRow row)
    {
        var binaryValue = row.Result.BinaryValue ?? string.Empty;

        if (binaryValue is "POSITIVE" or "DETECTED")
            return MicrobialRowOutcome.Fail;

        if (binaryValue is "NR" or "NT")
            return MicrobialRowOutcome.Unknown;

        if (row.Status.Equals("pass", StringComparison.OrdinalIgnoreCase) &&
            (binaryValue == "NEGATIVE" || binaryValue == "NOT DETECTED"))
        {
            return MicrobialRowOutcome.Pass;
        }

        return MicrobialRowOutcome.Unknown;
    }

    private static bool TryParseDigipathHeavyMetalTable(string text, out DigipathHeavyMetalTableResult result)
    {
        result = new DigipathHeavyMetalTableResult(0, 0, 0, 0);

        var rows = ExtractHeavyMetalSectionRows(text);

        if (rows.Count == 0)
            return false;

        var hasTableContext = rows.Any(IsHeavyMetalSectionStart) ||
                              rows.Any(row => FindHeavyMetalAnchors(row)
                                  .Any(anchor => anchor.Kind == AnalyteKind.HeavyMetal));

        if (!hasTableContext)
            return false;

        var parsedRowCount = 0;
        var passingRowCount = 0;
        var failingRowCount = 0;
        var unknownRowCount = 0;

        foreach (var row in rows)
        {
            if (!LooksLikeSafeAnalyteRow(row))
                continue;

            var anchors = FindHeavyMetalAnchors(row);

            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];

                if (anchor.Kind != AnalyteKind.HeavyMetal)
                    continue;

                var nextAnchor = i + 1 < anchors.Count ? anchors[i + 1] : null;

                if (!TryParseHeavyMetalAnchorWindow(row, anchor, nextAnchor, out var heavyMetalRow))
                    continue;

                parsedRowCount++;

                switch (EvaluateHeavyMetalRow(heavyMetalRow))
                {
                    case HeavyMetalRowOutcome.Pass:
                        passingRowCount++;
                        break;
                    case HeavyMetalRowOutcome.Fail:
                        failingRowCount++;
                        break;
                    case HeavyMetalRowOutcome.Unknown:
                        unknownRowCount++;
                        break;
                }
            }
        }

        result = new DigipathHeavyMetalTableResult(
            parsedRowCount,
            passingRowCount,
            failingRowCount,
            unknownRowCount);

        return hasTableContext;
    }

    private static List<string> ExtractHeavyMetalSectionRows(string text)
    {
        var rows = NormalizeRows(text);
        var sectionRows = new List<string>();
        var inHeavyMetalSection = false;

        foreach (var row in rows)
        {
            if (IsHeavyMetalSectionStart(row))
            {
                inHeavyMetalSection = true;
                sectionRows.Add(row);
                continue;
            }

            if (inHeavyMetalSection && IsHeavyMetalSectionEnd(row))
                inHeavyMetalSection = false;

            if (inHeavyMetalSection || LooksLikeHeavyMetalTableRow(row))
                sectionRows.Add(row);
        }

        return sectionRows;
    }

    private static IReadOnlyList<AnalyteAnchor> FindHeavyMetalAnchors(string row)
    {
        var anchors = new List<AnalyteAnchor>();

        foreach (var definition in HeavyMetalTableDefinitions)
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

    private static bool TryParseHeavyMetalAnchorWindow(
        string row,
        AnalyteAnchor anchor,
        AnalyteAnchor? nextAnchor,
        out ParsedDigipathHeavyMetalRow rowResult)
    {
        rowResult = new ParsedDigipathHeavyMetalRow(
            string.Empty,
            0m,
            0m,
            new HeavyMetalResult(null, null),
            string.Empty,
            string.Empty);

        if (anchor.Kind != AnalyteKind.HeavyMetal)
            return false;

        var segmentEnd = nextAnchor?.StartIndex ?? row.Length;

        if (segmentEnd <= anchor.EndIndex)
            return false;

        var segment = row[anchor.EndIndex..segmentEnd];

        if (!TryParseHeavyMetalResultColumns(segment, out var loq, out var limit, out var result, out var status))
            return false;

        rowResult = new ParsedDigipathHeavyMetalRow(
            anchor.CanonicalName,
            loq,
            limit,
            result,
            status,
            row);

        return true;
    }

    private static bool TryParseHeavyMetalResultColumns(
        string segment,
        out decimal loq,
        out decimal limit,
        out HeavyMetalResult result,
        out string status)
    {
        loq = 0m;
        limit = 0m;
        result = new HeavyMetalResult(null, null);
        status = string.Empty;

        var match = HeavyMetalColumnRegex.Match(segment);

        if (!match.Success)
            return false;

        if (!decimal.TryParse(match.Groups["loq"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out loq))
            return false;

        if (!decimal.TryParse(match.Groups["limit"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out limit))
            return false;

        var rawResult = match.Groups["result"].Value.Trim();

        if (AcceptedHeavyMetalResultQualifierRegex.IsMatch(rawResult))
        {
            result = new HeavyMetalResult(null, NormalizeQualifiedToken(rawResult));
        }
        else
        {
            if (!decimal.TryParse(rawResult, NumberStyles.Number, CultureInfo.InvariantCulture, out var resultValue))
                return false;

            result = new HeavyMetalResult(resultValue, null);
        }

        status = match.Groups["status"].Value.ToLowerInvariant();
        return true;
    }

    private static HeavyMetalRowOutcome EvaluateHeavyMetalRow(ParsedDigipathHeavyMetalRow row)
    {
        if (row.Status.Equals("fail", StringComparison.OrdinalIgnoreCase))
            return HeavyMetalRowOutcome.Fail;

        if (row.Status.Equals("nt", StringComparison.OrdinalIgnoreCase) ||
            row.Status.Equals("nr", StringComparison.OrdinalIgnoreCase))
        {
            return HeavyMetalRowOutcome.Unknown;
        }

        if (!row.Status.Equals("pass", StringComparison.OrdinalIgnoreCase))
            return HeavyMetalRowOutcome.Unknown;

        if (row.Result.Value is decimal resultValue)
        {
            if (row.Limit > 0m && resultValue > row.Limit)
                return HeavyMetalRowOutcome.Fail;

            if (row.Limit == 0m && resultValue > 0m)
                return HeavyMetalRowOutcome.Fail;

            return HeavyMetalRowOutcome.Pass;
        }

        if (!string.IsNullOrWhiteSpace(row.Result.Qualifier))
            return HeavyMetalRowOutcome.Pass;

        return HeavyMetalRowOutcome.Unknown;
    }

    private static bool TryParseDigipathPesticideTable(string text, out DigipathPesticideTableResult result)
    {
        result = new DigipathPesticideTableResult(0, 0, 0, 0);

        var rows = ExtractPesticideSectionRows(text);

        if (rows.Count == 0)
            return false;

        var hasTableContext = rows.Any(IsPesticideSectionStart) ||
                              rows.Any(row => FindPesticideTableAnchors(row)
                                  .Any(anchor => anchor.Kind == AnalyteKind.Pesticide));

        if (!hasTableContext)
            return false;

        var parsedRowCount = 0;
        var passingRowCount = 0;
        var failingRowCount = 0;
        var unknownRowCount = 0;

        foreach (var row in rows)
        {
            if (!LooksLikeSafeAnalyteRow(row))
                continue;

            var anchors = FindPesticideTableAnchors(row);

            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];

                if (anchor.Kind != AnalyteKind.Pesticide)
                    continue;

                var nextAnchor = i + 1 < anchors.Count ? anchors[i + 1] : null;

                if (!TryParsePesticideAnchorWindow(row, anchor, nextAnchor, out var pesticideRow))
                    continue;

                parsedRowCount++;

                switch (EvaluatePesticideRow(pesticideRow))
                {
                    case PesticideRowOutcome.Pass:
                        passingRowCount++;
                        break;
                    case PesticideRowOutcome.Fail:
                        failingRowCount++;
                        break;
                    case PesticideRowOutcome.Unknown:
                        unknownRowCount++;
                        break;
                }
            }
        }

        result = new DigipathPesticideTableResult(
            parsedRowCount,
            passingRowCount,
            failingRowCount,
            unknownRowCount);

        return hasTableContext;
    }

    private static List<string> ExtractPesticideSectionRows(string text)
    {
        var rows = NormalizeRows(text);
        var sectionRows = new List<string>();
        var inPesticideSection = false;

        foreach (var row in rows)
        {
            if (IsPesticideSectionStart(row))
            {
                inPesticideSection = true;
                sectionRows.Add(row);
                continue;
            }

            if (inPesticideSection && IsPesticideSectionEnd(row))
                inPesticideSection = false;

            if (inPesticideSection || LooksLikePesticideTableRow(row))
                sectionRows.Add(row);
        }

        return sectionRows;
    }

    private static IReadOnlyList<AnalyteAnchor> FindPesticideTableAnchors(string row)
    {
        var anchors = new List<AnalyteAnchor>();

        foreach (var definition in PesticideTableDefinitions)
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

    private static bool TryParsePesticideAnchorWindow(
        string row,
        AnalyteAnchor anchor,
        AnalyteAnchor? nextAnchor,
        out ParsedDigipathPesticideRow rowResult)
    {
        rowResult = new ParsedDigipathPesticideRow(
            string.Empty,
            0m,
            0m,
            new PesticideMassResult(null, null),
            string.Empty,
            string.Empty);

        if (anchor.Kind != AnalyteKind.Pesticide)
            return false;

        var segmentEnd = nextAnchor?.StartIndex ?? row.Length;

        if (segmentEnd <= anchor.EndIndex)
            return false;

        var segment = row[anchor.EndIndex..segmentEnd];

        if (!TryParsePesticideResultColumns(segment, out var loq, out var limit, out var mass, out var status))
            return false;

        rowResult = new ParsedDigipathPesticideRow(
            anchor.CanonicalName,
            loq,
            limit,
            mass,
            status,
            row);

        return true;
    }

    private static bool TryParsePesticideResultColumns(
        string segment,
        out decimal loq,
        out decimal limit,
        out PesticideMassResult mass,
        out string status)
    {
        loq = 0m;
        limit = 0m;
        mass = new PesticideMassResult(null, null);
        status = string.Empty;

        var match = PesticideColumnRegex.Match(segment);

        if (!match.Success)
            return false;

        if (!decimal.TryParse(match.Groups["loq"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out loq))
            return false;

        if (!decimal.TryParse(match.Groups["limit"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out limit))
            return false;

        var rawMass = match.Groups["mass"].Value.Trim();

        if (AcceptedPesticideMassQualifierRegex.IsMatch(rawMass))
        {
            mass = new PesticideMassResult(null, NormalizeQualifiedToken(rawMass));
        }
        else
        {
            if (!decimal.TryParse(rawMass, NumberStyles.Number, CultureInfo.InvariantCulture, out var massValue))
                return false;

            mass = new PesticideMassResult(massValue, null);
        }

        status = match.Groups["status"].Value.ToLowerInvariant();
        return true;
    }

    private static PesticideRowOutcome EvaluatePesticideRow(ParsedDigipathPesticideRow row)
    {
        if (row.Status.Equals("fail", StringComparison.OrdinalIgnoreCase))
            return PesticideRowOutcome.Fail;

        if (row.Status.Equals("nt", StringComparison.OrdinalIgnoreCase) ||
            row.Status.Equals("nr", StringComparison.OrdinalIgnoreCase))
        {
            return PesticideRowOutcome.Unknown;
        }

        if (!row.Status.Equals("pass", StringComparison.OrdinalIgnoreCase))
            return PesticideRowOutcome.Unknown;

        if (row.Mass.Value is decimal massValue)
        {
            if (row.Limit > 0m && massValue > row.Limit)
                return PesticideRowOutcome.Fail;

            if (row.Limit == 0m && massValue > 0m)
                return PesticideRowOutcome.Fail;

            return PesticideRowOutcome.Pass;
        }

        if (!string.IsNullOrWhiteSpace(row.Mass.Qualifier))
            return PesticideRowOutcome.Pass;

        return PesticideRowOutcome.Unknown;
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

    private static bool TryParseDigipathCannabinoidTable(string text, ProductType productType, out CannabinoidProfile profile)
    {
        profile = CreateEmptyProfile();

        var rows = ExtractCannabinoidSectionRows(text);

        if (rows.Count == 0)
            rows = NormalizeRows(text);

        if (rows.Count == 0)
            return false;

        var hasTableContext = HasDigipathCannabinoidTableContext(rows);

        if (!hasTableContext)
            return false;

        var context = DetectTableContext(rows);
        var useMgPerGram = ShouldStoreCannabinoidsAsMgPerGram(productType);
        var parsedAny = false;
        var delta8 = 0m;

        foreach (var row in rows)
        {
            foreach (var parsedRow in ParseCannabinoidRows(row, context, useMgPerGram))
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
                        parsedAny = true;
                        break;
                    case "THCA" when profile.THCA.Confidence == 0m:
                        profile.THCA = field;
                        parsedAny = true;
                        break;
                    case "CBD" when profile.CBD.Confidence == 0m:
                        profile.CBD = field;
                        parsedAny = true;
                        break;
                    case "CBDA" when profile.CBDA.Confidence == 0m:
                        profile.CBDA = field;
                        parsedAny = true;
                        break;
                    case "D8-THC":
                        delta8 = parsedRow.Value;
                        parsedAny = true;
                        break;
                }
            }
        }

        if (!parsedAny)
            return false;

        profile.TotalTHC = profile.THC.Value + (profile.THCA.Value * 0.877m) + delta8;
        profile.TotalCBD = profile.CBD.Value + (profile.CBDA.Value * 0.877m);

        return true;
    }

    private static IEnumerable<ParsedDigipathCannabinoidRow> ParseCannabinoidRows(
        string row,
        DigipathTableContext context,
        bool useMgPerGram)
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

            if (TryParseCannabinoidAnchorWindow(row, anchor, nextAnchor, context, useMgPerGram, out var parsedRow))
                yield return parsedRow;
        }
    }

    private static bool TryParseCannabinoidSideRow(string row, out ParsedDigipathCannabinoidRow parsedRow)
    {
        parsedRow = ParseCannabinoidRows(row, new DigipathTableContext(true, "%", false), useMgPerGram: false).FirstOrDefault()
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
        DigipathTableContext context,
        bool useMgPerGram,
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

        if (!TryParseDigipathResultTriple(tokens, context, out var percent, out var mgPerGram, out var isLoq))
            return false;

        var confidence = isLoq ? 0m : 0.95m;
        var value = useMgPerGram ? mgPerGram : percent;

        parsedRow = new ParsedDigipathCannabinoidRow(anchor.CanonicalName, value, row, confidence);
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
        return TryParseDigipathResultTriple(
            tokens,
            new DigipathTableContext(true, "%", false),
            out percent,
            out mgPerGram,
            out isLoq);
    }

    private static bool TryParseDigipathResultTriple(
        IReadOnlyList<ResultToken> tokens,
        DigipathTableContext context,
        out decimal percent,
        out decimal mgPerGram,
        out bool isLoq)
    {
        percent = 0m;
        mgPerGram = 0m;
        isLoq = false;

        if (tokens.Count < 3)
            return false;

        var percentToken = context.HasMgPerGramBeforePercent ? tokens[2] : tokens[1];
        var mgPerGramToken = context.HasMgPerGramBeforePercent ? tokens[1] : tokens[2];

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

    private static bool LooksLikePesticideTableRow(string row)
    {
        return FindPesticideTableAnchors(row)
            .Any(anchor => anchor.Kind == AnalyteKind.Pesticide);
    }

    private static bool LooksLikeMicrobialTableRow(string row)
    {
        return FindMicrobialTableAnchors(row)
            .Any(anchor => anchor.Kind == AnalyteKind.Microbial);
    }

    private static bool LooksLikeMycotoxinTableRow(string row)
    {
        return FindMycotoxinAnchors(row)
            .Any(anchor => anchor.Kind == AnalyteKind.Mycotoxin);
    }

    private static bool LooksLikeHeavyMetalTableRow(string row)
    {
        return FindHeavyMetalAnchors(row)
            .Any(anchor => anchor.Kind == AnalyteKind.HeavyMetal);
    }

    private static bool IsPesticideSectionStart(string row)
    {
        return row.Contains("Pesticides", StringComparison.OrdinalIgnoreCase) &&
               (row.Contains("Microbials", StringComparison.OrdinalIgnoreCase) ||
                row.Contains("Analyte", StringComparison.OrdinalIgnoreCase) ||
                row.Contains("Pass", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPesticideSectionEnd(string row)
    {
        return row.Contains("Heavy Metals", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Mycotoxins", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Residual Solvents", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Solvents", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Terpene Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Cannabinoid Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Overall Result", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Final Result", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMicrobialSectionStart(string row)
    {
        return row.Contains("Microbials", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Microbiological", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Aerobic Bacteria", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Salmonella", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Aspergillus", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMicrobialSectionEnd(string row)
    {
        return row.Contains("Heavy Metals", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Mycotoxins", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Residual Solvents", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Solvents", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Terpene Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Cannabinoid Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Overall Result", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Final Result", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMycotoxinSectionStart(string row)
    {
        return row.Equals("Mycotoxins", StringComparison.OrdinalIgnoreCase) ||
               row.StartsWith("Mycotoxins ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMycotoxinSectionEnd(string row)
    {
        return row.Contains("Residual Solvents", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Solvents", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Pesticides", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Microbials", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Heavy Metals", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Terpene Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Cannabinoid Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Overall Result", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Final Result", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHeavyMetalSectionStart(string row)
    {
        return row.Contains("Heavy Metals", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHeavyMetalSectionEnd(string row)
    {
        return row.Contains("Mycotoxins", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Residual Solvents", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Solvents", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Pesticides", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Terpene Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Cannabinoid Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Overall Result", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Final Result", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsClearComplianceStatus(ComplianceResult compliance)
    {
        return compliance.Status.Equals("pass", StringComparison.OrdinalIgnoreCase) ||
               compliance.Status.Equals("fail", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExplicitOverallComplianceRow(string row)
    {
        return row.Contains("Overall Result", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Final Result", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Overall Status", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Compliance Status", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Result Status", StringComparison.OrdinalIgnoreCase);
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
               token.Equals("NT", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("Not Detected", StringComparison.OrdinalIgnoreCase) ||
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
            return new ParsedDigipathValue(0m, 0m);

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
            raw.Equals("NT", StringComparison.OrdinalIgnoreCase) ||
            raw.Equals("Not Detected", StringComparison.OrdinalIgnoreCase) ||
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

            if (hasLoqColumn && units.Count >= 3)
                return new DigipathTableContext(
                    hasLoqColumn,
                    units[1],
                    units[1] == "MG/G" && units[2] == "%");

            if (hasLoqColumn && units.Count >= 2)
                return new DigipathTableContext(hasLoqColumn, units[1], false);

            if (units.Count >= 1)
                return new DigipathTableContext(hasLoqColumn, units[0], false);
        }

        return new DigipathTableContext(hasLoqColumn, string.Empty, false);
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
        return row.Contains("Cannabinoid Test Results", StringComparison.OrdinalIgnoreCase) ||
               row.Contains("Potency Test Results", StringComparison.OrdinalIgnoreCase);
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

    private static bool ShouldStoreCannabinoidsAsMgPerGram(ProductType productType)
    {
        return productType is not ProductType.Flower and not ProductType.PreRoll and not ProductType.Unknown;
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

    private sealed record DigipathTableContext(
        bool HasLoqColumn,
        string PrimaryResultUnit,
        bool HasMgPerGramBeforePercent);

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

    private sealed record DigipathPesticideTableResult(
        int ParsedRowCount,
        int PassingRowCount,
        int FailingRowCount,
        int UnknownRowCount);

    private sealed record ParsedDigipathPesticideRow(
        string Name,
        decimal Loq,
        decimal Limit,
        PesticideMassResult Mass,
        string Status,
        string SourceText);

    private sealed record PesticideMassResult(
        decimal? Value,
        string? Qualifier);

    private sealed record DigipathHeavyMetalTableResult(
        int ParsedRowCount,
        int PassingRowCount,
        int FailingRowCount,
        int UnknownRowCount);

    private sealed record ParsedDigipathHeavyMetalRow(
        string Name,
        decimal Loq,
        decimal Limit,
        HeavyMetalResult Result,
        string Status,
        string SourceText);

    private sealed record HeavyMetalResult(
        decimal? Value,
        string? Qualifier);

    private sealed record DigipathMicrobialTableResult(
        int ParsedRowCount,
        int PassingRowCount,
        int FailingRowCount,
        int UnknownRowCount);

    private sealed record ParsedDigipathMicrobialRow(
        string Name,
        MicrobialRowType RowType,
        decimal? Loq,
        decimal? Limit,
        MicrobialResult Result,
        string Status,
        string SourceText);

    private sealed record MicrobialResult(
        decimal? Value,
        decimal? LessThanValue,
        string? Qualifier,
        string? BinaryValue);

    private sealed record DigipathMycotoxinTableResult(
        int ParsedRowCount,
        int PassingRowCount,
        int FailingRowCount,
        int UnknownRowCount);

    private sealed record ParsedDigipathMycotoxinRow(
        string Name,
        decimal Loq,
        decimal Limit,
        MycotoxinMassResult Mass,
        string Status,
        string SourceText);

    private sealed record MycotoxinMassResult(
        decimal? Value,
        string? Qualifier);

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
        Terpene,
        Pesticide,
        Microbial,
        HeavyMetal,
        Mycotoxin
    }

    private enum PesticideRowOutcome
    {
        Pass,
        Fail,
        Unknown
    }

    private enum HeavyMetalRowOutcome
    {
        Pass,
        Fail,
        Unknown
    }

    private enum MicrobialRowType
    {
        Quantitative,
        Binary
    }

    private enum MicrobialRowOutcome
    {
        Pass,
        Fail,
        Unknown
    }

    private enum MycotoxinRowOutcome
    {
        Pass,
        Fail,
        Unknown
    }
}
