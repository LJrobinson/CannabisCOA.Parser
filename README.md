# CannabisCOA.Parser

CannabisCOA.Parser is a C#/.NET parser for cannabis Certificates of Analysis (COAs), built to normalize messy lab PDF output into structured lab, product, batch, cannabinoid, terpene, compliance, and audit data.

The project is currently focused on Nevada COA layouts and the Flower v1 audit workflow, with lab-specific parsing for real PDF text extraction issues: flattened tables, inconsistent headers, product-type edge cases, amended reports, side-by-side table bleed, missing lab headers, and partial/single-panel reports.

---

## 🚦 Current Status

Latest confirmed local validation:

```text
Test suite: 276/276 passing
Current batch testing folder: G:\COA_BatchTests\combined-current\
Latest stress batch size: 1,500 rows
Latest stress batch runtime: ~2 min 8 sec
```

Latest 1,500-row batch audit:

```text
Total rows:                 1,500
Flower rows:                1,450
Edible rows:                   43
Unknown rows:                   7

FullComplianceCoa rows:     1,482
SinglePanelTest rows:          18

Rows with no missing core fields: 1,435 / 1,500
Current clean rate:              95.7%
```

Important note: `SinglePanelTest` rows are now intentionally classified instead of being treated as parser failures. This matters for Digipath one-page partial-panel reports that are Flower-looking but are not full Flower compliance COAs.

---

## 🎯 Current Sprint Summary

The May Sprint has moved the parser from "works on known samples" to "survives large real-world batch audits."

Major recent wins:

- ✅ Added flat batch CSV audit output.
- ✅ Added audit fields for Flower v1 review.
- ✅ Added `DocumentClassification`.
- ✅ Added `IsFullComplianceCoa`.
- ✅ Classified Digipath single-panel / partial-panel reports.
- ✅ Cleaned all Digipath metadata gaps in the latest stress batch.
- ✅ Cleaned 374 Labs to 105/105 in the latest stress batch.
- ✅ Added 374 Labs `Plant, Popcorn Buds` detection.
- ✅ Added Ace `Plant, Popcorn Buds` detection with embedded quote CSV escaping.
- ✅ Added NV Cann Labs footer-marker lab detection using `nvcann.com` / Schuster Street markers.
- ✅ Added lab-specific product-name and batch-ID extraction across the major Flower v1 lab set.
- ✅ Preserved narrow fixture-backed fixes instead of broad generic parser guessing.

The project now has a repeatable parser hardening loop:

```text
Batch audit CSV
→ identify highest-impact dirty rows
→ inspect real COA layout
→ add narrow lab-specific fixture
→ make focused parser fix
→ run full tests locally
→ regenerate audit CSV
→ repeat
```

---

## 🧪 Latest 1,500-COA Stress Test

### Product Type Distribution

```text
Flower:   1,450
Edible:      43
Unknown:      7
```

The batch is intended to be Flower-focused, so remaining `Edible` and `Unknown` rows are treated as likely product-type/layout detection work unless manually confirmed otherwise.

### Document Classification

```text
FullComplianceCoa: 1,482
SinglePanelTest:      18
```

The 18 `SinglePanelTest` rows are Digipath partial-panel reports. These are no longer counted as cannabinoid parsing failures.

### Missing Core Field Summary

```text
No missing fields:       1,435
ProductName|BatchId:       50
ProductName only:          10
BatchId only:               5
```

### Missing Core Fields by Lab

```text
NV Cann Labs:                  37 missing ProductName|BatchId
Kaycha Labs:                   10 missing ProductName
Kaycha Labs:                    6 missing ProductName|BatchId
Kaycha Labs:                    4 missing BatchId
G3 Labs:                        3 missing ProductName|BatchId
RSR Analytical Laboratories:    2 missing ProductName|BatchId
Ace Analytical Laboratory:      1 missing ProductName|BatchId
MA Analytics:                   1 missing ProductName|BatchId
MA Analytics:                   1 missing BatchId
```

### Clean Rate by Lab

```text
374 Labs:                   105 / 105 clean     100.0%
Digipath:                    49 / 49 clean      100.0%
Ace Analytical Laboratory:  266 / 267 clean      99.6%
MA Analytics:                62 / 64 clean       96.9%
Kaycha Labs:                608 / 628 clean      96.8%
G3 Labs:                     69 / 72 clean       95.8%
RSR Analytical Laboratories: 16 / 18 clean       88.9%
NV Cann Labs:               260 / 297 clean      87.5%
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

- `AMENDED_COA` is expected compliance metadata, not a parser failure.
- `SINGLE_PANEL_TEST` is expected for partial Digipath panel reports.
- `TERPENE_TOTAL_MISMATCH` is now a real quality board item, concentrated mostly in G3 and RSR.
- `TERPENE_BREAKDOWN_MISSING` is a smaller Kaycha follow-up item.

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

Latest stress-batch lab counts:

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

---

## 🌿 Product Coverage

The current sprint emphasis is Flower v1. In this project, the Flower audit flow intentionally includes Nevada plant-material variants such as:

- Flower
- Flower - Cured
- Flower Cured
- Popcorn Buds
- Small Buds
- Shake
- Trim
- Raw Plant / Plant Material layouts where the test matrix clearly matches Flower/usable cannabis

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

The parser now separates product type from document completeness.

Example:

```text
ProductType = Flower
DocumentClassification = SinglePanelTest
IsFullComplianceCoa = false
Warnings = SINGLE_PANEL_TEST
```

This is important because some files are legitimate lab reports but not full compliance COAs.

Current classifications:

### `FullComplianceCoa`

Used for normal Flower v1 COAs with the expected compliance-style panel set.

### `SinglePanelTest`

Used for partial/single-panel reports, such as Digipath one-page Heavy Metals-only reports.

These files may describe a Flower product, but they should not be treated as complete Flower COAs or as cannabinoid parser failures.

---

## 🧠 Nevada / METRC Methodology

The project now distinguishes between:

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

The parser should not rely on one label alone. Instead, it uses:

1. Explicit COA product descriptors.
2. Lab-specific layout patterns.
3. Compliance panel evidence.
4. Warnings/classification when the document is partial or incomplete.

Core rule:

```text
Explicit COA text determines product type.
Test panel structure helps validate the compliance matrix.
DocumentClassification determines whether the report is a full COA or partial/single-panel report.
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

