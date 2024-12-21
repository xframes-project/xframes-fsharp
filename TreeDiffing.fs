module TreeDiffing

open DEdge.Diffract
open Types

let rec arePropsDifferent (props1: Map<string, obj>, props2: Map<string, obj>) =
    props1 |> Seq.exists (fun kvp ->
        match props2.TryFind kvp.Key with
        | Some value -> value <> kvp.Value
        | None -> true
    ) || props2 |> Seq.exists (fun kvp ->
        not (props1.ContainsKey kvp.Key)
    )

let rec diffNodes (node1: RawWidgetNode, node2: RawWidgetNode) : Option<Diff> =
    // Compare the types first
    if node1.Type <> node2.Type then
        Some(Diff.Value(node1.Type, node2.Type))
    
    // Compare the props
    elif arePropsDifferent(node1.Props, node2.Props) then
        Some(Diff.Record [
            { Name = "Props"; Diff = Diff.Value(node1.Props, node2.Props) }
        ])
    
    // Compare the children
    else
        match diffChildren(node1.Children, node2.Children) with
        | Some(Diff.Collection(_, _, childDiffs)) when childDiffs.Count = 0 ->
            None // No differences in children, return None
            
        | Some(Diff.Collection(_, _, childDiffs)) -> 
            Some(Diff.Record [
                { Name = "Children"; Diff = Diff.Collection(0, 0, childDiffs) }
            ])
        
        | None -> 
            None // No child differences

            
and diffChildren (children1: RawWidgetNode list, children2: RawWidgetNode list) : Option<Diff> =
    let count1 = List.length children1
    let count2 = List.length children2

    // Generate the list of differences between the child nodes
    let itemDiffs =
        [0 .. max count1 count2 - 1]
        |> List.choose (fun i ->
            match List.tryItem i children1, List.tryItem i children2 with
            | Some c1, Some c2 ->
                match diffNodes(c1, c2) with
                | Some diff when diff <> Diff.Value(c1, c2) ->
                    Some { Name = sprintf "Child[%d]" i; Diff = diff }
                | _ -> None
            | Some c1, None -> Some { Name = sprintf "Child[%d]" i; Diff = Diff.Value(c1, null) }
            | None, Some c2 -> Some { Name = sprintf "Child[%d]" i; Diff = Diff.Value(null, c2) }
            | None, None -> None
        )

    // If there are differences in the children, return Diff.Collection wrapped in Some, otherwise None.
    if List.isEmpty itemDiffs then
        None
    else
        Some(Diff.Collection(count1, count2, itemDiffs))
