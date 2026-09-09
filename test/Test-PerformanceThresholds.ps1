param(
    [Parameter(Mandatory = $true)]
    [string] $CoreResultsPath,

    [Parameter(Mandatory = $true)]
    [string] $WebResultsPath,

    [switch] $UpdateBaselines,

    [string] $CoreBaselinePath = (Join-Path $PSScriptRoot `
        'UnicornHack.Core.PerformanceTests\Baselines\PerformanceBaseline.csv'),

    [string] $WebBaselinePath = (Join-Path $PSScriptRoot `
        'UnicornHack.Web.PerformanceTests\Baselines\PerformanceBaseline.csv')
)

$ErrorActionPreference = 'Stop'
$InvariantCulture = [System.Globalization.CultureInfo]::InvariantCulture
$NumberStyles = [System.Globalization.NumberStyles]::Float -bor [System.Globalization.NumberStyles]::AllowThousands
$TimeRegressionRatio = 2.0
$AllocationRegressionRatio = 1.10

function Get-BenchmarkRows([string] $resultsPath, [string] $requiredMethod) {
    $reports = @(Get-ChildItem -Path $resultsPath -Recurse -Filter '*-report.csv' |
        ForEach-Object {
            $file = $_
            $rows = @(Import-Csv $file.FullName)
            if ($rows.Method -contains $requiredMethod) {
                [pscustomobject]@{
                    Rows = $rows
                    CaseCount = $rows.Count
                    LastWriteTimeUtc = $file.LastWriteTimeUtc
                }
            }
        })
    $report = $reports |
        Sort-Object CaseCount, LastWriteTimeUtc -Descending |
        Select-Object -First 1
    if ($null -ne $report) {
        return @($report.Rows)
    }

    throw "No benchmark report containing '$requiredMethod' was found under '$resultsPath'."
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

function Get-OptionalValue([object] $row, [string] $propertyName) {
    $property = $row.PSObject.Properties[$propertyName]
    if ($null -eq $property -or $property.Value -eq '?') {
        return ''
    }

    return [string] $property.Value
}

function Format-Metric([double] $value, [string] $format) {
    return $value.ToString($format, $InvariantCulture)
}

function Get-CoreBaselineRows([object[]] $rows) {
    return @($rows | ForEach-Object {
        [pscustomobject]@{
            Method = $_.Method
            MeanMs = Format-Metric (Get-Metric $_ 'Mean ') 'F3'
            AllocatedKB = Format-Metric (Get-Metric $_ 'Allocated ') 'F2'
        }
    } | Sort-Object Method)
}

function Get-WebBaselineRows([object[]] $rows) {
    return @($rows | ForEach-Object {
        $isProjection = $_.Method -eq 'BuildAndSerialize'
        [pscustomobject]@{
            Method = $_.Method
            Scenario = Get-OptionalValue $_ 'Scenario'
            MeanMs = Format-Metric (Get-Metric $_ 'Mean ') 'F3'
            AllocatedKB = Format-Metric (Get-Metric $_ 'Allocated ') 'F2'
            PayloadBytes = if ($isProjection) { [int] (Get-Metric $_ 'Payload B') } else { '' }
        }
    } | Sort-Object Method, Scenario)
}

function Get-CaseKey([object] $row, [string[]] $keyProperties) {
    return ($keyProperties | ForEach-Object { [string] $row.$_ }) -join '|'
}

function ConvertTo-CaseMap(
    [string] $name,
    [object[]] $rows,
    [string[]] $keyProperties) {
    $caseMap = @{}
    foreach ($row in $rows) {
        $key = Get-CaseKey $row $keyProperties
        if ($caseMap.ContainsKey($key)) {
            throw "$name contains duplicate benchmark case '$key'."
        }

        $caseMap[$key] = $row
    }

    return $caseMap
}

function ConvertTo-BaselineMetric([object] $value) {
    return [double]::Parse([string] $value, $NumberStyles, $InvariantCulture)
}

function Assert-Maximum([string] $name, [double] $actual, [double] $maximum) {
    if ($actual -gt $maximum) {
        throw "$name was $($actual.ToString('F3', $InvariantCulture)); maximum is $($maximum.ToString('F3', $InvariantCulture))."
    }

    Write-Host "PASS: $name = $($actual.ToString('F3', $InvariantCulture)) <= $($maximum.ToString('F3', $InvariantCulture))"
}

function Assert-Equal([string] $name, [double] $actual, [double] $expected) {
    if ($actual -ne $expected) {
        throw "$name was $($actual.ToString('F3', $InvariantCulture)); expected $($expected.ToString('F3', $InvariantCulture))."
    }

    Write-Host "PASS: $name = $($actual.ToString('F3', $InvariantCulture))"
}

function Assert-Baseline(
    [string] $name,
    [object[]] $actualRows,
    [object[]] $baselineRows,
    [string[]] $keyProperties,
    [double] $timeScale,
    [switch] $AssertPayload) {
    $actualByKey = ConvertTo-CaseMap "$name results" $actualRows $keyProperties
    $baselineByKey = ConvertTo-CaseMap "$name baseline" $baselineRows $keyProperties
    if ($actualByKey.Count -ne $baselineByKey.Count) {
        throw "$name results contain $($actualByKey.Count) cases; baseline contains $($baselineByKey.Count)."
    }

    foreach ($key in $baselineByKey.Keys) {
        if (-not $actualByKey.ContainsKey($key)) {
            throw "$name baseline case '$key' was not found in benchmark results."
        }

        $actual = $actualByKey[$key]
        $baseline = $baselineByKey[$key]
        Assert-Maximum "$name $key mean time" `
            (ConvertTo-BaselineMetric $actual.MeanMs) `
            ((ConvertTo-BaselineMetric $baseline.MeanMs) * $timeScale * $TimeRegressionRatio)
        Assert-Maximum "$name $key allocation" `
            (ConvertTo-BaselineMetric $actual.AllocatedKB) `
            ((ConvertTo-BaselineMetric $baseline.AllocatedKB) * $AllocationRegressionRatio)

        if ($AssertPayload -and $actual.Method -eq 'BuildAndSerialize') {
            Assert-Equal "$name $key payload bytes" $actual.PayloadBytes ([int] $baseline.PayloadBytes)
        }
    }

    Write-Host "PASS: $name contains all $($baselineByKey.Count) baseline cases."
}

function Export-Baseline([object[]] $rows, [string] $path) {
    $directory = Split-Path -Parent $path
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory | Out-Null
    }

    $rows | Export-Csv -LiteralPath $path -NoTypeInformation
    Write-Host "Updated baseline at '$path'."
}

