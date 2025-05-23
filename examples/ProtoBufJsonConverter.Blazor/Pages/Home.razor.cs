﻿using Microsoft.AspNetCore.Components;
using ProtoBufJsonConverter.Blazor.Enums;
using ProtoBufJsonConverter.Models;

namespace ProtoBufJsonConverter.Blazor.Pages;

public partial class Home
{
    [Inject]
    public required HttpClient Client { get; set; }

    [Inject]
    public required IConverter Converter { get; set; }

    private State _state = State.None;
    private string _error = string.Empty;
    private string _protoDefinition = string.Empty;
    private string _messageType = string.Empty;
    private string[] _messageTypes = [];
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

        await OnProtoDefinitionChanged(_protoDefinition);

        await base.OnInitializedAsync();
    }

    private async Task OnProtoDefinitionChanged(string? document)
    {
        if (!string.IsNullOrEmpty(document))
        {
            var informationRequest = new GetInformationRequest(document);

            _error = string.Empty;
            _state = State.Processing;

            try
            {
                var information = await Converter.GetInformationAsync(informationRequest);
                _messageTypes = information.MessageTypes.Keys.ToArray();
                _messageType = _messageTypes.FirstOrDefault(mt => mt == "greet.HelloRequest") ?? _messageTypes.FirstOrDefault() ?? string.Empty;

                _state = State.Done;
            }
            catch (Exception ex)
            {
                _messageTypes = [];
                _messageType = string.Empty;

                _error = ex.Message;
                _state = State.Error;
            }
            finally
            {
                StateHasChanged();
            }
        }
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

        _json = await Converter.ConvertAsync(convertToJsonRequest);
    }

    private async Task ConvertToProtoBufAsync()
    {
        _protobufAsBase64 = string.Empty;
        _protobufAsByteArray = string.Empty;

        StateHasChanged();

        var convertToProtoBufRequest = new ConvertToProtoBufRequest(_protoDefinition, _messageType, _json)
            .WithGrpcHeader(_addGrpcHeader);

        var bytes = await Converter.ConvertAsync(convertToProtoBufRequest);
        _protobufAsBase64 = Convert.ToBase64String(bytes);
        _protobufAsByteArray = ByteArrayToString(bytes);
    }

    public static string ByteArrayToString(byte[] byteArray)
    {
        return string.Concat("new byte[] { ", string.Join(", ", byteArray.Select(b => $"0x{b:X2}")), " };");
    }
}