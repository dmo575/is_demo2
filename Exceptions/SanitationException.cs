public class SanitationException : Exception
{
    public List<TokenError> errorList = new List<TokenError>();
    public SanitationException(List<TokenError> el)
    {
        errorList = el;
    }
}