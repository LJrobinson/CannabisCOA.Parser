# CannabisCOA.Parser

A .NET parser for extracting structured cannabis Certificate of Analysis (COA) data from messy lab PDF/text reports.

COAs are often formatted for humans, not databases. This project aims to turn inconsistent lab reports into clean, usable structured data for analytics, compliance review, inventory workflows, and automation.

## Current Status

Flower COA parsing is complete for 8 Nevada labs:

- 374Labs
- G3 Labs
- NV Cann Labs
- Ace Analytical Laboratory
- Kaycha Labs
- Digipath Labs
- MA Analytics
- RSR Analytical Laboratories

The parser handles:

- Lab-specific formatting differences
- Interleaved cannabinoid / terpene tables
- Column-based and row-based layouts
- mg/g ↔ % normalization
- Precision preservation
- ND / LOQ handling
- Total THC validation (THCa * 0.877 + THC)
- Terpene total validation

Batch parsing across real COA PDFs is supported via CLI.

## Supported / Target Labs

- 374Labs
- G3 Labs
- NV Cann Labs
- Ace Analytical Laboratory
- Kaycha Labs
- Digipath Labs
- MA Analytics
- RSR Analytical Laboratories

## Validation & Scoring

Parsed COAs are not just extracted — they are validated and scored:

- Total THC is verified using standard cannabinoid conversion formulas
- Terpene totals are cross-checked against individual analytes
- Freshness scoring based on test date
- Compliance flags (pass/fail detection)
- Overall product scoring (Potency / Terpenes / Freshness / Compliance)

This allows COAs to be used directly for analytics and decision-making.

## Example Usage

Parse a fixture file:

dotnet run --project src/CannabisCOA.Parser.Cli -- --file fixtures/digipath-flower.txt

Parse inline text:

dotnet run --project src/CannabisCOA.Parser.Cli -- "THC: 0.42% THCA: 24.88% ..."

Run score-only output:

dotnet run --project src/CannabisCOA.Parser.Cli -- --file fixtures/digipath-flower.txt --score-only

## Example Output

{
  "productType": "Flower",
  "labName": "Digipath Labs",
  "productName": "Sample Flower",
  "batchId": "ABC123",
  "harvestDate": "2025-10-23",
  "testDate": "2025-11-25",
  "cannabinoids": {
    "THC": {
      "value": 0.42,
      "unit": "%"
    },
    "THCA": {
      "value": 24.88,
      "unit": "%"
    }
  }
}

## Batch Processing

Parse a folder of COAs into newline-delimited JSON:

dotnet run --project src/CannabisCOA.Parser.Cli -- --batch G:\COAs --out parsed.jsonl

## Why This Exists

Cannabis operators, analysts, and compliance teams often need COA data in a usable format, but lab reports are usually locked inside inconsistent PDFs.

This project is built to reduce manual entry, improve repeatability, and make cannabis lab data easier to work with.

## Roadmap

- [x] Flower COA parsing across 8 Nevada labs
- [x] Batch processing via CLI
- [x] Chemistry validation (THC / terpenes)
- [ ] Improve CBD / CBDA edge-case handling
- [ ] Expand terpene parsing coverage for all labs
- [ ] Add safety/compliance category parsing
- [ ] Export parsed results to CSV / database
- [ ] Support additional product types (Pre-roll, Edible, Concentrate, etc.)

## Tech Stack

- C#
- .NET
- CLI-first workflow
- JSON output
- Test-driven parser rules

## Status

Early active development.

The parser is being built lab-by-lab with real-world messy COA samples.