- One row per parsed report.
- Dates are written as `yyyy-MM-dd`.
- Missing/null values are blank.
- Decimal values use invariant culture.
- Warnings are pipe-delimited in a single cell.
- Embedded commas, quotes, and line breaks are escaped correctly.
- Embedded quote values such as `8" Bagel` are CSV-escaped correctly.
- Terpene breakdown columns are intentionally excluded from the v1 summary CSV to keep it flat and stable.

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
276/276 passing
```

Note for AI-assisted development:

```text
CODEX must not run dotnet test.
CODEX must not run dotnet build.
CODEX must not run any dotnet test/build variant.
The user runs all build/test validation locally and reports results.
```

This avoids Windows file-lock issues with generated `bin` / `obj` artifacts.

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
6. User runs the full local test suite.
7. Regenerate batch audit CSV.
8. Use the audit board to choose the next target.

---

## 🧱 Development Principles

This project intentionally favors lab-specific parsing over aggressive generic guessing.

Core rules:

- ✅ Prefer real fixtures over assumptions.
- ✅ Fix one lab/product/layout at a time.
- ✅ Keep generic parsers conservative.
- ✅ Avoid broad refactors during parser hardening.
- ✅ Do not loosen validators to hide parser misses.
- ✅ Preserve source precision where the COA provides it.
- ✅ Treat side-by-side PDF extraction as a first-class problem.
- ✅ Use lab-specific parsing when layout identity is known.
- ✅ Keep source text traceability for key parsed values.
- ✅ Separate parser failures from partial/single-panel source documents.

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
AGENTS.md
.codex-build/
.tmp-build/
```

If generated files appear in Git status, clean or restore them before committing.

Example:

```powershell
git status
git diff --stat
```

If needed:

```powershell
git restore unknown-labs.txt
```

---

## 🧭 Current Roadmap

Completed / current milestone:

- [x] Stable Flower parsing baseline across main Nevada labs
- [x] Flower v1 CSV audit export
- [x] JSONL batch output
- [x] Lab-specific ProductName and BatchId extraction
- [x] Kaycha raw plant layout handling
- [x] Kaycha `Shake`, `Trim`, and `Popcorn Buds` handling
- [x] 374 Labs `Popcorn Buds` handling
- [x] Ace `Popcorn Buds` handling
- [x] NV Cann Labs footer-marker lab detection
- [x] Digipath compact sample-header ProductName extraction
- [x] Digipath single-panel report classification
- [x] CSV quote escaping coverage
- [x] `DocumentClassification` and `IsFullComplianceCoa`
- [x] Batch audit loop validated against 1,500 real COA/report files

Next practical targets:

- [ ] Fix / classify remaining NV Cann Labs rows currently detected as `Edible`
- [ ] Clean remaining Kaycha ProductName / BatchId edge cases
- [ ] Investigate remaining small `Unknown` groups: G3, RSR, MA, Ace
- [ ] Review `TERPENE_TOTAL_MISMATCH` warnings, especially G3 and RSR
- [ ] Review Kaycha `TERPENE_BREAKDOWN_MISSING`
- [ ] Add normalized cannabinoid CSV export
- [ ] Add normalized terpene CSV export
- [ ] Add safety/compliance panel extraction
- [ ] Add database ingestion path
- [ ] Add automated support matrix generation from fixture coverage

---

## 🔥 Recommended Next Engineering Step

Based on the latest 1,500-row stress audit, the next highest-impact target is:

```text
NV Cann Labs rows currently classified as Edible
```

Why:

```text
NV Cann Labs has 37 rows missing ProductName|BatchId.
All 37 are currently ProductType = Edible.
The batch is Flower-focused, so these are likely product-type detection false positives or Flower-adjacent layout variants.
```

Recommended next workflow:

1. Inspect 3-5 representative NV Cann `Edible` rows.
2. Determine whether they are true edibles or Flower/plant-material false positives.
3. If false positives, add narrow NV Cann product-type detection.
4. Add fixture-backed tests.
5. Regenerate the 1,500-row audit CSV.
6. Re-score the dirty row board.

Secondary targets:

```text
Kaycha: 20 smaller metadata gaps
G3 / RSR / MA / Ace: small Unknown or metadata leftovers
TERPENE_TOTAL_MISMATCH warning review
```

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

CannabisCOA.Parser has moved from a Flower-only parser into a real-world Nevada COA audit engine.

Current state:

```text
276/276 tests passing
1,500-row real batch stress test
95.7% clean audit rows
1,450 Flower rows detected
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
Identify dirty rows by lab/layout
Patch narrow parser behavior
Validate with fixtures
Repeat
```

This project is now ready for continued Flower v1 hardening, expanded compliance panel extraction, normalized cannabinoid/terpene exports, and future database ingestion.