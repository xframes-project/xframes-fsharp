﻿module Types

open System.Collections.Generic
open System.Reactive.Subjects
open System
open Enums

type WidgetTypes =
    | Component
    | Node
    | UnformattedText
    | Button
    override this.ToString() =
        match this with
        | Component -> "component"
        | Node -> "node"
        | UnformattedText -> "unformatted-text"
        | Button -> "di-button"

type RawWidgetNode = {
    Type: WidgetTypes
    Props: Map<string, obj>
    mutable Children: RawWidgetNode list
}

type RawWidgetNodeWithId = {
    Id: int
    Type: WidgetTypes
    Props: Map<string, obj>
    mutable Children: RawWidgetNodeWithId list
}

type RawChildlessWidgetNodeWithId = {
    Id: int
    Type: WidgetTypes
    Props: Map<string, obj>
}

[<AbstractClass>]
type BaseComponent() =
    member val Props = new BehaviorSubject<Map<string, obj>>(Map.empty) with get, set

    abstract member Init: unit -> unit
    abstract member Destroy: unit -> unit
    abstract member Render: unit -> Renderable

and Renderable =
    | BaseComponent of BaseComponent
    | WidgetNode of WidgetNode

and WidgetNode = 
    { 
        Type: WidgetTypes
        Props: BehaviorSubject<Map<string, obj>>
        Children: BehaviorSubject<Renderable list> 
    }

type Theme2(colorsDict: Dictionary<int, List<obj>>) =
    member val Colors = colorsDict with get, set


type FontDef = {
    Name: string
    Size: int
}

type HEXA = string * float

type StyleColValue =
    | HexString of string
    | HexaValue of HEXA

type StyleVarValue =
    | Float of float
    | ImVec2 of float * float

type Align = 
    | Left
    | Right
    override this.ToString() =
        match this with
        | Left -> "left"
        | Right -> "right"

type StyleRules = {
    Align: Align option
    Font: FontDef option
    Colors: Map<ImGuiCol, StyleColValue> option
    Vars: Map<ImGuiStyleVar, StyleVarValue> option
}

type Direction = 
    | Inherit
    | Ltr
    | Rtl
    member this.ToString() =
        match this with
        | Inherit -> "inherit"
        | Ltr -> "ltr"
        | Rtl -> "rtl"

type FlexDirection = 
    | Column
    | ColumnReverse
    | Row
    | RowReverse
    override this.ToString() =
        match this with
        | Column -> "column"
        | ColumnReverse -> "column-reverse"
        | Row -> "row"
        | RowReverse -> "row-reverse"

type JustifyContent =
    | FlexStart
    | Center
    | FlexEnd
    | SpaceBetween
    | SpaceAround
    | SpaceEvenly
    override this.ToString() =
        match this with
        | FlexStart -> "flex-start"
        | Center -> "center"
        | FlexEnd -> "flex-end"
        | SpaceBetween -> "space-between"
        | SpaceAround -> "space-around"
        | SpaceEvenly -> "space-evenly"

type AlignContent =
    | Auto
    | FlexStart
    | Center
    | FlexEnd
    | Stretch
    | SpaceBetween
    | SpaceAround
    | SpaceEvenly
    override this.ToString() =
        match this with
        | Auto -> "auto"
        | FlexStart -> "flex-start"
        | Center -> "center"
        | FlexEnd -> "flex-end"
        | Stretch -> "stretch"
        | SpaceBetween -> "space-between"
        | SpaceAround -> "space-around"
        | SpaceEvenly -> "space-evenly"

type AlignItems =
    | Auto
    | FlexStart
    | Center
    | FlexEnd
    | Stretch
    | Baseline
    override this.ToString() =
        match this with
        | Auto -> "auto"
        | FlexStart -> "flex-start"
        | Center -> "center"
        | FlexEnd -> "flex-end"
        | Stretch -> "stretch"
        | Baseline -> "baseline"

type AlignSelf =
    | Auto
    | FlexStart
    | Center
    | FlexEnd
    | Stretch
    | Baseline
    override this.ToString() =
        match this with
        | Auto -> "auto"
        | FlexStart -> "flex-start"
        | Center -> "center"
        | FlexEnd-> "flex-end"
        | Stretch -> "stretch"
        | Baseline -> "baseline"

