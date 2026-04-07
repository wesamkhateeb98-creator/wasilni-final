using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Accounts;
using SoftPro.Wasilni.Presentation.Models.Request.Account;

namespace SoftPro.Wasilni.Presentation.Extensions.AccountExtensions;

public static class ToModelExtensions
{
    public static RegisterModel ToModel(this SignupPassengerRequest registerRequest)
        => new(
            registerRequest.Username,
            registerRequest.Phonenumber,
            registerRequest.Password,
            registerRequest.FCMToken,
            Role.Passenger,
            registerRequest.key);

    public static LoginModel ToModel(this LoginAccountRequest loginRequest)
        => new(
            loginRequest.Phonenumber,
            loginRequest.Password);

    public static RefreshModel ToModel(this RefreshRequest loginRequest)
        => new(loginRequest.RefreshToken);

    //public static AddCityInputModel ToModel(this AddCityRequest addCity)
    //    => new(
    //        addCity.ArabicName,
    //        addCity.EnglishName,
    //        addCity.CityId);

    //public static UpdateCityInputModel ToModel(this UpdateCityRequest updateCity)
    //    => new(
    //        updateCity.Id,
    //        updateCity.ArabicName,
    //        updateCity.EnglishName,
    //        updateCity.CityId);

    //public static GetCityFilterModel ToModel(this GetCityFilterRequest getCityFilter)
    //    => new(
    //        getCityFilter.Name,
    //        getCityFilter.PageNumber,
    //        getCityFilter.PageSize);

    //public static UpdateLineModel ToModel(this UpdateLineRequest updateLineRequest)
    //    => new(
    //            updateLineRequest.Id,
    //            updateLineRequest.FromId,
    //            updateLineRequest.ToId,
    //            updateLineRequest.BusType,
    //            updateLineRequest.Cost);


    //public static Page<GetLineResponse> ToResponse(this Page<GetLineModel> model)
    //    => new Page<GetLineResponse>(
    //            model.PageNumber,
    //            model.PageSize,
    //            model.TotalPages,
    //            model.Content.Select(x => x.ToResponse()).ToList()
    //        );

    //public static GetLineResponse ToResponse(this GetLineModel model)
    //    => new GetLineResponse(model.Id, model.CityFromId, model.CityToId, model.CityFrom, model.CityTo,model.BusType, model.Cost);
}
