#requires -Version 7.0
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$policy = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'public-content.json') -Raw | ConvertFrom-Json
if ($policy.schemaVersion -cne 1) { throw 'Unsupported public-content policy.' }
$allowed = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($name in $policy.files) {
    if ($name -isnot [string] -or $name -match '(^/|\\|:|(^|/)\.{1,2}(/|$)|(^|/)\.git(/|$))' -or -not $allowed.Add($name)) {
        throw 'Invalid or duplicate path in the public-content policy.'
    }
}
if (-not $allowed.Contains('scripts/public-content.json') -or -not $allowed.Contains('scripts/verify-public.ps1')) {
    throw 'The public-content policy must include itself and its checker.'
}
$files = @(git -C $root ls-files --cached --others --exclude-standard)
if ($LASTEXITCODE -ne 0) { throw 'Could not enumerate repository files.' }
$found = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$errors = [Collections.Generic.List[string]]::new()
$content = [Collections.Generic.Dictionary[string,string]]::new([StringComparer]::Ordinal)
foreach ($relative in $files) {
    [void]$found.Add($relative)
    if (-not $allowed.Contains($relative)) {
        $errors.Add("File outside the explicit public-content inventory: $relative")
        continue
    }
    $path = $root
    $invalid = $false
    foreach ($segment in $relative.Split('/')) {
        $path = Join-Path $path $segment
        if (-not (Test-Path -LiteralPath $path)) { $invalid = $true; break }
        $item = Get-Item -LiteralPath $path -Force
        if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { $invalid = $true; break }
    }
    if ($invalid -or -not (Test-Path -LiteralPath $path -PathType Leaf) -or (Get-Item -LiteralPath $path).Length -gt 1MB) {
        $errors.Add("Missing, linked or oversized public file: $relative")
        continue
    }
    $text = [IO.File]::ReadAllText($path, [Text.UTF8Encoding]::new($false, $true))
    if ($text -match '-----BEGIN (?:[A-Z0-9]+ )*PRIVATE KEY(?: BLOCK)?-----' -or
        $text -match '(?:gh[pousr]_[A-Za-z0-9]{36}|github_pat_[A-Za-z0-9_]{60,}|AKIA[A-Z0-9]{16})') {
        $errors.Add("Possible private key or credential in: $relative")
    }
    $content[$relative] = $text
}
foreach ($required in $allowed) {
    if (-not $found.Contains($required)) { $errors.Add("Required public file is missing: $required") }
}
$linksChecked = 0
$patterns = @(
    '\]\((<?[^\s)>]+>?)(?:\s+"[^"]*")?\)',
    '(?m)^\s{0,3}\[[^\]\n]+\]:\s*(<?[^\s>]+>?)',
    '(?i)\b(?:href|src)\s*=\s*["'']([^"'']+)["'']'
)
foreach ($relative in $content.Keys) {
    if (-not $relative.EndsWith('.md', [StringComparison]::Ordinal)) { continue }
    foreach ($pattern in $patterns) {
        foreach ($match in [regex]::Matches($content[$relative], $pattern)) {
            $target = $match.Groups[1].Value.Trim('<','>')
            if ($target -match '^(https?|mailto):') { continue }
            if ($target -match '^[a-zA-Z][a-zA-Z0-9+.-]*:' -or $target.StartsWith('/') -or $target.StartsWith('\')) {
                $errors.Add("Unsupported absolute link in ${relative}: $target")
                continue
            }
            $fileTarget = ($target -split '[#?]',2)[0]
            for ($i=0; $i -lt 8; $i++) {
                $decoded = [Uri]::UnescapeDataString($fileTarget)
                if ($decoded -ceq $fileTarget) { break }
                $fileTarget = $decoded
            }
            if ($i -eq 8 -or $fileTarget -match '(%(?![0-9A-Fa-f]{2})|\\|:|^/)') {
                $errors.Add("Invalid encoded link in ${relative}: $target")
                continue
            }
            $documentPath = Join-Path $root $relative
            $resolved = if ($fileTarget) { [IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $documentPath) $fileTarget)) } else { $documentPath }
            if (-not $resolved.StartsWith($root+[IO.Path]::DirectorySeparatorChar,[StringComparison]::Ordinal)) {
                $errors.Add("Escaping relative link in ${relative}: $target")
                continue
            }
            $targetName = $resolved.Substring($root.Length+1).Replace('\','/')
            if (-not $allowed.Contains($targetName) -or -not $content.ContainsKey($targetName)) {
                $errors.Add("Broken or unlisted relative link in ${relative}: $target")
            }
            $linksChecked++
        }
    }
}
if ($errors.Count) { throw ($errors -join "`n") }
Write-Host "Public content verified: $($files.Count) files, $linksChecked relative links."
