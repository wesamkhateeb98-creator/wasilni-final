namespace SoftPro.Wasilni.Domain.Models;

public record Page<T>(
    int PageNumber,
    int PageSize,
    int TotalPages,
    List<T> Content
    );
