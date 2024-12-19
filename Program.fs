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
open Services
open WidgetNodeJsonAdapter
open TreeTraversal
open TreeDiffing
open WidgetHelpers
open SampleApp



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

type WidgetTreeApplier(jsonAdapter: WidgetNodeJsonAdapter, root: RawWidgetNodeWithId) =
    inherit AbstractApplier<RawWidgetNodeWithId>(root)

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
        root.Children <- []

    // Insert a node bottom-up (logic can be customized)
    override this.InsertBottomUp(index: int, instance: RawWidgetNode) =
        ignore()

    // Insert a node top-down
    override this.InsertTopDown(index: int, instance: RawWidgetNode) =
        let widgetWithId = createRawWidgetNodeWithIdFromRawWidgetNodeWithoutId(instance)

        WidgetRegistrationService.registerWidget(widgetWithId.Id, widgetWithId)

        match widgetWithId.Props.TryFind("onClick") with
        | Some value ->
            match value with
            | :? (unit -> unit) as onClickFn -> WidgetRegistrationService.registerWidgetForOnClickEvent(widgetWithId.Id, onClickFn)
            | _ -> ignore()
        | None -> ignore()
        
        let json = jsonAdapter.ToJson(widgetWithId)
        let jsonString = serializeToJson json

        printfn "%s" jsonString

        setElement(jsonString)

        let childrenJson = serializeToJson [ widgetWithId.Id ]
        setChildren(this.Current.Id, childrenJson)

        this.Root.Children <- this.Root.Children @ [widgetWithId]

        ignore()

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

        //updateChildren this.Current (fun children ->
        //    moveItems children fromIndex toIndex count
        //)
        ignore()

    // Remove a range of children
    override this.Remove(index: int, count: int) =
        ignore()
        //updateChildren this.Current (fun children ->
        //    children.[0..index - 1] @ children.[index + count..]
        //)



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

// Create an instance of the delegate
let onTextChangedDelegate = OnTextChangedCb(fun id value -> printfn "Text changed: %d, %s" id value)

// Create an instance of the delegate
let OnClickDelegate = OnClickCb(fun id -> WidgetRegistrationService.dispatchOnClickEvent(id))

