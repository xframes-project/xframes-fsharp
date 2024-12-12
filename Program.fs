// Program.fs
open System

type Widget = {
    Id: int
    Type: string
    mutable Props: Map<string, obj>
    mutable Children: Widget list
}

let createNode id type' props children =
    {
        Id = id
        Type = type'
        Props = props
        Children = children
    }


[<EntryPoint>]
let main argv =
    
    // Example of creating a Node widget containing a Button and a Label widget
    let buttonWidget = {
        Id = 1
        Type = "Button"
        Props = Map.ofList [("text", "Click Me")]
        Children = []
    }

    let labelWidget = {
        Id = 2
        Type = "Label"
        Props = Map.ofList [("text", "Hello World")]
        Children = []
    }

    let nodeWidget = createNode 3 "Node" (Map.ofList [("style", "vertical")]) [buttonWidget; labelWidget]

    printfn "Hello, F#"
    0 // return an integer exit code
