using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Services;

public class ClientService(GlmsDbContext context)
{
    public Task<List<Client>> GetAllAsync() =>
        context.Clients.OrderBy(client => client.Name).ToListAsync();

    public async Task CreateAsync(Client client)
    {
        context.Clients.Add(client);
        await context.SaveChangesAsync();
    }
}
