using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Greet;

var any1 = Any.Pack(new StringValue { Value = "stef" });
var any2 = Any.Pack(new Int32Value { Value = int.MaxValue });
var any3 = Any.Pack(new HelloRequest { Name = "stef" });
var result = Convert.ToBase64String(any3.ToByteArray());

int x = 0;