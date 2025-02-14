﻿module JsonAdapters

open System.Collections.Generic
open System.Reactive.Subjects
open Types

let mergeOptionalMap (baseData: Map<string, obj>) (optionalMap: Map<string, obj> option): Map<string, obj> =
    match optionalMap with
    | Some map -> Map.fold (fun acc key value -> Map.add key value acc) baseData map
    | None -> baseData


module StyleJsonAdapter =
    let fromJson(json: Map<string, obj>) =
        ignore()

    let styleColValueToJson (styleColValue: StyleColValue) : obj =
        match styleColValue with
        | HexString s -> box s
        | HexaValue (hex, alpha) -> 
            box [| box hex; box alpha |]

    let styleVarValueToJson (styleVarValue: StyleVarValue) : obj =
        match styleVarValue with
        | Float f -> f
        | ImVec2 (x, y) -> box [| box x; box y |]

    let borderStyleToJson (borderStyle: BorderStyle) : Map<string, obj> =
        let baseData = Map.ofList [
            "color", styleColValueToJson(borderStyle.Color)
        ]

        match borderStyle.Thickness with
        | Some thickness -> Map.add "thickness" (box thickness) baseData
        | None -> baseData
            

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

    let baseDrawStylePropertyKeyToString (property: BaseDrawStylePropertyKey): string =
        match property with
        | BaseDrawStylePropertyKey.BackgroundColor -> "backgroundColor"
        | BaseDrawStylePropertyKey.Border -> "border"
        | BaseDrawStylePropertyKey.BorderTop -> "borderTop"
        | BaseDrawStylePropertyKey.BorderRight -> "borderRight"
        | BaseDrawStylePropertyKey.BorderBottom -> "borderBottom"
        | BaseDrawStylePropertyKey.BorderLeft -> "borderLeft"
        | BaseDrawStylePropertyKey.RoundCorners -> "roundCorners"
        | BaseDrawStylePropertyKey.Rounding -> "rounding"

    let baseDrawStylePropertyToJsonValue (property: BaseDrawStyleProperty): obj =
        match property with
        | BaseDrawStyleProperty.BackgroundColor b -> styleColValueToJson(b)
        | BaseDrawStyleProperty.Border b -> borderStyleToJson(b)
        | BaseDrawStyleProperty.BorderTop b -> borderStyleToJson(b)
        | BaseDrawStyleProperty.BorderRight b -> borderStyleToJson(b)
        | BaseDrawStyleProperty.BorderBottom b -> borderStyleToJson(b)
        | BaseDrawStyleProperty.BorderLeft b-> borderStyleToJson(b)
        | BaseDrawStyleProperty.RoundCorners r -> 
            r 
            |> List.map (fun item -> box (item.ToString())) 
            |> box
        | BaseDrawStyleProperty.Rounding r -> r

    let baseDrawStyleToJson (baseDrawStyle: BaseDrawStyle) : Map<string, obj> =
        baseDrawStyle
        |> Map.fold (fun acc key value ->
            let keyString = baseDrawStylePropertyKeyToString key
            acc.Add(keyString, baseDrawStylePropertyToJsonValue value)
        ) (Map.empty<string, obj>)

    let fontDefToJson (fontDef: FontDef) : Map<string, obj> =
        Map.ofList [
            "name", fontDef.Name
            "size", fontDef.Size
        ]

    let fontDefsToJson (fontDefs: Dictionary<string, FontDef list>) : Dictionary<string, Map<string, obj> list> =
        let fontsDictionary = new Dictionary<string, Map<string, obj> list>()
        fontsDictionary.Add("defs", fontDefs["defs"] |> List.map fontDefToJson)

        fontsDictionary

    let styleRulesToJson (styleRules: StyleRules) : Map<string, obj> =
        let mutable baseData: Map<string, obj> = Map.empty

        match styleRules.Align with
        | Some a -> baseData <- Map.add "align" (a.ToString()) baseData
        | None -> ignore()

        match styleRules.Font with
        | Some f -> baseData <- Map.add "font" (fontDefToJson(f)) baseData
        | None -> ignore()

        match styleRules.Colors with
        | Some m -> 
            let colorsJson = 
                m 
                |> Map.fold (fun acc key value -> Map.add (string (int key)) (styleColValueToJson value) acc) Map.empty
            baseData <- Map.add "colors" (box colorsJson) baseData
        | None -> ignore()

        match styleRules.Vars with
        | Some v -> 
            let varsJson = 
                v 
                |> Map.fold (fun acc key value -> Map.add (string (int key)) (styleVarValueToJson value) acc) Map.empty
            baseData <- Map.add "vars" (box varsJson) baseData
        | None -> ignore()

        baseData

    let nodeStyleToJson(nodeStyle: NodeStyle): Map<string, obj> =
        let baseData: Map<string, obj> = Map.empty

        let baseData =
            nodeStyle.BaseDrawStyle
            |> Option.map baseDrawStyleToJson
            |> mergeOptionalMap baseData

        let baseData =
            nodeStyle.YogaStyle
            |> Option.map yogaStyleToJson
            |> mergeOptionalMap baseData

        baseData

    let widgetStyleToJson(widgetStyle: WidgetStyle): Map<string, obj> =
        let baseData: Map<string, obj> = Map.empty

        let baseData =
            widgetStyle.StyleRules
            |> Option.map styleRulesToJson
            |> mergeOptionalMap baseData

        let baseData =
            widgetStyle.BaseDrawStyle
            |> Option.map baseDrawStyleToJson
            |> mergeOptionalMap baseData

        let baseData =
            widgetStyle.YogaStyle
            |> Option.map yogaStyleToJson
            |> mergeOptionalMap baseData

        baseData


