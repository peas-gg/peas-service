namespace PEAS.Helpers
{
    public static class AppLogger
    {
        public static void Log(ILogger logger, Exception exception)
        {
            switch (exception)
            {
                case AppException:
                    logger.Log(LogLevel.Information, exception.Message);
                    break;
                case Exception:
                    logger.LogError($"{exception.Message} \n {exception.StackTrace}");
                    break;
            }
        }
    }
}