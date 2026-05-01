# CannabisCOA.Parser

CannabisCOA.Parser is a C#/.NET parser for cannabis Certificates of Analysis (COAs). It converts messy lab PDF output into structured lab, product, batch, cannabinoid, terpene, compliance, warning, and audit data.

The project is currently focused on Nevada COA layouts and the Flower v1 audit workflow. It handles real-world PDF extraction problems such as flattened tables, inconsistent headers, lab-specific layout drift, product-type edge cases, amended reports, side-by-side table bleed, missing lab headers, embedded quote characters, and partial/single-panel reports.

---

## 🚦 Current Status

Latest confirmed local validation:

```text
Test suite:                 288/288 passing
Current batch folder:       G:\COA_BatchTests\combined-current\
Latest stress batch size:   1,500 reports
Latest stress batch result: 1,500 / 1,500 Flower rows
Missing core fields:        0
Clean metadata rows:        1,500 / 1,500
```

Latest 1,500-row batch audit:

```text
Total rows:                 1,500
Flower rows:                1,500
Edible rows:                    0
Unknown rows:                   0

FullComplianceCoa rows:     1,482
SinglePanelTest rows:          18

Rows with no missing core fields: 1,500 / 1,500
Current clean rate:              100.0%
```

This is the current Flower v1 milestone: every report in the 1,500-file stress batch is classified as Flower, every row has required core metadata, and partial/single-panel reports are separated from full compliance COAs instead of being treated as parser failures.

---

## 🔥 Major Milestone

CannabisCOA.Parser now successfully processes a 1,500-report Nevada Flower stress batch with:

- ✅ 1,500 / 1,500 rows classified as Flower
- ✅ 1,500 / 1,500 rows with no missing core fields
- ✅ 0 Unknown product-type rows
- ✅ 0 false Edible rows in the Flower batch
- ✅ 18 Digipath single-panel reports correctly classified as `SinglePanelTest`
- ✅ 8 labs represented in the stress batch
- ✅ 288 / 288 tests passing

This moves the project from sample parsing into real batch audit readiness for Flower v1.

---

## 🧪 Latest 1,500-Report Stress Test

### Product Type Distribution

```text
Flower:   1,500
Edible:       0
Unknown:      0
```

### Document Classification

```text
FullComplianceCoa: 1,482
SinglePanelTest:      18
```

The 18 `SinglePanelTest` rows are Digipath partial-panel reports. These are legitimate lab reports, but they are not full Flower compliance COAs. They are intentionally classified separately so they do not appear as cannabinoid parser failures.

### Missing Core Field Summary

```text
No missing fields: 1,500
Missing fields:       0
```

### Lab Counts in the Stress Batch

```text
Kaycha Labs:                   628
NV Cann Labs:                  297
Ace Analytical Laboratory:     267
374 Labs:                      105
G3 Labs:                        72
MA Analytics:                   64
Digipath:                       49
RSR Analytical Laboratories:    18
```

### Clean Rate by Lab

```text
Kaycha Labs:                   628 / 628 clean   100.0%
NV Cann Labs:                  297 / 297 clean   100.0%
Ace Analytical Laboratory:     267 / 267 clean   100.0%
374 Labs:                      105 / 105 clean   100.0%
G3 Labs:                        72 / 72 clean    100.0%
MA Analytics:                   64 / 64 clean    100.0%
Digipath:                       49 / 49 clean    100.0%
RSR Analytical Laboratories:    18 / 18 clean    100.0%
```

### Current Warning Board

```text
No warning:                         1,365
TERPENE_TOTAL_MISMATCH:                50
AMENDED_COA:                           49
SINGLE_PANEL_TEST:                     18
AMENDED_COA|TERPENE_TOTAL_MISMATCH:    11
TERPENE_BREAKDOWN_MISSING:              7
```

Interpretation:

- `AMENDED_COA` is compliance metadata, not a parser failure.
- `SINGLE_PANEL_TEST` is expected for partial Digipath reports.
- `TERPENE_TOTAL_MISMATCH` is now a quality-review board item, not a missing-core-field issue.
- `TERPENE_BREAKDOWN_MISSING` is a smaller follow-up warning category.

