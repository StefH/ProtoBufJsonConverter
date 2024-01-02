using Blazorise.RichTextEdit;
using Client8.Enums;
using Client8.Services;
using JsonConverter.Abstractions;
using JsonConverter.Newtonsoft.Json;
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

    private object ProtoDefinitionEditorConstructionOptions;
    private object DestinationEditorConstructionOptions;

    protected RichTextEdit richTextProtoDefinition;

    private string _protoDefinition = string.Empty;
    private string _messageType = "greet.HelloRequest";
    private ConvertType _selectedConvertType = ConvertType.ToJson;
    private string _protobufAsBase64 = "CgRzdGVm";
    private bool _skipGrpcHeader = true;
    private bool _addGrpcHeader = true;
    private string _json = string.Empty;

    private bool ProcessButtonDisabled => _state == State.Processing;

    protected override async Task OnInitializedAsync()
    {
        _protoDefinition = await Client.GetStringAsync("greet.proto");

        ProtoDefinitionEditorConstructionOptions = new
        {
            value = _protoDefinition,
            language = "protobuf",
            automaticLayout = true
        };

        DestinationEditorConstructionOptions = new
        {
            value = _json,
            language = "json",
            automaticLayout = true
        };

        await base.OnInitializedAsync();
    }

    //private StandaloneEditorConstructionOptions ProtoDefinitionEditorConstructionOptions(StandaloneCodeEditor editor)
    //{
    //    return new StandaloneEditorConstructionOptions
    //    {
    //        AutomaticLayout = true,
    //        Language = "protobuf",
    //        Value = _protoDefinition
    //    };
    //}

    //private StandaloneEditorConstructionOptions DestinationEditorConstructionOptions(StandaloneCodeEditor editor)
    //{
    //    return new StandaloneEditorConstructionOptions
    //    {
    //        AutomaticLayout = true,
    //        Language = "json",
    //        Value = _json
    //    };
    //}

    private async Task OnClick()
    {
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
        _json = await ProtoBufConverterApi.ConvertToJsonAsync(convertToJsonRequest);
    }

    private async Task ConvertToProtoBufAsync()
    {
        _protobufAsBase64 = string.Empty;

        var convertToProtoBufRequest = new ConvertToProtoBufRequest(_protoDefinition, _messageType, _json)
            .WithGrpcHeader(_addGrpcHeader);
        var bytes = await ProtoBufConverterApi.ConvertToProtoBufAsync(convertToProtoBufRequest);

        _protobufAsBase64 = Convert.ToBase64String(bytes);
    }
}