module SampleApp

open System.Reactive.Subjects
open System
open Widgets
open Types
open Enums
open JsonAdapters
open Newtonsoft.Json
open StyleHelpers

type TodoItem = {
    Text: string
    Done: bool
}

type AppState = {
    TodoText: string
    TodoItems: TodoItem list
}

let sampleAppState = new BehaviorSubject<AppState>({ TodoText = ""; TodoItems = [] })

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
    (YogaStylePropertyKey.Margin, YogaStyleProperty.Margin (Map.ofList [
        Edge.Right, 5.0
        Edge.Bottom, 3.0
    ]))
    (YogaStylePropertyKey.Gap, YogaStyleProperty.Gap (Map.ofList [
        Gutter.Row, 5.0
    ]))
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

let s: StyleRules = makeStyleRules(
    Some(Align.Left),
    Some({ 
        Name = "roboto-regular"
        Size = 16
    }),
    Some(Map.ofList [
        ImGuiCol.Text, HexString "#8899aa"
        ImGuiCol.TitleBg, HexaValue ("#fff", 0.5)
    ]),
    Some(Map.ofList [
        ImGuiStyleVar.FramePadding, ImVec2 (2.0, 2.0)
        ImGuiStyleVar.FrameRounding, Float 2.0
    ])
)

printfn "%s" (JsonConvert.SerializeObject(StyleJsonAdapter.styleRulesToJson(s)))

type App() =
    inherit BaseComponent()

    let todoStyle: WidgetStyle = widgetNodeStyle(
        Some(makeStyleRules(
            None,
            Some({ 
                Name = "roboto-regular"
                Size = 32
            }),
            None,
            None
        )),
        None,
        None
    )

    let buttonStyle: WidgetStyle = widgetNodeStyle(
        Some(makeStyleRules(
            None,
            Some({ 
                Name = "roboto-regular"
                Size = 32
            }),
            None,
            None
        )),
        None,
        None
    )

    let onClick() =
        let newTodoItem = { Text = "New Todo"; Done = false }

        // Update the AppState with the new TodoItem appended
        sampleAppState.OnNext({
            TodoText = sampleAppState.Value.TodoText
            TodoItems = List.append sampleAppState.Value.TodoItems [newTodoItem]
        })

    override this.Init() =

        this.sub <- Some(sampleAppState.Subscribe(fun latestAppState -> 
            this.Props.OnNext(
                Map.ofList [
                    "TodoText", latestAppState.TodoText :> obj
                    "TodoItems", latestAppState.TodoItems :> obj
                ]
            )
        ))

    override this.Destroy() =
        match this.sub with
        | Some(subscription) -> subscription.Dispose()
        | None -> ()


    override this.Render() =
        let textNodes =
            match this.Props.Value.TryFind("TodoItems") with
            | Some (value : obj) ->
                match value :?> TodoItem list with
                | todoItems ->
                    [
                        for todoItem in todoItems do
                            Renderable.WidgetNode(
                                unformattedText((sprintf "%s (%s)." todoItem.Text (if todoItem.Done then "done" else "to do")), Some(todoStyle))
                            )
                    ]
            | None -> []

        let children = 
            [ Renderable.WidgetNode(button("Add todo", Some onClick, Some(buttonStyle)))] @ textNodes

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
