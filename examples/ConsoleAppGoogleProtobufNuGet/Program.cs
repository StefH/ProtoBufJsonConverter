using System.Runtime.InteropServices.JavaScript;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Greet;

var m = new MessageOptions();

var any1 = JSType.Any.Pack(new StringValue { Value = "stef" });
var any2 = JSType.Any.Pack(new Int32Value { Value = int.MaxValue });
var any3 = JSType.Any.Pack(new HelloRequest { Name = "stef" });
var result = Convert.ToBase64String(any3.ToByteArray());

int x = 0;