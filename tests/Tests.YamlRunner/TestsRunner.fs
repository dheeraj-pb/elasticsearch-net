namespace Tests.YamlRunner

open System
open System.Diagnostics
open ShellProgressBar
open Tests.YamlRunner.Models
open Tests.YamlRunner.TestsReader
open Tests.YamlRunner.OperationExecutor
open Tests.YamlRunner.Stashes
open Elasticsearch.Net

type TestRunner(client:IElasticLowLevelClient, version: string, progress:IProgressBar, barOptions:ProgressBarOptions) =
    
    member this.OperationExecutor = OperationExecutor(client)
    
    member private this.RunOperation file section operation nth stashes (subProgressBar:IProgressBar) = async {
        let executionContext = {
            Version = version
            Suite= OpenSource
            File= file
            Folder= file.Directory
            Section= section
            NthOperation= nth
            Operation= operation
            Stashes = stashes
            Elapsed = ref 0L
        }
        let sw = Stopwatch.StartNew()
        let! pass = this.OperationExecutor.Execute executionContext subProgressBar
        executionContext.Elapsed := sw.ElapsedMilliseconds
        match pass with
        | Failed f ->
            let c = pass.Context
            subProgressBar.WriteLine <| sprintf "%s %s %s: %s %s" pass.Name c.Folder.Name c.File.Name (operation.Log()) (f.Log())
        | _ -> ignore()
        return pass
    }
    
    member private this.CreateOperations m file (ops:Operations) subProgressBar = 
        let executedOperations =
            let stashes = Stashes()
            ops
            |> List.indexed
            |> List.map (fun (i, op) -> async {
                let! pass = this.RunOperation file m op i stashes subProgressBar
                //let! x = Async.Sleep <| randomTime.Next(0, 10)
                return pass
            })
        (m, executedOperations)
        
    member private this.RunTestFile subProgressbar (file:YamlTestDocument) = async {
        
        let m section ops = this.CreateOperations section file.FileInfo ops subProgressbar
        let bootstrap section operations =
            let ops = operations |> Option.map (m section) |> Option.toList |> List.collect (fun (s, ops) -> ops)
            ops
        
        let setup =  bootstrap "Setup" file.Setup 
        let teardown = bootstrap "Teardown" file.Teardown 
        let sections =
            file.Tests
            |> List.map (fun s -> s.Operations |> m s.Name)
            |> List.collect (fun s ->
                let (name, ops) = s
                [(name, setup @ ops)]
            )
        
        let l = sections.Length
        let ops = sections |> List.sumBy (fun (_, i) -> i.Length)
        subProgressbar.MaxTicks <- ops
        
        let runSection progressHeader sectionHeader (ops: Async<ExecutionResult> list) = async {
            let l = ops |> List.length
            let result =
                ops
                |> List.indexed
                |> Seq.unfold (fun ms ->
                    match ms with
                    | (i, op) :: tl ->
                        let operations = sprintf "%s [%i/%i] operations: %s" progressHeader (i+1) l sectionHeader
                        subProgressbar.Tick(operations)
                        let r = Async.RunSynchronously op
                        match r with
                        | Succeeded context -> Some (r, tl)
                        | NotSkipped context -> Some (r, tl)
                        | Skipped (context, reason) -> Some (r, [])
                        | Failed context -> Some (r, [])
                    | [] -> None
                )
                |> List.ofSeq
            return sectionHeader, result
        }
        
        let runAllSections =
            sections
            |> Seq.indexed
            |> Seq.collect (fun (i, suite) ->
                let run section =
                    let progressHeader = sprintf "[%i/%i] sections" (i+1) l
                    let (sectionHeader, ops) = section
                    runSection progressHeader sectionHeader ops;
                [
                    // setup run as part of the suite, unfold will stop if setup fails or skips
                    run suite;
                    //always run teardown
                    run ("Teardown", teardown)
                ]
            )
            |> Seq.map Async.RunSynchronously
        
        return runAllSections |> Seq.toList
        
    }

    member this.RunTestsInFolder mainMessage (folder:YamlTestFolder) = async {
        let l = folder.Files.Length
        let run (i, document) = async {
            let file = sprintf "%s/%s" document.FileInfo.Directory.Name document.FileInfo.Name
            let message = sprintf "%s [%i/%i] Files : %s" mainMessage (i+1) l file
            progress.Tick(message)
            let message = sprintf "Inspecting file for sections" 
            use p = progress.Spawn(0, message, barOptions)
            
            let! result = this.RunTestFile p document
            
            return document, result
        }
            
        let actions =
            folder.Files
            |> Seq.indexed 
            |> Seq.map run 
            |> Seq.map Async.RunSynchronously
        return actions
    }

