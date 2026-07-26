namespace Reciplex.Server.Host.Utility;

/// <summary>
/// Logic for handling command line arguments
/// </summary>
static class ArgumentHandling
{
    /// <summary>
    /// Parses arguments and applies them to the builder
    /// </summary>
    /// <param name="args">Incoming program arguments</param>
    /// <param name="builder">The builder to apply the arguments to</param>
    /// <remarks>This method will exit the application on parse failure. It assumes control over the whole program.</remarks>
    public static WebApplicationBuilder ParseAndApplyArgs(
        this WebApplicationBuilder builder,
        string[] args
    )
    {
        bool eatArg = false;
        bool seenConfig = false;

        if (!args.Any(arg => arg == "--noArgs"))
        {
            // Add custom config paths from command line
            for (int i = 0; i < args.Length; i++)
            {
                if (eatArg)
                {
                    eatArg = false;
                    continue;
                }

                if (args[i] == "-R")
                {
                    if (seenConfig)
                    {
                        Console.WriteLine("-R must come before -C, -c, or -E arguments");
                        Environment.Exit(-1);
                    }

                    builder.Configuration.Sources.Clear();
                    continue;
                }

                // Optional config, little c
                if (args[i] == "-c" && i + 1 < args.Length)
                {
                    seenConfig = true;
                    string additionalConfigPath = args[i + 1];

                    builder.Configuration.AddJsonFile(
                        additionalConfigPath,
                        optional: true,
                        reloadOnChange: true
                    );
                    eatArg = true;
                    continue;
                }

                // Mandatory config, big C
                if (args[i] == "-C" && i + 1 < args.Length)
                {
                    string additionalConfigPath = args[i + 1];
                    seenConfig = true;
                    builder.Configuration.AddJsonFile(
                        additionalConfigPath,
                        optional: false,
                        reloadOnChange: true
                    );
                    eatArg = true;
                    continue;
                }

                // -E enables environment variables sourced configuration
                if (args[i] == "-E")
                {
                    seenConfig = true;
                    // add env variables
                    builder.Configuration.AddEnvironmentVariables(prefix: "RCX_");
                    continue;
                }

                Console.WriteLine($"Unknown argument: {args[i]}");
                Environment.Exit(-1);
            }
        }

        return builder;
    }
}
