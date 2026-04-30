# CannabisCOA.Parser

CannabisCOA.Parser is a C#/.NET parser for cannabis Certificates of Analysis (COAs), built to normalize messy lab PDF output into structured cannabinoid, terpene, product, lab, date, and warning data.

The parser is designed around real Nevada COA layouts, including inconsistent PDF text extraction, flattened side-by-side tables, amended reports, lab-specific potency tables, and product-type-specific unit rules.

## Current Status

Latest validation state:

- Test suite: **243/243 passing**
- Latest mixed-product batch size: **120 COAs**
- Actionable parser warnings in latest batch: **0**
- Remaining batch warnings: **AMENDED_COA only**
- Current batch testing folder: `G:\COA_BatchTests\combined-current\`

The parser currently has stable flower parsing across the supported Nevada lab set, plus active mixed-product support for real-world edible, vape, concentrate, and pre-roll layouts observed in batch testing.

## Supported Labs

Current lab adapters / lab coverage include:

- 374 Labs
- G3 Labs
- NV Cann Labs
- Ace Analytical Laboratory
- Kaycha Labs
- Digipath
- MA Analytics
- RSR Analytical Laboratories

## Product Coverage

The original milestone focused on flower COAs. The project now supports a broader mixed-product baseline.

| Lab | Flower | Pre-Roll | Edible | Vape | Concentrate | Tincture | Topical |
|---|---:|---:|---:|---:|---:|---:|---:|
| 374 Labs | ✅ | Partial | — | ✅ | Partial | — | — |
| G3 Labs | ✅ | Partial | — | — | — | — | — |
| NV Cann Labs | ✅ | ✅ | — | Partial | ✅ | — | — |
| Ace Analytical Laboratory | ✅ | Partial | — | ✅ | — | — | — |
| Kaycha Labs | ✅ | Partial | ✅ | Partial | Partial | — | — |
| Digipath | ✅ | Partial | — | ✅ | ✅ | — | — |
| MA Analytics | ✅ | Partial | — | — | Partial | — | — |
| RSR Analytical Laboratories | ✅ | Partial | — | — | — | — | — |

Legend:

- ✅ = supported with fixture-backed parsing or batch-validated behavior
- Partial = supported for some observed layouts, but more fixtures are needed
- — = not yet validated

## What the Parser Extracts

The parser currently normalizes:

- Product type
- Lab name
- Product name
- Batch ID
- Harvest date
- Test date
- Package date
- Amended COA status
- Major cannabinoid values:
  - THC
  - THCA
  - CBD
  - CBDA
  - Total THC
  - Total CBD
- Total terpenes
- Individual terpene breakdowns where available
- Source text for key cannabinoids
- Confidence values
- Parser/validation warnings

## Nevada Unit Handling

CannabisCOA.Parser follows the practical Nevada reporting distinction used during project validation:

- Flower, trim, shake, and pre-roll products use percent as the canonical stored/display value.
- Other product types, including edibles, vapes, and concentrates, use mg/unit or mg/g depending on the source table and product layout.

Examples:

- Flower cannabinoid rows are generally stored from the percent column.
- Vape/concentrate cannabinoid rows are generally stored from the mg/g column.
- Edible potency rows are stored from the mg/unit value when reported per unit.

## Key Parsing Milestones

### Flower COA Baseline

Flower COA parsing is stable across the main Nevada lab set. The parser handles lab-specific cannabinoid and terpene table layouts, lab/date metadata, amended report flags, and common PDF text extraction issues.

### Kaycha Edible Cannabinoids

Kaycha edible COAs use a vertical/row-oriented cannabinoid potency table that differs from Kaycha flower layouts.

Current behavior:

- Parses edible THC from the actual cannabinoid potency table.
- Uses mg/unit for edible potency values.
- Prevents product description text, water activity text, and formula rows from contaminating cannabinoid fields.
- Handles `<LOQ`, `ND`, `NR`, `NT`, and related non-detect values as zero-confidence non-detects.

### Digipath Vape and Concentrate Cannabinoids

Digipath vape/concentrate COAs can use mg/g-based cannabinoid tables and side-by-side cannabinoid/terpene layouts.

Current behavior:

- Supports Digipath row-based cannabinoid tables.
- Supports vape/concentrate mg/g storage.
- Handles side-by-side cannabinoid/terpene extraction.
- Preserves percent behavior for flower/pre-roll layouts.
- Allows Digipath body/header identity to outrank NV Cann footer/subcontract mentions during lab resolution.

### 374 Labs Vape Terpenes

374 Labs vape COAs can extract as flattened cannabinoid/terpene rows.

Current behavior:

- Parses terpene segments from flattened side-by-side rows.
- Stores terpene result percent rather than LOQ.
- Validates mg/g and percent relationship before accepting terpene values.
- Avoids treating repeated LOQ values as terpene results.

### MA Analytics and NV Cann Labs Terpene Aliases

MA Analytics and NV Cann Labs terpene total mismatches were traced to missing terpene aliases rather than total parsing or rounding.

Current behavior:

- Expanded lab-specific terpene alias support.
- Preserves strict validator tolerance.
- Fixes observed terpene total mismatch warnings without weakening validation.

### NV Cann Labs Side-by-Side Cannabinoid Bleed

NV Cann concentrate COAs can extract as side-by-side cannabinoid/terpene rows.

Example raw row:

```text
Δ9-THC 0.106 10.125 1.420 Caryophyllene Oxide 0.015 <LOQ <LOQ
```

Current behavior:

- Adds NV-only bounded cannabinoid parsing.
- Slices cannabinoid rows before terpene analytes.
- Prevents terpene-side `<LOQ` values from making cannabinoids appear non-detect.
- Prevents terpene-side numeric values from becoming cannabinoid values.
- Stores mg/g for concentrates and vapes.
- Stores percent for flower and pre-rolls.

## Latest Batch Validation

Latest mixed-product batch validation:

```text
Batch folder: G:\COA_BatchTests\combined-current\
COAs parsed: 120
Tests passing: 243/243
Actionable parser warnings: 0
Remaining warnings: AMENDED_COA only
```

Validated real-world layout coverage includes:

- Flower cannabinoid tables
- Flower terpene tables
- Edible vertical cannabinoid potency tables
- Vape/concentrate mg/g cannabinoid layouts
- Flattened side-by-side cannabinoid/terpene layouts
- Side-by-side table bleed prevention
- Terpene total mismatch resolution
- Lab misclassification prevention for Digipath/NV footer overlap
- Non-detect normalization for `<LOQ`, `ND`, `NR`, `NT`, and related values

## CLI Usage

### Parse a Single PDF

```powershell
dotnet run --project src\CannabisCOA.Parser.Cli -- --file "G:\path\to\coa.pdf"
```

### Dump Extracted Text From a PDF

Useful when creating fixtures or debugging PDF extraction:

```powershell
dotnet run --project src\CannabisCOA.Parser.Cli -- --file "G:\path\to\coa.pdf" --dump-text
```

### Batch Parse COAs to JSONL

Current batch testing path:

```powershell
dotnet run --project src\CannabisCOA.Parser.Cli -- --batch "G:\COA_BatchTests\combined-current" --out "G:\COA_BatchTests\parsed.jsonl"
```

### Batch Parse COAs to JSONL and CSV

CSV export is CLI-only and additive. It does not change parser behavior.

```powershell
dotnet run --project src\CannabisCOA.Parser.Cli -- --batch "G:\COA_BatchTests\combined-current" --out "G:\COA_BatchTests\parsed.jsonl" --csv "G:\COA_BatchTests\parsed.csv"
```

## CSV Export

The CLI supports optional flat CSV export for Excel, Power BI, and operational review.

CSV is written only when `--csv` is provided.

Initial CSV columns:

```text
FileName,ProductType,LabName,ProductName,BatchId,IsAmended,HarvestDate,TestDate,PackageDate,THC,THCA,CBD,CBDA,TotalTHC,TotalCBD,TotalTerpenes,THCSourceText,THCConfidence,THCASourceText,THCAConfidence,CBDSourceText,CBDConfidence,CBDASourceText,CBDAConfidence,Warnings
```

CSV behavior:

- One row per COA.
- Dates are written as `yyyy-MM-dd`.
- Missing/null values are blank.
- Decimal values use invariant culture.
- Warnings are pipe-delimited in a single cell.
- CSV values are escaped correctly for commas, quotes, and line breaks.
- Terpene breakdown columns are intentionally excluded from v1 CSV to keep the export flat and stable.

## JSONL Output

Batch JSONL output writes one parsed COA result per line.

Typical result structure includes:

- `Coa`
  - `ProductType`
  - `IsAmended`
  - `LabName`
  - `ProductName`
  - `BatchId`
  - `HarvestDate`
  - `TestDate`
  - `PackageDate`
  - `Cannabinoids`
  - `TotalTerpenes`
  - `Terpenes`
- `Warnings`

JSONL remains the preferred machine-readable/debug format. CSV is the preferred business review format.

## Warnings

The parser emits warnings for important validation conditions.

Current warning examples include:

- `AMENDED_COA`
- `MISSING_THC_VALUES`
- `TERPENE_TOTAL_MISMATCH`
- `TERPENE_BREAKDOWN_MISSING`
- `MISSING_TEST_DATE`
- Unknown or unsupported product/lab patterns where applicable

Current latest batch status:

```text
MISSING_THC_VALUES: 0
TERPENE_TOTAL_MISMATCH: 0
TERPENE_BREAKDOWN_MISSING: 0
AMENDED_COA: 15
```

`AMENDED_COA` is expected metadata/compliance signaling, not a parser failure.

## Testing

Run the full test suite:

```powershell
dotnet test
```

Current expected result:

```text
243/243 passing
```

## Fixture Strategy

The parser is developed with real fixture-backed regression tests wherever possible.

Fixture location:

```text
tests/CannabisCOA.Parser.Core.Tests/Fixtures/Labs/
```

Fixture strategy:

1. Add a narrow fixture or raw text snippet for the exact failing layout.
2. Add one focused regression test.
3. Make the smallest lab-specific production fix.
4. Avoid broad generic parser changes unless the issue is truly generic.
5. Preserve existing lab/product behavior.
6. Re-run the full test suite.
7. Re-run the mixed-product batch test.

## Development Principles

This project intentionally favors lab-specific parsers over over-aggressive generic guessing.

Core rules:

- Prefer real fixtures over assumptions.
- Fix one lab/product/layout at a time.
- Avoid broad refactors during parser hardening.
- Do not loosen validators to hide parser misses.
- Preserve source precision where the COA provides it.
- Treat side-by-side PDF extraction as a first-class problem.
- Keep generic parsers conservative.
- Use lab-specific parsing when layout identity is known.
- Keep source text traceability for key parsed values.

## Current Roadmap

Completed / current milestone:

- [x] Stable flower parsing baseline across main Nevada labs
- [x] Mixed-product batch parsing baseline
- [x] Kaycha edible cannabinoid table support
- [x] Digipath vape/concentrate cannabinoid support
- [x] Digipath side-by-side vape cannabinoid support
- [x] 374 Labs vape terpene breakdown support
- [x] MA Analytics terpene alias expansion
- [x] NV Cann Labs terpene alias expansion
- [x] NV Cann Labs side-by-side concentrate cannabinoid bleed fix
- [x] Digipath vs NV Cann footer/subcontract lab detection improvement
- [x] Batch CSV export

Next practical targets:

- [ ] Add more fixture-backed coverage for pre-roll edge layouts
- [ ] Add more fixture-backed coverage for tincture and topical COAs
- [ ] Add safety/compliance category parsing
- [ ] Add terpene breakdown export option or normalized secondary CSV
- [ ] Add database export / ingestion path
- [ ] Add support matrix automation from fixtures/tests
- [ ] Polish SourceText slicing for already-correct values where source rows still contain appended table text
- [ ] Add CLI/output tests if/when a CLI test harness pattern is introduced

## Recommended Next Engineering Step

The parser warning board is currently clean for the latest mixed-product batch. The next highest-value technical step is not another parser change unless a new batch exposes a real failure.

Recommended next step:

1. Keep the current parser locked.
2. Use the new CSV export against the mixed batch.
3. Review CSV in Excel/Power BI.
4. Decide which downstream shape matters most:
   - one-row-per-COA summary CSV
   - normalized cannabinoid CSV
   - normalized terpene CSV
   - safety/compliance CSV
   - database import tables

## Example Workflow

```powershell
# Run full tests
dotnet test

