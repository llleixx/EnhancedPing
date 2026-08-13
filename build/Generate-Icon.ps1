param(
    [string]$OutputPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'icon.png')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$bitmap = New-Object System.Drawing.Bitmap 256, 256
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::FromArgb(18, 23, 25))

$mountainBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(59, 77, 73))
$snowBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(225, 235, 230))
$pathPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(81, 220, 184)), 7
$pathPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$pathPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$ringPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(246, 193, 77)), 5

$mountain = [System.Drawing.Point[]]@(
    (New-Object System.Drawing.Point 16, 220),
    (New-Object System.Drawing.Point 86, 92),
    (New-Object System.Drawing.Point 116, 139),
    (New-Object System.Drawing.Point 157, 48),
    (New-Object System.Drawing.Point 240, 220)
)
$graphics.FillPolygon($mountainBrush, $mountain)

$snow = [System.Drawing.Point[]]@(
    (New-Object System.Drawing.Point 128, 112),
    (New-Object System.Drawing.Point 157, 48),
    (New-Object System.Drawing.Point 188, 101),
    (New-Object System.Drawing.Point 174, 94),
    (New-Object System.Drawing.Point 162, 112),
    (New-Object System.Drawing.Point 149, 96)
)
$graphics.FillPolygon($snowBrush, $snow)

$pathPoints = [System.Drawing.Point[]]@(
    (New-Object System.Drawing.Point 39, 207),
    (New-Object System.Drawing.Point 62, 185),
    (New-Object System.Drawing.Point 85, 192),
    (New-Object System.Drawing.Point 105, 164),
    (New-Object System.Drawing.Point 128, 171),
    (New-Object System.Drawing.Point 147, 139),
    (New-Object System.Drawing.Point 174, 145),
    (New-Object System.Drawing.Point 198, 116)
)
$graphics.DrawCurve($pathPen, $pathPoints, 0.4)

foreach ($point in @($pathPoints[0], $pathPoints[2], $pathPoints[4], $pathPoints[6], $pathPoints[7])) {
    $graphics.DrawEllipse($ringPen, $point.X - 8, $point.Y - 8, 16, 16)
}
$graphics.DrawEllipse($ringPen, 181, 83, 34, 34)
$graphics.DrawLine($ringPen, 198, 117, 198, 132)

$directory = Split-Path -Parent $OutputPath
if ($directory -and -not (Test-Path -LiteralPath $directory)) {
    New-Item -ItemType Directory -Path $directory | Out-Null
}
$bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)

$ringPen.Dispose()
$pathPen.Dispose()
$snowBrush.Dispose()
$mountainBrush.Dispose()
$graphics.Dispose()
$bitmap.Dispose()

Write-Output "Generated $OutputPath (256x256 PNG)."

