# Creates one GitHub issue per draft in this folder. Requires the GitHub CLI (`gh auth login`).
# Run from the repo root:  pwsh .github/issue-drafts/create-issues.ps1   (add -WhatIf to preview)
[CmdletBinding(SupportsShouldProcess)]
param([string]$Repo = 'KiarashMinoo/ThunderPropagator.Clients.DotNet')
$ErrorActionPreference = 'Stop'
Get-ChildItem -Path $PSScriptRoot -Filter '[0-9][0-9]-*.md' | Sort-Object Name | ForEach-Object {
  $text   = Get-Content $_.FullName -Raw
  $title  = ([regex]::Match($text, '(?m)^title:\s*(.+)$')).Groups[1].Value.Trim()
  $labels = ([regex]::Match($text, '(?m)^labels:\s*(.+)$')).Groups[1].Value.Trim()
  $body   = ($text -replace '(?s)^---.*?---\s*', '')
  $tmp    = New-TemporaryFile
  Set-Content -Path $tmp -Value $body -Encoding utf8
  if ($PSCmdlet.ShouldProcess($title, "gh issue create in $Repo")) {
    foreach ($l in $labels.Split(',')) { gh label create $l.Trim() --repo $Repo --force 2>$null | Out-Null }
    gh issue create --repo $Repo --title $title --body-file $tmp --label $labels
  }
  Remove-Item $tmp -Force
}
