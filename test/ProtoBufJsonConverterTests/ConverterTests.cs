using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using ProtoBufJsonConverter;
using ProtoBufJsonConverter.Models;

namespace ProtoBufJsonConverterTests;

public class ConverterTests
{
    private const string ProtoDefinitionNoPackage = @"
syntax = ""proto3"";

service Greeter {
    rpc SayHello (HelloRequest) returns(HelloReply);
}

message HelloRequest {
    string name = 1;
}

message HelloReply {
    string message = 1;
}
";

    private const string ProtoDefinition = @"
syntax = ""proto3"";

import ""google/protobuf/empty.proto"";

package greet;

service Greeter {
    rpc SayHello (HelloRequest) returns(HelloReply);

    rpc SayNothing (google.protobuf.Empty) returns (google.protobuf.Empty);
}

message HelloRequest {
    string name = 1;
}

message HelloReply {
    string message = 1;
}
";

    private const string ProtoDefinitionWithDotInPackage = @"
syntax = ""proto3"";

package greet.foo;

service Greeter {
    rpc SayHello (HelloRequest) returns(HelloReply);
}

message HelloRequest {
    string name = 1;
}

message HelloReply {
    string message = 1;
}
";

    private const string ProtoDefinitionWithDotsInPackage = @"
syntax = ""proto3"";

package greet.foo.bar;

service Greeter {
    rpc SayHello (HelloRequest) returns(HelloReply);
}

message HelloRequest {
    string name = 1;
}

message HelloReply {
    string message = 1;
}
";

    private const string ProtoDefinitionWithWellKnownTypes = @"
syntax = ""proto3"";

import ""google/protobuf/empty.proto"";
import ""google/protobuf/timestamp.proto"";
import ""google/protobuf/duration.proto"";

service Greeter {
    rpc SayNothing (google.protobuf.Empty) returns (google.protobuf.Empty);
}

message MyMessageTimestamp {
    google.protobuf.Timestamp ts = 1;
}

message MyMessageDuration {
    google.protobuf.Duration du = 1;
}
";

    private const string ProtoDefinitionWithWellKnownTypesFromGoogle = @"
syntax = ""proto3"";

import ""google/protobuf/wrappers.proto"";
import ""google/protobuf/any.proto"";
import ""google/protobuf/struct.proto"";

message MyMessageStringValue {
    google.protobuf.StringValue val = 1;
}

message MyMessageInt64Value {
    google.protobuf.Int64Value val = 1; 
}

message MyMessageInt32Value {
    google.protobuf.Int32Value val = 1; 
}

message MyMessageNullValue {
    google.protobuf.NullValue val = 1;
}

message MyMessageAny {
    google.protobuf.Any val1 = 1;
    google.protobuf.Any val2 = 2;
}

message MyMessageValue {
    google.protobuf.Value val1 = 1;
    google.protobuf.Value val2 = 2;
    google.protobuf.Value val3 = 3;
    google.protobuf.Value val4 = 4;
}

message MyMessageStruct {
    google.protobuf.Struct val = 1;
}

message MyMessageListValue {
    google.protobuf.ListValue val = 1;
}
";

    private const string ProtoDefinitionWithEnum = @"
syntax = ""proto3"";

enum Enum {
    A = 0;
    B = 1;
}

message MyMessage {
    Enum e = 1;
}
";

    private const string ProtoDefinitionWithOneOf = @"
syntax = ""proto3"";

message MyMessage {
  oneof test
  {
    string name = 1;
    int32 age = 2;
  }
}
";

    private readonly Converter _sut = new();

    [Theory]
    [InlineData("AAAAAAYKBHN0ZWY=", ProtoDefinitionNoPackage, "HelloRequest")]
    [InlineData("CgRzdGVm", ProtoDefinitionNoPackage, "HelloRequest")]
    [InlineData("AAAAAAYKBHN0ZWY=", ProtoDefinition, "greet.HelloRequest")]
    [InlineData("CgRzdGVm", ProtoDefinition, "greet.HelloRequest")]
    [InlineData("AAAAAAYKBHN0ZWY=", ProtoDefinitionWithDotInPackage, "greet.foo.HelloRequest")]
    [InlineData("CgRzdGVm", ProtoDefinitionWithDotInPackage, "greet.foo.HelloRequest")]
    [InlineData("AAAAAAYKBHN0ZWY=", ProtoDefinitionWithDotsInPackage, "greet.foo.bar.HelloRequest")]
    [InlineData("CgRzdGVm", ProtoDefinitionWithDotsInPackage, "greet.foo.bar.HelloRequest")]
    public async Task ConvertAsync_ConvertToJsonRequest_SkipGrpcHeader_IsTrue(string data, string protoDefinition, string messageType)
    {
        // Arrange
        var bytes = Convert.FromBase64String(data);

        var request = new ConvertToJsonRequest(protoDefinition, messageType, bytes);

        // Act
        var json = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        json.Should().Be("""{"name":"stef"}""");
    }

