using DotNetEnv;

namespace meeplematch_web.Utils;

public class EnvironmentUtils
{
    private static EnvironmentUtils _instance;

    private EnvironmentUtils()
    {
    }

    public static EnvironmentUtils Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new EnvironmentUtils();
            }

            return _instance;
        }
    }

    public string GetEnvironmentVariable(string key)
    {
        Env.Load("Resources/.env");
        return Env.GetString(key);
    }
}