module Tests.YamlRunner.DoMapper

open System
open System.Reflection
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Globalization
open System.IO
open System.Linq
open System.Linq.Expressions
open System.Threading.Tasks
open Tests.YamlRunner.Models
open Elasticsearch.Net

type ApiInvoke = delegate of Object * Object[] -> Task<DynamicResponse>

type RequestParametersInvoke = delegate of unit ->  IRequestParameters

type FastApiInvoke(instance: Object, restName:string, pathParams:KeyedCollection<string, string>, methodInfo:MethodInfo) =
    member this.ClientMethodName = methodInfo.Name
    member this.ApiName = restName
    member private this.IndexOfParam p = pathParams.IndexOf p
    member private this.SupportsBody = pathParams.IndexOf "body" >= 0
    member this.PathParameters =
        pathParams |> Seq.map (fun k -> k) |> Seq.filter (fun k -> k <> "body") |> Set.ofSeq
        
    member private this.CreateRequestParameters = 
        let t = methodInfo.GetParameters() |> Array.find (fun p -> typeof<IRequestParameters>.IsAssignableFrom(p.ParameterType))
        let c = t.ParameterType.GetConstructors() |> Array.head
        
        let newExp = Expression.New(c)
        Expression.Lambda<RequestParametersInvoke>(newExp).Compile()
        
    ///<summary> Create a call into a specific client method </summary>
    member private this.Delegate =
        let instanceExpression = Expression.Parameter(typeof<Object>, "instance");
        let argumentsExpression = Expression.Parameter(typeof<Object[]>, "arguments");
        let argumentExpressions = new List<Expression>();
        methodInfo.GetParameters()
            |> Array.indexed
            |> Array.iter (fun (i, p) ->
                let constant = Expression.Constant i
                let index = Expression.ArrayIndex (argumentsExpression, constant)
                let convert = Expression.Convert (index, p.ParameterType)
                argumentExpressions.Add convert
            )
        let x = [|typeof<DynamicResponse>|] 
        let callExpression =
            let instance = Expression.Convert(instanceExpression, methodInfo.ReflectedType)
            Expression.Call(instance, methodInfo.Name, x, argumentExpressions.ToArray())
            
        let invokeExpression = Expression.Convert(callExpression, typeof<Task<DynamicResponse>>)
        Expression.Lambda<ApiInvoke>(invokeExpression, instanceExpression, argumentsExpression).Compile();
    
    member private this.toMap (o:YamlMap) = o |> Seq.map (fun o -> o.Key :?> String , o.Value) |> Map.ofSeq
        
    member this.CanInvoke (o:YamlMap) =
        let operationKeys =
            o
            |> this.toMap
            |> Seq.map (fun k -> k.Key)
            |> Seq.filter (fun k -> k <> "body")
            |> Set.ofSeq
        this.PathParameters.IsSubsetOf operationKeys
    
    member this.Invoke (map:YamlMap) =
        let o = map |> this.toMap
        
        let foundBody, body = o.TryGetValue "body"
        
        let arguments =
            o
            |> Map.toSeq
            |> Seq.filter (fun (k, v) -> this.PathParameters.Contains(k))
            |> Seq.sortBy (fun (k, v) -> this.IndexOfParam k)
            |> Seq.map (fun (k, v) ->
                let toString (value:Object) = 
                    match value with
                    | :? String as s -> s
                    | :? int32 as i -> i.ToString(CultureInfo.InvariantCulture)
                    | :? double as i -> i.ToString(CultureInfo.InvariantCulture)
                    | :? int64 as i -> i.ToString(CultureInfo.InvariantCulture)
                    | :? Boolean as b -> if b then "false" else "true"
                    | e -> failwithf "unknown type %s " (e.GetType().Name)
                
                match v with
                | :? List<Object> as a ->
                    let values = a |> Seq.map toString |> Seq.toList
                    // https://github.com/elastic/elasticsearch/blob/6f1359fb70fba1bd7a1e26f4a9d42a9098ed4371/rest-api-spec/src/main/resources/rest-api-spec/test/indices.refresh/10_basic.yml#L40-L42
                    match values with
                    | [] -> "_all" 
                    | _ -> String.Join(',', values)
                | e -> toString e
                ) 
            |> Seq.cast<Object>
            |> Seq.toArray
        
        let requestParameters = this.CreateRequestParameters.Invoke()
        o
        |> Map.toSeq
        |> Seq.filter (fun (k, v) -> not <| this.PathParameters.Contains(k))
        |> Seq.filter (fun (k, v) -> k <> "body")
        |> Seq.iter (fun (k, v) -> requestParameters.SetQueryString(k, v))
        
        let post =
            match body with
            | null -> null
            | :? List<Object> as e ->
                match e with
                | e when e.All(fun i -> i.GetType() = typeof<String>) ->
                    PostData.MultiJson(e.Cast<String>())
                | e -> PostData.MultiJson e
            | :? String as s -> PostData.String s
            | _ -> PostData.Serializable body :> PostData
        
        let args = 
            match (foundBody, this.SupportsBody) with
            | (true, true) ->
                Array.append arguments [|post; requestParameters; Async.DefaultCancellationToken|]
            | (false, true) ->
                Array.append arguments [|null ; requestParameters; Async.DefaultCancellationToken|]
            | (false, false) ->
                Array.append arguments [|requestParameters; Async.DefaultCancellationToken|]
            | (true, false) -> failwithf "found a body but this method does not take a body"
        
        this.Delegate.Invoke(instance, args)


