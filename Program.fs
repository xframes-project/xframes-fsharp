open System
open System.Threading
open System.Reactive.Subjects
open System.Threading.Tasks
open System.Collections.Generic
open System.Runtime.InteropServices
open Newtonsoft.Json
open Widgets
open Applier
open Externs
open Theme
open Types

// Create an instance of the delegate
let onTextChangedDelegate = OnTextChangedCb(fun id value -> printfn "Text changed: %d, %s" id value)

// Record types for data structures
type JsonSetData = { Op: string; Data: obj }
type JsonSetValue = { Op: string; Value: string }
type JsonSetSelectedIndex = { Op: string; Index: int }
type JsonResetData = { Op: string }
type JsonAppendData = { Op: string; Data: obj }
type JsonAppendDataToPlotLine = { Op: string; X: float; Y: float }
type JsonSetAxesDecimalDigits = { Op: string; X: float; Y: float }
type JsonSetAxesAutoFit = { Op: string; Enabled: bool }

type SetData = {
    op: string
    data: List<obj>
}


type WidgetNodeAdapter() =
    /// Converts a JSON-like Map<string, obj> to a WidgetNode
    member this.FromJson(json: Map<string, obj>) =
        let ``type`` = json.["type"] :?> string
        let id = json.["id"] :?> int
        let props =
            json 
            |> Map.remove "id" 
            |> Map.remove "type"

        // Initialize WidgetNode with reactive Props
        {
            Id = id
            Type = ``type``
            Props = new BehaviorSubject<Map<string, obj>>(props)
            Children = [] // Modify if children are part of the JSON
        }

    /// Converts a WidgetNode to a JSON-like Map<string, obj>
    member this.ToJson(widgetNode: WidgetNode) =
        // Extract Props' current value
        let props = widgetNode.Props.Value

        // Add `id` and `type` to Props for JSON representation
        props
        |> Map.add "id" (box widgetNode.Id)
        |> Map.add "type" (box widgetNode.Type)


type WidgetTreeApplier(jsonAdapter: WidgetNodeAdapter, root: WidgetNode) =
    inherit AbstractApplier<WidgetNode>(root)

    let serializeToJson (data: obj) = JsonConvert.SerializeObject(data)
    let deserializeList (json: string) =
        JsonConvert.DeserializeObject<List<int>>(json)

    override this.OnClear() =
        root.Children <- []

    override this.InsertBottomUp(index: int, instance: WidgetNode) =
        () // Logic can be implemented as needed

    override this.InsertTopDown(index: int, instance: WidgetNode) =
        this.Current.Children <- 
            this.Current.Children.[0..index - 1] @ [instance] @ this.Current.Children.[index..]
        
        let json = jsonAdapter.ToJson(instance)
        let jsonString = serializeToJson json
        setElement(jsonString)
        let childrenJson = serializeToJson [ instance.Id ]
        setChildren(this.Current.Id, childrenJson)

    override this.Move(fromIndex: int, toIndex: int, count: int) =
        let moveItems list fromIndex toIndex count =
            let itemsToMove = list |> List.skip fromIndex |> List.take count
            let remaining = 
                list 
                |> List.mapi (fun i x -> if i >= fromIndex && i < fromIndex + count then None else Some x)
                |> List.choose id
            let (before, after) = remaining |> List.splitAt toIndex
            before @ itemsToMove @ after
        this.Current.Children <- moveItems this.Current.Children fromIndex toIndex count

    override this.Remove(index: int, count: int) =
        this.Current.Children <- 
            this.Current.Children.[0..index - 1] @ this.Current.Children.[index + count..]



let fontDefs =
    let fontSizes = [16; 18; 20; 24; 28; 32; 36; 48]
    let fontName = "roboto-regular"
    fontSizes
    |> List.map (fun size -> { name = fontName; size = size })

let fontsDictionary = new Dictionary<string, FontDef list>()
fontsDictionary.Add("defs", fontDefs)


let rec keepProcessRunning () =
    async {
        let mutable flag = true

        while flag do
            // Simulate some periodic work here if needed
            // printfn "Process is running..."

            // Delay for 1000ms (1 second) without blocking the thread
            do! Task.Delay(1000) |> Async.AwaitTask
    }

//let App id type' props children =
//    {
//        Id = id
//        Type = type'
//        Props = props
//        Children = children
//    }

[<EntryPoint>]
let main argv =
    
    // Define individual widget nodes
    let buttonWidget = {
        Id = 1
        Type = "Button"
        Props = new BehaviorSubject<Map<string, obj>>(Map.ofList [("text", box "Click Me")])
        Children = []
    }

    let labelWidget = {
        Id = 2
        Type = "Label"
        Props = new BehaviorSubject<Map<string, obj>>(Map.ofList [("text", box "Hello World")])
        Children = []
    }

    // Create a node widget with children
    let nodeWidget = 
        createWidgetNode 
            3 
            "Node" 
            (Map.ofList [("style", box "vertical")]) 
            [buttonWidget; labelWidget]


    //printfn "%s" fontDefsJson

    // Example call to init (using some mock values)
    let assetsPath = "./assets"

    // Function that will contain the logic for initializing nodes
    let onInitLogic () =
        // Root node definition
        let rootNode = 
            let dict = Dictionary<string, obj>()
            dict.Add("id", 0 :> obj)
            dict.Add("type", "node" :> obj)
            dict.Add("root", true :> obj)
            dict

        // Text node definition
        let textNode = 
            let dict = Dictionary<string, obj>()
            dict.Add("id", 1 :> obj)
            dict.Add("type", "unformatted-text" :> obj)
            dict.Add("text", "Hello, world!" :> obj)
            dict

        // Serialize and set the elements
        setElement (JsonConvert.SerializeObject(rootNode))
        setElement (JsonConvert.SerializeObject(textNode))

        // Serialize and set the children
        setChildren (0, JsonConvert.SerializeObject([1]))
    

    let onInit = Marshal.GetFunctionPointerForDelegate(Action(fun () -> onInitLogic()))
    
    let onTextChangedPtr = Marshal.GetFunctionPointerForDelegate(onTextChangedDelegate)

    let onComboChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onNumericValueChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onBooleanValueChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onMultipleNumericValuesChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onClick = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))

    let fontDefsJson = JsonConvert.SerializeObject(fontsDictionary)
    let themeJson = JsonConvert.SerializeObject(colorsDict)


    init(assetsPath, fontDefsJson, themeJson, onInit, onTextChangedPtr, onComboChanged, onNumericValueChanged, onBooleanValueChanged, onMultipleNumericValuesChanged, onClick)

    let appProcess = keepProcessRunning ()
    Async.Start appProcess
    
    // Prevent the app from terminating by waiting indefinitely
    Console.ReadLine() |> ignore
    0 // return an integer exit code
