using Shared;

namespace Api;

internal record CombineRequest
(
    byte[] ImageBytes, 
    ProcessType ProcessType, 
    ResizeOption ResizeOption,
    int? Width = null
);