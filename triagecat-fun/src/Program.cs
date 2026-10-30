using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using tt.Database;
using tt.Model;
using tt.Middleware;

try
{
    Console.Error.WriteLine("At top of loop");
    string rawKey = System.Environment.GetEnvironmentVariable("TraynrexBlue.SigningKey") ?? "";

    tt.Model.Auth.Issuer = System.Environment.GetEnvironmentVariable("TraynrexBlue.Issuer") ?? "";
    tt.Model.Auth.Audience = System.Environment.GetEnvironmentVariable("TraynrexBlue.Audience") ?? "";
    
    tt.Model.Auth.Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey));
    
    Session.RexDatabase = RexDatabase.InitializeFromEnvironment(
        System.Environment.GetEnvironmentVariable("DatabaseSource") ?? "Must SqlServer | SQLite");
    
    var host = new HostBuilder()
       .ConfigureFunctionsWorkerDefaults(builder =>
                                         {
                                             builder.UseMiddleware<FetchOptions>();
                                             builder.UseMiddleware<JwtValidation>();
                                         })
       .Build();
    
    host.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"TriageCat FATAL STARTUP ERROR: {ex}");
    throw;
}