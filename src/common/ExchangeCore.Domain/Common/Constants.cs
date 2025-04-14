namespace ExchangeCore.Domain.Common;

public static class Constants
{
    public const string DATEONLY_FORMAT = "yyyy-MM-dd";
    
    public const string CURRENCY_CONVERTER_CLIENT = "currency_api";
    
    public const string CURRENCY_CONVERTER_URL_LATEST = "latest?from={0}";
    
    public const string CURRENCY_CONVERTER_URL_CONVERSION = "latest?amount={0}&from={1}&to={2}";
    
    public const string CURRENCY_CONVERTER_URL_HISTORIC_RATES = "{0}..{1}?from={2}";
    
    public const string HTTP_DEFAULT_RETRY_STRATEGY = "defaultretry";
    
    public const string JWT_Client_Id_Key = "client_id";
}
