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
    private IProtoBufConverterApi ProtoBufConverterApi { get; set; } = null!;

    [Inject]
    private HttpClient Client { get; set; } = null!;

    private State _state = State.None;

    private object ProtoDefinitionEditorConstructionOptions;
    private object DestinationEditorConstructionOptions;

    protected RichTextEdit richTextProtoDefinition;

    private string _protoDefinition = string.Empty;
    private string _protobufAsBase64 = "CgRzdGVm";
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
        _json = string.Empty;

        _state = State.Processing;

        try
        {
            var bytes = Convert.FromBase64String(_protobufAsBase64);

            var messageType = "greet.HelloRequest";
            
            var convertToJsonRequest = new ConvertToJsonRequest(_protoDefinition, messageType, bytes)
                .WithJsonConverter(new NewtonsoftJsonConverter())
                .WithJsonConverterOptions(new JsonConverterOptions { WriteIndented = true });
            _json = await ProtoBufConverterApi.ConvertToJsonAsync(convertToJsonRequest);

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
}