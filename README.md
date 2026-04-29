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

> Parse Flower COAs from 8 Nevada labs with 90%+ field accuracy.

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

```bash
dotnet run --project src/CannabisCOA.Parser.Cli -- --file fixtures/digipath-flower.txt