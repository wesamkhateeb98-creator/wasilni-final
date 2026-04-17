namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IWhatsAppRepository
{
    public Task<bool> SendCode(string phonenumber, string code, CancellationToken cancellationToken);
}