    [Theory]
    [InlineData(true, "AAAAAAYKBHN0ZWY=")]
    [InlineData(false, "CgRzdGVm")]
    public async Task ConvertAsync_ConvertJsonToProtoBufRequest(bool addGrpcHeader, string expectedBytes)
    {
        // Arrange
        const string messageType = "greet.HelloRequest";

        const string json = @"{""name"":""stef""}";

        var request = new ConvertToProtoBufRequest(ProtoDefinition, messageType, json, addGrpcHeader);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be(expectedBytes);
    }

    [Theory]
    [InlineData(true, "AAAAAAYKBHN0ZWY=")]
    [InlineData(false, "CgRzdGVm")]
    public async Task ConvertAsync_ConvertObjectToProtoBufRequest(bool addGrpcHeader, string expectedBytes)
    {
        // Arrange
        const string messageType = "greet.HelloRequest";

        var @object = new { name = "stef" };

        var request = new ConvertToProtoBufRequest(ProtoDefinition, messageType, @object, addGrpcHeader);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be(expectedBytes);
    }

    [Fact]
    public async Task ConvertAsync_ConvertEnumToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessage";

        var @object = new { e = 1 };

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithEnum, messageType, @object);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be("CAE=");

        // Act 2
        var convertToJsonRequest = new ConvertToJsonRequest(ProtoDefinitionWithEnum, messageType, bytes);
        var json = await _sut.ConvertAsync(convertToJsonRequest).ConfigureAwait(false);

        // Assert 2
        json.Should().Be("""{"e":1}""");
    }

    [Theory]
    [InlineData("""{"name":"stef"}""", "CgRzdGVm")]
    [InlineData("""{"age":99}""", "EGM=")]
    public async Task ConvertAsync_ConvertOneOfToProtoBufRequest(string text, string expectedBytes)
    {
        // Arrange
        const string messageType = "MyMessage";

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithOneOf, messageType, text);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be(expectedBytes);

        // Act 2
        var convertToJsonRequest = new ConvertToJsonRequest(ProtoDefinitionWithOneOf, messageType, bytes);
        var json = await _sut.ConvertAsync(convertToJsonRequest).ConfigureAwait(false);

        // Assert 2
        json.Should().Be(text);
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypesEmpty_ConvertJsonToProtoBufRequest()
    {
        // Arrange
        const string messageType = "google.protobuf.Empty";

        const string json = "{}";

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypes, messageType, json);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        bytes.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypesEmpty_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "google.protobuf.Empty";

        var @object = new { };

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypes, messageType, @object);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        bytes.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypesTimeStamp_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessageTimestamp";

        var datetime = new DateTime(2024, 7, 30, 1, 2, 3);
        var @object = new { ts = datetime };

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypes, messageType, @object);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be("CgYIi/egtQY=");
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypesDuration_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessageDuration";

        var timespan = new TimeSpan(1, 2, 3, 4, 5, 6);
        var @object = new { du = timespan };

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypes, messageType, @object);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be("CgkI2NwFELDFsQI=");
    }

    [Theory]
    [InlineData("MyMessageStringValue", "stef", "CgYKBHN0ZWY=", """{"val":"stef"}""")]
    [InlineData("MyMessageInt64Value", long.MaxValue, "CgoI//////////9/", """{"val":9223372036854775807}""")]
    [InlineData("MyMessageInt32Value", int.MaxValue, "CgYI/////wc=", """{"val":2147483647}""")]
    public async Task ConvertAsync_WellKnownTypes_ConvertObjectToProtoBufRequest(string messageType, object? val, string expectedBytes, string expectedJson)
    {
        // Arrange
        var @object = new { val };
        var convertToProtoBufRequest = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, @object);

        // Act 1
        var bytes = await _sut.ConvertAsync(convertToProtoBufRequest).ConfigureAwait(false);

        // Assert 1
        Convert.ToBase64String(bytes).Should().Be(expectedBytes);

        // Act 2
        var convertToJsonRequest = new ConvertToJsonRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, bytes);
        var json = await _sut.ConvertAsync(convertToJsonRequest).ConfigureAwait(false);

        // Assert 2
        json.Should().Be(expectedJson);
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypes_Struct_NullValue_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessageNullValue";
        var @object = new
        {
            val = NullValue.NullValue
        };
        var convertToProtoBufRequest = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, @object);

        // Act 1
        var bytes = await _sut.ConvertAsync(convertToProtoBufRequest).ConfigureAwait(false);

        // Assert 1
        Convert.ToBase64String(bytes).Should().Be("");

        // Act 2
        var convertToJsonRequest = new ConvertToJsonRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, bytes);
        var json = await _sut.ConvertAsync(convertToJsonRequest).ConfigureAwait(false);

        // Assert 2
        json.Should().Be("""{"val":0}""");
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypes_Struct_Value_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessageValue";
        var @object = new
        {
            val1 = new Value
            {
                NullValue = NullValue.NullValue
            },
            val2 = new Value
            {
                NumberValue = 42
            },
            val3 = new Value
            {
                StringValue = "Stef"
            },
            val4 = new Value
            {
                BoolValue = true
            }
        };
        var convertToProtoBufRequest = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, @object);

        // Act 1
        var bytes = await _sut.ConvertAsync(convertToProtoBufRequest).ConfigureAwait(false);

        // Assert 1
        Convert.ToBase64String(bytes).Should().Be("CgIIABIJEQAAAAAAAEVAGgYaBFN0ZWYiAiAB");

        // Act 2
        var convertToJsonRequest = new ConvertToJsonRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, bytes);
        var json = await _sut.ConvertAsync(convertToJsonRequest).ConfigureAwait(false);

        // Assert 2
        json.Should().Be("""{"val1":0,"val2":42.0,"val3":"Stef","val4":true}""");
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypes_Struct_Struct_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessageStruct";
        var @object = new
        {
            val = new Struct
            {
                Fields =
                {
                    { "str", new Value("strValue") },
                    { "bo", new Value(false) }
                }
            }
        };
        var convertToProtoBufRequest = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, @object);

        // Act 1
        var bytes = await _sut.ConvertAsync(convertToProtoBufRequest).ConfigureAwait(false);

        // Assert 1
        Convert.ToBase64String(bytes).Should().Be("Ch0KEQoDc3RyEgoaCHN0clZhbHVlCggKAmJvEgIgAA==");

        // Act 2
        var convertToJsonRequest = new ConvertToJsonRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, bytes);
        var json = await _sut.ConvertAsync(convertToJsonRequest).ConfigureAwait(false);

        // Assert 2
        json.Should().Be("""{"val":{"fields":{"str":"strValue","bo":false}}}""");
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypes_Struct_ListValue_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessageListValue";
        var @object = new
        {
            val = new ListValue
            {
                Values =
                {
                    new Value("strValue"),
                    new Value(99)
                }
            }
        };
        var convertToProtoBufRequest = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, @object);

        // Act 1
        var bytes = await _sut.ConvertAsync(convertToProtoBufRequest).ConfigureAwait(false);

        // Assert 1
        Convert.ToBase64String(bytes).Should().Be("ChcKChoIc3RyVmFsdWUKCREAAAAAAMBYQA==");

        // Act 2
        var convertToJsonRequest = new ConvertToJsonRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, bytes);
        var json = await _sut.ConvertAsync(convertToJsonRequest).ConfigureAwait(false);

        // Assert 2
        json.Should().Be("""{"val":{"values":["strValue",99.0]}}""");
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypesAny_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessageAny";

        var any1 = Any.Pack(new StringValue { Value = "stef" });
        var any2 = Any.Pack(new Int32Value { Value = int.MaxValue });

        var @object = new
        {
            val1 = any1,
            val2 = any2
        };
        var convertToProtoBufRequest = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, @object);

        // Act 1
        var bytes = await _sut.ConvertAsync(convertToProtoBufRequest).ConfigureAwait(false);

        // Assert 1
        Convert.ToBase64String(bytes).Should().Be("Cj0KL3R5cGUuZ29vZ2xlYXBpcy5jb20vZ29vZ2xlLnByb3RvYnVmLlN0cmluZ1ZhbHVlEAoQBBBzEHQQZRBmEkAKLnR5cGUuZ29vZ2xlYXBpcy5jb20vZ29vZ2xlLnByb3RvYnVmLkludDMyVmFsdWUQCBD/ARD/ARD/ARD/ARAH");

        // Act 2
        var convertToJsonRequest = new ConvertToJsonRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, bytes);
        var json = await _sut.ConvertAsync(convertToJsonRequest).ConfigureAwait(false);

        // Assert 2
        json.Should().Be("""{"val1":{"@type":"type.googleapis.com/google.protobuf.StringValue","value":"stef"},"val2":{"@type":"type.googleapis.com/google.protobuf.Int32Value","value":2147483647}}""");
    }
}