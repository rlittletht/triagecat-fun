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
    
    Console.Error.WriteLine("Read environment variables");
    
    tt.Model.Auth.Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey));

    Console.Error.WriteLine("Created symmetric key");
    
    Session.RexDatabase = RexDatabase.InitializeFromEnvironment(
        System.Environment.GetEnvironmentVariable("DatabaseSource") ?? "Must SqlServer | SQLite");

    Console.Error.WriteLine("Initialized database");
    
    var host = new HostBuilder()
       .ConfigureFunctionsWorkerDefaults(builder =>
                                         {
                                             builder.UseMiddleware<FetchOptions>();
                                             builder.UseMiddleware<JwtValidation>();
                                         })
       .Build();

    Console.Error.WriteLine("Built host");
    
    host.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"TriageCat FATAL STARTUP ERROR: {ex}");
    throw;
}