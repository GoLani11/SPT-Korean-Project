param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [string]$OutputPath,

    [switch]$RemoveDescriptionEnglishHeader
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 3.0

$Utf8NoBom = [Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $Utf8NoBom
[Console]::OutputEncoding = $Utf8NoBom
$OutputEncoding = $Utf8NoBom
$env:PYTHONUTF8 = "1"
$env:PYTHONIOENCODING = "utf-8"

Add-Type -AssemblyName System.Text.Json

$InputFull = [IO.Path]::GetFullPath($InputPath)
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = $InputFull
}
$OutputFull = [IO.Path]::GetFullPath($OutputPath)
$OutputDirectory = [IO.Path]::GetDirectoryName($OutputFull)

if (-not (Test-Path -LiteralPath $InputFull)) {
    throw "Input locale file is missing: $InputFull"
}

if (-not [string]::IsNullOrWhiteSpace($OutputDirectory)) {
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
}

$JsonText = [IO.File]::ReadAllText($InputFull, $Utf8NoBom)
$Document = [System.Text.Json.JsonDocument]::Parse($JsonText)

try {
    if ($Document.RootElement.ValueKind -ne [System.Text.Json.JsonValueKind]::Object) {
        throw "Locale JSON root must be an object: $InputFull"
    }

    $Stream = [IO.MemoryStream]::new()
    $WriterOptions = [System.Text.Json.JsonWriterOptions]::new()
    $WriterOptions.Indented = $true
    $WriterOptions.Encoder = [System.Text.Encodings.Web.JavaScriptEncoder]::UnsafeRelaxedJsonEscaping
    $Writer = [System.Text.Json.Utf8JsonWriter]::new($Stream, $WriterOptions)

    $ValueCount = 0
    $TrailingEnglishLineCount = 0
    $LeadingEnglishHeaderCount = 0

    try {
        $Writer.WriteStartObject()

        foreach ($Property in $Document.RootElement.EnumerateObject()) {
            if ($Property.Value.ValueKind -ne [System.Text.Json.JsonValueKind]::String) {
                throw "Locale value must be a string: $($Property.Name)"
            }

            $ValueCount++
            $OriginalValue = $Property.Value.GetString()
            $NewValue = $OriginalValue

            while ($true) {
                $WithoutTrailingEnglishLine = [regex]::Replace(
                    $NewValue,
                    "\r?\n\([^\r\n]*[A-Za-z][^\r\n]*\)$",
                    ""
                )
                if ($WithoutTrailingEnglishLine -eq $NewValue) {
                    break
                }

                $TrailingEnglishLineCount++
                $NewValue = $WithoutTrailingEnglishLine
            }

            if ($RemoveDescriptionEnglishHeader) {
                $WithoutLeadingEnglishHeader = [regex]::Replace(
                    $NewValue,
                    "^\[[^\r\n\]]*[A-Za-z][^\r\n\]]*\]\r?\n",
                    ""
                )
                if ($WithoutLeadingEnglishHeader -ne $NewValue) {
                    $LeadingEnglishHeaderCount++
                    $NewValue = $WithoutLeadingEnglishHeader
                }
            }

            $Writer.WriteString($Property.Name, $NewValue)
        }

        $Writer.WriteEndObject()
    }
    finally {
        $Writer.Dispose()
    }

    $OutputText = $Utf8NoBom.GetString($Stream.ToArray())
    $TempOutput = "$OutputFull.tmp"
    [IO.File]::WriteAllText($TempOutput, $OutputText, $Utf8NoBom)
    $null = Get-Content -LiteralPath $TempOutput -Raw | ConvertFrom-Json -AsHashtable
    Move-Item -LiteralPath $TempOutput -Destination $OutputFull -Force

    Write-Output "Korean-only locale created: $OutputFull"
    Write-Output "Values scanned: $ValueCount"
    Write-Output "Trailing English lines removed: $TrailingEnglishLineCount"
    if ($RemoveDescriptionEnglishHeader) {
        Write-Output "Leading English description headers removed: $LeadingEnglishHeaderCount"
    }
}
finally {
    $Document.Dispose()
}
