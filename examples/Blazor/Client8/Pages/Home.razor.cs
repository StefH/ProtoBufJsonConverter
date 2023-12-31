using Client8.Enums;
using Client8.Services;
using Microsoft.AspNetCore.Components;
using ProtoBufJsonConverter;
using ProtoBufJsonConverter.Models;

namespace Client8.Pages;

public partial class Home
{
    [Inject]
    private IProtoBufConverterApi ProtoBufConverterApi { get; set; } = null!;

    [Inject]
    private HttpClient Client { get; set; } = null!;

    private State _state = State.None;

    private string _json = string.Empty;

    private bool ProcessButtonDisabled => _state == State.Processing;

    private async Task OnClick()
    {
        _json = string.Empty;

        _state = State.Processing;

        try
        {
            var protoDefinition = await Client.GetStringAsync("greet.proto");

            var bytes = Convert.FromBase64String("CgRzdGVm");

            var messageType = "greet.HelloRequest";
            
            var convertToJsonRequest = new ConvertToJsonRequest(protoDefinition, messageType, bytes);
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