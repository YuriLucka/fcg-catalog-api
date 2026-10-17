namespace Catalog.Application.Exceptions
{
    public class SearchUnavailableException : Exception
    {
        public SearchUnavailableException(Exception inner)
            : base("Serviço de busca indisponível no momento.", inner) { }
    }
}
