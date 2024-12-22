module JsonAdapters

open System.Reactive.Subjects
open Types

module WidgetNodeJsonAdapter =
    let fromJson(json: Map<string, obj>) =
        ignore()

    let toJson(widgetNode: RawWidgetNodeWithId) : Map<string, obj> =
        let props = widgetNode.Props |> Map.map (fun _ v -> box v)
        props
        |> Map.add "id" (box widgetNode.Id)
        |> Map.add "type" (box (widgetNode.Type.ToString()))

    let childlessWidgetNodeToJson(widgetNode: RawChildlessWidgetNodeWithId) : Map<string, obj> =
        let props = widgetNode.Props |> Map.map (fun _ v -> box v)
        props
        |> Map.add "id" (box widgetNode.Id)
        |> Map.add "type" (box (widgetNode.Type.ToString()))


module StyleJsonAdapter =
    let fromJson(json: Map<string, obj>) =
        ignore()

    let yogaStylePropertyKeyToString (property: YogaStylePropertyKey): string =
        match property with
        | YogaStylePropertyKey.Direction -> "direction"
        | YogaStylePropertyKey.FlexDirection -> "flexDirection"
        | YogaStylePropertyKey.JustifyContent -> "justifyContent"
        | YogaStylePropertyKey.AlignContent -> "alignContent"
        | YogaStylePropertyKey.AlignItems -> "alignItems"
        | YogaStylePropertyKey.AlignSelf -> "alignSelf"
        | YogaStylePropertyKey.PositionType -> "positionType"
        | YogaStylePropertyKey.FlexWrap -> "flexWrap"
        | YogaStylePropertyKey.Overflow -> "overflow"
        | YogaStylePropertyKey.Display -> "display"
        | YogaStylePropertyKey.Flex -> "flex"
        | YogaStylePropertyKey.FlexGrow -> "flexGrow"
        | YogaStylePropertyKey.FlexShrink -> "flexShrink"
        | YogaStylePropertyKey.FlexBasis -> "flexBasis"
        | YogaStylePropertyKey.FlexBasisPercent -> "flexBasisPercent"
        | YogaStylePropertyKey.AspectRatio -> "aspectRatio"
        | YogaStylePropertyKey.Width -> "width"
        | YogaStylePropertyKey.Height -> "height"
        | YogaStylePropertyKey.MinWidth -> "minWidth"
        | YogaStylePropertyKey.MinHeight -> "minHeight"
        | YogaStylePropertyKey.MaxWidth -> "maxWidth"
        | YogaStylePropertyKey.MaxHeight -> "maxHeight"
        | YogaStylePropertyKey.Position -> "position"
        | YogaStylePropertyKey.Margin -> "margin"
        | YogaStylePropertyKey.Padding -> "padding"
        | YogaStylePropertyKey.Gap -> "gap"

    let yogaStylePropertyToJsonValue (property: YogaStyleProperty): obj =
        match property with
        | YogaStyleProperty.Direction d -> d.ToString()
        | YogaStyleProperty.FlexDirection f -> f.ToString()
        | YogaStyleProperty.JustifyContent j -> j.ToString()
        | YogaStyleProperty.AlignContent a -> a.ToString()
        | YogaStyleProperty.AlignItems a -> a.ToString()
        | YogaStyleProperty.AlignSelf a -> a.ToString()
        | YogaStyleProperty.PositionType p -> p.ToString()
        | YogaStyleProperty.FlexWrap f -> f.ToString()
        | YogaStyleProperty.Overflow o -> o.ToString()
        | YogaStyleProperty.Display d -> d.ToString()
        | YogaStyleProperty.Flex f -> f
        | YogaStyleProperty.FlexGrow f -> f
        | YogaStyleProperty.FlexShrink f -> f
        | YogaStyleProperty.FlexBasis f -> f
        | YogaStyleProperty.FlexBasisPercent f -> f
        | YogaStyleProperty.AspectRatio f -> f
        | YogaStyleProperty.Width (Value v) -> v
        | YogaStyleProperty.Width (Percentage p) -> p
        | YogaStyleProperty.Height (Value v) -> v
        | YogaStyleProperty.Height (Percentage p) -> p
        | YogaStyleProperty.MinWidth (Value v) -> v
        | YogaStyleProperty.MinWidth (Percentage p) -> p
        | YogaStyleProperty.MinHeight (Value v) -> v
        | YogaStyleProperty.MinHeight (Percentage p) -> p
        | YogaStyleProperty.MaxHeight (Value v) -> v
        | YogaStyleProperty.MaxHeight (Percentage p) -> p
        | YogaStyleProperty.MaxWidth (Value v) -> v
        | YogaStyleProperty.MaxWidth (Percentage p) -> p
        | YogaStyleProperty.Position p -> p
        | YogaStyleProperty.Margin p -> p
        | YogaStyleProperty.Padding p -> p
        | YogaStyleProperty.Gap p -> p

    let yogaStyleToJson (yogaStyle: Map<YogaStylePropertyKey, YogaStyleProperty>) : Map<string, obj> =
        yogaStyle
        |> Map.fold (fun acc key value ->
            let keyString = yogaStylePropertyKeyToString key
            acc.Add(keyString, yogaStylePropertyToJsonValue value)
        ) (Map.empty<string, obj>)