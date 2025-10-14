namespace Pastebin.Tests;

using Pastebin.Services;
using Xunit;

namespace Pastebin.Tests.UnitTests;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var jwtSettings = configuration.GetSection<JwtSettings>("JwtSettings");

        _jwtService = new JwtService(jwtSettings);
    }
}
