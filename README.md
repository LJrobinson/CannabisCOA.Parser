# CannabisCOA.Parser

A .NET parser for extracting structured cannabis Certificate of Analysis (COA) data from messy lab PDF/text reports.

COAs are often formatted for humans, not databases. This project aims to turn inconsistent lab reports into clean, usable structured data for analytics, compliance review, inventory workflows, and automation.

## Current Focus

The current target is:

- Flower COAs
- Nevada cannabis labs
- Text-based PDFs / extracted text
- Clean JSON output
- Repeatable parsing rules by lab

Target milestone:

Parse Flower COAs from 8 Nevada labs with 90%+ field accuracy.

## Supported / Target Labs

- 374Labs
- G3 Labs
- NV Cann Labs
- Ace Analytical Laboratory
- Kaycha Labs
- Digipath Labs
- MA Analytics
- RSR Analytical Laboratories

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

## Why This Exists

Cannabis operators, analysts, and compliance teams often need COA data in a usable format, but lab reports are usually locked inside inconsistent PDFs.

This project is built to reduce manual entry, improve repeatability, and make cannabis lab data easier to work with.

## Roadmap

- [ ] Support all 8 target labs for Flower COAs
- [ ] Add accuracy scoring against known expected outputs
- [ ] Improve field-level validation
- [ ] Export parsed results to CSV
- [ ] Expand support to additional product types
- [ ] Add batch processing for folders of COAs

## Tech Stack

- C#
- .NET
- CLI-first workflow
- JSON output
- Test-driven parser rules

## Status

Early active development.

The parser is being built lab-by-lab with real-world messy COA samples.
