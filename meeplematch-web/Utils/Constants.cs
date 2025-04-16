using System.Security.Cryptography;
using DotNetEnv;
//using Microsoft.IdentityModel.Tokens;

namespace meeplematch_web.Utils;

public static class Constants
{
    //// ENVIRONMENT VARIABLES
    //public static readonly string JwtEncryptionKey =
    //    EnvironmentUtils.Instance.GetEnvironmentVariable("JWT_ENCRYPTION_KEY");

    public static readonly string PsqlConnectionString =
        EnvironmentUtils.Instance.GetEnvironmentVariable("PSQL_CONNECTION_STRING");


    public static readonly string ApiName = "MeepleMatch";
    public static string JwtToken = string.Empty;
    public static readonly string JwtTokenFromSession = "JwtToken";
    //// JWT
    //public static readonly string SecurityAlgorithm = SecurityAlgorithms.HmacSha256;

    //// ENCRYPTION
    //public static readonly int IterationCount = 700_001;
    //public static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;
    //public static readonly int HashSize = 64;
}