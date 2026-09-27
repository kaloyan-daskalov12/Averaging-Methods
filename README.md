# Averaging Methods App

A console application (C#) for combining a set of measured values, each with its own uncertainty, using several different statistical averaging methods. Typical use case: processing physics lab measurements — combining repeated measurements of a quantity into a single best estimate with an uncertainty.

## Overview

The app reads data from a single text file or a directory tree of text files, computes one or more selected averaging methods, and writes the results to a single file or a directory tree mirroring the input structure.

Each input line is expected in the format:

```
value {uncertainty}
```

for example `12.34 {0.05}`. Lines that don't match this format exactly (wrong number of tokens, or non-numeric values) are silently skipped by `ParseValues` in `UIApp.cs` — there is no warning, so malformed lines can be dropped without notice.

## Project Structure

- `Program.cs` – entry point; creates a `UIApp` and calls `Run()`.
- `Form.cs` – drives the sequence of console prompts (input type, input path, output type, output path, methods) and validates each answer.
- `Tools.cs` – helper class with a single static method, `ParseSquareBrackets`, which parses strings like `keyword[1,2,3]` into a keyword and a list of arguments.
- `UIApp.cs` – the main application loop: reads commands, handles the special keywords (`?`, `help`, `exit`, `stop`), reads the input file(s)/directory, and writes the results.
- `IAverageMethod.cs` – interface implemented by every averaging method (`Name` + `Compute`).
- `Value.cs` – simple data holder with two fields: `Val` (measured value) and `DVal` (its uncertainty).
- `WeightedMean.cs`, `WeightedAverage.cs` – two implementations of the classic inverse-variance weighted mean.
- `EvaluatedAverage.cs` – a stricter variant with a weight-limiting rule and a chi-squared consistency check.
- `LimitedWeightMean.cs` – an iterative, standalone version of the weight-limiting rule.
- `NormalizedResidualMethod.cs` – iteratively downweights points whose residual from the mean is too large.
- `GaussianConsensusAveraging.cs` – a "soft" method where each point contributes a bell-curve-based vote of confidence in the others.
- `BirgeRatioTool.cs` – not an averaging method; a consistency test for the data (the Birge ratio).

## Usage

The program asks a sequence of questions:

1. **Input type (file or directory) `[f/d]`** – `f` for a single file, `d` for a directory.
2. **Input path** – must exist (checked with `Path.Exists`).
3. **Output type (single file or directory) `[f/d]`** (there's a small typo in the source: "single file **ot** directory" instead of "or").
4. **Output path** – not validated at all, so an invalid path will only surface once the program tries to write to it.
5. **Methods** – expects something like `x[0,2,4]`, where the numbers in brackets are the indices of the methods to run (the actual list of available methods comes from `AveragingTool.GetMethods()`).

Special commands, available at any prompt:

- `?` or `help` – prints an explanation of each step plus the list of available methods.
- `exit` – quits the program.
- `stop` – resets the form and starts over (useful if you made a mistake).
- An empty line is simply ignored.

Once all five answers are collected, the program reads the input, computes the selected methods, and writes the results — either mirroring the input directory structure, or into a single combined file (with each source prefixed by `Source: ...`).

## Averaging Methods

Most methods build on the classic inverse-variance weighted mean: each point's weight is one divided by the square of its uncertainty, so more precise measurements carry more weight. The mean is the weighted sum of the values divided by the sum of the weights, and the uncertainty of that mean is the square root of one divided by the sum of the weights.

### Weighted Mean / Weighted Average v2
Both compute the weighted mean described above. The difference: `WeightedMean` requires every uncertainty to be greater than zero and throws an error otherwise; `WeightedAverage` (v2) first filters out points with an invalid uncertainty and averages only the valid ones.

### Evaluated Average
A more careful variant:
- If any single point carries more than half of the total weight, its weight is capped so that it contributes exactly half of the new total weight (the "LWM" — Limitation of relative statistical Weight — rule).
- The final uncertainty is never allowed to fall below the smallest input uncertainty (the "minimum error" rule).
- A reduced chi-squared value is calculated by summing, for every point, its weight multiplied by the squared difference between its value and the mean, then dividing that sum by the number of points minus one. If this value is greater than one, the final uncertainty is scaled up by its square root (the scaled-up value is called the "external uncertainty").
- The result is compared against a tabulated 95% critical chi-squared value for the relevant degrees of freedom, to flag the data as "discrepant" or not.

### Limited Weight Mean
The same weight-capping idea as above — limiting any point to at most half the total weight — but applied repeatedly (up to 100 passes) until no further correction is needed. It's a standalone method with a configurable threshold, rather than a rule built into another method.

### Normalized Residual Method
An iterative method: compute the weighted mean, then for each point work out how far its value is from the mean relative to its own uncertainty (its "normalized residual"). If that residual is larger than a set threshold (two, by default), the point's weight is reduced in proportion to the square of the residual. This repeats until the weights stabilize (up to 50 iterations) — a soft way of suppressing outliers rather than discarding them outright.

### Gaussian Consensus Averaging
A different approach: each point "votes" on how well the others fit it, using a bell-curve (Gaussian) function centered on its own value and uncertainty. The votes are summed and normalized into weights. The final uncertainty combines, in quadrature, the spread of the values around the mean and, optionally, the averaged reported uncertainties.

### Birge Ratio (not an averaging method)
This class does not implement `IAverageMethod` — it only checks whether the scatter in the data is consistent with the reported uncertainties. It computes the standard weighted mean and the reduced chi-squared value described above, then takes the square root of that value to get the Birge ratio. This ratio is compared against a critical limit — one plus the square root of two divided by the degrees of freedom. A ratio above that limit suggests the data may be inconsistent (uncertainties underestimated, or a systematic effect present).

## Missing File: AveragingTool.cs

The provided source does not include `AveragingTool.cs`, even though `UIApp.cs` depends on it (`Tool.GetMethods()` and `Tool.GetResult(values, indices)`). Based on how it's used:

- `GetMethods()` returns a string listing the available methods (used in the help text).
- `GetResult(List<Value>, List<int>)` returns a list of text lines — presumably one result line per selected method index, formatted for writing directly to a file.

The exact logic for selecting methods by index, or for formatting the results, can't be documented without that file.

## Known Limitations

- Typo in `Form.cs`: "single file **ot** directory" instead of "or".
- `ParseValues` silently skips malformed lines — no warning is given if a data line fails to parse.
- The input format expects exactly two whitespace-separated tokens per line; extra spaces inside the braces (e.g. `12.3 { 0.4 }`) will cause the line to be rejected.
- `WeightedMean` throws on a non-positive uncertainty, while `WeightedAverage` (v2) silently filters such points out — the near-identical names but different behavior can be confusing.
- `Output path` is not validated at all; an invalid path only fails once the program attempts to write to it.
