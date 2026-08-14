# Changelog

## 1.0.1 - 2026-08-14

- Updated the required BepInExPack_PEAK dependency to 5.4.75301.

## 1.0.0 - 2026-08-11

- Added distance labels to visible pings from every scout.
- Adapted the implementation to PEAK 2.0.a's native ping eligibility and send helpers.
- Added stable dead-scout ping input without instruction-index transpilers.
- Routed ghost pings exclusively through the currently observed living scout so vanilla peers use that scout's identity, color, and visibility origin.
- Suppressed ghost Ping input when no valid living observed scout is available instead of falling back to the dead scout.
- Added held-Ping path drawing with release-time single/path classification and a local preview.
- Added view-angle sampling and evenly spaced spherical path selection.
- Added vanilla-compatible sequential playback with a configurable duration cap.
- Shipped the production code as one `plugins/EnhancedPing.dll` without nested plugin folders.
