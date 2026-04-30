# CannabisCOA.Parser

A .NET parser for extracting structured cannabis Certificate of Analysis (COA) data from messy lab PDF/text reports.

COAs are designed for people, not databases. This project turns inconsistent lab reports into structured JSON that can support analytics, compliance review, inventory workflows, batch QA, and automation.

The goal is not to build a toy parser that works on one perfect PDF. The goal is to survive real-world COA chaos: different labs, different units, different table layouts, side-by-side extraction bleed, non-detect rows, amended reports, duplicated templates, and PDF text that looks like it was assembled by a raccoon with a label maker.

## Current Status

Active development, fixture-first, with a passing regression suite.

Current local test status:

```powershell
dotnet test
# 243/243 passing
```

Current mixed-product batch stress test status:

- 120 real COAs processed
- 0 `MISSING_TEST_DATE` warnings
- 0 unknown product type failures observed in the latest batch
- 0 actionable chemistry warnings remaining in the latest post-fix batch
- Remaining warnings are currently amended-report flags only: `AMENDED_COA`

The parser is currently strongest on Nevada lab COAs and has locked coverage across multiple labs, product types, and table layouts.

## Supported Labs

The parser currently targets these Nevada COA lab formats:

- 374 Labs
- G3 Labs
- NV Cann Labs
- Ace Analytical Laboratory
- Kaycha Labs
- Digipath Labs
- MA Analytics
- RSR Analytical Laboratories

## Supported Product Types

The parser detects and validates multiple product types:

- Flower
- Pre-Roll
- Edible
- Concentrate
- Vape
- Topical
- Tincture

Flower parsing is the most mature baseline across the full lab set. Recent work has expanded and regression-locked several non-flower layouts, including Kaycha edibles, Digipath vapes, 374 Labs vapes, and NV Cann concentrate/vape side-by-side table patterns.

## What It Extracts

`CannabisCOA.Parser` extracts structured COA data including:

- Product type
- Lab name
- Product name
- Batch ID
- Harvest date
- Test date
- Package date
- Amended COA status
- Cannabinoids
  - THC
  - THCA
  - CBD
  - CBDA
  - Total THC
  - Total CBD
- Terpenes
  - Individual terpene breakdown
  - Total terpenes
  - Dominant terpene profile support
- Compliance status
- Freshness scoring
- Validation warnings
- Product score and score breakdown

Each parsed cannabinoid field includes:

- Field name
- Parsed value
- Source text
- Confidence

The source text is intentionally preserved so parser output can be audited against the raw extracted COA text.

## Why This Exists

Cannabis operators, analysts, inventory teams, and compliance staff often need COA data in a usable format, but the data is usually trapped in inconsistent PDFs.

Manual entry is slow, error-prone, and hard to scale. This project reduces that pain by turning messy lab reports into consistent structured output.

Practical use cases include:

- Internal COA QA
- Inventory enrichment
- Product scoring
- Retail analytics
- Compliance review
- Batch-level data validation
- Vendor/lab consistency checks
- Future database loading and dashboard workflows

## Parser Philosophy

This project follows a narrow, fixture-first strategy:

1. Use real COA text fixtures whenever possible.
2. Add one narrow regression test for one known layout.
3. Fix the lab adapter, not the generic parser, unless the problem is truly generic.
4. Preserve existing passing lab/product behavior.
5. Avoid broad refactors during parser lock-in work.
6. Keep source text auditable.
7. Treat side-by-side table bleed as a lab-layout problem, not a reason to loosen validation.

The project favors boring, targeted fixes over heroic parser wizardry. Less magic, more receipts.

## Important Parsing Rules

### Flower / Pre-Roll

Nevada flower-like products are commonly reported and validated using percentage values.

For flower and pre-roll COAs, cannabinoid values generally use the percent column as the canonical parsed value.

### Vape / Concentrate / Edible / Other Non-Flower Products

Non-flower products often require mg/g or mg/unit handling depending on product type and lab format.

Examples already handled:

- Kaycha edible potency tables use mg/unit as the canonical edible value.
- Digipath vape/concentrate tables may report cannabinoid rows as `LOQ | mg/g | %`.
- NV Cann concentrate/vape side-by-side tables require bounded parsing to avoid terpene bleed.

### Non-Detect Handling

The parser normalizes common non-detect tokens such as:

- `<LOQ`
- `ND`
- `NR`
- `NT`
- `Not Detected`

When a cannabinoid is non-detect, the value is set to `0` and confidence is set to `0`.

### Total THC / Total CBD

Total THC is validated using:

```text
Total THC = THC + (THCA * 0.877)
```

Total CBD is validated using:

```text
Total CBD = CBD + (CBDA * 0.877)
```

Some lab-specific parsers may include additional cannabinoids such as Delta-8 THC in local total calculations when the source table supports it.

## Recent Locked Fixes

Recent parser lock-in work includes:

- Kaycha edible cannabinoid table parsing
  - Parses vertical edible potency rows
  - Uses mg/unit values
  - Avoids product description, formula text, and water activity bleed

