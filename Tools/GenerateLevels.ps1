# Generator for Kit level files 2.json - 120.json
# Per spec: difficulty bands, cumulative obstacles, board shape variations.
# Obstacle mapping (Kit only supports Marshmallow/Chocolate/Unbreakable + Ice element):
#   Dust   -> Chocolate (spreading hazard)
#   Crate  -> Marshmallow
#   TeaCup -> Marshmallow (themed reuse)
#   Frozen -> Ice elementType wrapping candies
#   Rope   -> Marshmallow (themed reuse)
# Uses a custom JSON serializer to match the 1.json format (4-space indent, ordered keys).

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$LevelDir = Join-Path $RepoRoot "Assets\CandyMatch3Kit\Resources\Levels"
$Colors = @("Blue", "Green", "Orange", "Purple", "Red", "Yellow")

function New-CandyTile {
    return [ordered]@{
        type = "RandomCandy"
        elementType = "None"
        '$type' = "GameVanilla.Game.Common.CandyTile"
    }
}

function New-IceCandyTile {
    return [ordered]@{
        type = "RandomCandy"
        elementType = "Ice"
        '$type' = "GameVanilla.Game.Common.CandyTile"
    }
}

function New-MarshmallowTile {
    return [ordered]@{
        type = "Marshmallow"
        elementType = "None"
        '$type' = "GameVanilla.Game.Common.SpecialBlockTile"
    }
}

function New-ChocolateTile {
    return [ordered]@{
        type = "Chocolate"
        elementType = "None"
        '$type' = "GameVanilla.Game.Common.SpecialBlockTile"
    }
}

function Get-Band {
    param([int]$n)
    if ($n -le 10)  { return @{ moves = 35; target = 500;   w = 7; h = 7 } }
    if ($n -le 17)  { return @{ moves = 30; target = 1000;  w = 7; h = 7 } }
    if ($n -le 25)  { return @{ moves = 30; target = 1000;  w = 8; h = 8 } }
    if ($n -le 40)  { return @{ moves = 25; target = 2000;  w = 8; h = 8 } }
    if ($n -le 55)  { return @{ moves = 22; target = 3500;  w = 8; h = 8 } }
    if ($n -le 80)  { return @{ moves = 20; target = 5000;  w = 9; h = 9 } }
    if ($n -le 100) { return @{ moves = 18; target = 7000;  w = 9; h = 9 } }
    return              @{ moves = 16; target = 10000; w = 9; h = 9 }
}

function Build-Level {
    param([int]$n)

    $band = Get-Band $n
    $moves = $band.moves
    $target = $band.target
    $w = $band.w
    $h = $band.h

    $introLevels = @(12, 20, 28, 40, 55)
    $easyFollowers = @(13, 21, 29, 41, 56)
    if ($introLevels -contains $n)   { $moves += 5 }
    if ($easyFollowers -contains $n) { $moves += 3 }

    $total = $w * $h
    $tiles = New-Object 'System.Collections.ArrayList'
    for ($i = 0; $i -lt $total; $i++) {
        [void]$tiles.Add((New-CandyTile))
    }

    # Board shape
    if ($n -ge 26 -and $n -le 50) {
        $cutSize = if ($n -le 35) { 1 } else { 2 }
        for ($r = 0; $r -lt $cutSize; $r++) {
            for ($c = 0; $c -lt $cutSize; $c++) {
                $tiles[$r * $w + $c] = $null
                $tiles[$r * $w + ($w - 1 - $c)] = $null
                $tiles[($h - 1 - $r) * $w + $c] = $null
                $tiles[($h - 1 - $r) * $w + ($w - 1 - $c)] = $null
            }
        }
    } elseif ($n -ge 51) {
        $cutCount = 4 + ($n % 6)
        for ($i = 0; $i -lt $cutCount; $i++) {
            $idx = (($n * 7) + ($i * 13)) % $total
            $tiles[$idx] = $null
        }
    }

    $hasDust   = $n -ge 12 -and $n -ne 13
    $hasCrate  = $n -ge 20 -and $n -ne 21
    $hasTeacup = $n -ge 28 -and $n -ne 29
    $hasFrozen = $n -ge 40 -and $n -ne 41
    $hasRope   = $n -ge 55 -and $n -ne 56

    $obstacles = New-Object 'System.Collections.ArrayList'
    if ($hasDust)   { [void]$obstacles.Add("chocolate") }
    if ($hasCrate)  { [void]$obstacles.Add("marshmallow") }
    if ($hasTeacup) { [void]$obstacles.Add("marshmallow") }
    if ($hasRope)   { [void]$obstacles.Add("marshmallow") }

    # Place obstacles, retrying when a candidate index is a hole
    for ($oi = 0; $oi -lt $obstacles.Count; $oi++) {
        $ob = $obstacles[$oi]
        $count = 1 + (($n + $oi) % 3)
        $placed = 0
        $attempt = 0
        while ($placed -lt $count -and $attempt -lt ($total * 2)) {
            $idx = (($n * 11) + ($oi * 17) + ($attempt * 23)) % $total
            $attempt++
            if ($null -eq $tiles[$idx]) { continue }
            if ($tiles[$idx].'$type' -ne "GameVanilla.Game.Common.CandyTile") { continue }
            if ($ob -eq "chocolate") {
                $tiles[$idx] = New-ChocolateTile
            } else {
                $tiles[$idx] = New-MarshmallowTile
            }
            $placed++
        }
    }

    if ($hasFrozen) {
        $iceCount = 3 + ($n % 4)
        $placed = 0
        $attempt = 0
        while ($placed -lt $iceCount -and $attempt -lt ($total * 2)) {
            $idx = (($n * 13) + ($attempt * 7)) % $total
            $attempt++
            if ($null -eq $tiles[$idx]) { continue }
            if ($tiles[$idx].'$type' -ne "GameVanilla.Game.Common.CandyTile") { continue }
            if ($tiles[$idx].elementType -eq "Ice") { continue }
            $tiles[$idx] = New-IceCandyTile
            $placed++
        }
    }

    $boostersEnabled = $n -ge 5

    return [ordered]@{
        id = $n
        width = $w
        height = $h
        tiles = $tiles
        limitType = "Moves"
        limit = $moves
        goals = @(
            [ordered]@{
                score = $target
                '$type' = "GameVanilla.Game.Common.ReachScoreGoal"
            }
        )
        availableColors = $Colors
        score1 = [int]($target / 3)
        score2 = [int](($target * 2) / 3)
        score3 = $target
        awardSpecialCandies = ($n -ge 11)
        awardedSpecialCandyType = "Striped"
        collectableChance = 0
        availableBoosters = [ordered]@{
            Lollipop = $boostersEnabled
            Bomb     = $boostersEnabled
            Switch   = $boostersEnabled
            ColorBomb = $boostersEnabled
        }
    }
}

