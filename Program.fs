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
open JsonAdapters

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
    |> List.map (fun size -> { Name = fontName; Size = size })

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

let onTextChangedDelegate = OnTextChangedCb(fun id value -> printfn "Text changed: %d, %s" id value)
let onComboChangedDelegate = OnComboChangedCb(fun id value -> printfn "Value changed: %d, %d" id value)
let onNumericValueChanged = OnNumericValueChangedCb(fun id value -> printfn "Value changed: %d, %f" id value)
let onBooleanValueChanged = OnBooleanValueChangedCb(fun id value -> printfn "Value changed: %d, %b" id value)
let onMultipleNumericValuesChanged = OnMultipleNumericValuesChangedCb(fun id rawValues numValues -> 
    for value in marshalFloatArray rawValues numValues do
        printfn "Value: %f" value)
let onClickDelegate = OnClickCb(fun id -> WidgetRegistrationService.dispatchOnClickEvent(id))

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
    let onComboChangedPtr = Marshal.GetFunctionPointerForDelegate(onComboChangedDelegate)
    let onNumericValueChangedPtr = Marshal.GetFunctionPointerForDelegate(onNumericValueChanged)
    let onBooleanValueChangedPtr = Marshal.GetFunctionPointerForDelegate(onBooleanValueChanged)
    let onMultipleNumericValuesChangedPtr = Marshal.GetFunctionPointerForDelegate(onMultipleNumericValuesChanged)
    let onClickPtr = Marshal.GetFunctionPointerForDelegate(onClickDelegate)

    let fontDefsJson = JsonConvert.SerializeObject(StyleJsonAdapter.fontDefsToJson(fontsDictionary))
    let themeJson = JsonConvert.SerializeObject(colorsDict)

    init(assetsPath, fontDefsJson, themeJson, onInit, onTextChangedPtr, onComboChangedPtr, onNumericValueChangedPtr, onBooleanValueChangedPtr, onMultipleNumericValuesChangedPtr, onClickPtr)

    let appProcess = keepProcessRunning ()
    Async.Start appProcess
    
    // Prevent the app from terminating by waiting indefinitely
    Console.ReadLine() |> ignore
    0 // return an integer exit code
