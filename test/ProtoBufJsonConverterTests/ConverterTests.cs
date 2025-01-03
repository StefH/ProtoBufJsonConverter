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

    private const string ProtoDefinitionWithCSharpNamespace = @"
option csharp_namespace = ""Test"";

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
    rpc SayEmpty (MyMessageEmpty) returns (MyMessageEmpty);
    rpc SayTimestamp (MyMessageTimestamp) returns (MyMessageTimestamp);
    rpc SayDuration (MyMessageDuration) returns (MyMessageDuration);
}

message MyMessageTimestamp {
    google.protobuf.Timestamp ts = 1;
}

message MyMessageDuration {
    google.protobuf.Duration du = 1;
}

message MyMessageEmpty {
    google.protobuf.Empty e = 1;
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
    google.protobuf.Value val5 = 5;
    google.protobuf.Value val6 = 6;
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

    [Fact]
    public async Task ConvertAsync_ConvertToJsonRequest_WithCSharpNamespace()
    {
        // Arrange
        var bytes = Convert.FromBase64String("CgRzdGVm");

        var request = new ConvertToJsonRequest(ProtoDefinitionWithCSharpNamespace, "Test.HelloRequest", bytes);

        // Act
        var json = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        json.Should().Be("""{"name":"stef"}""");
    }

    [Fact]
    public async Task ConvertAsync_ConvertJsonToProtoBufRequest_WithCSharpNamespace()
    {
        // Arrange
        const string messageType = "Test.HelloRequest";
        const string json = @"{""name"":""stef""}";
        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithCSharpNamespace, messageType, json);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be("CgRzdGVm");
    }

    [Theory]
    [InlineData("MyMessageTimestamp", """{"ts":{"seconds":1722301323,"nanos":12300}}""")]
    [InlineData("MyMessageDuration", """{"du":{"seconds":1722301323,"nanos":12300}}""")]
    public async Task ConvertAsync_ConvertToJsonRequest_ConvertNewerGoogleWellKnownTypesToJsonRequest(string messageType, string expectedJson)
    {
        // Arrange
        var bytes = Convert.FromBase64String("CgkIi/egtQYQuWA=");

        var request = new ConvertToJsonRequest(ProtoDefinitionWithWellKnownTypes, messageType, bytes);

        // Act
        var json = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        json.Should().Be(expectedJson);
    }

    [Theory]
    [InlineData("MyMessageTimestamp", """{"ts":"2024-12-31T23:59:58Z"}""")]
    [InlineData("MyMessageDuration", """{"du":"20088.23:59:58"}""")]
    public async Task ConvertAsync_ConvertToJsonRequest_NoNewerGoogleWellKnownTypes_ConvertTimestampToJsonRequest(string messageType, string expectedJson)
    {
        // Arrange
        var bytes = Convert.FromBase64String("CgYI/orSuwY=");

        var request = new ConvertToJsonRequest(ProtoDefinitionWithWellKnownTypes, messageType, bytes, supportNewerGoogleWellKnownTypes: false);

        // Act
        var json = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        json.Should().Be(expectedJson);
    }

    [Theory]
    [InlineData(true, "AAAAAAYKBHN0ZWY=")]
    [InlineData(false, "CgRzdGVm")]
    public async Task ConvertAsync_ConvertJsonToProtoBufRequest(bool addGrpcHeader, string expectedBytes)
    {
        // Arrange
        const string messageType = "greet.HelloRequest";
        const string json = """{"name":"stef"}""";

        var request = new ConvertToProtoBufRequest(ProtoDefinition, messageType, json, addGrpcHeader);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be(expectedBytes);
    }

    [Theory]
    [InlineData("MyMessageTimestamp", """{"ts":{"seconds":1722301323,"nanos":12345}}""")]
    [InlineData("MyMessageDuration", """{"du":{"seconds":1722301323,"nanos":12345}}""")]
    public async Task ConvertAsync_ConvertJsonToProtoBufRequest_NewerGoogleWellKnownTypes(string messageType, string json)
    {
        // Arrange
        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypes, messageType, json);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be("CgkIi/egtQYQjGA=");
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
    public async Task ConvertAsync_WellKnownTypesMessageWithEmpty_ConvertJsonToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessageEmpty";
        const string json = "{ \"e\": {} }";

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypes, messageType, json, addGrpcHeader: false);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be("CgA=");
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypesMessageWithEmpty_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessageEmpty";
        var @object = new
        {
            e = new { }
        };

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypes, messageType, @object, addGrpcHeader: false);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be("CgA=");
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
    public async Task ConvertAsync_WellKnownTypesTimestamp_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessageTimestamp";

        var @object = new
        {
            ts = new
            {
                seconds = 1722301323,
                nanos = 12345
            }
        };

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypes, messageType, @object);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be("CgkIi/egtQYQjGA=");
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypesTimestamp_NoNewerGoogleWellKnownTypes_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessageTimestamp";

        var @object = new
        {
            ts = new DateTime(2024, 12, 31, 23, 59, 58, DateTimeKind.Utc)
        };

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypes, messageType, @object, supportNewerGoogleWellKnownTypes: false);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be("CgYI/orSuwY=");
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
            },
            val5 = new Value
            {
                StructValue = new Struct
                {
                    Fields =
                    {
                        { "str", new Value("strValue") },
                        { "bo", new Value(false) }
                    }
                }
            },
            val6 = new Value
            {
                ListValue = new ListValue
                {
                    Values =
                    {
                        new Value("test"),
                        new Value(-42)
                    }
                }
            }
        };
        var convertToProtoBufRequest = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, @object);

        // Act 1
        var bytes = await _sut.ConvertAsync(convertToProtoBufRequest).ConfigureAwait(false);

        // Assert 1
        Convert.ToBase64String(bytes).Should().Be("CgIIABIJEQAAAAAAAEVAGgYaBFN0ZWYiAiABKh8qHQoRCgNzdHISChoIc3RyVmFsdWUKCAoCYm8SAiAAMhUyEwoGGgR0ZXN0CgkRAAAAAAAARcA=");

        // Act 2
        var convertToJsonRequest = new ConvertToJsonRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, bytes);
        var json = await _sut.ConvertAsync(convertToJsonRequest).ConfigureAwait(false);

        // Assert 2
        json.Should().Be("""{"val1":{"null_value":0},"val2":{"number_value":42.0},"val3":{"string_value":"Stef"},"val4":{"bool_value":true},"val5":{"struct_value":{"fields":{"str":{"string_value":"strValue"},"bo":{"bool_value":false}}}},"val6":{"list_value":{"values":[{"string_value":"test"},{"number_value":-42.0}]}}}""");
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
        json.Should().Be("{\"val\":{\"fields\":{\"str\":{\"string_value\":\"strValue\"},\"bo\":{\"bool_value\":false}}}}");
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
        json.Should().Be("{\"val\":{\"values\":[{\"string_value\":\"strValue\"},{\"number_value\":99.0}]}}");
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