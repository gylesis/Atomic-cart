namespace Dev.Infrastructure
{
    public struct Result
    {
        public bool IsError { get; set; }
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
}