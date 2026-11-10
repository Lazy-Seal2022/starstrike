# Common

## Overview
Procedural sprite builders and TextMeshPro typography layout helpers.

## Interface
- `class UIHelper` [UIHelper.cs]
- `CreateTextMeshPro(Transform parent, string name, string text, int fontSize, FontStyles style, Color color, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax) -> TextMeshProUGUI` [UIHelper.cs]
- `CreateTextMeshProExplicit(Transform parent, string name, string text, int fontSize, FontStyles style, Color color, TextAlignmentOptions alignment, Vector2 anchor, Vector2 pos, Vector2 size) -> TextMeshProUGUI` [UIHelper.cs]
- `ConvertTextAnchor(TextAnchor anchor) -> TextAlignmentOptions` [UIHelper.cs]

## Invariants
- MonoBehaviour lifecycle contracts strictly preserved [UIHelper.cs]

## Side Effects
- In-memory state manipulation [UIHelper.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

