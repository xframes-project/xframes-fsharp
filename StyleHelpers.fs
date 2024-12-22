module StyleHelpers

open Enums
open Types

let styleVarsImVec2 : ImGuiStyleVar list = [
    ImGuiStyleVar.WindowPadding
    ImGuiStyleVar.WindowMinSize
    ImGuiStyleVar.WindowTitleAlign
    ImGuiStyleVar.ItemSpacing
    ImGuiStyleVar.ItemInnerSpacing
    ImGuiStyleVar.CellPadding
    ImGuiStyleVar.TableAngledHeadersTextAlign
    ImGuiStyleVar.ButtonTextAlign
    ImGuiStyleVar.SelectableTextAlign
    ImGuiStyleVar.SeparatorTextAlign
    ImGuiStyleVar.SeparatorTextPadding
]

let styleVarsFloat : ImGuiStyleVar list = [
    ImGuiStyleVar.Alpha
    ImGuiStyleVar.WindowRounding
    ImGuiStyleVar.WindowBorderSize
    ImGuiStyleVar.ChildRounding
    ImGuiStyleVar.ChildBorderSize
    ImGuiStyleVar.PopupRounding
    ImGuiStyleVar.PopupBorderSize
    ImGuiStyleVar.FrameRounding
    ImGuiStyleVar.FrameBorderSize
    ImGuiStyleVar.IndentSpacing
    ImGuiStyleVar.ScrollbarSize
    ImGuiStyleVar.ScrollbarRounding
    ImGuiStyleVar.GrabMinSize
    ImGuiStyleVar.GrabRounding
    ImGuiStyleVar.TabRounding
    ImGuiStyleVar.TabBorderSize
    ImGuiStyleVar.TabBarBorderSize
    ImGuiStyleVar.TableAngledHeadersAngle
    ImGuiStyleVar.SeparatorTextBorderSize
]


// The function that checks if the variable is in styleVarsImVec2 or styleVarsFloat
let makeStyleVars (styleVars: (ImGuiStyleVar * StyleVarValue) list) : Map<ImGuiStyleVar, StyleVarValue> =
    styleVars
    |> List.fold (fun acc (styleVar, value) ->
        match styleVar with
        | _ when List.contains styleVar styleVarsImVec2 ->
            match value with
            | ImVec2 _ -> Map.add styleVar value acc  // Correct type, add to map
            | _ -> failwithf "Expected ImVec2 for %A, but got %A" styleVar value  // Type mismatch error
        | _ when List.contains styleVar styleVarsFloat ->
            match value with
            | Float _ -> Map.add styleVar value acc  // Correct type, add to map
            | _ -> failwithf "Expected Float for %A, but got %A" styleVar value  // Type mismatch error
        | _ -> acc  // No match, just return the accumulator
    ) Map.empty

let makeStyleRules(maybeAlign: Align option, maybeFontDef: FontDef option, maybeColors: Map<ImGuiCol, StyleColValue> option, maybeStyleVars: Map<ImGuiStyleVar, StyleVarValue> option) =
    {
        Align = maybeAlign
        Font = maybeFontDef
        Colors = maybeColors
        Vars = maybeStyleVars
    }