---

## 🎯 Flower v1 Sprint Summary

The Flower v1 sprint focused on turning real PDF chaos into a stable audit pipeline.

Major completed wins:

- ✅ Added flat batch CSV audit output
- ✅ Added Flower v1 audit fields
- ✅ Added `DocumentClassification`
- ✅ Added `IsFullComplianceCoa`
- ✅ Added Digipath single-panel / partial-panel classification
- ✅ Cleaned Digipath metadata and single-panel handling
- ✅ Cleaned 374 Labs to 105/105 in the stress batch
- ✅ Added 374 Labs `Popcorn Buds` detection
- ✅ Added Ace `Popcorn Buds` and `Trim` detection
- ✅ Added G3 `Popcorn Buds` / `Light Deprivation` detection
- ✅ Added MA Analytics `Trim` detection and BatchId fallback
- ✅ Added RSR `Trim` detection
- ✅ Added NV Cann Labs footer-marker lab detection using `nvcann.com` / Schuster Street markers
- ✅ Added NV Cann Labs plant-material detection for `Popcorn Buds` and `Trim`
- ✅ Added Kaycha raw plant handling for `Flower - Cured`, `Trim`, `Shake`, `Popcorn Buds`, and `Other - Not Listed`
- ✅ Added CSV escaping coverage for embedded quote values like `8" Bagel`
- ✅ Added lab-specific ProductName and BatchId extraction across the main Nevada Flower lab set
- ✅ Preserved conservative generic parsing by keeping layout fixes lab-specific

---

## 🧬 Supported Labs

Current lab adapters / lab coverage include:

- ✅ 374 Labs
- ✅ Ace Analytical Laboratory
- ✅ Digipath
- ✅ G3 Labs
- ✅ Kaycha Labs
- ✅ MA Analytics
- ✅ NV Cann Labs
- ✅ RSR Analytical Laboratories

---

## 🌿 Product Coverage

The current production-grade workflow is Flower v1.

For Nevada audit purposes, Flower v1 includes usable cannabis / plant-material variants such as:

- Flower
- Flower - Cured
- Flower Cured
- Popcorn Buds
- Small Buds
- Shake
- Trim
- Raw Plant / Plant Material layouts where the test matrix clearly matches Flower or usable cannabis

Current coverage matrix:

| Lab | Flower / Plant Material | Pre-Roll | Edible | Vape | Concentrate | Tincture | Topical |
|---|---:|---:|---:|---:|---:|---:|---:|
| 374 Labs | ✅ | Partial | — | Partial | Partial | — | — |
| Ace Analytical Laboratory | ✅ | Partial | — | Partial | — | — | — |
| Digipath | ✅ | Partial | — | Partial | Partial | — | — |
| G3 Labs | ✅ | Partial | — | — | — | — | — |
| Kaycha Labs | ✅ | Partial | ✅ | Partial | Partial | — | — |
| MA Analytics | ✅ | Partial | — | — | Partial | — | — |
| NV Cann Labs | ✅ | ✅ | — | Partial | ✅ | — | — |
| RSR Analytical Laboratories | ✅ | Partial | — | — | — | — | — |

Legend:

- ✅ = fixture-backed and/or strongly batch-validated
- Partial = observed support exists, but more fixtures are needed
- — = not yet validated

---

## 📦 What the Parser Extracts

The parser currently normalizes:

- Lab name
- Product type
- Product name
- Batch ID
- Harvest date
- Test date
- Package date
- Amended COA status
- Document classification
- Full compliance COA flag
- Major cannabinoid values:
  - THC
  - THCA
  - CBD
  - CBDA
  - Total THC
  - Total CBD
- Total terpenes
- Individual terpene breakdowns where available
- Source text for key cannabinoid values
- Confidence values
- Parser / validation warnings

---

## 🧾 Document Classification

The parser separates product type from document completeness.