$coreRows = @(Get-BenchmarkRows $CoreResultsPath 'Integer_workload')
$webRows = @(Get-BenchmarkRows $WebResultsPath 'WaitTurn_Delta')
$coreResults = @(Get-CoreBaselineRows $coreRows)
$webResults = @(Get-WebBaselineRows $webRows)

if ($UpdateBaselines) {
    Export-Baseline $coreResults $CoreBaselinePath
    Export-Baseline $webResults $WebBaselinePath
    return
}

foreach ($baselinePath in @($CoreBaselinePath, $WebBaselinePath)) {
    if (-not (Test-Path -LiteralPath $baselinePath)) {
        throw "Performance baseline was not found at '$baselinePath'."
    }
}

$coreBaseline = @(Import-Csv -LiteralPath $CoreBaselinePath)
$webBaseline = @(Import-Csv -LiteralPath $WebBaselinePath)
$coreKeyProperties = @('Method')
$webKeyProperties = @('Method', 'Scenario')
$calibrationResult = @($coreResults | Where-Object Method -eq 'Integer_workload')
$calibrationBaseline = @($coreBaseline | Where-Object Method -eq 'Integer_workload')
if ($calibrationResult.Count -ne 1 -or $calibrationBaseline.Count -ne 1) {
    throw 'Core results and baseline must each contain exactly one Integer_workload calibration case.'
}

$calibrationMean = ConvertTo-BaselineMetric $calibrationResult[0].MeanMs
$referenceCalibrationMean = ConvertTo-BaselineMetric $calibrationBaseline[0].MeanMs
$timeScale = [Math]::Max(1.0, $calibrationMean / $referenceCalibrationMean)
Write-Host "Calibration mean = $($calibrationMean.ToString('F3', $InvariantCulture)) ms; time threshold scale = $($timeScale.ToString('F3', $InvariantCulture))."

Assert-Baseline 'Core' $coreResults $coreBaseline $coreKeyProperties $timeScale
Assert-Baseline 'Web' $webResults $webBaseline $webKeyProperties $timeScale -AssertPayload

Write-Host 'All performance regression baselines passed.'