let validateYogaPropertyValue (key: YogaStylePropertyKey, value: YogaStyleProperty) : Result<unit, string> =
    match key, value with
    | YogaStylePropertyKey.Direction, YogaStyleProperty.Direction _v -> Ok()
    | YogaStylePropertyKey.FlexDirection, YogaStyleProperty.FlexDirection _v -> Ok()
    | YogaStylePropertyKey.JustifyContent, YogaStyleProperty.JustifyContent _v -> Ok()
    | YogaStylePropertyKey.AlignContent, YogaStyleProperty.AlignContent _v -> Ok()
    | YogaStylePropertyKey.AlignItems, YogaStyleProperty.AlignItems _v -> Ok()
    | YogaStylePropertyKey.AlignSelf, YogaStyleProperty.AlignSelf _v -> Ok()
    | YogaStylePropertyKey.PositionType, YogaStyleProperty.PositionType _v -> Ok()
    | YogaStylePropertyKey.FlexWrap, YogaStyleProperty.FlexWrap _v -> Ok()
    | YogaStylePropertyKey.Overflow, YogaStyleProperty.Overflow _v -> Ok()
    | YogaStylePropertyKey.Display, YogaStyleProperty.Display _v -> Ok()
    | YogaStylePropertyKey.Flex, YogaStyleProperty.Flex f when f >= 0.0 -> Ok()
    | YogaStylePropertyKey.FlexGrow, YogaStyleProperty.FlexGrow f when f >= 0.0 -> Ok()
    | YogaStylePropertyKey.FlexShrink, YogaStyleProperty.FlexShrink f when f >= 0.0 -> Ok()
    | YogaStylePropertyKey.FlexBasis, YogaStyleProperty.FlexBasis f when f >= 0.0 -> Ok()
    | YogaStylePropertyKey.FlexBasisPercent, YogaStyleProperty.FlexBasisPercent f when f >= 0.0 -> Ok()
    | YogaStylePropertyKey.Position, YogaStyleProperty.Position _v -> Ok()
    | YogaStylePropertyKey.Margin, YogaStyleProperty.Margin _v -> Ok()
    | YogaStylePropertyKey.Padding, YogaStyleProperty.Padding _v -> Ok()
    | YogaStylePropertyKey.Gap, YogaStyleProperty.Gap _v -> Ok()
    | YogaStylePropertyKey.AspectRatio, YogaStyleProperty.AspectRatio f when f >= 0.0 -> Ok()
    | YogaStylePropertyKey.Width, YogaStyleProperty.Width (Value _) -> Ok()
    | YogaStylePropertyKey.MinWidth, YogaStyleProperty.MinWidth (Value _) -> Ok()
    | YogaStylePropertyKey.MaxWidth, YogaStyleProperty.MaxWidth (Value _) -> Ok()
    | YogaStylePropertyKey.Height, YogaStyleProperty.Height (Value _) -> Ok()
    | YogaStylePropertyKey.MinHeight, YogaStyleProperty.MinHeight (Value _) -> Ok()
    | YogaStylePropertyKey.MaxHeight, YogaStyleProperty.MaxHeight (Value _) -> Ok()
    | YogaStylePropertyKey.Width, YogaStyleProperty.Width (Percentage _) -> Ok()
    | YogaStylePropertyKey.MinWidth, YogaStyleProperty.MinWidth (Percentage _) -> Ok()
    | YogaStylePropertyKey.MaxWidth, YogaStyleProperty.MaxWidth (Percentage _) -> Ok()
    | YogaStylePropertyKey.Height, YogaStyleProperty.Height (Percentage _) -> Ok()
    | YogaStylePropertyKey.MinHeight, YogaStyleProperty.MinHeight (Percentage _) -> Ok()
    | YogaStylePropertyKey.MaxHeight, YogaStyleProperty.MaxHeight (Percentage _) -> Ok()
    | _ -> Error "Invalid property value"

let createYogaStyleWithValidation (properties: (YogaStylePropertyKey * YogaStyleProperty) list) : Result<YogaStyle, string> =
    properties
    |> List.fold (fun acc (key, value) ->
        match validateYogaPropertyValue(key, value) with
        | Ok () -> Map.add key value acc
        | Error msg -> acc // Optionally log or throw error here
    ) Map.empty
    |> fun map ->
        if map.Count = properties.Length then
            Ok map
        else
            Error "One or more property values are invalid"

let validateBaseDrawPropertyValue (key: BaseDrawStylePropertyKey, value: BaseDrawStyleProperty) : Result<unit, string> =
    match key, value with
    | BaseDrawStylePropertyKey.BackgroundColor, BaseDrawStyleProperty.BackgroundColor _v -> Ok()
    | BaseDrawStylePropertyKey.Border, BaseDrawStyleProperty.Border _v -> Ok()
    | BaseDrawStylePropertyKey.BorderTop, BaseDrawStyleProperty.BorderTop _v -> Ok()
    | BaseDrawStylePropertyKey.BorderRight, BaseDrawStyleProperty.BorderRight _v -> Ok()
    | BaseDrawStylePropertyKey.BorderBottom, BaseDrawStyleProperty.BorderBottom _v -> Ok()
    | BaseDrawStylePropertyKey.BorderLeft, BaseDrawStyleProperty.BorderLeft _v -> Ok()
    | BaseDrawStylePropertyKey.Rounding, BaseDrawStyleProperty.Rounding f when f >= 0.0 -> Ok()
    | BaseDrawStylePropertyKey.RoundCorners, BaseDrawStyleProperty.RoundCorners _v -> Ok()
    | _ -> Error "Invalid property value"

let createBaseDrawStyleWithValidation (properties: (BaseDrawStylePropertyKey * BaseDrawStyleProperty) list) : Result<BaseDrawStyle, string> =
    properties
    |> List.fold (fun acc (key, value) ->
        match validateBaseDrawPropertyValue(key, value) with
        | Ok () -> Map.add key value acc
        | Error msg -> acc // Optionally log or throw error here
    ) Map.empty
    |> fun map ->
        if map.Count = properties.Length then
            Ok map
        else
            Error "One or more property values are invalid"

let makeNodeStyle(maybeYogaStyle: Result<YogaStyle, string>, maybeBaseDrawStyle: Result<BaseDrawStyle, string>): NodeStyle =
    {
        YogaStyle =
            match maybeYogaStyle with
            | Ok yogaStyle -> Some yogaStyle
            | Error _ -> None
        BaseDrawStyle =
            match maybeBaseDrawStyle with
            | Ok baseDrawStyle -> Some baseDrawStyle
            | Error _ -> None
    }