Example:

```text
ProductType = Flower
DocumentClassification = SinglePanelTest
IsFullComplianceCoa = false
Warnings = SINGLE_PANEL_TEST
```

This distinction matters because some lab reports describe a Flower product but are not full compliance COAs.

### `FullComplianceCoa`

Used for normal Flower v1 COAs with the expected compliance-style panel set.

### `SinglePanelTest`

Used for partial/single-panel reports, such as Digipath one-page Heavy Metals-only reports.

These files may describe Flower products, but they should not be treated as complete Flower COAs or as cannabinoid parser failures.

---

## 🧠 Nevada / METRC Methodology

The parser distinguishes between:

```text
ProductType
Compliance-style document behavior
DocumentClassification
Parser warnings
```

This matters because Nevada plant-material products can include:

- Flower
- Trim
- Shake
- Popcorn Buds
- Non-infused pre-roll material
- Other usable cannabis / raw plant variants

Core rule:

```text
Explicit COA text determines product type.
Test panel structure helps validate the compliance matrix.
DocumentClassification determines whether the report is a full COA or a partial/single-panel report.
```

---

## 🖥️ CLI Usage

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

```powershell
dotnet run --project src\CannabisCOA.Parser.Cli -- --batch "G:\COA_BatchTests\combined-current" --out "G:\COA_BatchTests\parsed.jsonl"
```

### Batch Parse COAs to CSV Audit

Current Flower v1 audit workflow:

```powershell
dotnet run --project src\CannabisCOA.Parser.Cli -- --batch "G:\COA_BatchTests\combined-current\" --csv "G:\COA_BatchTests\combined-current\batch-audit.csv"
```

Expected console behavior:

```text
Processed <n> files → CSV <path>
```

---

## 📊 CSV Audit Export

The CLI supports flat CSV export for Excel, Power BI, and batch QA review.

CSV behavior:

- One row per parsed report
- Dates are written as `yyyy-MM-dd`
- Missing/null values are blank
- Decimal values use invariant culture
- Warnings are pipe-delimited in a single cell
- Embedded commas, quotes, and line breaks are escaped correctly
- Embedded quote values such as `8" Bagel` are CSV-escaped correctly
- Terpene breakdown columns are intentionally excluded from the v1 summary CSV to keep it flat and stable

Current CSV audit columns include:

```text
SourceFile
AuditProfile
IsFlowerV1Candidate
MapperSchemaVersion
DocumentClassification
IsFullComplianceCoa
LabName
ProductType
ProductName
BatchId
HarvestDate
TestDate
PackageDate
IsAmended
OverallStatus
MissingCoreFields
CannabinoidCount
TerpeneCount
TotalTHC
TotalCBD
TotalTerpenes
THC
THCA
CBD
CBDA
THCSourceText
THCConfidence
THCASourceText
THCAConfidence
CBDSourceText
CBDConfidence
CBDASourceText
CBDAConfidence
Warnings
```

---

## 🧪 Testing

Run the full test suite locally:

```powershell
dotnet test
```

Current latest confirmed result:

```text
288/288 passing
```

---

## 🧩 Fixture Strategy

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
6. Run the full local test suite.
7. Regenerate batch audit CSV.
8. Use the audit board to choose the next target.

---

## 🧱 Development Principles

This project intentionally favors lab-specific parsing over aggressive generic guessing.

Core rules:

- ✅ Prefer real fixtures over assumptions
- ✅ Fix one lab/product/layout at a time
- ✅ Keep generic parsers conservative
- ✅ Avoid broad refactors during parser hardening
- ✅ Do not loosen validators to hide parser misses
- ✅ Preserve source precision where the COA provides it
- ✅ Treat side-by-side PDF extraction as a first-class problem
- ✅ Use lab-specific parsing when layout identity is known
- ✅ Keep source text traceability for key parsed values
- ✅ Separate parser failures from partial/single-panel source documents

---

## 🧹 Repository Hygiene

Generated build outputs should not be committed.

Common generated folders:

