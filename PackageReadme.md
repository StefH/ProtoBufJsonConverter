# ProtoBufJsonConverter

## This project uses [protobuf-net](https://github.com/protobuf-net/protobuf-net) to:
- Convert a protobuf message to a JSON string using the proto definition file.
- Convert a protobuf message to an object using the proto definition file.
- Convert a JSON string or an object to a protobuf message using the proto definition file.

## Usage

### Proto Definition
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
var protoDefinition = "...". // See above

var bytes = Convert.FromBase64String("CgRzdGVm");

var request = new ConvertToJsonRequest(protoDefinition, "greet.HelloRequest", bytes);

var converter = new Converter();

var json = await converter.ConvertAsync(request);
```

#### JSON
``` json
{"name":"stef"}
```

### :one: Convert ProtoBuf `byte[]` to an object

#### Code
``` csharp
var protoDefinition = "...". // See above

var bytes = Convert.FromBase64String("CgRzdGVm");

var request = new ConvertToObjectRequest(protoDefinition, "greet.HelloRequest", bytes);

var converter = new Converter();

var @object = await converter.ConvertAsync(request);
```

### :three: Convert JSON `string` to a ProtoBuf `byte[]`
#### Code
``` csharp
var protoDefinition = "...". // See above

var json = @"{""name"":""stef""}";

var request = new ConvertToProtoBufRequest(protoDefinition, "greet.HelloRequest", json);

var converter = new Converter();

var protobuf = await converter.ConvertAsync(request);
```

### :four: Convert any `object` to a ProtoBuf `byte[]`
#### Code
``` csharp
var protoDefinition = "...". // See above

var obj = new
{
    name = "stef"
};

var request = new ConvertToProtoBufRequest(protoDefinition, "greet.HelloRequest", obj);

var converter = new Converter();

var protobuf = await converter.ConvertAsync(request);
```

### :five: Convert any `object` to a ProtoBuf `byte[]` including the Grpc Header
#### Code
``` csharp
var protoDefinition = "...". // See above

var obj = new
{
    name = "stef"
};

var request = new ConvertToProtoBufRequest(protoDefinition, "greet.HelloRequest", obj)
    .WithGrpcHeader();

var converter = new Converter();

var protobufWithGrpcHeader = await ConvertAsync.Convert(request);
```

## Examples
- [Blazor WASM](https://wonderful-beach-0d16cee03.4.azurestaticapps.net/)
- [Blazor Static Web App](https://zealous-desert-029b2f003.4.azurestaticapps.net/)