# Parse current mixed batch to JSONL and CSV
dotnet run --project src\CannabisCOA.Parser.Cli -- --batch "G:\COA_BatchTests\combined-current" --out "G:\COA_BatchTests\parsed.jsonl" --csv "G:\COA_BatchTests\parsed.csv"

# Review git changes
git status
git diff --stat
```

## Repository Hygiene

Generated build outputs should not be committed.

Common generated folders:

```text
bin/
obj/
TestResults/
```

If generated files appear in Git status, clean them before committing.

Example:

```powershell
git restore -- src\CannabisCOA.Parser.Cli\bin src\CannabisCOA.Parser.Cli\obj src\CannabisCOA.Parser.Core\bin src\CannabisCOA.Parser.Core\obj tests\CannabisCOA.Parser.Core.Tests\bin tests\CannabisCOA.Parser.Core.Tests\obj
```

## License

Add license details here if/when the project is ready for public reuse.

## Summary

CannabisCOA.Parser has moved from a flower-only parsing baseline into a mixed-product, fixture-backed COA parsing engine with real batch validation.

Current state:

```text
243/243 tests passing
120-file mixed-product batch clean of actionable parser warnings
JSONL batch output
CSV batch export
lab-specific handling for multiple real-world PDF extraction layouts
```

At this point, the project is ready to support downstream analytics workflows, QA review, CSV/Power BI reporting, and future safety/compliance parsing.