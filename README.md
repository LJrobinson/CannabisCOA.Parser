# CannabisCOA.Parser

A .NET parser for extracting structured cannabis Certificate of Analysis (COA) data from messy lab PDF/text reports.

COAs are often formatted for humans, not databases. This project aims to turn inconsistent lab reports into clean, usable structured data for analytics, compliance review, inventory workflows, and automation.

This parser is designed to handle real-world COA variability, where formatting, units, and structure differ significantly between laboratories.

## Current Status

Flower COA parsing is complete for 8 Nevada labs:

- 374 Labs
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
- ProductType-aware validation thresholds
- Non-detect confidence normalization
- Total THC / CBD sanity checks
- Terpene breakdown completeness warnings
- Terpene total vs analyte-sum mismatch detection

Batch parsing across real COA PDFs is supported via CLI.

## Supported / Target Labs

- 374 Labs
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
- Terpene totals are cross-checked against individual analytes with mismatch detection
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
        "SourceText": "\u03949-THC 0.160 0.544 5.44 \u03B1-Terpinene 0.0125 \u003C0.0125 \u003C0.00125",
        "Confidence": 0.95
      },
      "THCA": {
        "FieldName": "THCA",
        "Value": 26.278,
        "SourceText": "THCa 0.160 26.278 262.78 \u03B1-Bisabolol 0.0125 0.2833 0.02833",
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
        "SourceText": "CBDa 0.160 ND ND cis-Nerolidol 0.0125 \u003C0.0125 \u003C0.00125",
        "Confidence": 0
      },
      "TotalTHC": 23.59,
      "TotalCBD": 0.00
    },
    "Terpenes": {
      "Terpenes": {
        "\u03B2-Myrcene": 0.66359,
        "\u03B2-Caryophyllene": 0.53103,
        "\u03B4-Limonene": 0.41148,
        "Linalool": 0.23188,
        "\u03B1-Humulene": 0.17647,
        "\u03B2-Pinene": 0.06839,
        "\u03B1-Pinene": 0.06135,
        "\u03B1-Bisabolol": 0.02833
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
    "DominantTerpene": "\u03B2-Myrcene",
    "TopTerpenes": [
      "\u03B2-Myrcene",
      "\u03B2-Caryophyllene",
      "\u03B4-Limonene"
    ],
    "ProfileType": "Floral",
    "Lean": "Unknown"
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
- [x] Chemistry validation (THC / CBD / terpenes)
- [x] ProductType-aware validation profiles
- [x] ND / LOQ normalization with confidence handling
- [x] Terpene total vs breakdown validation
- [x] Mixed-product batch testing baseline
- [ ] Expand terpene breakdown parsing for remaining total-only lab outputs
- [ ] Add safety/compliance category parsing
- [ ] Export parsed results to CSV / database
- [ ] Expand full support for Pre-roll, Edible, Concentrate, Vape, Tincture, and Topical COAs

## Tech Stack

- C#
- .NET
- CLI-first workflow
- JSON output
- Test-driven parser rules

## Status

Active development — Flower COA parsing is currently stable across 8 Nevada labs, with ongoing expansion into additional product types and deeper validation rules.

The parser is built fixture-first using real-world messy COA samples, with lab-specific adapters and regression tests for known extraction edge cases.

The current validation layer includes product-aware thresholds and data integrity checks to reduce false positives across different cannabis product categories.