open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Runtime.InteropServices
open Newtonsoft.Json
open Externs
open Theme
open Types
open Services
open TreeTraversal
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
    let assetsPath = "./assets"

    let onInitLogic () =

        let root = Root()
        let shadowTree = traverseTree (BaseComponent (root))

        //printfn "shadowTree: %A" shadowTree

        ignore()


    let onInit = Marshal.GetFunctionPointerForDelegate(Action(fun () -> onInitLogic()))
    
    let onTextChangedPtr = Marshal.GetFunctionPointerForDelegate(onTextChangedDelegate)
    let OnClickPtr = Marshal.GetFunctionPointerForDelegate(OnClickDelegate)

    let onComboChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onNumericValueChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onBooleanValueChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))
    let onMultipleNumericValuesChanged = Marshal.GetFunctionPointerForDelegate(Action(fun () -> printfn "Initialization callback called!"))

    let fontDefsJson = JsonConvert.SerializeObject(fontsDictionary)
    let themeJson = JsonConvert.SerializeObject(colorsDict)


    init(assetsPath, fontDefsJson, themeJson, onInit, onTextChangedPtr, onComboChanged, onNumericValueChanged, onBooleanValueChanged, onMultipleNumericValuesChanged, OnClickPtr)

    let appProcess = keepProcessRunning ()
    Async.Start appProcess
    
    // Prevent the app from terminating by waiting indefinitely
    Console.ReadLine() |> ignore
    0 // return an integer exit code