- Digipath vape cannabinoid parsing
  - Supports vape/concentrate cannabinoid rows using mg/g-first layouts
  - Handles side-by-side cannabinoid/terpene table extraction
  - Avoids terpene-side values contaminating THC/CBD

- 374 Labs vape terpene breakdown parsing
  - Parses flattened cannabinoid/terpene table rows
  - Stores terpene result percent, not LOQ
  - Skips non-detect terpene rows

- MA Analytics terpene alias expansion
  - Adds missing terpene analytes and aliases
  - Resolves terpene total mismatch warnings caused by missed rows

- NV Cann Labs terpene alias expansion
  - Adds missing terpene analytes and aliases
  - Resolves terpene total mismatch warnings caused by missed rows

- NV Cann Labs side-by-side table bleed fix
  - Adds NV-only bounded cannabinoid parsing
  - Slices cannabinoid rows before terpene analytes
  - Prevents terpene-side `<LOQ` from zeroing valid cannabinoid values
  - Prevents terpene-side numbers from being parsed as THC/CBD

- Digipath vs NV adapter detection tightening
  - Prevents Digipath COAs with NV Cann footer/subcontract text from being misrouted to NV Cann parsing

## Example CLI Usage

Run the parser against a single PDF or text fixture:

```powershell
dotnet run --project src\CannabisCOA.Parser.Cli -- --file "path\to\coa.pdf"
```

Dump extracted text from a PDF:

```powershell
dotnet run --project src\CannabisCOA.Parser.Cli -- --file "path\to\coa.pdf" --dump-text
```

Parse a batch folder into JSON output:

```powershell
dotnet run --project src\CannabisCOA.Parser.Cli -- --batch "G:\COA_BatchTests\combined-current" --out "wave-current.jsonl"
```

Run the test suite:

```powershell
dotnet test
```

Build the test project without running tests:

```powershell
dotnet build tests\CannabisCOA.Parser.Core.Tests\CannabisCOA.Parser.Core.Tests.csproj -v minimal
```

## Example Output Shape

```json
{
  "Coa": {
    "ProductType": "Flower",
    "IsAmended": false,
    "LabName": "MA Analytics",
    "ProductName": "Sample Flower",
    "BatchId": "ABC123",
    "HarvestDate": "2026-02-25T00:00:00",
    "TestDate": "2026-03-27T00:00:00",
    "PackageDate": "2026-03-29T00:00:00",
    "Cannabinoids": {
      "THC": {
        "FieldName": "THC",
        "Value": 0.544,
        "SourceText": "Δ9-THC 0.160 0.544 5.44",
        "Confidence": 0.95
      },
      "THCA": {
        "FieldName": "THCA",
        "Value": 26.278,
        "SourceText": "THCa 0.160 26.278 262.78",
        "Confidence": 0.95
      },
      "CBD": {
        "FieldName": "CBD",
        "Value": 0,
        "SourceText": "CBD 0.640 ND ND",
        "Confidence": 0
      },
      "CBDA": {
        "FieldName": "CBDA",
        "Value": 0,
        "SourceText": "CBDa 0.160 ND ND",
        "Confidence": 0
      },
      "TotalTHC": 23.59,
      "TotalCBD": 0.00
    },
    "Terpenes": {
      "Terpenes": {
        "β-Myrcene": 0.66359,
        "β-Caryophyllene": 0.53103,
        "δ-Limonene": 0.41148,
        "Linalool": 0.23188
      },
      "TotalTerpenes": 2.17
    },
    "Compliance": {
      "Passed": true,
      "ContaminantsPassed": true,
      "Status": "pass"
    },
    "Freshness": {
      "DaysSinceTest": 33,
      "Score": 85,
      "Band": "Good"
    }
  },
  "Validation": {
    "IsValid": true,
    "Warnings": []
  },
  "Score": {
    "Score": 75,
    "Tier": "Solid",
    "Breakdown": {
      "Potency": 30,
      "Terpenes": 15,
      "Freshness": 20,
      "Compliance": 10
    }
  },
  "Profile": {
    "DominantTerpene": "β-Myrcene",
    "TopTerpenes": [
      "β-Myrcene",
      "β-Caryophyllene",
      "δ-Limonene"
    ],
    "ProfileType": "Floral",
    "Lean": "Unknown"
  }
}
```

## Validation & Scoring

Parsed COAs are validated and scored after extraction.

Validation currently checks for issues such as:

- Missing THC values
- Missing test dates
- Unknown product types
- Amended COAs
- Terpene total mismatches
- Missing terpene breakdowns when a total is present
- Total THC / CBD sanity checks
- Product-type-aware chemistry expectations

Scoring currently considers:

- Potency
- Terpenes
- Freshness
- Compliance status

The validation layer is intentionally separate from parsing so extraction, warnings, and scoring can evolve independently.

## Project Structure

