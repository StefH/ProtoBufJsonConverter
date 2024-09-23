using Client8.Enums;
using Client8.Services;
using Microsoft.AspNetCore.Components;
using ProtoBufJsonConverter.Models;

namespace Client8.Pages;

public partial class Home
{
    [Inject]
    public required IProtoBufConverterApi ProtoBufConverterApi { get; set; }

    [Inject]
    public required HttpClient Client { get; set; }

    private State _state = State.None;
    private string _error = string.Empty;
    private string _protoDefinition = string.Empty;
    private string _messageType = "greet.HelloRequest";
    private ConvertType _selectedConvertType = ConvertType.ToJson;
    private string _protobufAsBase64 = "CgRzdGVm";
    private string _protobufAsByteArray = "new byte[] { 0x0A, 0x04, 0x73, 0x74, 0x65, 0x66 }";
    private bool _skipGrpcHeader = true;
    private bool _addGrpcHeader = true;
    private string _json = "{\r\n  \"name\": \"stef\"\r\n}";

    private bool IsProcessing => _state == State.Processing;

    protected override async Task OnInitializedAsync()
    {
        _protoDefinition = await Client.GetStringAsync("greet.proto");

        await base.OnInitializedAsync();
    }

    private async Task OnClick()
    {
        _error = string.Empty;
        _state = State.Processing;

        try
        {
            if (_selectedConvertType == ConvertType.ToJson)
            {
                await ConvertToJsonAsync();
            }
            else
            {
                await ConvertToProtoBufAsync();
            }

            _state = State.Done;
        }
        catch (Exception ex)
        {
            _error = ex.Message;
            _state = State.Error;
        }
        finally
        {
            StateHasChanged();
        }
    }

    private async Task ConvertToJsonAsync()
    {
        _json = string.Empty;

        var bytes = Convert.FromBase64String(_protobufAsBase64);

        var convertToJsonRequest = new ConvertToJsonRequest(_protoDefinition, _messageType, bytes)
            .WithSkipGrpcHeader(_skipGrpcHeader)
            .WithWriteIndented();

        _json = await ProtoBufConverterApi.ConvertToJsonAsync(convertToJsonRequest);
    }

    private async Task ConvertToProtoBufAsync()
    {
        _protobufAsBase64 = string.Empty;
        _protobufAsByteArray = string.Empty;

        StateHasChanged();

        var convertToProtoBufRequest = new ConvertToProtoBufRequest(_protoDefinition, _messageType, _json)
            .WithGrpcHeader(_addGrpcHeader);

        var bytes = await ProtoBufConverterApi.ConvertToProtoBufAsync(convertToProtoBufRequest);
        _protobufAsBase64 = Convert.ToBase64String(bytes);
        _protobufAsByteArray = ByteArrayToString(bytes);
    }

    public static string ByteArrayToString(byte[] byteArray)
    {
        return string.Concat("new byte[] { ", string.Join(", ", byteArray.Select(b => $"0x{b:X2}")), " };");
    }
}