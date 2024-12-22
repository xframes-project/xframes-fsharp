module SampleApp

open System.Reactive.Subjects
open System.Reactive.Linq
open System
open Widgets
open Types
open Enums
open JsonAdapters
open Newtonsoft.Json

type AppState = {
    Text: string
    Count: int
}

let sampleAppState = new BehaviorSubject<AppState>({ Text = "Hello..."; Count = 1 })

let styleVars: (ImGuiStyleVar * StyleVarValue) list = [
    ImGuiStyleVar.WindowPadding, StyleVarValue.ImVec2(10.0, 10.0)
    ImGuiStyleVar.Alpha, StyleVarValue.Float(0.5)
    ImGuiStyleVar.WindowRounding, StyleVarValue.Float(4.0)
]

let styleVarsMap = makeStyleVars(styleVars)

let yogaStyleProperties: (YogaStylePropertyKey * YogaStyleProperty) list = [
    (YogaStylePropertyKey.Direction, YogaStyleProperty.Direction Ltr)
    (YogaStylePropertyKey.Flex, YogaStyleProperty.Flex 1.0)
    (YogaStylePropertyKey.Width, YogaStyleProperty.Width (Value 200.0))
    (YogaStylePropertyKey.Height, YogaStyleProperty.Height (Percentage "100%"))
]

let yogaStyleResult = createYogaStyleWithValidation(yogaStyleProperties)

match yogaStyleResult with
| Ok v -> printfn "%s" (JsonConvert.SerializeObject(StyleJsonAdapter.yogaStyleToJson(v)))

let baseDrawStyleProperties: (BaseDrawStylePropertyKey * BaseDrawStyleProperty) list = [
    (BaseDrawStylePropertyKey.BackgroundColor, BaseDrawStyleProperty.BackgroundColor (HexString "#fff"))
    (BaseDrawStylePropertyKey.Border, BaseDrawStyleProperty.Border { Color = HexString "#fff"; Thickness = Some 1.0 })
    (BaseDrawStylePropertyKey.BorderRight, BaseDrawStyleProperty.BorderRight { Color = HexString "#fff"; Thickness = None })
    (BaseDrawStylePropertyKey.Rounding, BaseDrawStyleProperty.Rounding 1.0)
    (BaseDrawStylePropertyKey.RoundCorners, BaseDrawStyleProperty.RoundCorners [RoundCorners.BottomLeft; RoundCorners.BottomRight])
]

let baseDrawStyleResult = createBaseDrawStyleWithValidation(baseDrawStyleProperties)

let s: StyleRules = {
    Align = Some(Align.Left)
    Font = Some( { 
        Name = "roboto-regular"
        Size = 16
    } )
    Colors = Some( Map.ofList [
        ImGuiCol.Text, HexString "#8899aa"
        ImGuiCol.TitleBg, HexaValue ("#fff", 0.5)
    ])
    Vars = Some ( Map.ofList [
        ImGuiStyleVar.FramePadding, ImVec2 (2.0, 2.0)
        ImGuiStyleVar.FrameRounding, Float 2.0
    ])
}

match yogaStyleResult with
| Ok v -> printfn "%s" (JsonConvert.SerializeObject(StyleJsonAdapter.yogaStyleToJson(v)))

match baseDrawStyleResult with
| Ok v -> printfn "%s" (JsonConvert.SerializeObject(StyleJsonAdapter.baseDrawStyleToJson(v)))

printfn "%s" (JsonConvert.SerializeObject(StyleJsonAdapter.styleRulesToJson(s)))

type App() =
    inherit BaseComponent()

    let onClick() =
        sampleAppState.OnNext({ Text = "Hello, world!"; Count = sampleAppState.Value.Count + 1 })

    override this.Init() =

        this.sub <- Some(sampleAppState.Subscribe(fun latestAppState -> 
            this.Props.OnNext(
                Map.ofList [
                    "text", latestAppState.Text :> obj
                    "count", latestAppState.Count :> obj
                ]
            )
        ))

    override this.Destroy() =
        match this.sub with
        | Some(subscription) -> subscription.Dispose()
        | None -> ()


    override this.Render() =
        let textNodes = 
            [ for _ in 1 .. sampleAppState.Value.Count -> 
                Renderable.WidgetNode(unformattedText sampleAppState.Value.Text)
            ]

        let children = 
            [ Renderable.WidgetNode(button("Add text", Some onClick)) ] @ textNodes

        WidgetNode (
            node children
        )

    member val sub: IDisposable option = None with get, set

type Root() =
    inherit BaseComponent()

    override this.Init() =
        ignore()

    override this.Destroy() =
        ignore()

    override this.Render() =
        let ret: BaseComponent = App()

        WidgetNode (
            makeRootNode [(BaseComponent ret)]
        )

    member val sub: IDisposable option = None with get, set
