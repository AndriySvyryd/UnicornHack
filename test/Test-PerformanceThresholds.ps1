param(
    [Parameter(Mandatory = $true)]
    [string] $CoreResultsPath,

    [Parameter(Mandatory = $true)]
    [string] $WebResultsPath
)

$ErrorActionPreference = 'Stop'
$InvariantCulture = [System.Globalization.CultureInfo]::InvariantCulture
$NumberStyles = [System.Globalization.NumberStyles]::Float -bor [System.Globalization.NumberStyles]::AllowThousands

function Get-BenchmarkRows([string] $resultsPath, [string] $requiredMethod) {
    $files = Get-ChildItem -Path $resultsPath -Recurse -Filter '*-report.csv' |
        Sort-Object LastWriteTimeUtc -Descending

    foreach ($file in $files) {
        $rows = @(Import-Csv $file.FullName)
        if ($rows.Method -contains $requiredMethod) {
            return $rows
        }
    }

    throw "No benchmark report containing '$requiredMethod' was found under '$resultsPath'."
}

function Get-BenchmarkCase(
    [object[]] $rows,
    [string] $method,
    [string] $parameter,
    [int] $value) {
    $benchmarkCases = @($rows | Where-Object {
        $_.Method -eq $method -and $_.$parameter -eq [string] $value
    })
    if ($benchmarkCases.Count -eq 0) {
        throw "Benchmark case '$method' with $parameter=$value was not found."
    }

    $shortRunMatches = @($benchmarkCases | Where-Object Job -eq 'ShortRun')
    if ($shortRunMatches.Count -ne 0) {
        return $shortRunMatches[0]
    }

    $measuredMatches = @($benchmarkCases | Where-Object Job -ne 'Dry')
    return ($measuredMatches.Count -ne 0 ? $measuredMatches[0] : $benchmarkCases[0])
}

function Get-Metric([object] $row, [string] $prefix) {
    $column = $row.PSObject.Properties.Name |
        Where-Object { $_.StartsWith($prefix, [System.StringComparison]::Ordinal) } |
        Select-Object -First 1
    if ($null -eq $column) {
        throw "Benchmark report does not contain a '$prefix' column."
    }

    return [double]::Parse($row.$column, $NumberStyles, $InvariantCulture)
}

function Assert-Maximum([string] $name, [double] $actual, [double] $maximum) {
    if ($actual -gt $maximum) {
        throw "$name was $($actual.ToString('F3', $InvariantCulture)); maximum is $($maximum.ToString('F3', $InvariantCulture))."
    }

    Write-Host "PASS: $name = $($actual.ToString('F3', $InvariantCulture)) <= $($maximum.ToString('F3', $InvariantCulture))"
}

$coreRows = @(Get-BenchmarkRows $CoreResultsPath 'Resolve_property')
$effectThresholds = @(
    @{ Method = 'Resolve_property'; MaximumRatio = 6.0 },
    @{ Method = 'Apply_status'; MaximumRatio = 2.0 },
    @{ Method = 'Query_effective_ability_level'; MaximumRatio = 2.0 },
    @{ Method = 'Extend_movement_reactive_effects'; MaximumRatio = 2.0 }
)

foreach ($threshold in $effectThresholds) {
    $small = Get-BenchmarkCase $coreRows $threshold.Method 'EffectCount' 16
    $large = Get-BenchmarkCase $coreRows $threshold.Method 'EffectCount' 64
    $ratio = (Get-Metric $large 'Mean ') / (Get-Metric $small 'Mean ')
    Assert-Maximum "$($threshold.Method) 64/16 time ratio" $ratio $threshold.MaximumRatio
}

$abilityRemovalRows = @(Get-BenchmarkRows $CoreResultsPath 'Creature_death')
$abilityRemoval64 = Get-BenchmarkCase $abilityRemovalRows 'Creature_death' 'Count' 64
$abilityRemoval256 = Get-BenchmarkCase $abilityRemovalRows 'Creature_death' 'Count' 256
Assert-Maximum 'Creature death 256/64 ability time ratio' `
    ((Get-Metric $abilityRemoval256 'Mean ') / (Get-Metric $abilityRemoval64 'Mean ')) 6.0
Assert-Maximum 'Creature death 256/64 ability allocation ratio' `
    ((Get-Metric $abilityRemoval256 'Allocated ') / (Get-Metric $abilityRemoval64 'Allocated ')) 4.0
$creatureInstantiation64 = Get-BenchmarkCase $abilityRemovalRows 'Instantiate_creatures' 'Count' 64
$creatureInstantiation256 = Get-BenchmarkCase $abilityRemovalRows 'Instantiate_creatures' 'Count' 256
Assert-Maximum 'Creature instantiation 256/64 time ratio' `
    ((Get-Metric $creatureInstantiation256 'Mean ') / (Get-Metric $creatureInstantiation64 'Mean ')) 6.0
Assert-Maximum 'Creature instantiation 256/64 allocation ratio' `
    ((Get-Metric $creatureInstantiation256 'Allocated ') / (Get-Metric $creatureInstantiation64 'Allocated ')) 4.0

$webRows = @(Get-BenchmarkRows $WebResultsPath 'WaitTurn_Delta')
foreach ($scenario in @('WaitTurn', 'Move')) {
    $delta64 = Get-BenchmarkCase $webRows "${scenario}_Delta" 'NpcCount' 64
    $snapshot64 = Get-BenchmarkCase $webRows "${scenario}_FullSnapshot" 'NpcCount' 64
    $delta24 = Get-BenchmarkCase $webRows "${scenario}_Delta" 'NpcCount' 24

    Assert-Maximum "$scenario 64-NPC delta/full time ratio" `
        ((Get-Metric $delta64 'Mean ') / (Get-Metric $snapshot64 'Mean ')) 1.25
    Assert-Maximum "$scenario 64-NPC delta/full allocation ratio" `
        ((Get-Metric $delta64 'Allocated ') / (Get-Metric $snapshot64 'Allocated ')) 0.65
    Assert-Maximum "$scenario delta allocation 64/24 scaling" `
        ((Get-Metric $delta64 'Allocated ') / (Get-Metric $delta24 'Allocated ')) 3.0
}

Write-Host 'All performance regression thresholds passed.'