let getProp (t:Type) prop = t.GetProperty(prop).GetGetMethod()
let getRestName (t:Type) a = (getProp t "RestSpecName").Invoke(a, null) :?> String
let getParameters (t:Type) a = (getProp t "Parameters").Invoke(a, null) :?> KeyedCollection<string, string>

let private methodsWithAttribute instance mapsApiAttribute  =
    let clientType = instance.GetType()
    clientType.GetMethods()
    |> Array.map (fun m -> (m, m.GetCustomAttributes(mapsApiAttribute, false)))
    |> Array.filter (fun (_, a) -> a.Length > 0)
    |> Array.map (fun (m, a) -> (m, a.[0] :?> Attribute))
    |> Array.map (fun (m, a) -> (m, getRestName mapsApiAttribute a, getParameters mapsApiAttribute a))
    |> Array.map (fun (m, restName, pathParams) -> (FastApiInvoke(instance, restName, pathParams, m)))

exception ParamException of string 

let private createApiLookup (invokers: FastApiInvoke list) : (YamlMap -> FastApiInvoke) =
    let first = invokers |> List.head
    let name = first.ApiName
    let clientMethod = first.ClientMethodName
    
    let lookup (o:YamlMap) =
        
        let invokers =
            invokers
            |> Seq.sortByDescending (fun i -> i.PathParameters.Count)
            |> Seq.filter (fun i -> i.CanInvoke o)
            |> Seq.toList
        
        match invokers with
        | [] ->
            raise <| ParamException(sprintf "%s matched no method on %s: %O " name clientMethod o)
        | invoker::tail ->
           invoker 
    lookup
    
    
let createDoMap (client:IElasticLowLevelClient) =
    let t = client.GetType()
    let mapsApiAttribute = t.Assembly.GetType("Elasticsearch.Net.MapsApiAttribute")
    
    let rootMethods = methodsWithAttribute client mapsApiAttribute
    let namespaces =
        t.GetProperties()
        |> Array.filter (fun p -> typeof<NamespacedClientProxy>.IsAssignableFrom(p.PropertyType))
        |> Array.map (fun p -> methodsWithAttribute (p.GetGetMethod().Invoke(client, null)) mapsApiAttribute)
        |> Array.concat
        |> Array.append rootMethods
    
    namespaces
    |> List.ofArray
    |> List.groupBy (fun n -> n.ApiName)
    |> Map.ofList<String, FastApiInvoke list>
    |> Map.map<String, FastApiInvoke list, (YamlMap -> FastApiInvoke)>(fun k v -> createApiLookup v)

    
        

