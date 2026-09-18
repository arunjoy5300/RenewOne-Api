namespace RenewOne.Application.DTOs.Response.Dashboard
{
    public record InsuranceProductDto(
        Guid Id,
        Guid InsuranceTypeId,
        string InsuranceTypeName,
        Guid? VehicleTypeId,
        string Name,
        string Subtitle,
        string Code,
        string RoutePath,
        string? BadgeText,
        string? BadgeColor,
        string IconKey,
        int DisplayOrder
    );
}
