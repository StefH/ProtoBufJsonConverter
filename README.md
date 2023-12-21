# ProtoBufJsonConverter
Convert a protobuf message to a JSON string using the proto definition file.

## NuGet
[![NuGet Badge](https://buildstats.info/nuget/ProtoBufJsonConverter)](https://www.nuget.org/packages/ProtoBufJsonConverter) 

## Usage

### Proto
``` proto
syntax = "proto3";

// Package name
package greet;

// The greeting service definition.
service Greeter {
  rpc SayHello (HelloRequest) returns (HelloReply);
}

// The request message containing the user's name.
message HelloRequest {
  string name = 1;
}

// The response message containing the greetings
message HelloReply {
  string message = 1;
}
```

### :one: Convert ProtoBuf `byte[]` to a JSON `string`

#### Code
``` csharp
var protoDefinition = "...".

var bytes = Convert.FromBase64String("CgRzdGVm");

var convertToJsonRequest = new ConvertToJsonRequest(protoDefinition, bytes, "greet.Greeter.SayHello");

var converter = new Converter();

var json = converter.ConvertToJson(convertToJsonRequest);
```

#### JSON
``` json
{"name":"stef"}
```

### :two: Convert JSON `string` to a ProtoBuf `byte[]`
#### Code
``` csharp
var protoDefinition = "...".

var json = @"{""name"":""stef""}";

var convertToProtoBufRequest = new ConvertToProtoBufRequest(protoDefinition, json, "greet.Greeter.SayHello");

var protobuf = converter.ConvertToProtoBuf(convertToProtoBufRequest);
```