#requires -Version 7.0
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$files = @(git -C $root ls-files --cached --others --exclude-standard)
if ($LASTEXITCODE -ne 0) { throw 'Could not enumerate repository files.' }
$allowed = '^(README\.md|ARCHITECTURE\.md|OPERATIONS\.md|LICENSE|CONTRIBUTING\.md|SECURITY\.md|docs/(?:[^/]+/)*[^/]+\.md|docs/examples/LicenseVerifier\.cs|\.github/FUNDING\.yml|\.github/ISSUE_TEMPLATE/(bug_report|feature_request|config)\.yml|\.github/workflows/verify-docs\.yml|scripts/verify-public\.ps1)$'
$errors = [Collections.Generic.List[string]]::new()
$linksChecked = 0
foreach ($relative in $files) {
    if ($relative -cnotmatch $allowed) {
        $errors.Add("File outside the public-content allowlist: $relative")
        continue
    }
    $path = Join-Path $root $relative
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { $errors.Add("Missing tracked file: $relative"); continue }
    $item = Get-Item -LiteralPath $path
    if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -or $item.Length -gt 1MB) {
        $errors.Add("Linked or oversized public file: $relative")
        continue
    }
    $text = [IO.File]::ReadAllText($path)
    if ($text -match '-----BEGIN (?:[A-Z0-9]+ )*PRIVATE KEY(?: BLOCK)?-----' -or
        $text -match '(?:gh[pousr]_[A-Za-z0-9]{36}|github_pat_[A-Za-z0-9_]{60,}|AKIA[A-Z0-9]{16})') {
        $errors.Add("Possible private key or credential in: $relative")
    }
    if (-not $relative.EndsWith('.md',[StringComparison]::Ordinal)) { continue }
    foreach ($match in [regex]::Matches($text,'\[[^\]]*\]\(([^\s)]+)(?:\s+"[^"]*")?\)')) {
        $target = $match.Groups[1].Value.Trim('<','>')
        if ($target -match '^[a-zA-Z][a-zA-Z0-9+.-]*:' -or $target.StartsWith('#')) { continue }
        $fileTarget = [Uri]::UnescapeDataString(($target -split '[#?]',2)[0])
        if (-not $fileTarget) { continue }
        $resolved = [IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $path) $fileTarget))
        $contained = $resolved.StartsWith($root+[IO.Path]::DirectorySeparatorChar,[StringComparison]::Ordinal)
        if (-not $contained -or -not (Test-Path -LiteralPath $resolved)) {
            $errors.Add("Broken or external relative path in ${relative}: $target")
        }
        $linksChecked++
    }
}
if ($errors.Count) { throw ($errors -join "`n") }
Write-Host "Public content verified: $($files.Count) files, $linksChecked relative file links."
