namespace Dev.Infrastructure
{
    public struct Result
    {
        public bool IsError { get; set; }
        public bool IsSuccess => !IsError;

        public string ErrorMessage { get; set; }


        public static Result Error(string message)
        {
            return new Result { IsError = true, ErrorMessage = message };
        }

        public static Result Success()
        {
            return new Result { IsError = false };
        }

        public static implicit operator bool(Result result)
        {
            return !result.IsError;
        }
       
    }
    
    public struct Result<TData>
    {
        public bool IsError { get; set; }
        public bool IsSuccess => !IsError;
        public string ErrorMessage { get; set; }

        public TData Data { get; private set; }

        public static Result<TData> Error(string message)
        {
            return new Result<TData> { IsError = true, ErrorMessage = message };
        }

        public static Result<TData> Success(TData data)
        {
            return new Result<TData> { IsError = false, Data = data };
        }

        public static implicit operator bool(Result<TData> result)
        {
            return !result.IsError;
        }
       
    }
    
    
    
}