type PositionType =
    | Static
    | Relative
    | Absolute
    override this.ToString() =
        match this with
        | Static -> "static"
        | Relative -> "relative"
        | Absolute -> "absolute"

type FlexWrap =
    | NoWrap
    | Wrap
    | WrapReverse
    override this.ToString() =
        match this with
        | NoWrap -> "no-wrap"
        | Wrap -> "wrap"
        | WrapReverse -> "wrap-reverse"

type Overflow =
    | Visible
    | Hidden
    | Scroll
    override this.ToString() =
        match this with
        | Visible -> "visible"
        | Hidden -> "hidden"
        | Scroll -> "scroll"

type Display =
    | Flex
    | DisplayNone
    override this.ToString() =
        match this with
        | Flex -> "flex"
        | DisplayNone -> "none"

type Edge =
    | Left
    | Top
    | Right
    | Bottom
    | Start
    | End
    | Horizontal
    | Vertical
    | All
    override this.ToString() =
        match this with
        | Left -> "left"
        | Top -> "top"
        | Right -> "right"
        | Bottom -> "bottom"
        | Start -> "start"
        | End -> "end"
        | Horizontal -> "horizontal"
        | Vertical -> "vertical"
        | All -> "all"

// Representing the Gutter type as a discriminated union
type Gutter =
    | Column
    | Row
    | All
    override this.ToString() =
        match this with
        | Column -> "column"
        | Row -> "row"
        | All -> "all"

type DimensionValue =
    | Percentage of string  // e.g., "100%"
    | Value of float        // e.g., 200.0
    
type YogaStyleProperty =
    | Direction of Direction
    | FlexDirection of FlexDirection
    | JustifyContent of JustifyContent
    | AlignContent of AlignContent
    | AlignItems of AlignItems
    | AlignSelf of AlignSelf
    | PositionType of PositionType
    | FlexWrap of FlexWrap
    | Overflow of Overflow
    | Display of Display
    | Flex of float
    | FlexGrow of float
    | FlexShrink of float
    | FlexBasis of float
    | FlexBasisPercent of float
    | Position of Map<Edge, float>
    | Margin of Map<Edge, float>
    | Padding of Map<Edge, float>
    | Gap of Map<Gutter, float>
    | AspectRatio of float
    | Width of DimensionValue
    | MinWidth of DimensionValue
    | MaxWidth of DimensionValue
    | Height of DimensionValue
    | MinHeight of DimensionValue
    | MaxHeight of DimensionValue

type YogaStylePropertyKey =
    | Direction
    | FlexDirection
    | JustifyContent
    | AlignContent
    | AlignItems
    | AlignSelf
    | PositionType
    | FlexWrap
    | Overflow
    | Display
    | Flex
    | FlexGrow
    | FlexShrink
    | FlexBasis
    | FlexBasisPercent
    | Position
    | Margin
    | Padding
    | Gap
    | AspectRatio
    | Width
    | MinWidth
    | MaxWidth
    | Height
    | MinHeight
    | MaxHeight

type YogaStyle = Map<YogaStylePropertyKey, YogaStyleProperty>

type RoundCorners =
    | All
    | TopLeft
    | TopRight
    | BottomLeft
    | BottomRight
    override this.ToString() =
        match this with
        | All -> "all"
        | TopLeft -> "topLeft"
        | TopRight -> "topRight"
        | BottomLeft -> "bottomLeft"
        | BottomRight -> "bottomRight"

type BorderStyle = {
    Color: StyleColValue
    Thickness: float option
}

type BaseDrawStyleProperty =
    | BackgroundColor of StyleColValue
    | Border of BorderStyle
    | BorderTop of BorderStyle
    | BorderRight of BorderStyle
    | BorderBottom of BorderStyle
    | BorderLeft of BorderStyle
    | Rounding of float
    | RoundCorners of RoundCorners list

type BaseDrawStylePropertyKey =
    | BackgroundColor
    | Border
    | BorderTop
    | BorderRight
    | BorderBottom
    | BorderLeft
    | Rounding
    | RoundCorners

type BaseDrawStyle = Map<BaseDrawStylePropertyKey, BaseDrawStyleProperty>

type NodeStyle = 
    {
        YogaStyle: YogaStyle option
        BaseDrawStyle: BaseDrawStyle option
    }

type WidgetStyle = 
    {
        StyleRules: StyleRules option
        YogaStyle: YogaStyle option
        BaseDrawStyle: BaseDrawStyle option
    }

