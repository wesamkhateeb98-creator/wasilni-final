using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Accounts;
using SoftPro.Wasilni.Presentation.Models.Response.Account;

namespace SoftPro.Wasilni.Presentation.Extensions.AccountExtensions;

public static class ToResponseExtensions
{
    //public static Page<GetCityResponse> ToResponse(this Page<GetCityModel> getCities)
    //    => new(
    //          getCities.PageNumber,
    //          getCities.PageSize,
    //          getCities.TotalPages,
    //          getCities.Content.Select(x => x.ToResponse()).ToList()
    //        );

    //private static GetCityResponse ToResponse(this GetCityModel cityModel)
    //    => new(cityModel.Id, cityModel.ArabicName, cityModel.EnglishName, cityModel.CityId, cityModel.Lock);

    public static Page<SearchByPhonenumberResponse> ToResponse(this Page<SearchByPhoneNumberModel> getCities)
        => new(
              getCities.PageNumber,
              getCities.PageSize,
              getCities.TotalPages,
              getCities.Content.Select(x => x.ToResponse()).ToList()
            );

    public static TokenResponse ToResponse(this LoginModelExtended loginModelExtended)
        => new(
            loginModelExtended.Id,
            loginModelExtended.PhoneNumber,
            loginModelExtended.FirstName,
            loginModelExtended.LastName,
            loginModelExtended.DateOfBirth,
            loginModelExtended.Gender,
            loginModelExtended.Token,
            loginModelExtended.ExpirationDate,
            loginModelExtended.Role,
            loginModelExtended.RefreshToken,
            loginModelExtended.Permission
            );
    private static SearchByPhonenumberResponse ToResponse(this SearchByPhoneNumberModel searchByPhoneNumber)
        => new(searchByPhoneNumber.Id, searchByPhoneNumber.FirstName, searchByPhoneNumber.LastName, searchByPhoneNumber.DateOfBirth, searchByPhoneNumber.Gender, searchByPhoneNumber.Phonenumber);

    public static Page<UserResponse> ToUserResponse(this Page<SearchByPhoneNumberModel> page)
        => new(
            page.PageNumber,
            page.PageSize,
            page.TotalPages,
            page.Content.Select(x => new UserResponse(x.Id, x.FirstName, x.LastName, x.DateOfBirth, x.Gender, x.Phonenumber)).ToList()
        );

}