function ConvertTo-PrettyJson {
    param($obj, [int]$indent = 0)
    $pad = '    ' * $indent
    $padInner = '    ' * ($indent + 1)

    if ($null -eq $obj) { return 'null' }
    if ($obj -is [bool]) { return $(if ($obj) { 'true' } else { 'false' }) }
    if ($obj -is [int] -or $obj -is [long] -or $obj -is [double] -or $obj -is [decimal]) {
        return "$obj"
    }
    if ($obj -is [string]) {
        $escaped = $obj.Replace('\', '\\').Replace('"', '\"')
        return '"' + $escaped + '"'
    }
    if ($obj -is [System.Collections.IDictionary]) {
        if ($obj.Count -eq 0) { return '{}' }
        $sb = New-Object System.Text.StringBuilder
        [void]$sb.Append("{`n")
        $keys = @($obj.Keys)
        for ($i = 0; $i -lt $keys.Count; $i++) {
            $key = $keys[$i]
            $val = ConvertTo-PrettyJson -obj $obj[$key] -indent ($indent + 1)
            [void]$sb.Append($padInner)
            [void]$sb.Append('"' + $key + '": ' + $val)
            if ($i -lt $keys.Count - 1) { [void]$sb.Append(',') }
            [void]$sb.Append("`n")
        }
        [void]$sb.Append($pad + '}')
        return $sb.ToString()
    }
    if ($obj -is [System.Collections.IEnumerable]) {
        $items = @()
        foreach ($x in $obj) { $items += ,$x }
        if ($items.Count -eq 0) { return '[]' }
        $sb = New-Object System.Text.StringBuilder
        [void]$sb.Append("[`n")
        for ($i = 0; $i -lt $items.Count; $i++) {
            $val = ConvertTo-PrettyJson -obj $items[$i] -indent ($indent + 1)
            [void]$sb.Append($padInner)
            [void]$sb.Append($val)
            if ($i -lt $items.Count - 1) { [void]$sb.Append(',') }
            [void]$sb.Append("`n")
        }
        [void]$sb.Append($pad + ']')
        return $sb.ToString()
    }
    return '"' + $obj.ToString() + '"'
}

function Write-MetaIfMissing {
    param([int]$n)
    $metaPath = Join-Path $LevelDir "$n.json.meta"
    if (Test-Path -LiteralPath $metaPath) { return }
    $guid = [guid]::NewGuid().ToString("N")
    $now = [int][Math]::Floor(([DateTime]::UtcNow - [DateTime]'1970-01-01').TotalSeconds)
    $meta = "fileFormatVersion: 2`nguid: $guid`ntimeCreated: $now`nlicenseType: Store`nTextScriptImporter:`n  userData: `n  assetBundleName: `n  assetBundleVariant: `n"
    [System.IO.File]::WriteAllText($metaPath, $meta)
}

$created = 0
$updated = 0
for ($n = 2; $n -le 120; $n++) {
    $level = Build-Level $n
    $json = ConvertTo-PrettyJson -obj $level
    $path = Join-Path $LevelDir "$n.json"
    $existed = Test-Path -LiteralPath $path
    [System.IO.File]::WriteAllText($path, $json + "`n")
    if ($existed) { $updated++ } else { $created++ }
    Write-MetaIfMissing -n $n
}

Write-Host "Levels generated. Updated: $updated  Created: $created"
