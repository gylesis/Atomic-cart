namespace Dev
{
    public class ZenjectSingleton<T> where T : class, new() // TODO
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null) 
                    _instance = new T();
                
                return _instance;
            }
        }


        public ZenjectSingleton()
        {
           
        }
        
    }
}