namespace BitcoinTransactionTool.Services
{
    public class Response<T> : Response
    {
        public T Result { get; set; }
    }


    public class Response
    {
        public ErrorCollection Errors { get; } = new ErrorCollection();
    }
}
