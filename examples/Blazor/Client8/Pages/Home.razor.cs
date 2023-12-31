using Client8.Enums;
using Client8.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared;

namespace Client8.Pages;

public partial class Home
{
    [Inject]
    private IProcessImageApi ProcessImageApi { get; set; } = null!;

    private State _state = State.None;

    private ProcessType _selectedProcessType;
    private ResizeOption _selectedResizeOption;
    private int? _selectedWidth = 512;

    private IBrowserFile? _selectedFile;
    private byte[]? _sourceBytes;
    private byte[] _processedBytes = Array.Empty<byte>();
    private string _url = string.Empty;
    private string _name = string.Empty;
    private string _error = string.Empty;
    private ImageDetails? _imageDetails;

    private bool ProcessButtonDisabled => _selectedFile == null || _state == State.Processing;

    private async Task OnClick()
    {
        _url = string.Empty;
        _name = string.Empty;
        _error = string.Empty;

        _state = State.Processing;

        if (_selectedFile == null || _sourceBytes == null)
        {
            _error = "No file selected";
            _state = State.Error;
            return;
        }

        try
        {
            var width = _selectedResizeOption == ResizeOption.Custom ? _selectedWidth : null;

            await using var processedStream = await ProcessImageApi
                .ProcessImageAsync(_sourceBytes, _selectedResizeOption, _selectedProcessType, width)
                .ConfigureAwait(false);

            _processedBytes = ((MemoryStream)processedStream).ToArray();

            _url = $"data:image/jpg;base64,{Convert.ToBase64String(_processedBytes)}";
            _name = $"{Path.GetFileNameWithoutExtension(_selectedFile.Name)}_processed.jpg";
            _error = string.Empty;
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

    private async Task LoadFile(InputFileChangeEventArgs e)
    {
        _selectedFile = e.GetMultipleFiles(1).FirstOrDefault();

        if (_selectedFile == null)
        {
            _error = "No file selected";
            _state = State.Error;
            return;
        }

        _imageDetails = null;

        await using var fileStream = _selectedFile.OpenReadStream(int.MaxValue);
        using var memoryStream = new MemoryStream();

        await fileStream.CopyToAsync(memoryStream).ConfigureAwait(false);

        _sourceBytes = memoryStream.ToArray();

        _imageDetails = await ProcessImageApi.GetImageDetailsAsync(_sourceBytes).ConfigureAwait(false);

        _url = string.Empty;
        _name = string.Empty;
        _error = string.Empty;

        StateHasChanged();
    }
}