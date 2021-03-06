using System.Diagnostics.CodeAnalysis;

[assembly:
	SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member",
		Target =
			"Elasticsearch.Net.IElasticLowLevelClient.#IndicesGetUpgradeAsync`1(System.String,System.Func`2<Elasticsearch.Net.UpgradeStatusRequestParameters,Elasticsearch.Net.UpgradeStatusRequestParameters>)")]
[assembly:
	SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Scope = "member",
		Target =
			"Elasticsearch.Net.ElasticLowLevelClient.#CatAliasesAsync`1(System.Func`2<Elasticsearch.Net.CatAliasesRequestParameters,Elasticsearch.Net.CatAliasesRequestParameters>)")]
[assembly:
	SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member",
		Target = "Elasticsearch.Net.ConnectionConfiguration`1.#Elasticsearch.Net.IConnectionConfigurationValues.RequestTimeout")]
[assembly:
	SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#", Scope = "member",
		Target = "Elasticsearch.Net.ConnectionConfiguration`1.#SniffLifeSpan(System.Nullable`1<System.TimeSpan>)")]
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.
