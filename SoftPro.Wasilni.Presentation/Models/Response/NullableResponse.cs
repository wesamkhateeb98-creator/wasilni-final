namespace SoftPro.Wasilni.Presentation.Models.Response;

public record NullableResponse<T> (T? Data) where T : class;

