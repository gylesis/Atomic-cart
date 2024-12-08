using Cysharp.Threading.Tasks;

namespace Dev.Infrastructure.Networking
{
    public class ModulesService
    {
        private IInitializableModule[] _modules;
        private InternetChecker _internetChecker;

        public bool IsInitialized { get; private set; }
        
        public ModulesService(IInitializableModule[] modules, InternetChecker internetChecker)
        {
            _internetChecker = internetChecker;
            _modules = modules;
        }

        public async UniTask<Result> Initialize(bool reinitializeModules = false)
        {
            foreach (var module in _modules)    
            {
                if (!reinitializeModules && module.IsInitialized) continue;

                var check = await _internetChecker.Check(GlobalDisposable.SceneScopeToken);
                
                if(check == false)
                    return Result.Error("Invalid Internet Connection");

                var result = await module.Initialize();
                
                if(result.IsError)
                    return result;
            }

            IsInitialized = true;
            
            return Result.Success();
        }
    }
}