```text
bin/
obj/
TestResults/
```

Local scratch output should also stay out of Git:

```text
unknown-labs.txt
.codex-build/
.tmp-build/
```

If generated files appear in Git status, clean or restore them before committing.

Example:

```powershell
git status
git diff --stat
```

---

## 🧭 Current Roadmap

Completed / current milestone:

- [x] Stable Flower parsing baseline across main Nevada labs
- [x] Flower v1 CSV audit export
- [x] JSONL batch output
- [x] Lab-specific ProductName and BatchId extraction
- [x] Kaycha raw plant layout handling
- [x] Kaycha `Shake`, `Trim`, `Popcorn Buds`, and `Other - Not Listed` handling
- [x] 374 Labs `Popcorn Buds` handling
- [x] Ace `Popcorn Buds` and `Trim` handling
- [x] G3 `Popcorn Buds` / `Light Deprivation` handling
- [x] MA Analytics `Trim` handling and BatchId fallback
- [x] RSR `Trim` handling
- [x] NV Cann Labs footer-marker lab detection
- [x] NV Cann Labs `Popcorn Buds` and `Trim` handling
- [x] Digipath compact sample-header ProductName extraction
- [x] Digipath single-panel report classification
- [x] CSV quote escaping coverage
- [x] `DocumentClassification` and `IsFullComplianceCoa`
- [x] Batch audit loop validated against 1,500 real COA/report files
- [x] 1,500 / 1,500 Flower rows in the latest stress batch
- [x] 1,500 / 1,500 rows with no missing core fields

Next practical targets:

- [ ] Review `TERPENE_TOTAL_MISMATCH` warnings, especially G3 and RSR
- [ ] Review `TERPENE_BREAKDOWN_MISSING` warnings
- [ ] Add normalized cannabinoid CSV export
- [ ] Add normalized terpene CSV export
- [ ] Add safety/compliance panel extraction
- [ ] Add database ingestion path
- [ ] Add automated support matrix generation from fixture coverage
- [ ] Run larger folder-level stress tests across the sorted COA archive

---

## 🔥 Recommended Next Engineering Step

The Flower v1 metadata/classification board is currently clean on the 1,500-report stress batch.

Recommended next target:

```text
Terpene warning review
```

Why:

```text
MissingCoreFields is now 0.
ProductType false positives are now 0.
The remaining quality signals are warning-based, especially TERPENE_TOTAL_MISMATCH and TERPENE_BREAKDOWN_MISSING.
```

Recommended next workflow:

1. Group warning rows by lab.
2. Inspect representative `TERPENE_TOTAL_MISMATCH` rows.
3. Determine whether the mismatch is a parser issue, source rounding issue, or intentional COA display behavior.
4. Add fixture-backed warning behavior where needed.
5. Keep warning fixes separate from core metadata fixes.

---

## 🧾 Example Workflow

```powershell
# Run full tests locally
dotnet test

# Parse current batch to CSV audit
dotnet run --project src\CannabisCOA.Parser.Cli -- --batch "G:\COA_BatchTests\combined-current\" --csv "G:\COA_BatchTests\combined-current\batch-audit.csv"

# Review git changes
git status
git diff --stat
```

---

## 📌 Project Summary

CannabisCOA.Parser has moved from a Flower-focused parser into a real-world Nevada COA audit engine.

Current state:

```text
288/288 tests passing
1,500-row real batch stress test
1,500 / 1,500 Flower rows
1,500 / 1,500 rows with no missing core fields
1,482 FullComplianceCoa rows
18 SinglePanelTest rows correctly classified
CSV audit output
JSONL output
fixture-backed lab-specific parser hardening
```

The parser now supports a practical analyst workflow:

```text
Parse real COA folders
Export clean audit CSV
Identify warning patterns by lab/layout
Patch narrow parser behavior
Validate with fixtures
Repeat
```

This project is now ready for warning-quality review, normalized cannabinoid/terpene exports, compliance panel extraction, database ingestion, and larger folder-level stress testing across the sorted COA archive.