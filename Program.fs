open System
open System.Threading
open System.Reactive.Subjects
open System.Threading.Tasks
open System.Collections.Generic
open System.Runtime.InteropServices
open Newtonsoft.Json
open DEdge.Diffract
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
            Children = new BehaviorSubject<WidgetNode list>([]) // Modify if children are part of the JSON
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

    // Helper functions for serialization and deserialization
    let serializeToJson (data: obj) = JsonConvert.SerializeObject(data)
    let deserializeList (json: string) =
        JsonConvert.DeserializeObject<List<int>>(json)

    // Reactive helper: Update `Children` safely
    let updateChildren (node: WidgetNode) (updateFn: WidgetNode list -> WidgetNode list) =
        let currentChildren = node.Children.Value
        node.Children.OnNext(updateFn currentChildren)

    // Clear all children of the root
    override this.OnClear() =
        root.Children.OnNext([])

    // Insert a node bottom-up (logic can be customized)
    override this.InsertBottomUp(index: int, instance: WidgetNode) =
        updateChildren this.Current (fun children ->
            children.[0..index - 1] @ [instance] @ children.[index..]
        )

    // Insert a node top-down
    override this.InsertTopDown(index: int, instance: WidgetNode) =
        updateChildren this.Current (fun children ->
            children.[0..index - 1] @ [instance] @ children.[index..]
        )
        
        let json = jsonAdapter.ToJson(instance)
        let jsonString = serializeToJson json
        setElement(jsonString)

        let childrenJson = serializeToJson [ instance.Id ]
        setChildren(this.Current.Id, childrenJson)

    // Move nodes within the current node's children
    override this.Move(fromIndex: int, toIndex: int, count: int) =
        let moveItems (list: WidgetNode list) fromIndex toIndex count =
            let itemsToMove = list |> List.skip fromIndex |> List.take count
            let remaining = 
                list 
                |> List.mapi (fun i x -> if i >= fromIndex && i < fromIndex + count then None else Some x)
                |> List.choose id
            let (before, after) = remaining |> List.splitAt toIndex
            before @ itemsToMove @ after

        updateChildren this.Current (fun children ->
            moveItems children fromIndex toIndex count
        )

    // Remove a range of children
    override this.Remove(index: int, count: int) =
        updateChildren this.Current (fun children ->
            children.[0..index - 1] @ children.[index + count..]
        )



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

[<EntryPoint>]
let main argv =

    let appState = new BehaviorSubject<AppState>({ Text = "Hello, world" })

    let app () =
        let onClick = Some(fun () ->
            appState.OnNext({ Text = "Button clicked!" })
        )

        node [
            unformattedText (appState.Value.Text)
            button "Click me!" onClick
        ]

    let runApp () =
        let mutable oldTree = app

        // Function to apply changes
        let applyChanges (changes: Diff option) =
            match changes with
            | Some (Value(x1, x2)) ->
                // Handle leaf value differences, e.g., text content
                printfn "Value difference detected: %A -> %A" x1 x2
                // Update the widget content here

            | Some (Nullness(x1, x2)) ->
                // Handle nullness differences (null vs. non-null)
                printfn "Nullness difference detected: %A -> %A" x1 x2
                // Handle the widget addition/removal here

            | Some (Record fields) ->
                // Handle record differences (widget fields differ)
                printfn "Record difference detected, fields: %A" fields
                // Update the widget fields accordingly

            | Some (UnionCase(caseName1, caseName2)) ->
                // Handle union case differences (different union cases)
                printfn "Union case difference detected: %s -> %s" caseName1 caseName2
                // Replace the widget or update accordingly

            | Some (UnionField(case, fields)) ->
                // Handle field differences within the same union case
                printfn "Union field difference detected for case %s, fields: %A" case fields
                // Update the widget fields here

            | Some (Collection(count1, count2, items)) ->
                // Handle collection differences (lengths or items differ)
                printfn "Collection difference detected: %d -> %d, items: %A" count1 count2 items
                // Update child widgets accordingly

            | Some (Dictionary(keysInX1, keysInX2, common)) ->
                // Handle dictionary differences (keys or values differ)
                printfn "Dictionary difference detected, keys in X1: %A, keys in X2: %A" keysInX1 keysInX2
                // Update the widget properties accordingly

            | Some (Custom customDiff) ->
                // Handle custom diffing logic
                printfn "Custom diff detected: %A" customDiff
                // Apply custom diff logic here

            | None ->
                // No changes detected
                printfn "No changes detected"

        // Subscribe to state changes
        appState.Subscribe(fun _ ->
            let newTree = app
            let changes = Differ.Diff(oldTree, newTree) // Calculate the diff
            applyChanges changes // Apply the changes to your UI
            oldTree <- newTree // Update the old tree
        ) |> ignore

        // Initial render
        oldTree

    runApp
    
    // Define individual widget nodes
    let buttonWidget = {
        Id = 1
        Type = "Button"
        Props = new BehaviorSubject<Map<string, obj>>(Map.ofList [("text", box "Click Me")])
        Children = new BehaviorSubject<WidgetNode list>([])
    }

    let labelWidget = {
        Id = 2
        Type = "Label"
        Props = new BehaviorSubject<Map<string, obj>>(Map.ofList [("text", box "Hello World")])
        Children = new BehaviorSubject<WidgetNode list>([])
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
            let dict = new Dictionary<string, obj>()
            dict.Add("id", 0 :> obj)
            dict.Add("type", "node" :> obj)
            dict.Add("root", true :> obj)
            dict

        // Text node definition
        let textNode = 
            let dict = new Dictionary<string, obj>()
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