```text
src/
  CannabisCOA.Parser.Core/
    Adapters/
      Labs/
        374Labs/
        AceAnalytical/
        Digipath/
        G3Labs/
        KaychaLabs/
        MAAnalytics/
        NVCannLabs/
        RSRAnalytical/
    Parsers/
    Validation/
    Scoring/

  CannabisCOA.Parser.Cli/
    CLI entry point for single-file and batch parsing

tests/
  CannabisCOA.Parser.Core.Tests/
    Fixtures/
      Labs/
    Lab-specific parser tests
    Adapter detection tests
    Validator tests
```

## Fixture Workflow

Preferred workflow for locking a new COA layout:

1. Dump raw text from the PDF.
2. Save the raw text fixture under `tests/CannabisCOA.Parser.Core.Tests/Fixtures/Labs/`.
3. Add one narrow failing test for the exact layout.
4. Patch only the relevant lab adapter.
5. Run `dotnet test`.
6. Batch test against `G:\COA_BatchTests\combined-current`.
7. Compare warnings before/after.

Example fixture dump:

```powershell
dotnet run --project src\CannabisCOA.Parser.Cli -- --file "G:\COA_BatchTests\combined-current\sample.pdf" --dump-text > tests\CannabisCOA.Parser.Core.Tests\Fixtures\Labs\new-fixture.txt
```

## Batch Testing Workflow

Current batch testing folder:

```text
G:\COA_BatchTests\combined-current\
```

Recommended batch command:

```powershell
dotnet run --project src\CannabisCOA.Parser.Cli -- --batch "G:\COA_BatchTests\combined-current" --out "wave-current.jsonl"
```

Batch review focuses on warning deltas:

- `MISSING_THC_VALUES`
- `MISSING_TEST_DATE`
- `TERPENE_BREAKDOWN_MISSING`
- `TERPENE_TOTAL_MISMATCH`
- `AMENDED_COA`
- Unknown or incorrect product type
- Wrong lab adapter resolution
- Suspicious source text bleed

A clean chemistry batch does not necessarily mean every future COA is solved. It means the current known fixture/batch patterns are locked and regression-protected.

## Development Notes

Generated build outputs should not be committed:

- `bin/`
- `obj/`
- `Debug/`
- `Release/`

If build artifacts show up in `git status`, restore them before committing:

```powershell
git restore -- src\CannabisCOA.Parser.Cli\bin src\CannabisCOA.Parser.Cli\obj src\CannabisCOA.Parser.Core\bin src\CannabisCOA.Parser.Core\obj tests\CannabisCOA.Parser.Core.Tests\bin tests\CannabisCOA.Parser.Core.Tests\obj
```

Recommended `.gitignore` entries:

```gitignore
bin/
obj/
.vs/
.vscode/
TestResults/
*.user
*.suo
*.cache
*.pdb
```

## Roadmap

### Locked / Completed

- [x] Lab adapter framework
- [x] CLI parser entry point
- [x] Single-file parsing
- [x] Batch parsing
- [x] JSON output
- [x] Flower COA parsing across 8 Nevada labs
- [x] Product type detection
- [x] Cannabinoid parsing
- [x] Terpene parsing
- [x] Total THC / CBD validation
- [x] Terpene total validation
- [x] Product-type-aware validation
- [x] Non-detect confidence normalization
- [x] Source text preservation
- [x] Lab adapter detection tests
- [x] Kaycha edible cannabinoid parsing
- [x] Digipath vape cannabinoid parsing
- [x] Digipath side-by-side vape cannabinoid parsing
- [x] 374 Labs vape terpene breakdown parsing
- [x] MA/NV terpene alias expansion
- [x] NV Cann side-by-side table bleed fix
- [x] Mixed-product batch stress testing baseline

### Next Best Targets

- [ ] Add more real fixture coverage for each lab/product combination
- [ ] Add CSV export for analyst workflows
- [ ] Add database-ready output shape
- [ ] Expand safety/compliance contaminant category parsing
- [ ] Add stronger product name and batch ID extraction
- [ ] Add duplicate COA detection for batch runs
- [ ] Add batch summary report output
- [ ] Add CI workflow for automated test runs
- [ ] Add package/release workflow when API shape stabilizes

### Future Ideas

- [ ] COA quality score by lab/vendor
- [ ] COA freshness dashboard
- [ ] Terpene profile clustering
- [ ] Retail inventory enrichment pipeline
- [ ] Compliance exception reports
- [ ] Parser confidence dashboard
- [ ] Postgres loading pipeline
- [ ] Excel-friendly exports

## Tech Stack

- C#
- .NET 10
- xUnit
- CLI-first workflow
- JSON output
- Fixture-backed parser tests
- Lab-specific adapters

## Portfolio Summary

`CannabisCOA.Parser` is a real-world cannabis data extraction tool built to convert inconsistent COA PDFs into structured, validated JSON.

It demonstrates:

- Domain-specific parser design
- Test-driven development
- Batch data validation
- Lab-specific adapter architecture
- PDF text extraction handling
- RegEx and bounded table parsing
- Data quality checks
- Product-type-aware cannabis chemistry logic
- Practical analytics pipeline thinking

This project is built from actual operational pain: cannabis COA data exists, but it is messy, inconsistent, and trapped in documents. This parser turns that pain into structured data.