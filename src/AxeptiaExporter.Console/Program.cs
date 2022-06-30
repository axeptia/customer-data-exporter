using AxeptiaExporter.ConsoleApp.DbCommunication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AxeptiaExporter.ConsoleApp
{
    public class Program
    {
        const int lockfileMaxLockHours = 24;

        //If we want to send files to test instead of production.
        const string TEST_ENVIRONMENT_FLAG = "--testenvironment";
        public static async Task Main(string[] args)
        {

            var programBaseDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var logDirectory = programBaseDirectory + "/log";
            var services = new ServiceCollection();
            var lockFile = new Lockfile(programBaseDirectory);
            ILogger<Program> _logger;

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            var runProduction = true;
            if (args.Contains(TEST_ENVIRONMENT_FLAG))
            {
                runProduction = false;
                var testEnvironmentFlagParam = new string[]{TEST_ENVIRONMENT_FLAG};
                args = args.Except(testEnvironmentFlagParam).ToArray();
            }

            ConfigureServices(services, logDirectory, runProduction);
            using ServiceProvider serviceProvider = services.BuildServiceProvider();

            _logger = serviceProvider.GetService<ILogger<Program>>();

            if (lockFile.DoLockfileExists())
            {
                _logger.LogInformation("Lockfile exist, program is already running");
                if (lockFile.WasLockFileCreatedBefore(DateTime.UtcNow.AddHours(-lockfileMaxLockHours)))
                {
                    _logger.LogInformation("Program has been running for 24 hours. Lockfile is being removed. Program will run normal next time");
                    lockFile.RemoveLockFile();
                }
                return;
            }

            //Create lockfile to prevent staring program when it's already running;
            _logger.LogInformation("Lockfile created to prevent running this program when it's already running");
            lockFile.CreateLockFile();

            try
            {
                var app = serviceProvider.GetService<App>();
                // Start up logic here
                await app.Run(args, programBaseDirectory);
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetService<ILogger<Program>>();
                logger.LogError(ex, "Something bad happened when executing the program");
            }
            finally
            {
                var logger = serviceProvider.GetService<ILogger<Program>>();
                try
                {
                    logger.LogInformation("Try removing lockfile");
                    lockFile.RemoveLockFile();
                    logger.LogInformation("Lockfile removed");
                }
                catch (Exception ex)
                { 
                    logger.LogError(ex,"Something bad happened when removing lockfile");
                }
            }
        }

        private static void ConfigureServices(ServiceCollection services, string logDirectory, bool runProduction)
        {

            const string BASE_URL_API_MANAGEMENT_TEST = "https://test-api.axeptia.app/";
            const string BASE_URL_API_MANAGEMENT_PROD = "https://api.axeptia.app/";
            const string OCP_KEY_API_MANAGEMENT_TEST = "4282f05f4aa04aa69811bd045847d980";
            const string OCP_KEY_API_MANAGEMENT_PROD = "771e65cf45484e3aae03034971586c65";

            var baseUrlApiManagement = runProduction ? BASE_URL_API_MANAGEMENT_PROD : BASE_URL_API_MANAGEMENT_TEST;
            var apiManagementOcpKey = runProduction ? OCP_KEY_API_MANAGEMENT_PROD : OCP_KEY_API_MANAGEMENT_TEST;

            var serilogLogger = new LoggerConfiguration()
             .WriteTo.RollingFile(logDirectory + "/Axeptia-exporter-log-{Date}.txt", retainedFileCountLimit: 31)
             .MinimumLevel.Information()
             .CreateLogger();

            services.AddLogging(configure => configure.AddSerilog(serilogLogger))
            .AddTransient<Core>()
            .AddTransient<ConfigFileManager>()
            .AddTransient<IDbCommunication, MssqlDbCommunication>()
            .AddTransient<Persistor>()
            .AddTransient<BlobUploader>()
            .AddTransient<App>()
            .AddTransient<SasTokenManagementInfrastructure>()
            .AddHttpClient("ApiManagement", client => 
            {
                client.BaseAddress = new Uri(baseUrlApiManagement);
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiManagementOcpKey);
            });
        }
    }
}