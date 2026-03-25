namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IWhatsAppRepository
{
    public Task SendCode(string phonenumber, string code);
}