[<EntryPoint>]
let main argv =

    let rootNodeWithId = createRawWidgetNodeWithId(0, WidgetTypes.Node, Map.ofList [("root", box true)], [])
    let rootNode = createRawWidgetNode(WidgetTypes.Node, Map.ofList [("root", box true)], [])

    let appState = new BehaviorSubject<AppState>({ Text = "Hello, world"; Count = 1 })

    let app () =
        let onClick = Some(fun () ->
            appState.OnNext({ Text = "Button clicked!"; Count = appState.Value.Count + 1 })
        )

        makeRootNode [
            unformattedText (appState.Value.Text)
            button(appState.Value.Text, onClick)
        ]

            

    let runApp () =
        let mutable oldTree = rootNode

        let applier = WidgetTreeApplier(WidgetNodeJsonAdapter(), rootNodeWithId)

        // Function to apply changes
        let applyChanges (changes: Diff option) =
            match changes with
            | Some (Value(x1, x2)) ->
                printfn "Value difference detected: %A -> %A" x1 x2

            | Some (Nullness(x1, x2)) ->
                printfn "Nullness difference detected: %A -> %A" x1 x2

            | Some (Record fields) ->
                printfn "Record difference detected, fields: %A" fields

                for i = 0 to fields.Count - 1 do
                    match fields.Item(i).Name with
                    | "Props" -> 
                        printfn "Props are different!"
                    | "Children" -> 
                        printfn "Children are different!"
                        //let changes = Differ.Diff(oldTree.Children, normalizedNewTree)
                        ignore()
                    | _ -> printfn "Unrecognized Name in Field"

            | Some (UnionCase(caseName1, caseName2)) ->
                printfn "Union case difference detected: %s -> %s" caseName1 caseName2

            | Some (UnionField(case, fields)) ->
                printfn "Union field difference detected for case %s, fields: %A" case fields

            | Some (Collection(count1, count2, items)) ->
                printfn "Collection difference detected: %d -> %d, items: %A" count1 count2 items

                for i = 0 to items.Count - 1  do
                    //applier.InsertTopDown(i, items.Item(i))
                    printfn "item: %A" (items.Item(i))

            | Some (Dictionary(keysInX1, keysInX2, common)) ->
                printfn "Dictionary difference detected, keys in X1: %A, keys in X2: %A" keysInX1 keysInX2

            | Some (Custom customDiff) ->
                printfn "Custom diff detected: %A" customDiff

            | None ->
                printfn "No changes detected"

        appState.Subscribe(fun state ->
            printfn "%A" state

            let currentTree = app()
            let normalizedOldTree = normalizeRawWidgetNodeWithIdTree applier.Root
            let normalizedNewTree = normalizeWidgetNodeTree currentTree

            printfn "Old tree: %A" normalizedOldTree
            printfn "New tree: %A" normalizedNewTree

            // Compute the differences
            //let changes = Differ.Diff(normalizedOldTree, normalizedNewTree)

            let changes = diffNodes(normalizedOldTree, normalizedNewTree)

            match changes with
            | Some (Value(x1, x2)) ->
                printfn "Value difference detected: %A -> %A" x1 x2

            | Some (Nullness(x1, x2)) ->
                printfn "Nullness difference detected: %A -> %A" x1 x2

            | Some (Record fields) ->
                //printfn "Record difference detected, fields: %A" fields

                for field in fields do
                    match field.Name with
                    | "Props" -> 
                        printfn "Props are different!"

                    | "Children" -> 
                        printfn "Children are different!"
                        // Here you could apply your props diff handling logic


                        //// Assuming normalizedNewTree.Children and normalizedOldTree.Children
                        //let changes = diffNodes (normalizedOldTree.Children, normalizedNewTree.Children)
                        printfn "Children difference detected: %A" field.Diff

                        match field.Diff with
                        | Collection(_, _, items) ->
                            for i = 0 to items.Count - 1 do
                                let child = items.Item(i)
        
                                match child.Diff with
                                | Value(x1, x2) ->
                                    match x1, x2 with
                                    | null, RawWidgetNode -> 
                                        // Directly cast x2 to RawWidgetNode and insert
                                        applier.InsertTopDown(i, x2 :?> RawWidgetNode)
                                        printfn "Child added"
                                    | RawWidgetNode, null -> 
                                        // Handle child removal (x1 is a RawWidgetNode)
                                        printfn "Child removed"
                                    | _ -> 
                                        // Handle other situations if needed
                                        printfn "Other Value diff case"
                                | Record fields ->
                                    //printfn "Fields difference detected: %A" fields
                                    for i = 0 to fields.Count - 1 do
                                        let field = fields.Item(i)
                                        printfn "Field difference detected: %A" field



                                    ignore()
                                | _ -> 
                                    // Handle other types of diffs, e.g., Nullness, Record, etc.
                                    printfn "Other diff type detected for child"

                            ignore() // Optional if ignoring the return value


                        //// Apply changes to the children using InsertTopDown
                        //match changes with
                        //| Some (Collection(_, _, items)) -> 
                        //    for i = 0 to items.Count - 1 do
                        //        let child = items.Item(i)
                        //        printfn "Inserting child: %A" child
                        //        applier.InsertTopDown(i, child)
                        //| _ -> printfn "Unexpected changes in children"

                    | name when name.StartsWith("Child") ->
                        printfn "Child node difference detected: %s" name
                        // Here, you can handle the diff of child nodes

                    | unreco -> printfn "Unrecognized Name in Field: %s" unreco

            | Some (UnionCase(caseName1, caseName2)) ->
                printfn "Union case difference detected: %s -> %s" caseName1 caseName2

            | Some (UnionField(case, fields)) ->
                printfn "Union field difference detected for case %s, fields: %A" case fields

            | Some (Collection(count1, count2, items)) ->
                printfn "Collection difference detected: %d -> %d, items: %A" count1 count2 items

                for i = 0 to items.Count - 1 do
                    // Assuming you want to apply changes to each collection item
                    printfn "Item: %A" (items.Item(i))

            | Some (Dictionary(keysInX1, keysInX2, common)) ->
                printfn "Dictionary difference detected, keys in X1: %A, keys in X2: %A" keysInX1 keysInX2

            | Some (Custom customDiff) ->
                printfn "Custom diff detected: %A" customDiff

            | None ->
                printfn "No changes detected"

            // Apply the changes using the WidgetTreeApplier
            //applyChanges changes

            // Update the old tree reference
            oldTree <- normalizedNewTree
        ) |> ignore

        ignore

    
    

    //printfn "%s" fontDefsJson

    // Example call to init (using some mock values)
    let assetsPath = "./assets"

    // Function that will contain the logic for initializing nodes
    let onInitLogic () =
        let rootNode = 
            let dict = new Dictionary<string, obj>()
            dict.Add("id", 0 :> obj)
            dict.Add("type", "node" :> obj)
            dict.Add("root", true :> obj)
            dict

        setElement (JsonConvert.SerializeObject(rootNode))

        //runApp()

        let shadowTree = traverseTree (IComponent (App()))

        printfn "shadowTree: %A" shadowTree

        ignore()


    let onInit = Marshal.GetFunctionPointerForDelegate(Action(fun () -> onInitLogic()))
    //let onInit = Marshal.GetFunctionPointerForDelegate(Action(fun () -> ignore()))
    
    let onTextChangedPtr = Marshal.GetFunctionPointerForDelegate(onTextChangedDelegate)
    let OnClickPtr = Marshal.GetFunctionPointerForDelegate(OnClickDelegate)

    let onComboChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onNumericValueChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onBooleanValueChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onMultipleNumericValuesChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    //let onClick = Marshal.GetFunctionPointerForDelegate(Action<int>(fun (id: int) -> onClickLogic(id)))

    let fontDefsJson = JsonConvert.SerializeObject(fontsDictionary)
    let themeJson = JsonConvert.SerializeObject(colorsDict)


    init(assetsPath, fontDefsJson, themeJson, onInit, onTextChangedPtr, onComboChanged, onNumericValueChanged, onBooleanValueChanged, onMultipleNumericValuesChanged, OnClickPtr)

    let appProcess = keepProcessRunning ()
    Async.Start appProcess
    
    // Prevent the app from terminating by waiting indefinitely
    Console.ReadLine() |> ignore
    0 // return an integer exit code
