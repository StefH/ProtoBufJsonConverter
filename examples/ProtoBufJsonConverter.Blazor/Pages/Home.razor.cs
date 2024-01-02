using Blazorise;
using JsonConverter.Abstractions;
using Microsoft.AspNetCore.Components;
using ProtoBufJsonConverter.Blazor.Enums;
using ProtoBufJsonConverter.Models;

namespace ProtoBufJsonConverter.Blazor.Pages;

public partial class Home
{
    [Inject]
    public required HttpClient Client { get; set; }

    [Inject]
    public required IConverter Converter { get; set; }

    private Button _btnClick;

    private State _state = State.None;
    private string _error = string.Empty;
    private string _protoDefinition = string.Empty;
    private string _messageType = "greet.HelloRequest";
    private ConvertType _selectedConvertType = ConvertType.ToJson;
    private string _protobufAsBase64 = "CgRzdGVm";
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
            .WithJsonConverterOptions(new JsonConverterOptions { WriteIndented = true });

        _json = await Converter.ConvertAsync(convertToJsonRequest);
    }

    private async Task ConvertToProtoBufAsync()
    {
        _protobufAsBase64 = string.Empty;

        var convertToProtoBufRequest = new ConvertToProtoBufRequest(_protoDefinition, _messageType, _json)
            .WithGrpcHeader(_addGrpcHeader);

        var bytes = await Converter.ConvertAsync(convertToProtoBufRequest);
        _protobufAsBase64 = Convert.ToBase64String(bytes);
    }
}