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
$handBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(246, 193, 77))
$handShadeBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(217, 145, 42))
$handOutlinePen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(18, 23, 25)), 3
$handOutlinePen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round

# Leave clear title bands above and below the illustration while preserving its proportions.
$artTransform = New-Object System.Drawing.Drawing2D.Matrix 0.68, 0, 0, 0.68, 40.96, 31
$graphics.Transform = $artTransform

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
    (New-Object System.Drawing.Point 194, 126)
)
$graphics.DrawCurve($pathPen, $pathPoints, 0.4)

foreach ($point in @($pathPoints[0], $pathPoints[2], $pathPoints[4], $pathPoints[6])) {
    $graphics.DrawEllipse($ringPen, $point.X - 8, $point.Y - 8, 16, 16)
}

# A downward pointing hand makes the final ping read as an intentional gesture.
# Its index fingertip lands directly on the end of the route.
$hand = New-Object System.Drawing.Drawing2D.GraphicsPath
$hand.StartFigure()
$hand.AddBezier(184, 83, 185, 78, 190, 76, 197, 76)
$hand.AddBezier(197, 76, 204, 76, 210, 79, 211, 84)
$hand.AddBezier(211, 84, 212, 91, 209, 98, 208, 104)
$hand.AddBezier(208, 104, 207, 111, 207, 118, 203, 122)
$hand.AddBezier(203, 122, 201, 125, 197, 124, 195, 120)
$hand.AddBezier(195, 120, 194, 118, 192, 117, 191, 119)
$hand.AddBezier(191, 119, 190, 121, 190, 126, 188, 129)
$hand.AddBezier(188, 129, 186, 133, 181, 131, 181, 127)
$hand.AddBezier(181, 127, 181, 122, 184, 116, 183, 108)
$hand.AddLine(183, 108, 181, 88)
$hand.AddBezier(181, 88, 181, 86, 182, 84, 184, 83)
$hand.CloseFigure()
$graphics.FillPath($handBrush, $hand)
$graphics.DrawPath($handOutlinePen, $hand)

# A restrained inner contour separates the curled fingers from the index finger.
$fingerFold = New-Object System.Drawing.Drawing2D.GraphicsPath
$fingerFold.StartFigure()
$fingerFold.AddBezier(195, 103, 198, 106, 201, 107, 207, 106)
$fingerFold.AddBezier(207, 106, 207, 112, 206, 118, 203, 122)
$fingerFold.AddBezier(203, 122, 201, 125, 197, 124, 195, 120)
$fingerFold.AddBezier(195, 120, 194, 116, 194, 109, 195, 103)
$fingerFold.CloseFigure()
$graphics.FillPath($handShadeBrush, $fingerFold)

$graphics.ResetTransform()

# Draw exact, outlined title text after resetting the illustration transform.
$fontFamily = New-Object System.Drawing.FontFamily 'Arial Black'
$textFormat = New-Object System.Drawing.StringFormat
$textFormat.Alignment = [System.Drawing.StringAlignment]::Center
$textFormat.LineAlignment = [System.Drawing.StringAlignment]::Center
$textOutlinePen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(18, 23, 25)), 3.5
$textOutlinePen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
$textBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(246, 193, 77))

$titlePath = New-Object System.Drawing.Drawing2D.GraphicsPath
$titleRect = New-Object System.Drawing.RectangleF 3, 0, 250, 52
$titlePath.AddString('ENHANCED', $fontFamily, [int][System.Drawing.FontStyle]::Regular, 32, $titleRect, $textFormat)
$graphics.DrawPath($textOutlinePen, $titlePath)
$graphics.FillPath($textBrush, $titlePath)

$pingPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$pingRect = New-Object System.Drawing.RectangleF 4, 202, 248, 51
$pingPath.AddString('PING', $fontFamily, [int][System.Drawing.FontStyle]::Regular, 35, $pingRect, $textFormat)
$graphics.DrawPath($textOutlinePen, $pingPath)
$graphics.FillPath($textBrush, $pingPath)


$directory = Split-Path -Parent $OutputPath
if ($directory -and -not (Test-Path -LiteralPath $directory)) {
    New-Item -ItemType Directory -Path $directory | Out-Null
}
$bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)

$pingPath.Dispose()
$titlePath.Dispose()
$textBrush.Dispose()
$textOutlinePen.Dispose()
$textFormat.Dispose()
$fontFamily.Dispose()
$artTransform.Dispose()
$fingerFold.Dispose()
$hand.Dispose()
$handOutlinePen.Dispose()
$handShadeBrush.Dispose()
$handBrush.Dispose()
$ringPen.Dispose()
$pathPen.Dispose()
$snowBrush.Dispose()
$mountainBrush.Dispose()
$graphics.Dispose()
$bitmap.Dispose()

Write-Output "Generated $OutputPath (256x256 PNG)."

