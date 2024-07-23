namespace CertainCourse.GatewayService.Infrastructure.Validation;

internal sealed record ValidationFailureDto(string ParameterName, string ErrorMessage);