//let addStyleIfPresent(styleKey: string, props: Map<string, obj>, widgetType: WidgetTypes): Map<string, obj> option =
//    match widgetType with
//    | WidgetTypes.Node ->
//        match props.TryFind(styleKey) with
//        | Some (style : obj) ->
//            match style :?> NodeStyle with
//            | style -> Some(StyleJsonAdapter.nodeStyleToJson(style))
//        | None -> None
//    | _ ->
//        match props.TryFind(styleKey) with
//        | Some (style : obj) ->
//            match style :?> WidgetStyle with
//            | style -> Some(StyleJsonAdapter.widgetStyleToJson(style))
//        | None -> None

let addStyleIfPresent (styleKey: string, props: Map<string, obj>, widgetType: WidgetTypes) : Map<string, obj> option =
    props.TryFind(styleKey)
    |> Option.bind (fun style ->
        match widgetType, style with
        | WidgetTypes.Node, (:? NodeStyle as nodeStyle) ->
            Some (StyleJsonAdapter.nodeStyleToJson nodeStyle)
        | _, (:? WidgetStyle as widgetStyle) ->
            Some (StyleJsonAdapter.widgetStyleToJson widgetStyle)
        | _ -> None
    )

module WidgetNodeJsonAdapter =
    let fromJson(json: Map<string, obj>) =
        ignore()

    let toJson(widgetNode: RawWidgetNodeWithId) : Map<string, obj> =
        let props = widgetNode.Props |> Map.map (fun _ v -> box v)

        let propsWithStyles =
            [ "style"; "hoverStyle"; "activeStyle"; "disabledStyle" ]
            |> List.fold (fun acc key ->
                match addStyleIfPresent(key, acc, widgetNode.Type) with
                | Some styleJson -> Map.add key (box styleJson) acc
                | None -> acc
            ) props

        propsWithStyles
        |> Map.add "id" (box widgetNode.Id)
        |> Map.add "type" (box (widgetNode.Type.ToString()))

    let childlessWidgetNodeToJson(widgetNode: RawChildlessWidgetNodeWithId) : Map<string, obj> =
        let props = widgetNode.Props |> Map.map (fun _ v -> box v)

        let propsWithStyles =
            [ "style"; "hoverStyle"; "activeStyle"; "disabledStyle" ]
            |> List.fold (fun acc key ->
                match addStyleIfPresent(key, acc, widgetNode.Type) with
                | Some styleJson -> Map.add key (box styleJson) acc
                | None -> acc
            ) props

        propsWithStyles
        |> Map.add "id" (box widgetNode.Id)
        |> Map.add "type" (box (widgetNode.Type.ToString()))